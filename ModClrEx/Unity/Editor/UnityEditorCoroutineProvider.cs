using System;
using System.Reflection;
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
    /// (EditorApplication.timeSinceStartup deltas) multiplied by the provider's own TimeScale
    /// on every update — never derived from frame counts: there is no player clock in edit
    /// mode, and editor updates tick at an irregular rate, so the standalone fixed
    /// VirtualTimePerFrame would be wrong. The editor's own time scale is read from the
    /// internal UnityEditor.TimeHelper.timeScale via GetEditorTimeScale (falling back to
    /// UnityEngine.Time.timeScale when reflection is unavailable) and used as the default
    /// TimeScale; it can still be overridden by setting TimeScale.
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

        // Cached reflection result for reading the editor's internal time scale.
        // Resolved once on first use; a typed delegate avoids per-call PropertyInfo boxing.
        private static Func<float> _TimeScaleReader;

        /// <summary>
        /// Time scaling applied to scaled waits (CoroutineWaitForSeconds) in edit mode.
        /// Defaults to the editor's own time scale as reported by UnityEditor.TimeHelper
        /// (see GetEditorTimeScale, which falls back to UnityEngine.Time.timeScale when the
        /// internal type is unavailable). Set this to a higher value to speed up, lower to
        /// slow down, overriding the editor default.
        /// </summary>
        public float TimeScale
        {
            get { return _EditorLooper.TimeScale; }
            set { _EditorLooper.TimeScale = value; }
        }

        /// <summary>
        /// Reads the editor's own time scale from the internal UnityEditor.TimeHelper
        /// (exposed as a static property/field), falling back to UnityEngine.Time.timeScale
        /// when reflection is unavailable. The resolution happens once and is reused as a
        /// typed delegate, so each call is a direct static getter invoke rather than a
        /// PropertyInfo.GetValue box — equivalent to what an UnsafeEx.CreateInvoker delegate
        /// would give, without taking a dependency on ModClrExUnsafe in the Unity provider.
        /// </summary>
        public static float GetEditorTimeScale()
        {
            if (_TimeScaleReader == null)
            {
                _TimeScaleReader = ResolveEditorTimeScaleReader() ?? (() => Time.timeScale);
            }
            return _TimeScaleReader();
        }

        private static Func<float> ResolveEditorTimeScaleReader()
        {
            try
            {
                var timeHelperType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.TimeHelper");
                if (timeHelperType != null)
                {
                    // Fast path: bind a typed delegate to the static getter (get_timeScale),
                    // avoiding per-call PropertyInfo reflection.
                    var prop = timeHelperType.GetProperty("timeScale",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    var getter = prop?.GetGetMethod(true);
                    if (getter != null && getter.ReturnType == typeof(float))
                    {
                        return (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), getter);
                    }
                    // Slow fallback: read the backing field directly.
                    var field = timeHelperType.GetField("m_TimeScale",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (field != null)
                    {
                        return () => (float)field.GetValue(null);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Mod.Unity] Failed to reflect UnityEditor.TimeHelper.timeScale, falling back to Time.timeScale: {e.Message}");
            }
            return null;
        }

        private readonly EditorLooper _EditorLooper;

        private UnityEditorCoroutineProvider()
        {
            _EditorLooper = new EditorLooper();
            // Default to the editor's own time scale (reflected from UnityEditor.TimeHelper,
            // falling back to Time.timeScale) instead of a hardcoded 1.
            _EditorLooper.TimeScale = GetEditorTimeScale();
            _Looper = _EditorLooper;
            EditorApplication.update += OnEditorUpdate;
        }

        public override bool TickCoroutineLooper()
        {
            // Driven by EditorApplication.update; external driving (RunCoroutineLooper) is not needed.
            return false;
        }

        protected override void TryFastForwardRealTime()
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
        /// Looper whose scaled time accumulates real time (EditorApplication.timeSinceStartup
        /// deltas) multiplied by the provider's own TimeScale, replacing the standalone fixed
        /// VirtualTimePerFrame model.
        /// </summary>
        protected class EditorLooper : Looper
        {
            private double _LastRealTime;
            private float _VirtualTime;
            public float TimeScale = 1f;
            public override float VirtualTime => _VirtualTime;
            public EditorLooper()
            {
                _LastRealTime = EditorApplication.timeSinceStartup;
            }
            public void Accumulate(double realTime)
            {
                _VirtualTime += (float)((realTime - _LastRealTime) * TimeScale);
                _LastRealTime = realTime;
            }
        }
    }
}
