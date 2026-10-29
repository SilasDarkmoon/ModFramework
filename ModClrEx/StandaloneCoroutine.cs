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
    public class CoroutineWaitForSub : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.PostUpdate_WaitForSub;
        public object Attachment { get; set; }
        public IEnumerator Sub { get; protected internal set; }
        public CoroutineWaitForSub(IEnumerator sub)
        {
            Sub = sub;
        }
    }
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
            public BaseChecker()
            {
                Start();
            }
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
            }
        }
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
                StartTick = Environment.TickCount;
                WaitTick = (int)Math.Ceiling(Target.WaitTime * 1000f);
            }
            public override bool CheckDone()
            {
                return Environment.TickCount - StartTick >= WaitTick;
            }
            public CheckerForWaitForSecondsRealtime(StandaloneCoroutineProvider provider, CoroutineWaitForSecondsRealtime ci)
                : base(provider, ci)
            {
            }
        }

        protected internal ICoroutineInstructionChecker CreateChecker(object ci)
        {
            if (ci == null)
            {
                return new CheckerForDefault(this, null);
            }
            if (ci is CoroutineWaitForSecondsRealtime cirt)
            {
                return new CheckerForWaitForSecondsRealtime(this, cirt);
            }
            return null; // TODO: return waitfordefault.
        }

        protected internal LinkedList<ICoroutineInstructionChecker> _Checkers = new LinkedList<ICoroutineInstructionChecker>();
        public void StartCoroutine(IEnumerator work)
        {
            while (work.MoveNext())
            {
                var ci = work.Current;
                var checker = CreateChecker(ci);
                if (!checker.CheckDone())
                {
                    checker.Work = work;
                    _Checkers.AddLast(checker);
                    break;
                }
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
                if (_Looper.Phase == checker.Target.Phase)
                {
                    if (checker.CheckDone())
                    {
                        if (checker.Work != null)
                        {
                            var nextwork = checker.Work;
                            bool notDone;
                            while (notDone = nextwork.MoveNext())
                            {
                                var nextci = nextwork.Current;
                                var nextchecker = CreateChecker(nextci);
                                if (!nextchecker.CheckDone())
                                {
                                    nextchecker.Work = nextwork;
                                    node.Value = nextchecker;
                                    break;
                                }
                            }
                            if (!notDone)
                            {
                                _Checkers.Remove(node);
                            }
                        }
                    }
                }
                node = next;
            }
            return _Checkers.Count > 0;
        }

        protected internal class Looper
        {
            private int _StartTick;
            public int RealTimeTick => IsStarted ? Environment.TickCount - _StartTick : 0;
            public float RealTime => RealTimeTick / 1000f;

            private int _FrameCount = -1;
            public bool IsStarted => _FrameCount >= 0;
            public int FrameCount => IsStarted ? _FrameCount : 0;
            public float VirtualTimePerFrame = 0.017f;
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
