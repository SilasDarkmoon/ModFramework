using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mod
{
    public interface ICoroutineProvider
    {
        void StartCoroutine(IEnumerator work);
        bool TickCoroutineLooper();
    }
    public enum CoroutinePhase
    {
        OnStart = 0,

        PostFixedUpdate = 10,

        PostUpdate = 20,
        PostUpdate_WaitForRealTime = PostUpdate,
        PostUpdate_WaitForTime,
        PostUpdate_WaitForAsync,
        PostUpdate_WaitForCustom,
        PostUpdate_WaitForDefault,
        PostUpdate_WaitForSub,

        PostLateUpdate = 30,

        OnEnd = 40,
    }
    public interface ICoroutineInstruction
    {
        CoroutinePhase Phase { get; }
        object Attachment { get; set; }
    }
    /// <summary>
    /// Waits for scaled virtual time (Unity's WaitForSeconds).
    /// Time source is the Looper's VirtualTime (advanced by VirtualTimePerFrame seconds per frame);
    /// a WaitTime of zero or less is immediately Done and passes through to the following code.
    /// </summary>
    public class CoroutineWaitForSeconds : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.PostUpdate_WaitForTime;
        public object Attachment { get; set; }
        public float WaitTime { get; protected internal set; }
        public CoroutineWaitForSeconds(float timeInSeconds)
        {
            WaitTime = timeInSeconds;
        }
    }
    /// <summary>
    /// Waits for real wall-clock time (Unity's WaitForSecondsRealtime), unaffected by the virtual time scale;
    /// a WaitTime of zero or less is immediately Done and passes through to the following code.
    /// </summary>
    public class CoroutineWaitForSecondsRealtime : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.PostUpdate_WaitForRealTime;
        public object Attachment { get; set; }
        public float WaitTime { get; protected internal set; }
        public CoroutineWaitForSecondsRealtime(float timeInSeconds)
        {
            WaitTime = timeInSeconds;
        }
    }
    /// <summary>
    /// Waits until the end of the current frame (Unity's WaitForEndOfFrame).
    /// Resumes at OnEnd of the same frame if that phase has not been passed yet, otherwise at OnEnd of the next frame; never passes through.
    /// </summary>
    public class CoroutineWaitForEndOfFrame : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.OnEnd;
        public object Attachment { get; set; }
    }
    /// <summary>
    /// Waits until the next PostFixedUpdate phase (Unity's WaitForFixedUpdate).
    /// Resumes within the same frame if that phase has not been passed yet, otherwise next frame; never passes through.
    /// </summary>
    public class CoroutineWaitForFixedUpdate : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.PostFixedUpdate;
        public object Attachment { get; set; }
    }
    /// <summary>
    /// Evaluates the predicate every frame at PostUpdate_WaitForCustom until it returns true (Unity's WaitUntil).
    /// If the predicate is already true at creation, the instruction is immediately Done and passes through.
    /// </summary>
    public class CoroutineWaitUntil : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.PostUpdate_WaitForCustom;
        public object Attachment { get; set; }
        public Func<bool> Predicate { get; protected internal set; }
        public CoroutineWaitUntil(Func<bool> predicate)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }
    }
    /// <summary>
    /// Evaluates the predicate every frame at PostUpdate_WaitForCustom until it returns false (Unity's WaitWhile).
    /// If the predicate is already false at creation, the instruction is immediately Done and passes through.
    /// </summary>
    public class CoroutineWaitWhile : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.PostUpdate_WaitForCustom;
        public object Attachment { get; set; }
        public Func<bool> Predicate { get; protected internal set; }
        public CoroutineWaitWhile(Func<bool> predicate)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }
    }
    /// <summary>
    /// Waits for a sub-coroutine to finish (Unity's yield return StartCoroutine(...)).
    /// The sub-coroutine runs to its first suspension point immediately when the instruction is created,
    /// then advances in parallel with the parent; if the sub-coroutine completes synchronously
    /// (or all of its instructions are immediately Done), the parent passes through right away.
    /// The parent observes completion at its PostUpdate_WaitForSub phase:
    /// same frame if the sub finished before that phase, otherwise the next frame.
    /// </summary>
    public class CoroutineWaitForSub : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.PostUpdate_WaitForSub;
        public object Attachment { get; set; }
        public IEnumerator Sub { get; protected internal set; }
        public CoroutineWaitForSub(IEnumerator sub)
        {
            Sub = sub ?? throw new ArgumentNullException(nameof(sub));
        }
    }
    /// <summary>
    /// Default wait (Unity's yield return null):
    /// waits until the PostUpdate_WaitForDefault phase of the current (or next) frame; never passes through.
    /// </summary>
    public class CoroutineWaitDefault : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.PostUpdate_WaitForDefault;
        public object Attachment { get; set; }
    }
    public class StandaloneCoroutineProvider : ICoroutineProvider
    {
        protected internal interface ICoroutineInstructionChecker
        {
            ICoroutineInstruction Target { get; set; }
            ICoroutineProvider Provider { get; set; }
            IEnumerator Work { get; set; }
            void Start();
            bool CheckDone();
        }
        protected internal abstract class BaseChecker : ICoroutineInstructionChecker
        {
            protected abstract ICoroutineInstruction RealTarget { get; set; }
            ICoroutineInstruction ICoroutineInstructionChecker.Target { get => RealTarget; set => RealTarget = value; }
            public StandaloneCoroutineProvider Provider;
            ICoroutineProvider ICoroutineInstructionChecker.Provider { get => Provider; set => Provider = value as StandaloneCoroutineProvider; }
            public IEnumerator Work { get; set; }
            public abstract void Start();
            public abstract bool CheckDone();
        }
        protected internal abstract class BaseChecker<T> : BaseChecker where T : ICoroutineInstruction
        {
            private T _Target;
            public T Target => _Target;
            protected override ICoroutineInstruction RealTarget
            {
                get => _Target;
                set
                {
                    if (value is T tar)
                    {
                        _Target = tar;
                        tar.Attachment = this;
                    }
                }
            }
            protected BaseChecker(StandaloneCoroutineProvider provider, T ci)
            {
                Provider = provider;
                RealTarget = ci;
                // Start must be called only after Provider/Target are ready (the Start of
                // NonUrgentChecker, WaitForSub etc. both depend on them), so the launch
                // point lives here instead of the BaseChecker constructor.
                Start();
            }
        }
        /// <summary>
        /// Generic checker for "non-urgent" instructions: Done as soon as the Looper has left
        /// the (frame, phase) point captured at start AND the instruction's declared Phase has been reached.
        /// This yields Unity-style resume timing: if the instruction's Phase has not been passed yet
        /// this frame, resume at that phase in the same frame (like a coroutine started before Update
        /// yielding null resumes after Update in the same frame in Unity); if already passed
        /// (including starting exactly at that phase), resume at that phase next frame.
        /// Never passes through when created synchronously.
        /// </summary>
        protected internal abstract class NonUrgentChecker<T> : BaseChecker<T> where T : ICoroutineInstruction
        {
            private int _StartFrame;
            private CoroutinePhase _StartPhase;
            protected NonUrgentChecker(StandaloneCoroutineProvider provider, T ci)
                : base(provider, ci)
            {
            }
            public override void Start()
            {
                _StartFrame = Provider._Looper.FrameCount;
                _StartPhase = Provider._Looper.Phase;
            }
            public override bool CheckDone()
            {
                if (_StartFrame == Provider._Looper.FrameCount && _StartPhase == Provider._Looper.Phase)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        protected internal class CheckerForDefault : NonUrgentChecker<CoroutineWaitDefault>
        {
            public CheckerForDefault(StandaloneCoroutineProvider provider, CoroutineWaitDefault ci)
                : base(provider, ci)
            {
            }
        }
        protected internal class CheckerForWaitForSecondsRealtime : BaseChecker<CoroutineWaitForSecondsRealtime>
        {
            public int StartTick;
            public int WaitTick;
            public override void Start()
            {
                // Based on the Looper's RealTimeTick (which includes fast-forward),
                // so a fast-forward is perceived immediately.
                StartTick = Provider._Looper.RealTimeTick;
                WaitTick = (int)Math.Ceiling(Target.WaitTime * 1000f);
            }
            public override bool CheckDone()
            {
                return Provider._Looper.RealTimeTick - StartTick >= WaitTick;
            }
            public CheckerForWaitForSecondsRealtime(StandaloneCoroutineProvider provider, CoroutineWaitForSecondsRealtime ci)
                : base(provider, ci)
            {
            }
        }
        protected internal class CheckerForWaitForSeconds : BaseChecker<CoroutineWaitForSeconds>
        {
            public float StartVirtualTime;
            public override void Start()
            {
                StartVirtualTime = Provider._Looper.VirtualTime;
            }
            public override bool CheckDone()
            {
                return Provider._Looper.VirtualTime - StartVirtualTime >= Target.WaitTime;
            }
            public CheckerForWaitForSeconds(StandaloneCoroutineProvider provider, CoroutineWaitForSeconds ci)
                : base(provider, ci)
            {
            }
        }
        protected internal class CheckerForWaitForEndOfFrame : NonUrgentChecker<CoroutineWaitForEndOfFrame>
        {
            public CheckerForWaitForEndOfFrame(StandaloneCoroutineProvider provider, CoroutineWaitForEndOfFrame ci)
                : base(provider, ci)
            {
            }
        }
        protected internal class CheckerForWaitForFixedUpdate : NonUrgentChecker<CoroutineWaitForFixedUpdate>
        {
            public CheckerForWaitForFixedUpdate(StandaloneCoroutineProvider provider, CoroutineWaitForFixedUpdate ci)
                : base(provider, ci)
            {
            }
        }
        protected internal class CheckerForWaitUntil : BaseChecker<CoroutineWaitUntil>
        {
            public CheckerForWaitUntil(StandaloneCoroutineProvider provider, CoroutineWaitUntil ci)
                : base(provider, ci)
            {
            }
            public override void Start()
            {
            }
            public override bool CheckDone()
            {
                return Target.Predicate();
            }
        }
        protected internal class CheckerForWaitWhile : BaseChecker<CoroutineWaitWhile>
        {
            public CheckerForWaitWhile(StandaloneCoroutineProvider provider, CoroutineWaitWhile ci)
                : base(provider, ci)
            {
            }
            public override void Start()
            {
            }
            public override bool CheckDone()
            {
                return !Target.Predicate();
            }
        }
        /// <summary>Generic checker for custom ICoroutineInstruction: NonUrgent semantics at the phase declared by the instruction.</summary>
        protected internal class CheckerForCustom : NonUrgentChecker<ICoroutineInstruction>
        {
            public CheckerForCustom(StandaloneCoroutineProvider provider, ICoroutineInstruction ci)
                : base(provider, ci)
            {
            }
        }
        /// <summary>
        /// Sub-coroutine checker: Start drives the sub-coroutine to its first suspension point
        /// and enqueues it; completion is then detected by whether that queue node has been
        /// removed (node.List == null). Tick advances coroutines by replacing node.Value in place,
        /// so node identity is stable and safe to track.
        /// </summary>
        protected internal class CheckerForWaitForSub : BaseChecker<CoroutineWaitForSub>
        {
            private LinkedListNode<ICoroutineInstructionChecker> _SubNode;
            private bool _SubDone;
            public CheckerForWaitForSub(StandaloneCoroutineProvider provider, CoroutineWaitForSub ci)
                : base(provider, ci)
            {
            }
            public override void Start()
            {
                var subchecker = Provider.DriveWork(Target.Sub);
                if (subchecker != null)
                {
                    _SubNode = Provider._Checkers.AddLast(subchecker);
                }
                else
                {
                    _SubDone = true; // sub-coroutine completed synchronously (or fully passed through); parent need not wait
                }
            }
            public override bool CheckDone()
            {
                return _SubDone || (_SubNode != null && _SubNode.List == null);
            }
        }

        protected internal ICoroutineInstructionChecker CreateChecker(object ci)
        {
            if (ci == null)
            {
                // yield return null: treated as the default wait
                return new CheckerForDefault(this, new CoroutineWaitDefault());
            }
            if (ci is CoroutineWaitForSecondsRealtime cirt)
            {
                return new CheckerForWaitForSecondsRealtime(this, cirt);
            }
            if (ci is CoroutineWaitForSeconds cis)
            {
                return new CheckerForWaitForSeconds(this, cis);
            }
            if (ci is CoroutineWaitForEndOfFrame ceof)
            {
                return new CheckerForWaitForEndOfFrame(this, ceof);
            }
            if (ci is CoroutineWaitForFixedUpdate cffu)
            {
                return new CheckerForWaitForFixedUpdate(this, cffu);
            }
            if (ci is CoroutineWaitUntil cu)
            {
                return new CheckerForWaitUntil(this, cu);
            }
            if (ci is CoroutineWaitWhile cw)
            {
                return new CheckerForWaitWhile(this, cw);
            }
            if (ci is CoroutineWaitForSub cs)
            {
                return new CheckerForWaitForSub(this, cs);
            }
            if (ci is ICoroutineInstruction instr)
            {
                // custom instruction: waits at the phase it declares
                return new CheckerForCustom(this, instr);
            }
            // unknown objects are treated as yield return null (default wait), per Unity semantics
            return new CheckerForDefault(this, new CoroutineWaitDefault());
        }

        protected internal LinkedList<ICoroutineInstructionChecker> _Checkers = new LinkedList<ICoroutineInstructionChecker>();

        /// <summary>
        /// Drives a coroutine body to its next suspension point: creates a checker for each
        /// yielded instruction; instructions that are immediately Done pass straight through
        /// (not suspended, the following code executes right away).
        /// Returns the suspending checker (already associated with Work); null if the coroutine ran to completion.
        /// </summary>
        protected internal ICoroutineInstructionChecker DriveWork(IEnumerator work)
        {
            while (work.MoveNext())
            {
                var checker = CreateChecker(work.Current);
                if (!checker.CheckDone())
                {
                    checker.Work = work;
                    return checker;
                }
            }
            return null;
        }
        public void StartCoroutine(IEnumerator work)
        {
            var checker = DriveWork(work);
            if (checker != null)
            {
                _Checkers.AddLast(checker);
            }
        }
        public bool TickCoroutineLooper()
        {
            _Looper.Tick();
            var node = _Checkers.First;
            while (node != null)
            {
                var next = node.Next;
                var checker = node.Value;
                if (_Looper.Phase == checker.Target.Phase && checker.CheckDone())
                {
                    // Instruction Done: drive the coroutine body; replace the checker in place
                    // if it suspends again, remove the node if it ran to completion.
                    var nextchecker = checker.Work != null ? DriveWork(checker.Work) : null;
                    if (nextchecker != null)
                    {
                        node.Value = nextchecker;
                    }
                    else
                    {
                        _Checkers.Remove(node);
                    }
                }
                node = next;
            }
            TryFastForwardRealTime();
            return _Checkers.Count > 0;
        }

        /// <summary>
        /// When only realtime waits (WaitForSecondsRealtime) remain in the queue, fast-forwards
        /// RealTime to the earliest deadline so the outer loop (e.g. RunCoroutineLooper) does not
        /// spin real time just for pure realtime waits; does nothing while other instructions
        /// (virtual time / predicates / sub-coroutines etc.) are mixed in, keeping their
        /// frame-by-frame timing undisturbed.
        /// </summary>
        private void TryFastForwardRealTime()
        {
            if (_Checkers.Count == 0)
            {
                return;
            }
            int earliest = int.MaxValue;
            foreach (var checker in _Checkers)
            {
                if (!(checker is CheckerForWaitForSecondsRealtime rt))
                {
                    return;
                }
                int end = rt.StartTick + rt.WaitTick;
                if (end < earliest)
                {
                    earliest = end;
                }
            }
            int now = _Looper.RealTimeTick;
            if (earliest > now)
            {
                _Looper.FastForwardTicks += earliest - now;
            }
        }

        protected internal class Looper
        {
            private int _StartTick;
            /// <summary>Accumulated fast-forwarded milliseconds: the amount skipped when RealTime is pulled to the earliest deadline with only realtime waits left.</summary>
            public int FastForwardTicks;
            public int RealTimeTick => (IsStarted ? Environment.TickCount - _StartTick : 0) + FastForwardTicks;
            public float RealTime => RealTimeTick / 1000f;

            private int _FrameCount = -1;
            public bool IsStarted => _FrameCount >= 0;
            public int FrameCount => IsStarted ? _FrameCount : 0;
            public float VirtualTimePerFrame = 1f / 60f;
            public float VirtualTime => FrameCount * VirtualTimePerFrame;

            private const int _LastPhase = (int)CoroutinePhase.OnEnd;
            private int _CurrentPhase = _LastPhase;
            public CoroutinePhase Phase => IsStarted ? (CoroutinePhase)_CurrentPhase : CoroutinePhase.OnStart;

            public void Tick()
            {
                if (!IsStarted)
                {
                    _StartTick = Environment.TickCount;
                }
                if (_CurrentPhase >= _LastPhase)
                {
                    _CurrentPhase = 0;
                    ++_FrameCount;
                }
                else
                {
                    ++_CurrentPhase;
                }
            }
        }
        protected internal Looper _Looper = new Looper();

        /// <summary>Current phase; OnStart while the Looper has not started yet.</summary>
        public CoroutinePhase CurrentPhase => _Looper.Phase;
        /// <summary>Current frame number, starting from 0; 0 while the Looper has not started yet.</summary>
        public int CurrentFrameCount => _Looper.FrameCount;
        /// <summary>Virtual time (seconds) advanced per frame, i.e. the scaled time flow rate.</summary>
        public float VirtualTimePerFrame
        {
            get => _Looper.VirtualTimePerFrame;
            set => _Looper.VirtualTimePerFrame = value;
        }
    }

    public static class CoroutineProvider
    {
        public static ICoroutineProvider Instance;
        public static void StartCoroutine(IEnumerator work)
        {
            Instance?.StartCoroutine(work);
        }
        public static bool TickCoroutineLooper()
        {
            return Instance?.TickCoroutineLooper() ?? false;
        }
        public static void RunCoroutineLooper()
        {
            while (Instance?.TickCoroutineLooper() ?? false) ;
        }
        public static void SetProvider(ICoroutineProvider provider = null)
        {
            Instance = provider ?? new StandaloneCoroutineProvider();
        }
    }
}
