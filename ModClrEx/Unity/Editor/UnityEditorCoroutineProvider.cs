using Mod;
using UnityEditor;
using UnityEngine;

namespace Mod.Unity
{
    /// <summary>
    /// Editor-mode ICoroutineProvider — works while Unity is NOT playing. Reuses the
    /// standalone checker machinery, driven by EditorApplication.update instead of an
    /// external loop (TickCoroutineLooper returns false: no external driving needed).
    ///
    /// Scaled time (CoroutineWaitForSeconds) is simulated by accumulating real time
    /// multiplied by UnityEngine.Time.timeScale on every update — never derived from
    /// frame counts: there is no player clock in edit mode, and editor updates tick
    /// at an irregular rate, so the standalone fixed VirtualTimePerFrame would be wrong.
    /// Realtime waits (CoroutineWaitForSecondsRealtime) observe real
    /// time; the standalone fast-forward is disabled (editor updates may be seconds
    /// apart when the editor is idle). Non-time instructions (yield null, end of frame
    /// etc.) advance by update count — 41 updates per virtual frame, like the standalone looper.
    /// </summary>
    public class UnityEditorCoroutineProvider : StandaloneCoroutineProvider
    {
        private static UnityEditorCoroutineProvider _Instance;
        public static UnityEditorCoroutineProvider Instance
        {
            get { return _Instance ?? (_Instance = new UnityEditorCoroutineProvider()); }
        }

        private readonly EditorLooper _EditorLooper;

        private UnityEditorCoroutineProvider()
        {
            _EditorLooper = new EditorLooper();
            _Looper = _EditorLooper;
            EditorApplication.update += OnEditorUpdate;
        }

        public override bool TickCoroutineLooper()
        {
            // Driven by EditorApplication.update; external driving (RunCoroutineLooper) is not needed.
            return false;
        }

        protected internal override void TryFastForwardRealTime()
        {
            // No fast-forward in the editor: updates are event-driven and may be seconds
            // apart when the editor is idle — realtime waits must observe real time.
        }

        private void OnEditorUpdate()
        {
            _EditorLooper.Accumulate(EditorApplication.timeSinceStartup);
            base.TickCoroutineLooper();
        }

        [InitializeOnLoadMethod]
        private static void AutoInstall()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CoroutineProvider.SetProvider(Instance);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Back to non-playing mode: the editor provider takes over again.
                // (Entering play mode is handled by UnityCoroutineProvider's
                // RuntimeInitializeOnLoadMethod in the runtime assembly.)
                CoroutineProvider.SetProvider(Instance);
            }
        }

        /// <summary>
        /// Looper whose scaled time accumulates real time multiplied by
        /// UnityEngine.Time.timeScale (EditorApplication.timeSinceStartup deltas),
        /// replacing the standalone fixed VirtualTimePerFrame model.
        /// </summary>
        protected class EditorLooper : Looper
        {
            private double _LastRealTime;
            private float _VirtualTime;
            public override float VirtualTime => _VirtualTime;
            public EditorLooper()
            {
                _LastRealTime = EditorApplication.timeSinceStartup;
            }
            public void Accumulate(double realTime)
            {
                _VirtualTime += (float)((realTime - _LastRealTime) * Time.timeScale);
                _LastRealTime = realTime;
            }
        }
    }
}
