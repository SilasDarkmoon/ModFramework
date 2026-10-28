using System;
using System.Collections.Generic;
using System.Text;

namespace Mod
{
    public static class StandaloneCoroutine
    {
        public enum LoopPhase
        {
            OnStart = 0,

            PostFixedUpdate = 100,

            PostUpdate = 200,
            PostUpdate_WaitForRealTime = PostUpdate,
            PostUpdate_WaitForTime,
            PostUpdate_WaitForAsync,
            PostUpdate_WaitForCustom,
            PostUpdate_WaitForDefault,
            PostUpdate_WaitForSub,

            PostLateUpdate = 300,

            OnEnd = 400,
        }
    }

    public class StandaloneCoroutineLoop
    {
        private static StandaloneCoroutineLoop _Loop;
        public static StandaloneCoroutineLoop Loop
        {
            get
            {
                if (_Loop == null)
                {
                    _Loop = new StandaloneCoroutineLoop();
                }
                return _Loop;
            }
        }
        private static int _DefaultTickCountPerLoop = 17;
        public static int DefaultTickCountPerLoop
        {
            get => _DefaultTickCountPerLoop;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "DefaultTickCountPerLoop must be greater than 0.");
                }
                _DefaultTickCountPerLoop = value;
            }
        }
        private int _TickCountPerLoop = _DefaultTickCountPerLoop;
        public int TickCountPerLoop
        {
            get => _TickCountPerLoop;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "TickCountPerLoop must be greater than 0.");
                }
                _TickCountPerLoop = value;
            }
        }

        public void Run()
        {
        }
        public void Stop()
        {
        }
    }
}
