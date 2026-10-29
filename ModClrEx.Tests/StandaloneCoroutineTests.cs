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
    }
}
