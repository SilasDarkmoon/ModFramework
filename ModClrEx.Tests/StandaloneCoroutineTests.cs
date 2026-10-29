using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace ModClrEx.Tests
{
    /// <summary>
    /// Instruction semantics tests for StandaloneCoroutine.
    /// Three core dimensions:
    ///  1. Resume phase (CoroutinePhase): an instruction resumes at its declared Phase once Done;
    ///  2. Resume frame: same frame if the instruction's Phase has not been passed yet this frame,
    ///     next frame otherwise;
    ///  3. Pass-through: an instruction that is already Done at creation (zero-second wait,
    ///     satisfied predicate, synchronously completed sub-coroutine) does not suspend and the
    ///     following code executes immediately.
    /// Frame layout: 41 ticks per frame (Phase 0..40); tick 1+41*k+p is Phase p of frame k.
    /// </summary>
    public class StandaloneCoroutineTests
    {
        /// <summary>Records the (phase, frame) at which each mark occurs, for precise resume-timing assertions.</summary>
        private sealed class Trace
        {
            public Mod.StandaloneCoroutineProvider Provider = null!;
            public readonly List<(string Tag, Mod.CoroutinePhase Phase, int Frame)> Marks
                = new List<(string, Mod.CoroutinePhase, int)>();
            public void Mark(string tag)
            {
                Marks.Add((tag, Provider.CurrentPhase, Provider.CurrentFrameCount));
            }
            public (Mod.CoroutinePhase Phase, int Frame) At(string tag)
            {
                int idx = Marks.FindIndex(m => m.Tag == tag);
                Assert.True(idx >= 0, $"Mark {tag} not found, actual order: {string.Join(",", Tags)}");
                var mark = Marks[idx];
                return (mark.Phase, mark.Frame);
            }
            public string[] Tags => Marks.Select(m => m.Tag).ToArray();
        }

        private static int RunUntil(Mod.StandaloneCoroutineProvider provider, Func<bool> done, int maxTicks = 500)
        {
            int ticks = 0;
            while (!done() && ticks < maxTicks)
            {
                provider.TickCoroutineLooper();
                ++ticks;
            }
            Assert.True(done(), $"Coroutine did not complete within {maxTicks} ticks");
            return ticks;
        }

        private sealed class CustomLateUpdateInstruction : Mod.ICoroutineInstruction
        {
            public Mod.CoroutinePhase Phase => Mod.CoroutinePhase.PostLateUpdate;
            public object Attachment { get; set; } = null!;
        }

        // ---- Basics / default wait ----

        [Fact]
        public void TestEmptyCoroutineRunsSynchronously()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator Work()
            {
                trace.Mark("a");
                yield break;
            }
            provider.StartCoroutine(Work());
            Assert.Equal(new[] { "a" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper());
        }

        [Fact]
        public void TestYieldNullResumesAtDefaultPhaseSameFrame()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("start");
                yield return null;
                trace.Mark("resumed");
                done = true;
            }
            provider.StartCoroutine(Work());
            // executed only up to the yield, no pass-through
            Assert.Equal(new[] { "start" }, trace.Tags);
            RunUntil(provider, () => done);
            var (phase, frame) = trace.At("resumed");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phase);
            Assert.Equal(0, frame);
        }

        [Fact]
        public void TestYieldDefaultInstructionSameAsNull()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitDefault();
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phase);
            Assert.Equal(0, frame);
        }

        [Fact]
        public void TestYieldNullAtDefaultPhaseWaitsNextFrame()
        {
            // Yielding null again after resuming at PostUpdate_WaitForDefault:
            // that phase has already passed this frame, so it must wait until the same phase next frame
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                yield return null;
                trace.Mark("first");
                yield return null;
                trace.Mark("second");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (phase1, frame1) = trace.At("first");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phase1);
            Assert.Equal(0, frame1);
            var (phase2, frame2) = trace.At("second");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phase2);
            Assert.Equal(1, frame2);
        }

        [Fact]
        public void TestUnknownYieldObjectTreatedAsDefault()
        {
            // Unity semantics: yielding any non-instruction object is equivalent to yield return null
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return 42;
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phase);
            Assert.Equal(0, frame);
        }

        // ---- WaitForSecondsRealtime (real wall-clock time) ----

        [Fact]
        public void TestWaitForSecondsRealtimeZeroPassesThrough()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitForSecondsRealtime(0f);
                trace.Mark("b");
            }
            provider.StartCoroutine(Work());
            // zero-second wait is immediately Done: passes through, everything runs synchronously
            Assert.Equal(new[] { "a", "b" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper());
        }

        [Fact]
        public void TestWaitForSecondsRealtimeNegativePassesThrough()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitForSecondsRealtime(-1f);
                trace.Mark("b");
            }
            provider.StartCoroutine(Work());
            Assert.Equal(new[] { "a", "b" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper());
        }

        [Fact]
        public void TestWaitForSecondsRealtimeFastForwardsWhenSoleWaiter()
        {
            // With only realtime waits left in the queue, the Looper fast-forwards RealTime
            // to the earliest deadline: resumes at the nearest PostUpdate_WaitForRealTime
            // phase (frame 0 Phase 20) without any real waiting
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitForSecondsRealtime(0.05f);
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            var sw = Stopwatch.StartNew();
            RunUntil(provider, () => done);
            sw.Stop();
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForRealTime, phase);
            Assert.Equal(0, frame);
            // the 50ms wait is skipped by fast-forward; real elapsed time should be far below 50ms
            Assert.True(sw.ElapsedMilliseconds < 45,
                $"pure realtime wait should be fast-forwarded, actual elapsed {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void TestRealtimeNotFastForwardedWithMixedWaiters()
        {
            // No fast-forward while a virtual-time wait is mixed in: the realtime wait does not
            // resume while the phase matches but time has not elapsed; it resumes via fast-forward
            // only after the virtual wait completes and only realtime waits remain
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = 0;
            IEnumerator RealtimeWork()
            {
                trace.Mark("r0");
                yield return new Mod.CoroutineWaitForSecondsRealtime(0.05f);
                trace.Mark("r1");
                ++done;
            }
            IEnumerator VirtualWork()
            {
                trace.Mark("v0");
                yield return new Mod.CoroutineWaitForSeconds(2 * provider.VirtualTimePerFrame);
                trace.Mark("v1");
                ++done;
            }
            provider.StartCoroutine(RealtimeWork());
            provider.StartCoroutine(VirtualWork());
            var sw = Stopwatch.StartNew();
            RunUntil(provider, () => done >= 2);
            sw.Stop();
            // the virtual wait (frame 2 Phase 21) completes first; the realtime wait resumes
            // afterwards via fast-forward (frame 3 Phase 20)
            Assert.Equal(new[] { "r0", "v0", "v1", "r1" }, trace.Tags);
            var (phaseV, frameV) = trace.At("v1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForTime, phaseV);
            Assert.Equal(2, frameV);
            var (phaseR, frameR) = trace.At("r1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForRealTime, phaseR);
            Assert.Equal(3, frameR);
            Assert.True(sw.ElapsedMilliseconds < 45,
                $"should fast-forward after the mixed wait ends, actual elapsed {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void TestRealtimeFastForwardToEarliestDeadline()
        {
            // Multiple realtime waits: fast-forward pulls to the earliest deadline, the shorter one
            // resumes first; the longer one resumes after another fast-forward
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = 0;
            IEnumerator Short()
            {
                trace.Mark("s0");
                yield return new Mod.CoroutineWaitForSecondsRealtime(0.03f);
                trace.Mark("s1");
                ++done;
            }
            IEnumerator Long()
            {
                trace.Mark("l0");
                yield return new Mod.CoroutineWaitForSecondsRealtime(0.06f);
                trace.Mark("l1");
                ++done;
            }
            provider.StartCoroutine(Short());
            provider.StartCoroutine(Long());
            var sw = Stopwatch.StartNew();
            RunUntil(provider, () => done >= 2);
            sw.Stop();
            Assert.Equal(new[] { "s0", "l0", "s1", "l1" }, trace.Tags);
            var (phaseS, frameS) = trace.At("s1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForRealTime, phaseS);
            Assert.Equal(0, frameS); // frame 0 Phase 20
            var (phaseL, frameL) = trace.At("l1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForRealTime, phaseL);
            Assert.Equal(1, frameL); // frame 1 Phase 20
            Assert.True(sw.ElapsedMilliseconds < 45,
                $"realtime waits should be fast-forwarded in segments, actual elapsed {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void TestVirtualTimePerFrameDefaultIsSixtyFps()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            Assert.Equal(1f / 60f, provider.VirtualTimePerFrame);
        }

        // ---- WaitForSeconds (scaled virtual time) ----

        [Fact]
        public void TestWaitForSecondsUsesVirtualTime()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitForSeconds(2 * provider.VirtualTimePerFrame);
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForTime, phase);
            Assert.Equal(2, frame);
        }

        [Fact]
        public void TestWaitForSecondsZeroPassesThrough()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitForSeconds(0f);
                trace.Mark("b");
            }
            provider.StartCoroutine(Work());
            Assert.Equal(new[] { "a", "b" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper());
        }

        [Fact]
        public void TestWaitForSecondsRespectsTimeScale()
        {
            // After changing the virtual time per frame (equivalent to timescale),
            // the same WaitTime spans more frames
            var provider = new Mod.StandaloneCoroutineProvider();
            provider.VirtualTimePerFrame = 0.5f;
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitForSeconds(1.5f);
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForTime, phase);
            Assert.Equal(3, frame); // 3 * 0.5 == 1.5
        }

        // ---- WaitForEndOfFrame / WaitForFixedUpdate ----

        [Fact]
        public void TestWaitForEndOfFrameResumesAtOnEndSameFrame()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitForEndOfFrame();
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.OnEnd, phase);
            Assert.Equal(0, frame);
        }

        [Fact]
        public void TestWaitForEndOfFrameAtOnEndWaitsNextFrame()
        {
            // Waiting for end of frame again after resuming at OnEnd:
            // the frame has already ended, so it must wait until OnEnd of the next frame
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                yield return new Mod.CoroutineWaitForEndOfFrame();
                trace.Mark("first");
                yield return new Mod.CoroutineWaitForEndOfFrame();
                trace.Mark("second");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (phase1, frame1) = trace.At("first");
            Assert.Equal(Mod.CoroutinePhase.OnEnd, phase1);
            Assert.Equal(0, frame1);
            var (phase2, frame2) = trace.At("second");
            Assert.Equal(Mod.CoroutinePhase.OnEnd, phase2);
            Assert.Equal(1, frame2);
        }

        [Fact]
        public void TestWaitForFixedUpdateResumesAtPostFixedUpdate()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitForFixedUpdate();
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.PostFixedUpdate, phase);
            Assert.Equal(0, frame);
        }

        // ---- WaitUntil / WaitWhile ----

        [Fact]
        public void TestWaitUntilAlreadyTruePassesThrough()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitUntil(() => true);
                trace.Mark("b");
            }
            provider.StartCoroutine(Work());
            Assert.Equal(new[] { "a", "b" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper());
        }

        [Fact]
        public void TestWaitUntilResumesWhenPredicateTurnsTrue()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var flag = false;
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitUntil(() => flag);
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            int ticks = 0;
            while (!done && ticks < 500)
            {
                provider.TickCoroutineLooper();
                ++ticks;
                if (ticks == 42)
                {
                    flag = true; // set after entering frame 1 (OnStart)
                }
            }
            Assert.True(done);
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForCustom, phase);
            Assert.Equal(1, frame);
        }

        [Fact]
        public void TestWaitWhileAlreadyFalsePassesThrough()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitWhile(() => false);
                trace.Mark("b");
            }
            provider.StartCoroutine(Work());
            Assert.Equal(new[] { "a", "b" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper());
        }

        [Fact]
        public void TestWaitWhileResumesWhenPredicateTurnsFalse()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var busy = true;
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitWhile(() => busy);
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            int ticks = 0;
            while (!done && ticks < 500)
            {
                provider.TickCoroutineLooper();
                ++ticks;
                if (ticks == 42)
                {
                    busy = false; // cleared after entering frame 1
                }
            }
            Assert.True(done);
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForCustom, phase);
            Assert.Equal(1, frame);
        }

        // ---- WaitForSub (sub-coroutines) ----

        [Fact]
        public void TestWaitForSubWaitsSubThenResumesSameFrame()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield return null;
                trace.Mark("s1");
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return new Mod.CoroutineWaitForSub(Sub());
                trace.Mark("w1");
                done = true;
            }
            provider.StartCoroutine(Work());
            // the sub-coroutine has synchronously run to its suspension point
            Assert.Equal(new[] { "w0", "s0" }, trace.Tags);
            RunUntil(provider, () => done);
            Assert.Equal(new[] { "w0", "s0", "s1", "w1" }, trace.Tags);
            var (subPhase, subFrame) = trace.At("s1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, subPhase);
            Assert.Equal(0, subFrame);
            // the sub finishes at Phase 24; the parent observes it at its Phase 25 in the same frame
            var (workPhase, workFrame) = trace.At("w1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForSub, workPhase);
            Assert.Equal(0, workFrame);
        }

        [Fact]
        public void TestWaitForSubImmediateCompletionPassesThrough()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield break;
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return new Mod.CoroutineWaitForSub(Sub());
                trace.Mark("w1");
            }
            provider.StartCoroutine(Work());
            // sub-coroutine completes synchronously -> the parent passes through immediately
            Assert.Equal(new[] { "w0", "s0", "w1" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper());
        }

        [Fact]
        public void TestWaitForSubNestedThreeLevels()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Grand()
            {
                trace.Mark("g0");
                yield return null;
                trace.Mark("g1");
            }
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield return new Mod.CoroutineWaitForSub(Grand());
                trace.Mark("s1");
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return new Mod.CoroutineWaitForSub(Sub());
                trace.Mark("w1");
                done = true;
            }
            provider.StartCoroutine(Work());
            // three levels run synchronously down to the grandchild's suspension point
            Assert.Equal(new[] { "w0", "s0", "g0" }, trace.Tags);
            RunUntil(provider, () => done);
            Assert.Equal(new[] { "w0", "s0", "g0", "g1", "s1", "w1" }, trace.Tags);
            var (grandPhase, grandFrame) = trace.At("g1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, grandPhase);
            Assert.Equal(0, grandFrame);
            var (subPhase, subFrame) = trace.At("s1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForSub, subPhase);
            Assert.Equal(0, subFrame);
            var (workPhase, workFrame) = trace.At("w1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForSub, workPhase);
            Assert.Equal(0, workFrame);
        }

        [Fact]
        public void TestWaitForSubSubEndsInLaterPhaseParentResumesNextFrame()
        {
            // The sub-coroutine finishes only at OnEnd(40), while the parent's
            // PostUpdate_WaitForSub(25) has already passed this frame
            // -> the parent must resume at Phase 25 of the next frame
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield return new Mod.CoroutineWaitForEndOfFrame();
                trace.Mark("s1");
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return new Mod.CoroutineWaitForSub(Sub());
                trace.Mark("w1");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (subPhase, subFrame) = trace.At("s1");
            Assert.Equal(Mod.CoroutinePhase.OnEnd, subPhase);
            Assert.Equal(0, subFrame);
            var (workPhase, workFrame) = trace.At("w1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForSub, workPhase);
            Assert.Equal(1, workFrame);
        }

        // ---- Custom instructions ----

        [Fact]
        public void TestCustomInstructionResumesAtDeclaredPhase()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new CustomLateUpdateInstruction();
                trace.Mark("b");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            var (phase, frame) = trace.At("b");
            Assert.Equal(Mod.CoroutinePhase.PostLateUpdate, phase);
            Assert.Equal(0, frame);
        }

        // ---- Multi-coroutine interleaving / starting while ticking ----

        [Fact]
        public void TestMultipleCoroutinesInterleave()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator A()
            {
                trace.Mark("a0");
                yield return null;
                trace.Mark("a1");
                yield return null;
                trace.Mark("a2");
            }
            IEnumerator B()
            {
                trace.Mark("b0");
                yield return null;
                trace.Mark("b1");
                yield return null;
                trace.Mark("b2");
            }
            provider.StartCoroutine(A());
            provider.StartCoroutine(B());
            RunUntil(provider, () => trace.Marks.Count >= 6);
            Assert.Equal(new[] { "a0", "b0", "a1", "b1", "a2", "b2" }, trace.Tags);
            var (phaseA1, frameA1) = trace.At("a1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseA1);
            Assert.Equal(0, frameA1);
            var (phaseA2, frameA2) = trace.At("a2");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseA2);
            Assert.Equal(1, frameA2);
        }

        [Fact]
        public void TestStartCoroutineDuringTickWaitsNextFrameAtSamePhase()
        {
            // Starting a new coroutine from a coroutine body resumed at Phase 24 and then
            // yielding null: both start exactly at Phase 24, which has already passed this
            // frame, so both must wait until Phase 24 of the next frame
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Other()
            {
                trace.Mark("o0");
                yield return null;
                trace.Mark("o1");
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return null;
                trace.Mark("w1");
                provider.StartCoroutine(Other());
                yield return null;
                trace.Mark("w2");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            Assert.Equal(new[] { "w0", "w1", "o0", "w2", "o1" }, trace.Tags);
            var (phaseW1, frameW1) = trace.At("w1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseW1);
            Assert.Equal(0, frameW1);
            var (phaseO0, frameO0) = trace.At("o0");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseO0);
            Assert.Equal(0, frameO0);
            var (phaseW2, frameW2) = trace.At("w2");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseW2);
            Assert.Equal(1, frameW2);
            var (phaseO1, frameO1) = trace.At("o1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseO1);
            Assert.Equal(1, frameO1);
        }

        // ---- Pass-through chain ----

        [Fact]
        public void TestPassThroughChainCompletesSynchronously()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator Empty()
            {
                yield break;
            }
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return new Mod.CoroutineWaitForSecondsRealtime(0f);
                trace.Mark("b");
                yield return new Mod.CoroutineWaitUntil(() => true);
                trace.Mark("c");
                yield return new Mod.CoroutineWaitForSub(Empty());
                trace.Mark("d");
                yield return new Mod.CoroutineWaitForSeconds(0f);
                trace.Mark("e");
                yield return new Mod.CoroutineWaitWhile(() => false);
                trace.Mark("f");
            }
            provider.StartCoroutine(Work());
            // consecutive immediately-Done instructions run through in one go, all synchronous
            Assert.Equal(new[] { "a", "b", "c", "d", "e", "f" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper());
        }

        // ---- Looper frame advancement ----

        [Fact]
        public void TestFrameAndPhaseAdvance()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            Assert.Equal(Mod.CoroutinePhase.OnStart, provider.CurrentPhase);
            Assert.Equal(0, provider.CurrentFrameCount);
            // 41 ticks per frame (Phase 0..40); after the i-th tick, Phase == i-1
            for (int i = 1; i <= 41; ++i)
            {
                Assert.False(provider.TickCoroutineLooper());
                Assert.Equal(0, provider.CurrentFrameCount);
                Assert.Equal((Mod.CoroutinePhase)(i - 1), provider.CurrentPhase);
            }
            // the 42nd tick enters frame 1
            provider.TickCoroutineLooper();
            Assert.Equal(1, provider.CurrentFrameCount);
            Assert.Equal(Mod.CoroutinePhase.OnStart, provider.CurrentPhase);
        }

        // ---- Static facade / RunCoroutineLooper ----

        [Fact]
        public void TestRunCoroutineLooperDrainsAllCoroutines()
        {
            try
            {
                Mod.CoroutineProvider.SetProvider();
                var provider = (Mod.StandaloneCoroutineProvider)Mod.CoroutineProvider.Instance;
                var trace = new Trace { Provider = provider };
                var done = 0;
                IEnumerator Work()
                {
                    trace.Mark("a");
                    yield return new Mod.CoroutineWaitForSeconds(3 * provider.VirtualTimePerFrame);
                    trace.Mark("b");
                    yield return null;
                    trace.Mark("c");
                    ++done;
                }
                Mod.CoroutineProvider.StartCoroutine(Work());
                Mod.CoroutineProvider.RunCoroutineLooper();
                Assert.Equal(1, done);
                Assert.Equal(new[] { "a", "b", "c" }, trace.Tags);
                var (phaseB, frameB) = trace.At("b");
                Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForTime, phaseB);
                Assert.Equal(3, frameB);
                // yielding null again after resuming at frame 3 Phase 21: Phase 24 has not
                // passed yet this frame, so it resumes in the same frame
                var (phaseC, frameC) = trace.At("c");
                Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseC);
                Assert.Equal(3, frameC);
            }
            finally
            {
                Mod.CoroutineProvider.Instance = null;
            }
        }

        // ---- Argument validation ----

        [Fact]
        public void TestNullArgumentsThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new Mod.CoroutineWaitUntil(null));
            Assert.Throws<ArgumentNullException>(() => new Mod.CoroutineWaitWhile(null));
            Assert.Throws<ArgumentNullException>(() => new Mod.CoroutineWaitForSub(null));
        }

        // ---- CoroutineHandle / StopCoroutine ----

        [Fact]
        public void TestHandleIsDoneTracksCompletion()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return null;
                trace.Mark("b");
                done = true;
            }
            var handle = provider.StartCoroutine(Work());
            Assert.NotNull(handle);
            Assert.False(handle.IsDone);
            RunUntil(provider, () => done);
            Assert.True(handle.IsDone);
        }

        [Fact]
        public void TestHandleIsDoneForSynchronouslyCompletedWork()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            IEnumerator Work()
            {
                yield break;
            }
            var handle = provider.StartCoroutine(Work());
            Assert.True(handle.IsDone);
        }

        [Fact]
        public void TestStopCoroutineHaltsAdvancement()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return null;
                trace.Mark("b");
                done = true;
            }
            var handle = provider.StartCoroutine(Work());
            provider.StopCoroutine(handle);
            Assert.True(handle.IsDone); // stopped counts as done
            Assert.False(provider.TickCoroutineLooper()); // dead node removed, queue drained
            Assert.Equal(new[] { "a" }, trace.Tags);
            Assert.False(done);
        }

        [Fact]
        public void TestStopCoroutineTwiceAndAfterCompletionAreNoOps()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            IEnumerator Work()
            {
                yield return null;
            }
            IEnumerator Sync()
            {
                yield break;
            }
            // double stop on a suspended coroutine (node marked dead, still queued)
            var h1 = provider.StartCoroutine(Work());
            provider.StopCoroutine(h1);
            provider.StopCoroutine(h1);
            // stop on a synchronously completed coroutine (no node at all)
            var h2 = provider.StartCoroutine(Sync());
            provider.StopCoroutine(h2);
            // stop on a naturally completed coroutine (node already detached)
            var h3 = provider.StartCoroutine(Work());
            for (int i = 0; i < 100; ++i)
            {
                provider.TickCoroutineLooper();
            }
            Assert.True(h3.IsDone);
            provider.StopCoroutine(h3);
            Assert.False(provider.TickCoroutineLooper());
        }

        [Fact]
        public void TestStopSelfInsideCoroutineBodyStopsAfterCurrentMoveNext()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            Mod.CoroutineHandle self = null!;
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return null; // frame 0 Phase 24
                trace.Mark("b");
                provider.StopCoroutine(self);
                trace.Mark("c"); // the current MoveNext still finishes after the stop
                yield return null; // the coroutine never resumes past this point
                trace.Mark("d"); // must not execute
            }
            self = provider.StartCoroutine(Work());
            RunUntil(provider, () => trace.Marks.Count >= 3);
            Assert.Equal(new[] { "a", "b", "c" }, trace.Tags);
            Assert.True(self.IsDone);
        }

        // ---- yield return handle (Unity's "yield return StartCoroutine(sub)") ----

        [Fact]
        public void TestYieldHandleWaitsOneFrameAfterCompletion()
        {
            // Unity's "yield return StartCoroutine(sub)": the parent is traversed before the
            // sub (registration order), so it only sees the finished Coroutine on the next
            // frame — the parent resumes one frame after the sub completes
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield return null;
                trace.Mark("s1");
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return provider.StartCoroutine(Sub());
                trace.Mark("w1");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            Assert.Equal(new[] { "w0", "s0", "s1", "w1" }, trace.Tags);
            var (phaseS, frameS) = trace.At("s1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseS);
            Assert.Equal(0, frameS); // sub finishes in frame 0
            var (phaseW, frameW) = trace.At("w1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForSub, phaseW);
            Assert.Equal(1, frameW); // parent resumes one frame later
        }

        [Fact]
        public void TestYieldHandleSynchronousCompletionPassesThrough()
        {
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield break;
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return provider.StartCoroutine(Sub());
                trace.Mark("w1");
            }
            provider.StartCoroutine(Work());
            Assert.Equal(new[] { "w0", "s0", "w1" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper());
        }

        [Fact]
        public void TestStopSubResumesWaitingParent()
        {
            // Unlike Unity (where a parent waiting on a stopped sub is stuck forever — a known
            // pitfall), stopping the sub here resumes the waiting parent
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            Mod.CoroutineHandle subhandle = null!;
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield return null;
                trace.Mark("s1");
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return subhandle = provider.StartCoroutine(Sub());
                trace.Mark("w1");
                done = true;
            }
            provider.StartCoroutine(Work());
            Assert.Equal(new[] { "w0", "s0" }, trace.Tags);
            provider.StopCoroutine(subhandle);
            RunUntil(provider, () => done);
            Assert.Equal(new[] { "w0", "s0", "w1" }, trace.Tags); // s1 never ran
            var (phaseW, frameW) = trace.At("w1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForSub, phaseW);
            Assert.Equal(1, frameW);
        }

        [Fact]
        public void TestStopParentKeepsIndependentSubRunning()
        {
            // Unity's "yield return StartCoroutine(sub)": the sub is an independent coroutine
            // and keeps running after the parent is stopped
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var subdone = false;
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield return null;
                trace.Mark("s1");
                subdone = true;
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return provider.StartCoroutine(Sub());
                trace.Mark("w1");
            }
            var phandle = provider.StartCoroutine(Work());
            provider.StopCoroutine(phandle);
            RunUntil(provider, () => subdone);
            Assert.Equal(new[] { "w0", "s0", "s1" }, trace.Tags); // w1 never ran, sub finished
        }

        // ---- yield return IEnumerator (Unity's in-place nested iterator) ----

        [Fact]
        public void TestStopParentCascadesIntoNestedSub()
        {
            // Unity's "yield return subIEnumerator": the nested iterator is part of the
            // parent's execution flow — stopping the parent stops the nested sub as well
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var subdone = false;
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield return null;
                trace.Mark("s1");
                subdone = true;
            }
            IEnumerator Work()
            {
                trace.Mark("w0");
                yield return new Mod.CoroutineWaitForSub(Sub());
                trace.Mark("w1");
            }
            var phandle = provider.StartCoroutine(Work());
            provider.StopCoroutine(phandle);
            int ticks = 0;
            while (provider.TickCoroutineLooper() && ticks++ < 200) { }
            Assert.False(subdone); // the sub was stopped together with the parent
            Assert.Equal(new[] { "w0", "s0" }, trace.Tags);
            Assert.False(provider.TickCoroutineLooper()); // queue fully drained
        }

        [Fact]
        public void TestNestedSubSamePhaseSameFrameResume()
        {
            // In-place nested style started mid-frame (via Tick's in-place swap): the sub's
            // node is moved in front of the parent's, so when the sub finishes exactly at the
            // parent's phase, the parent still observes it in the same frame
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Grand()
            {
                trace.Mark("g0");
                yield return null;
                trace.Mark("g1");
            }
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield return new Mod.CoroutineWaitForSub(Grand());
                trace.Mark("s1");
            }
            IEnumerator Work()
            {
                yield return null; // frame 0 Phase 24
                trace.Mark("w0");
                yield return new Mod.CoroutineWaitForSub(Sub());
                trace.Mark("w1");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            Assert.Equal(new[] { "w0", "s0", "g0", "g1", "s1", "w1" }, trace.Tags);
            var (phaseG, frameG) = trace.At("g1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseG);
            Assert.Equal(1, frameG);
            var (phaseS, frameS) = trace.At("s1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForSub, phaseS);
            Assert.Equal(1, frameS);
            var (phaseW, frameW) = trace.At("w1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForSub, phaseW);
            Assert.Equal(1, frameW); // same frame as the sub, despite sharing the same phase
        }

        [Fact]
        public void TestYieldHandleAfterWaitStartsSubAtParentResumeAndResumesNextFrameAfterCompletion()
        {
            // "yield return StartCoroutine(sub)" after a preceding wait (Tick's in-place swap
            // path, sub enqueued AFTER the parent — Unity's registration order):
            // - the sub's first MoveNext happens at the very scheduling point the parent
            //   resumes from its previous wait (s0 marks frame 0 Phase 24, same as w0);
            // - the parent is traversed before the sub, so it only sees the finished
            //   Coroutine on the following frame — w1 lands one frame after s1.
            var provider = new Mod.StandaloneCoroutineProvider();
            var trace = new Trace { Provider = provider };
            var done = false;
            IEnumerator Sub()
            {
                trace.Mark("s0");
                yield return null;
                trace.Mark("s1");
            }
            IEnumerator Work()
            {
                trace.Mark("a");
                yield return null; // frame 0 Phase 24
                trace.Mark("w0");
                yield return provider.StartCoroutine(Sub());
                trace.Mark("w1");
                done = true;
            }
            provider.StartCoroutine(Work());
            RunUntil(provider, () => done);
            Assert.Equal(new[] { "a", "w0", "s0", "s1", "w1" }, trace.Tags);
            var (phaseW0, frameW0) = trace.At("w0");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseW0);
            Assert.Equal(0, frameW0);
            var (phaseS0, frameS0) = trace.At("s0");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseS0);
            Assert.Equal(0, frameS0); // sub's first MoveNext at the parent's resume point
            var (phaseS1, frameS1) = trace.At("s1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForDefault, phaseS1);
            Assert.Equal(1, frameS1); // sub completes in frame 1
            var (phaseW1, frameW1) = trace.At("w1");
            Assert.Equal(Mod.CoroutinePhase.PostUpdate_WaitForSub, phaseW1);
            Assert.Equal(2, frameW1); // parent resumes one frame after completion
        }

        // ---- Mixed providers (e.g. Unity runtime + editor) / StopCoroutine dispatch ----

        [Fact]
        public void TestFacadeStopCoroutineDispatchesToOwningProvider()
        {
            // Two coexisting providers (like Unity's runtime + editor providers): the static
            // facade's Instance is A, but a handle started on B must be stopped through B —
            // the handle records its owning provider and the facade dispatches by it
            var a = new Mod.StandaloneCoroutineProvider();
            var b = new Mod.StandaloneCoroutineProvider();
            var traceA = new Trace { Provider = a };
            var traceB = new Trace { Provider = b };
            IEnumerator WorkA()
            {
                traceA.Mark("a0");
                yield return null;
                traceA.Mark("a1");
            }
            IEnumerator WorkB()
            {
                traceB.Mark("b0");
                yield return null;
                traceB.Mark("b1");
            }
            var ha = a.StartCoroutine(WorkA());
            var hb = b.StartCoroutine(WorkB());
            Assert.Same(a, ha.Provider);
            Assert.Same(b, hb.Provider);
            Mod.CoroutineProvider.Instance = a; // facade instance is A...
            Mod.CoroutineProvider.StopCoroutine(hb); // ...yet the stop lands on B
            Assert.True(hb.IsDone);
            Assert.False(ha.IsDone);
            int ticks = 0;
            while (ticks++ < 200 && (a.TickCoroutineLooper() | b.TickCoroutineLooper())) { }
            Assert.Equal(new[] { "a0", "a1" }, traceA.Tags); // A's coroutine unaffected
            Assert.Equal(new[] { "b0" }, traceB.Tags); // B's coroutine halted by the stop
        }

        [Fact]
        public void TestStopCoroutineWithForeignHandleThrows()
        {
            // Calling StopCoroutine directly on the wrong provider is a programming error
            var a = new Mod.StandaloneCoroutineProvider();
            var b = new Mod.StandaloneCoroutineProvider();
            IEnumerator Work()
            {
                yield return null;
            }
            var hb = b.StartCoroutine(Work());
            Assert.Throws<ArgumentException>(() => a.StopCoroutine(hb));
            // the foreign stop must not have touched B's coroutine
            Assert.False(hb.IsDone);
        }

        [Fact]
        public void TestFacadeStopCoroutineWithNullHandleIsNoOp()
        {
            Mod.CoroutineProvider.StopCoroutine(null); // must not throw
        }
    }
}
