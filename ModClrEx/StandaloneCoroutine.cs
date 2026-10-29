using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mod
{
    public interface ICoroutineProvider
    {
        CoroutineHandle StartCoroutine(IEnumerator work);
        void StopCoroutine(CoroutineHandle co);
        bool TickCoroutineLooper();
        /// <summary>
        /// Converts a provider-agnostic ICoroutineInstruction into an object the current engine can yield.
        /// For the Unity CoroutineProvider this produces the matching Unity yield instruction;
        /// for the standalone provider it returns the instruction itself.
        /// Callers write: yield return new CoroutineWaitForSeconds(10f).ToYield();
        /// </summary>
        object ConvertInstruction(object facade);
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
    /// Handle to a started coroutine — the counterpart of Unity's Coroutine object.
    /// It can be yielded (equivalent to Unity's "yield return StartCoroutine(sub)"),
    /// polled via IsDone, and stopped via ICoroutineProvider.StopCoroutine.
    ///
    /// Unity-side implementation (for the Unity ICoroutineProvider):
    /// - Derive a UnityCoroutineHandle : CoroutineHandle inside the Unity provider, passing
    ///   the provider into the base constructor: handles record their owning provider so
    ///   CoroutineProvider.StopCoroutine(handle) dispatches to the right one even when several
    ///   providers coexist (e.g. a runtime provider and an editor provider).
    /// - StartCoroutine: call MonoBehaviour.StartCoroutine(work) and keep the returned
    ///   UnityEngine.Coroutine inside the handle. ConvertInstruction must return that
    ///   UnityEngine.Coroutine for this handle, so "yield return handle.ToYield()" hands it
    ///   back to Unity's engine, which natively waits on a Coroutine object.
    /// - StopCoroutine: call MonoBehaviour.StopCoroutine(unityCoroutine).
    /// - IsDone: UnityEngine.Coroutine exposes no completion state; either wrap the user's
    ///   IEnumerator with a completion-flag wrapper before handing it to
    ///   MonoBehaviour.StartCoroutine, or conservatively return false.
    ///
    /// Standalone-side implementation (StandaloneCoroutineHandle):
    /// - Wraps the LinkedListNode carrying the coroutine's current checker. Tick advances
    ///   coroutines by replacing node.Value in place, so the node identity is stable for the
    ///   coroutine's whole lifetime; the coroutine is done once the node is detached
    ///   (List == null) or carries the dead marker (stopped).
    /// - Stop does not detach the node immediately: removing an arbitrary node during Tick's
    ///   traversal would sever the traversal chain (a detached node's Next is null). Instead
    ///   the node's checker is swapped for a dead marker and the node is removed when the
    ///   next traversal reaches it.
    /// </summary>
    public abstract class CoroutineHandle : ICoroutineInstruction
    {
        public CoroutinePhase Phase => CoroutinePhase.PostUpdate_WaitForSub;
        public object Attachment { get; set; }
        public ICoroutineProvider Provider { get; protected internal set; }
        /// <summary>Whether the coroutine has finished — completed normally or stopped.</summary>
        public abstract bool IsDone { get; }
        public CoroutineHandle(ICoroutineProvider provider)
        {
            Provider = provider;
        }
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
    /// Waits for a sub-coroutine driven in place — Unity's "yield return subIEnumerator" style.
    /// The sub-coroutine runs to its first suspension point immediately when the instruction is
    /// created, then advances in parallel with the parent; if the sub-coroutine completes
    /// synchronously (or all of its instructions are immediately Done), the parent passes
    /// through right away.
    ///
    /// Unity semantics reproduced:
    /// - The sub's node is kept in front of the parent's, so within the same phase the sub is
    ///   advanced first and the parent observes completion in the same frame — like Unity
    ///   resuming the parent at the very point the nested iterator finishes.
    /// - Stopping the parent cascades into the sub (and recursively any sub-of-sub): in Unity
    ///   a nested iterator is part of the parent's execution flow.
    /// - The sub has no handle of its own and cannot be stopped individually.
    ///
    /// For Unity's "yield return StartCoroutine(sub)" style (independent sub with its own
    /// handle that survives the parent's stop; the parent resumes one frame after the sub
    /// finishes) use: yield return provider.StartCoroutine(sub); — see CoroutineHandle.
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
            /// <summary>
            /// Invoked right after this checker becomes the value of a queue node — either
            /// freshly enqueued by StartCoroutine or swapped in place by Tick. Overridden by
            /// CheckerForWaitForSub to keep the sub-coroutine's node in front of the parent's.
            /// </summary>
            void OnEnqueued(LinkedListNode<ICoroutineInstructionChecker> node);
        }
        protected internal class StandaloneCoroutineHandle : CoroutineHandle
        {
            /// <summary>The queue node owning the coroutine's current checker; null when the coroutine completed synchronously.</summary>
            public LinkedListNode<ICoroutineInstructionChecker> Node;
            public override bool IsDone => Node == null || Node.List == null || Node.Value is DeadChecker;
            public StandaloneCoroutineHandle(ICoroutineProvider provider, LinkedListNode<ICoroutineInstructionChecker> node = null)
                : base(provider)
            {
                Node = node;
            }
        }
        /// <summary>
        /// Marker checker swapped into a stopped coroutine's node. The node must stay in the
        /// queue until the traversal reaches it: detaching an arbitrary node mid-traversal
        /// would sever the chain (a detached node's Next is null). The phase loop removes dead
        /// nodes when it reaches them (detaching the current node is safe — next is cached).
        /// </summary>
        protected internal class DeadChecker : BaseChecker<CoroutineWaitDefault>
        {
            public static readonly DeadChecker Instance = new DeadChecker();
            private DeadChecker()
                : base(null, new CoroutineWaitDefault())
            {
            }
            public override void Start()
            {
            }
            public override bool CheckDone()
            {
                return true;
            }
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
            /// <summary>
            /// Invoked right after this checker becomes the value of a queue node — either
            /// freshly enqueued by StartCoroutine or swapped in place by Tick. Overridden by
            /// CheckerForWaitForSub to keep the sub-coroutine's node in front of the parent's.
            /// </summary>
            public virtual void OnEnqueued(LinkedListNode<ICoroutineInstructionChecker> node)
            {
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
        /// Sub-coroutine checker for the "yield return subIEnumerator" style — Unity's in-place
        /// nested iterators. Start drives the sub to its first suspension point and enqueues it;
        /// completion is detected by the sub's queue node having been detached (node.List == null).
        ///
        /// Unity semantics reproduced:
        /// - The sub's node is kept in front of the parent's (OnEnqueued), so within the same
        ///   phase the sub is advanced first and the parent observes completion in the same
        ///   frame — like Unity resuming the parent at the very point the nested iterator finishes.
        /// - Stopping the parent cascades into the sub (and recursively any sub-of-sub): in Unity
        ///   a nested iterator is part of the parent's execution flow.
        /// - The sub has no handle of its own and cannot be stopped individually.
        ///
        /// For Unity's "yield return StartCoroutine(sub)" style (independent sub with its own
        /// handle that survives the parent's stop; the parent resumes one frame after the sub
        /// finishes) use: yield return provider.StartCoroutine(sub); — see CoroutineHandle.
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
                    var subnode = Provider._Checkers.AddLast(subchecker);
                    subchecker.OnEnqueued(subnode);
                    _SubNode = subnode;
                }
                else
                {
                    _SubDone = true; // sub-coroutine completed synchronously (or fully passed through); parent need not wait
                }
            }
            public override bool CheckDone()
            {
                return _SubDone || (_SubNode != null && (_SubNode.List == null || _SubNode.Value is DeadChecker));
            }
            public override void OnEnqueued(LinkedListNode<ICoroutineInstructionChecker> parentnode)
            {
                // Keep the sub's node in front of the parent's so that, within the same phase,
                // the sub is advanced first and the parent can observe its completion in the
                // same frame (mirrors Unity's in-place nested iterator timing).
                if (_SubNode != null && _SubNode.List != null)
                {
                    Provider._Checkers.Remove(_SubNode);
                    Provider._Checkers.AddBefore(parentnode, _SubNode);
                }
            }
            /// <summary>
            /// Stops the sub (and recursively any sub-of-sub) when the parent is stopped.
            /// Marks nodes dead instead of detaching them — see DeadChecker.
            /// </summary>
            internal void CascadeStop()
            {
                if (_SubNode != null && _SubNode.List != null && !(_SubNode.Value is DeadChecker))
                {
                    if (_SubNode.Value is CheckerForWaitForSub wfs)
                    {
                        wfs.CascadeStop();
                    }
                    _SubNode.Value = DeadChecker.Instance;
                }
            }
        }
        /// <summary>
        /// Checker for yielding a CoroutineHandle — Unity's "yield return StartCoroutine(sub)" style.
        /// The sub is already started and already enqueued behind the parent (mirroring Unity's
        /// registration order), so this checker only observes its completion.
        /// Unity semantics reproduced: the parent is always traversed before the sub, so it only
        /// sees the finished Coroutine on the FOLLOWING frame — hence the parent resumes one
        /// frame after completion was observed. A sub that completed synchronously passes
        /// through immediately (Unity's already-finished Coroutine).
        /// Difference from Unity (intentional): Unity leaves a parent waiting on a stopped sub
        /// stuck forever (a known pitfall); here stopping the sub resumes the parent.
        /// </summary>
        protected internal class CheckerForHandle : BaseChecker<CoroutineHandle>
        {
            private int _DoneFrame = -1;
            public CheckerForHandle(StandaloneCoroutineProvider provider, CoroutineHandle ci)
                : base(provider, ci)
            {
            }
            public override void Start()
            {
            }
            public override bool CheckDone()
            {
                if (!(Target is StandaloneCoroutineHandle sch))
                {
                    throw new ArgumentException("non-recognizable CoroutineHandle");
                }
                if (sch.Node == null)
                {
                    return true; // completed synchronously: pass through
                }
                var list = sch.Node.List;
                if (list == Provider._Checkers)
                {
                    if (sch.Node.Value is DeadChecker)
                    {
                        return true; // stopped: treat as finished so the waiting parent resumes
                    }
                    return false; // still running
                }
                if (list != null)
                {
                    throw new ArgumentException("CoroutineHandle belongs to another provider");
                }
                // Finished after suspension: resume the frame AFTER completion was observed
                // (mirrors Unity's list order — the parent is traversed before the sub and
                // only sees the finished Coroutine on the following frame).
                if (_DoneFrame < 0)
                {
                    _DoneFrame = Provider._Looper.FrameCount;
                    return false;
                }
                return Provider._Looper.FrameCount > _DoneFrame;
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
            if (ci is CoroutineHandle ch)
            {
                return new CheckerForHandle(this, ch);
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
        /// Returns the suspending checker (already associated with Work); null if the coroutine
        /// ran to completion. When a queue node is supplied (Tick's in-place swap path), a
        /// coroutine stopping itself from inside its body is detected here: driving stops
        /// right after the current MoveNext finishes, mirroring Unity's stop semantics.
        /// </summary>
        protected internal ICoroutineInstructionChecker DriveWork(IEnumerator work, LinkedListNode<ICoroutineInstructionChecker> node = null)
        {
            while (work.MoveNext())
            {
                if (node != null && (node.List == null || node.Value is DeadChecker))
                {
                    // The coroutine stopped itself while being driven: the body has run up to
                    // its yield (the current MoveNext finished); it must never resume.
                    return null;
                }
                var checker = CreateChecker(work.Current);
                if (!checker.CheckDone())
                {
                    checker.Work = work;
                    return checker;
                }
            }
            return null;
        }
        public CoroutineHandle StartCoroutine(IEnumerator work)
        {
            var checker = DriveWork(work);
            if (checker != null)
            {
                var node = _Checkers.AddLast(checker);
                checker.OnEnqueued(node);
                return new StandaloneCoroutineHandle(this, node);
            }
            else
            {
                return new StandaloneCoroutineHandle(this);
            }
        }
        public void StopCoroutine(CoroutineHandle co)
        {
            if (co != null)
            {
                if (co is StandaloneCoroutineHandle realco)
                {
                    // Dispatch guard: the handle records the provider that started it. The static
                    // facade dispatches by co.Provider, so landing here with a foreign handle
                    // means StopCoroutine was called directly on the wrong provider.
                    if (realco.Provider != this)
                    {
                        throw new ArgumentException("trying to stop a coroutine with a foreign provider", nameof(co));
                    }
                    // Node.List == _Checkers guards against double stops and against stopping a
                    // coroutine that already finished: LinkedList.Remove(node) throws
                    // InvalidOperationException for a node that does not belong to this list
                    // (a removed node's List is null and its Next/Prev are invalidated).
                    if (realco.Node != null && realco.Node.List == _Checkers)
                    {
                        if (realco.Node.Value is CheckerForWaitForSub wfs)
                        {
                            // Unity: a nested iterator (yield return IEnumerator) is part of the
                            // parent's execution flow — stopping the parent stops the nested sub too.
                            wfs.CascadeStop();
                        }
                        // Mark dead instead of detaching: the node may sit ahead of Tick's current
                        // traversal cursor, and detaching it would sever the traversal chain
                        // (a detached node's Next is null). The next traversal removes it.
                        realco.Node.Value = DeadChecker.Instance;
                    }
                }
                else
                {
                    throw new ArgumentException("non-recognizable CoroutineHandle", nameof(co));
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
                if (checker is DeadChecker)
                {
                    // A stopped coroutine: detach its node now that the traversal has reached it
                    // (detaching the current node is safe — next is already cached).
                    _Checkers.Remove(node);
                }
                else if (_Looper.Phase == checker.Target.Phase && checker.CheckDone())
                {
                    // Instruction Done: drive the coroutine body; replace the checker in place
                    // if it suspends again, remove the node if it ran to completion (or stopped
                    // itself while being driven — DriveWork returns null in both cases).
                    var nextchecker = checker.Work != null ? DriveWork(checker.Work, node) : null;
                    if (nextchecker != null)
                    {
                        node.Value = nextchecker;
                        nextchecker.OnEnqueued(node);
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
        public object ConvertInstruction(object facade) => facade;

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
        public static CoroutineHandle StartCoroutine(IEnumerator work)
        {
            return Instance?.StartCoroutine(work);
        }
        public static void StopCoroutine(CoroutineHandle co)
        {
            // Dispatch to the provider that started the coroutine: handles may come from a
            // different provider than the current Instance (e.g. mixed runtime/editor
            // providers in Unity), and only the owning provider knows how to stop it.
            co?.Provider?.StopCoroutine(co);
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
        public static void SetProvider<T>() where T : ICoroutineProvider, new()
        {
            Instance = new T();
        }
        /// <summary>
        /// Converts this instruction into the yield object understood by the current provider
        /// (Unity: the matching Unity yield instruction; standalone: the instruction itself).
        /// Usage: yield return new CoroutineWaitForSeconds(10f).ToYield();
        /// </summary>
        public static object ToYield(this ICoroutineInstruction ci)
        {
            return Instance?.ConvertInstruction(ci) ?? ci;
        }
    }
}
