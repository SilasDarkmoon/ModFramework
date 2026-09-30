using Mod;
using Mod.LowLevel;
using System;
using Xunit;

namespace ModClrEx.Tests
{
    /// <summary>
    /// Tests for WeakDelegate.cs: the StrongRef/WeakRef value types, their implicit
    /// conversions and GC-death semantics, and the WeakEvent subscription machinery
    /// (WeakWrapper is internal, so it is covered through WeakEvent).
    ///
    /// <para>
    /// GC-death tests use plain objects only (no RefContainer/UnionRef involvement),
    /// so forced collections are safe here.
    /// </para>
    /// </summary>
    public class WeakDelegateTests
    {
        private static void Kill(object slot)
        {
            // slot is already nulled by the caller; force the death through.
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.KeepAlive(slot);
        }

        // =====================================================================
        // StrongRef
        // =====================================================================

        [Fact]
        public void StrongRef_ImplicitConversions_RoundTrip()
        {
            // NOTE: T must be a concrete reference type - with T = object the standard
            // boxing conversion wins over the user-defined implicit operator and the
            // assert would compare against the boxed StrongRef itself.
            var obj = "payload";
            StrongRef<string> sr = obj;                  // T -> StrongRef<T>
            string back = sr;                            // StrongRef<T> -> T
            Assert.Same(obj, back);
            Assert.Same(obj, sr.Value);

            sr.Value = null;
            Assert.Null((string)sr);
        }

        [Fact]
        public void StrongRef_Extension_CreatesTypedRef()
        {
            var s = "hello";
            var sr = s.StrongRef();
            Assert.Equal("hello", (string)sr);
        }

        // =====================================================================
        // WeakRef / WeakRef<T>: lifecycle
        // =====================================================================

        [Fact]
        public void WeakRef_BasicLifecycle()
        {
            var obj = "payload";
            WeakRef<string> wr = obj;                    // implicit T -> WeakRef<T>

            Assert.True(wr.HasValue);
            Assert.True(wr.IsAlive);
            Assert.Same(obj, wr.Target);
            Assert.True(wr.TryGetTarget(out var got));
            Assert.Same(obj, got);
            Assert.Same(obj, (string)wr);               // implicit WeakRef<T> -> T (concrete T; boxing would win for object)

            wr.Target = null;                            // clearing the slot empties it
            Assert.False(wr.HasValue);
            Assert.True(wr.IsAlive);                     // empty slot counts as alive
            Assert.Null(wr.Target);
            Assert.True(wr.TryGetTarget(out var none));
            Assert.Null(none);
        }

        [Fact]
        public void WeakRef_EmptySlot_TryGetTargetTrueNull()
        {
            var wr = default(WeakRef<object>);
            Assert.False(wr.HasValue);
            Assert.True(wr.IsAlive);
            Assert.True(wr.TryGetTarget(out var target));
            Assert.Null(target);
        }

        [Fact]
        public void WeakRef_SetTarget_Replaces()
        {
            var a = new object();
            var b = new object();
            WeakRef<object> wr = a;
            wr.Target = b;
            Assert.Same(b, wr.Target);
            Assert.True(wr.TryGetTarget(out var got));
            Assert.Same(b, got);
        }

        private static WeakRef<object> MakeWeakRefOfTempObject()
        {
            return new object();                          // implicit conversion; object unreachable after return
        }

        [Fact]
        public void WeakRef_Death_AfterGC_ThrowsAndReportsDead()
        {
            var wr = MakeWeakRefOfTempObject();
            Assert.True(wr.IsAlive);

            Kill(null);                                   // the temp object has no strong reference anywhere

            Assert.True(wr.HasValue);                     // slot still there
            Assert.False(wr.IsAlive);                     // but the target died
            Assert.False(wr.TryGetTarget(out var target));
            Assert.Null(target);
            var ex = Assert.Throws<WeakRefDeadException>(() => wr.Target);
            Assert.Equal("WeakRef is dead!", ex.Message);
        }

        [Fact]
        public void WeakRef_NonGeneric_BehavesLikeTyped()
        {
            var obj = new object();
            WeakRef wr = default;
            wr.Target = obj;
            Assert.Same(obj, ((IObjectRef)wr).Target);
            Assert.True(wr.IsAlive);

            wr.Target = null;
            Assert.False(wr.HasValue);
        }

        [Fact]
        public void WeakRef_Extension_CreatesTypedRef()
        {
            var s = "text";
            var wr = s.WeakRef();
            Assert.True(wr.TryGetTarget(out string got));
            Assert.Equal("text", got);
        }

        // =====================================================================
        // WeakEvent: basic invocation
        // =====================================================================

        private class Subscriber
        {
            public int Calls;
            public int LastArg;
            public void OnEvent() { ++Calls; }
            public void OnEventWithArg(int arg) { ++Calls; LastArg = arg; }
            public int Add(int a, int b) { ++Calls; return a + b; }
            public int GetAnswer() { ++Calls; return 42; }
        }

        private static void StaticHandler(Counter c) { ++c.Count; }

        private class Counter { public int Count; }

        private class Holder { public int Count; }

        [Fact]
        public void WeakEvent_Action_InvokesInstanceSubscriber()
        {
            var ev = new WeakDelegate.WeakEvent<VoidReturn>();
            var sub = new Subscriber();
            ev.AddListener(sub.OnEvent);

            ev.TrigEvent();
            ev.TrigEvent();

            Assert.Equal(2, sub.Calls);
        }

        [Fact]
        public void WeakEvent_Func_ReturnsResult()
        {
            var ev = new WeakDelegate.WeakEvent<int>();
            var sub = new Subscriber();
            ev.AddListener(sub.GetAnswer);

            Assert.Equal(42, ev.TrigEvent());
            Assert.Equal(1, sub.Calls);
        }

        [Fact]
        public void WeakEvent_WithParameter_PassesArgument()
        {
            var ev = new WeakDelegate.WeakEvent<VoidReturn, int>();
            var sub = new Subscriber();
            ev.AddListener(sub.OnEventWithArg);

            ev.TrigEvent(77);

            Assert.Equal(1, sub.Calls);
            Assert.Equal(77, sub.LastArg);
        }

        [Fact]
        public void WeakEvent_FuncWithParameter_ReturnsValue()
        {
            var ev = new WeakDelegate.WeakEvent<int, int, int>();
            var sub = new Subscriber();
            ev.AddListener(sub.Add);

            Assert.Equal(30, ev.TrigEvent(10, 20));
            Assert.Equal(1, sub.Calls);
        }

        [Fact]
        public void WeakEvent_MultipleListeners_AllInvoked()
        {
            var ev = new WeakDelegate.WeakEvent<VoidReturn>();
            var a = new Subscriber();
            var b = new Subscriber();
            var c = new Subscriber();
            ev.AddListener(a.OnEvent);
            ev.AddListener(b.OnEvent);
            ev.AddListener(c.OnEvent);

            ev.TrigEvent();

            Assert.Equal(1, a.Calls);
            Assert.Equal(1, b.Calls);
            Assert.Equal(1, c.Calls);
        }

        // =====================================================================
        // WeakEvent: GC death
        // =====================================================================
        // Pitfall these tests are shaped around: creating the object in the test body and
        // doing `x = null; GC.Collect()` does NOT kill it - the JIT may drop the null store
        // (dead-store elimination) or keep reporting the stack slot as a GC root. Every
        // doomed object is therefore created inside a helper method and never escapes it.

        private static WeakDelegate.WeakEvent<VoidReturn> MakeInstanceSubscribedEvent()
        {
            var ev = new WeakDelegate.WeakEvent<VoidReturn>();
            var sub = new Subscriber();
            ev.AddListener(sub.OnEvent);
            GC.KeepAlive(sub);
            return ev;                                     // sub does not escape - dead after return
        }

        [Fact]
        public void WeakEvent_InstanceSubscriber_GCDeath_SkipsInvocation()
        {
            var ev = MakeInstanceSubscribedEvent();
            Kill(null);

            ev.TrigEvent();                               // must not throw, must not call
            ev.TrigEvent();
        }

        [Fact]
        public void WeakEvent_LambdaClosure_Alive_Invokes()
        {
            var ev = new WeakDelegate.WeakEvent<VoidReturn>();
            var holder = new Holder();
            ev.AddListener(() => ++holder.Count);

            ev.TrigEvent();
            ev.TrigEvent();

            Assert.Equal(2, holder.Count);                // cloned-closure path works while alive
        }

        private static (WeakDelegate.WeakEvent<VoidReturn> ev, WeakReference<Holder> watch) MakeLambdaSubscribedDeadHolder()
        {
            var ev = new WeakDelegate.WeakEvent<VoidReturn>();
            var holder = new Holder();
            var watch = new WeakReference<Holder>(holder);
            ev.AddListener(() => ++holder.Count);
            GC.KeepAlive(ev);
            return (ev, watch);                           // holder does not escape
        }

        [Fact]
        public void WeakEvent_LambdaClosure_GCDeath_StopsCalling()
        {
            var (ev, watch) = MakeLambdaSubscribedDeadHolder();
            Kill(null);

            Assert.False(watch.TryGetTarget(out _));      // the captured holder really died
            ev.TrigEvent();                               // no call, no throw
        }

        private static (WeakDelegate.WeakEvent<VoidReturn> ev, WeakReference<Holder> h1Watch, Holder liveH2) MakeLambdaSubscribedDeadH1()
        {
            var ev = new WeakDelegate.WeakEvent<VoidReturn>();
            var h1 = new Holder();
            var h2 = new Holder();
            var watch = new WeakReference<Holder>(h1);
            ev.AddListener(() => { ++h1.Count; ++h2.Count; });
            h1 = null;                                    // closure field nulled inside the helper
            GC.KeepAlive(ev);
            return (ev, watch, h2);                        // h1 dead, h2 escapes alive
        }

        [Fact]
        public void WeakEvent_LambdaClosure_PartialDeath_StopsCalling()
        {
            // A closure capturing two objects stops being invokable when ANY captured
            // reference dies (the wrapper's per-field weak refs gate the call).
            var (ev, h1Watch, h2) = MakeLambdaSubscribedDeadH1();
            Kill(null);

            Assert.False(h1Watch.TryGetTarget(out _));    // h1 really died
            ev.TrigEvent();
            Assert.Equal(0, h2.Count);                    // dead capture blocks the whole call

            ev.TrigEvent();                               // still no throw on repeat
            Assert.Equal(0, h2.Count);
        }

        [Fact]
        public void WeakEvent_StaticSubscriber_NeverDies()
        {
            var ev = new WeakDelegate.WeakEvent<VoidReturn>();
            var counter = new Counter();
            ev.AddListener(() => StaticHandler(counter));

            Kill(null);
            Kill(null);

            ev.TrigEvent();
            Assert.Equal(1, counter.Count);               // static handler survives GC
        }

        // =====================================================================
        // WeakEvent: explicit removal and the throw-to-unsubscribe path
        // =====================================================================

        [Fact]
        public void WeakEvent_RemoveListener_ExplicitUnsubscribe()
        {
            var ev = new WeakDelegate.WeakEvent<VoidReturn>();
            var sub = new Subscriber();
            Action handler = sub.OnEvent;
            ev.AddListener(handler);

            ev.RemoveListener(handler);
            ev.TrigEvent();

            Assert.Equal(0, sub.Calls);
        }

        private class WeakBox
        {
            public WeakRef<object> Ref;
        }

        private static WeakBox MakeDeadWeakBox()
        {
            return new WeakBox { Ref = new object() };    // object unreachable after return
        }

        [Fact]
        public void WeakEvent_SubscriberThrowingWeakRefDead_AutoUnsubscribes()
        {
            // A listener whose body throws WeakRefDeadException is treated as dead by
            // TrigEvent and removed; the second trigger no longer reaches it.
            // The dead WeakRef lives in a box: capturing the local itself would hoist it
            // into the closure, and clearing the local would clear the closure field too.
            var ev = new WeakDelegate.WeakEvent<VoidReturn>();
            var probe = new Subscriber();
            var box = MakeDeadWeakBox();
            Kill(null);                                   // box.Ref.Target now throws

            var calls = 0;
            ev.AddListener(() =>
            {
                ++calls;
                _ = box.Ref.Target;                       // throws WeakRefDeadException
            });
            ev.AddListener(probe.OnEvent);

            ev.TrigEvent();                               // thrower removed, probe still called
            Assert.Equal(1, probe.Calls);

            ev.TrigEvent();                               // thrower gone: probe called once more
            Assert.Equal(2, probe.Calls);
            Assert.Equal(1, calls);
        }
    }
}
