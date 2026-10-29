using System;
using System.Collections;
using Mod;
using UnityEngine;

namespace Mod.Unity
{
    /// <summary>
    /// Runtime ICoroutineProvider for Unity. Coroutines run on a hidden, persistent
    /// runner MonoBehaviour and are driven by Unity's own player loop, so
    /// TickCoroutineLooper always returns false — no external driving is ever needed.
    /// Automatically installed on play via [RuntimeInitializeOnLoadMethod]; for the
    /// non-playing editor mode see UnityEditorCoroutineProvider (Editor subfolder).
    /// </summary>
    public class UnityCoroutineProvider : ICoroutineProvider
    {
        public CoroutineHandle StartCoroutine(IEnumerator work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }
            var handle = new UnityCoroutineHandle(this, work);
            handle.UnityCoroutine = CoroutineRunner.GetOrCreate().StartCoroutine(handle.Wrap());
            return handle;
        }
        public void StopCoroutine(CoroutineHandle co)
        {
            if (co == null)
            {
                return;
            }
            if (co is UnityCoroutineHandle uh && ReferenceEquals(uh.Provider, this))
            {
                uh._Stopped = true;
                if (uh.UnityCoroutine != null)
                {
                    CoroutineRunner.GetOrCreate().StopCoroutine(uh.UnityCoroutine);
                }
            }
            else
            {
                throw new ArgumentException("trying to stop a coroutine with a foreign provider", nameof(co));
            }
        }
        public bool TickCoroutineLooper()
        {
            // Unity's player loop drives the coroutines; external driving is never needed.
            return false;
        }
        public object ConvertInstruction(object facade)
        {
            switch (facade)
            {
                case UnityCoroutineHandle uh:
                    return uh.UnityCoroutine; // Unity natively waits on a Coroutine object
                case CoroutineWaitForSeconds w:
                    return new WaitForSeconds(w.WaitTime);
                case CoroutineWaitForSecondsRealtime w:
                    return new WaitForSecondsRealtime(w.WaitTime);
                case CoroutineWaitForEndOfFrame _:
                    return new WaitForEndOfFrame();
                case CoroutineWaitForFixedUpdate _:
                    return new WaitForFixedUpdate();
                case CoroutineWaitUntil w:
                    return new WaitUntil(w.Predicate);
                case CoroutineWaitWhile w:
                    return new WaitWhile(w.Predicate);
                case CoroutineWaitForSub w:
                    return w.Sub; // Unity drives nested iterators in place (yield return subIEnumerator)
                case CoroutineWaitDefault _:
                    return null; // yield return null
                default:
                    return facade; // unknown objects: Unity treats them as yield return null
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            CoroutineProvider.SetProvider(new UnityCoroutineProvider());
        }
    }

    /// <summary>
    /// Handle wrapping the UnityEngine.Coroutine returned by MonoBehaviour.StartCoroutine.
    /// The user's IEnumerator is wrapped so completion can be observed (UnityEngine.Coroutine
    /// exposes no state): the wrapper yields the work as a nested iterator and flags
    /// completion afterwards, so IsDone lags at most one resume point behind actual
    /// completion; a stopped handle reports IsDone immediately via the stop flag.
    /// </summary>
    public class UnityCoroutineHandle : CoroutineHandle
    {
        internal Coroutine UnityCoroutine;
        internal bool _Done;
        internal bool _Stopped;
        private readonly IEnumerator _Work;
        internal UnityCoroutineHandle(ICoroutineProvider provider, IEnumerator work)
            : base(provider)
        {
            _Work = work;
        }
        public override bool IsDone => _Done || _Stopped;
        internal IEnumerator Wrap()
        {
            yield return _Work; // Unity drives the nested iterator in place
            _Done = true;
        }
    }

    /// <summary>
    /// Hidden, persistent host for coroutines started by UnityCoroutineProvider:
    /// never destroyed (DontDestroyOnLoad) and never saved or shown in the editor
    /// (HideAndDontSave). Re-created automatically after play mode ends or a domain
    /// reload (Unity's overloaded == catches the destroyed instance).
    /// </summary>
    internal class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _Instance;
        public static CoroutineRunner GetOrCreate()
        {
            if (_Instance == null) // Unity's overloaded == also catches a destroyed runner
            {
                var go = new GameObject("Mod.Unity CoroutineRunner");
                _Instance = go.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(go); // survives scene loads
                go.hideFlags = HideFlags.HideAndDontSave; // hidden in hierarchy, never saved
            }
            return _Instance;
        }
    }
}
