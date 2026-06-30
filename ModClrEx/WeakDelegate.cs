using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mod
{
    public interface IObjectRef
    {
        object Target { get; set; }
    }
    public interface IStrongRef : IObjectRef
    {
    }
    public interface IWeakRef : IObjectRef
    {
    }
    public struct StrongRef : IStrongRef
    {
        private object _Target;
        public object Target { get => _Target; set => _Target = value; }
    }
    public struct StrongRef<T> : IStrongRef where T : class
    {
        private T _Target;
        object IObjectRef.Target { get => _Target; set => _Target = (T)value; }
        public T Value
        {
            get => _Target;
            set => _Target = value;
        }

        public static implicit operator T(StrongRef<T> r)
        {
            return r.Value;
        }
        public static implicit operator StrongRef<T>(T value)
        {
            return new StrongRef<T>() { Value = value };
        }
    }
    public class WeakRefDeadException : Exception
    {
        public override string Message => "WeakRef is dead!";
    }
    public struct WeakRef : IWeakRef
    {
        private WeakReference<object> _Inner;
        internal object UncheckedTarget
        {
            get
            {
                if (_Inner == null) return null;
                _Inner.TryGetTarget(out object result);
                return result;
            }
            set
            {
                if (value == null)
                {
                    _Inner = null;
                }
                else
                {
                    if (_Inner == null)
                    {
                        _Inner = new WeakReference<object>(value);
                    }
                    else
                    {
                        _Inner.SetTarget(value);
                    }
                }
            }
        }
        public object Target
        {
            get
            {
                if (_Inner == null) return null;
                if (_Inner.TryGetTarget(out object result))
                {
                    return result;
                }
                throw new WeakRefDeadException();
            }
            set => UncheckedTarget = value;
        }
        public bool TryGetTarget(out object target)
        {
            if (_Inner == null)
            {
                target = null;
                return true;
            }
            else
            {
                return _Inner.TryGetTarget(out target);
            }
        }
        public bool HasValue
        {
            get => _Inner != null;
        }
        public bool IsAlive
        {
            get => !HasValue || UncheckedTarget != null;
        }
    }
    public struct WeakRef<T> : IWeakRef where T : class
    {
        private WeakReference<T> _Inner;
        object IObjectRef.Target { get => Target; set => Target = (T)value; }
        internal T UncheckedTarget
        {
            get
            {
                if (_Inner == null) return null;
                _Inner.TryGetTarget(out T result);
                return result;
            }
            set
            {
                if (value == null)
                {
                    _Inner = null;
                }
                else
                {
                    if (_Inner == null)
                    {
                        _Inner = new WeakReference<T>(value);
                    }
                    else
                    {
                        _Inner.SetTarget(value);
                    }
                }
            }
        }
        public T Target
        {
            get
            {
                if (_Inner == null) return null;
                if (_Inner.TryGetTarget(out T result))
                {
                    return result;
                }
                throw new WeakRefDeadException();
            }
            set => UncheckedTarget = value;
        }
        public bool TryGetTarget(out T target)
        {
            if (_Inner == null)
            {
                target = null;
                return true;
            }
            else
            {
                return _Inner.TryGetTarget(out target);
            }
        }
        public static implicit operator T(WeakRef<T> r)
        {
            return r.Target;
        }
        public static implicit operator WeakRef<T>(T value)
        {
            return new WeakRef<T>() { Target = value };
        }
        public bool HasValue
        {
            get => _Inner != null;
        }
        public bool IsAlive
        {
            get => !HasValue || UncheckedTarget != null;
        }
    }

    public static class WeakDelegate
    {
        public static StrongRef<T> StrongRef<T>(this T value) where T : class
        {
            return value;
        }
        public static WeakRef<T> WeakRef<T>(this T value) where T : class
        {
            return value;
        }

        internal abstract class WeakWrapper
        {
            protected (int offset, WeakRef<object> wref)[] _ParsedRef;
            protected object _ClonedTarget;
            protected WeakRef<object> _WeakTarget;
            protected bool _IsObjectInstanceDelegate;
            protected Delegate _InnerDelegate;
            protected internal bool _RethrowWeakRefDeadException;

            public virtual void SetDelegate(Delegate del)
            {
                var target = del.Target;
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (target == (object)del)
                    {
                        target = null;
                    }
                }
                if (_IsObjectInstanceDelegate = (target != null && !target.GetType().IsValueType))
                {
                    if (target.GetType().Name.StartsWith("<>"))
                    { // Lambda
                        var ct = _ClonedTarget = target.MemberwiseClone();
                        _WeakTarget = new WeakRef<object>() { UncheckedTarget = target };
                        List<(int offset, WeakRef<object> wref)> parsed = new List<(int offset, WeakRef<object> wref)>();
                        foreach (var field in ct.GetType().GetAllFields())
                        {
                            if (!field.FieldType.IsValueType)
                            {
                                var offset = field.GetFieldOffset();
                                ref var fval = ref ct.GetField<object>(offset);
                                if (fval != null)
                                {
                                    parsed.Add((offset, new WeakRef<object>() { UncheckedTarget = fval }));
                                }
                                else
                                {
                                    parsed.Add((offset, default));
                                }
                                fval = null;
                            }
                        }
                        _ParsedRef = parsed.ToArray();
                    }
                    else
                    { // instance delegate
                        _WeakTarget = new WeakRef<object>() { UncheckedTarget = target };
                    }
                }
            }

            protected (object target, bool alive) PrepareTarget()
            {
                if (_IsObjectInstanceDelegate)
                {
                    if (_ParsedRef != null && _ClonedTarget != null)
                    {
                        bool alive = true;
                        foreach ((var offset, var wref) in _ParsedRef)
                        {
                            var fval = _ClonedTarget.GetField<object>(offset) = wref.UncheckedTarget;
                            alive &= !wref.HasValue || fval != null;
                        }
                        return (_ClonedTarget, alive);
                    }
                    else
                    {
                        var target = _WeakTarget.UncheckedTarget;
                        _InnerDelegate.ChangeTarget(target);
                        return (target, target != null);
                    }
                }
                else
                {
                    return (null, true);
                }
            }
            protected void ApplyChangesToTarget()
            {
                if (_IsObjectInstanceDelegate)
                {
                    if (_ParsedRef != null && _ClonedTarget != null)
                    {
                        var target = _WeakTarget.UncheckedTarget;
                        if (target != null)
                        {
                            UnsafeEx.MemCopy(target, _ClonedTarget);
                        }
                        for (int i = 0; i < _ParsedRef.Length; ++i)
                        {
                            (var offset, var wref) = _ParsedRef[i];
                            ref var newfval = ref _ClonedTarget.GetField<object>(offset);
                            if (newfval != wref.UncheckedTarget)
                            {
                                if (newfval == null)
                                {
                                    wref.UncheckedTarget = null;
                                }
                                else
                                {
                                    wref.UncheckedTarget = newfval;
                                }
                                _ParsedRef[i] = (offset, wref);
                            }
                            newfval = null;
                        }
                    }
                    else
                    {
                        _InnerDelegate.ChangeTarget(null);
                    }
                }
            }
            public bool IsAlive
            {
                get
                {
                    if (_IsObjectInstanceDelegate)
                    {
                        if (_ParsedRef != null && _ClonedTarget != null)
                        {
                            foreach ((var offset, var wref) in _ParsedRef)
                            {
                                if (wref.HasValue && wref.UncheckedTarget == null)
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                        else
                        {
                            return !_WeakTarget.HasValue || _WeakTarget.UncheckedTarget != null;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            public (bool isAlive, object originalTarget, MethodBase originalMethod) GetDelegateInfo()
            {
                object otar = null;
                MethodBase omethod = _InnerDelegate.Method;
                if (_IsObjectInstanceDelegate)
                {
                    otar = _WeakTarget.UncheckedTarget;
                }
                else
                {
                    otar = _InnerDelegate.Target;
                    if (RuntimeInfo.IsNativeAOT)
                    {
                        if (otar == (object)_InnerDelegate)
                        {
                            otar = null;
                        }
                    }
                }
                return (IsAlive, otar, omethod);
            }
        }
        private class WeakWrapper<R> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn()
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action)_InnerDelegate)();
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<R>)_InnerDelegate)();
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke()
            {
                R rv = default;
                TryInvoke(ref rv);
                return rv;
            }
        }
        private class WeakWrapper<R, U1> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1>)_InnerDelegate)(u1);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, R>)_InnerDelegate)(u1);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1)
            {
                R rv = default;
                TryInvoke(ref rv, u1);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2>)_InnerDelegate)(u1, u2);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, R>)_InnerDelegate)(u1, u2);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3>)_InnerDelegate)(u1, u2, u3);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, R>)_InnerDelegate)(u1, u2, u3);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4>)_InnerDelegate)(u1, u2, u3, u4);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, R>)_InnerDelegate)(u1, u2, u3, u4);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5>)_InnerDelegate)(u1, u2, u3, u4, u5);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, R>)_InnerDelegate)(u1, u2, u3, u4, u5);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6>)_InnerDelegate)(u1, u2, u3, u4, u5, u6);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7, U8>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, U8, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7, U8, U9>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                return rv;
            }
        }
        private class WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : WeakWrapper
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerDelegate = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>>();
                }
                else
                {
                    _InnerDelegate = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R>>();
                }
                if (_IsObjectInstanceDelegate)
                {
                    _InnerDelegate.ChangeTarget(_ClonedTarget);
                }
            }
            public WeakWrapper() { }
            public WeakWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public WeakWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> act)
            {
                SetDelegate(act);
            }
            public WeakWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> func)
            {
                SetDelegate(func);
            }

            public void InvokeNoReturn(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15, U16 u16)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return;
                try
                {
                    ((Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
            }
            internal bool TryInvoke(ref R result, U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15, U16 u16)
            {
                (var target, var alive) = PrepareTarget();
                if (!alive) return false;
                try
                {
                    result = ((Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R>)_InnerDelegate)(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                }
                catch (WeakRefDeadException)
                {
                    if (_RethrowWeakRefDeadException) throw;
                }
                finally
                {
                    ApplyChangesToTarget();
                }
                return true;
            }
            public R Invoke(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15, U16 u16)
            {
                R rv = default;
                TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                return rv;
            }
        }

        public static Func<R> Weak<R>(this Func<R> func)
        {
            return new WeakWrapper<R>(func).Invoke;
        }
        public static Func<U1, R> Weak<R, U1>(this Func<U1, R> func)
        {
            return new WeakWrapper<R, U1>(func).Invoke;
        }
        public static Func<U1, U2, R> Weak<R, U1, U2>(this Func<U1, U2, R> func)
        {
            return new WeakWrapper<R, U1, U2>(func).Invoke;
        }
        public static Func<U1, U2, U3, R> Weak<R, U1, U2, U3>(this Func<U1, U2, U3, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, R> Weak<R, U1, U2, U3, U4>(this Func<U1, U2, U3, U4, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, R> Weak<R, U1, U2, U3, U4, U5>(this Func<U1, U2, U3, U4, U5, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, R> Weak<R, U1, U2, U3, U4, U5, U6>(this Func<U1, U2, U3, U4, U5, U6, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, R> Weak<R, U1, U2, U3, U4, U5, U6, U7>(this Func<U1, U2, U3, U4, U5, U6, U7, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, U8, R> Weak<R, U1, U2, U3, U4, U5, U6, U7, U8>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> Weak<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> Weak<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> Weak<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> Weak<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> Weak<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> Weak<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> Weak<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(func).Invoke;
        }
        public static Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> Weak<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> func)
        {
            return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(func).Invoke;
        }

        public static Action Weak(this Action func)
        {
            return new WeakWrapper<VoidReturn>(func).InvokeNoReturn;
        }
        public static Action<U1> Weak<U1>(this Action<U1> func)
        {
            return new WeakWrapper<VoidReturn, U1>(func).InvokeNoReturn;
        }
        public static Action<U1, U2> Weak<U1, U2>(this Action<U1, U2> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3> Weak<U1, U2, U3>(this Action<U1, U2, U3> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4> Weak<U1, U2, U3, U4>(this Action<U1, U2, U3, U4> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5> Weak<U1, U2, U3, U4, U5>(this Action<U1, U2, U3, U4, U5> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6> Weak<U1, U2, U3, U4, U5, U6>(this Action<U1, U2, U3, U4, U5, U6> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7> Weak<U1, U2, U3, U4, U5, U6, U7>(this Action<U1, U2, U3, U4, U5, U6, U7> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7, U8> Weak<U1, U2, U3, U4, U5, U6, U7, U8>(this Action<U1, U2, U3, U4, U5, U6, U7, U8> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> Weak<U1, U2, U3, U4, U5, U6, U7, U8, U9>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> Weak<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Weak<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Weak<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Weak<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Weak<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Weak<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(func).InvokeNoReturn;
        }
        public static Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Weak<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> func)
        {
            return new WeakWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(func).InvokeNoReturn;
        }

        private static Type[] _Wrappers =
        {
            typeof(WeakWrapper<>),
            typeof(WeakWrapper<,>),
            typeof(WeakWrapper<,,>),
            typeof(WeakWrapper<,,,>),
            typeof(WeakWrapper<,,,,>),
            typeof(WeakWrapper<,,,,,>),
            typeof(WeakWrapper<,,,,,,>),
            typeof(WeakWrapper<,,,,,,,>),
            typeof(WeakWrapper<,,,,,,,,>),
            typeof(WeakWrapper<,,,,,,,,,>),
            typeof(WeakWrapper<,,,,,,,,,,>),
            typeof(WeakWrapper<,,,,,,,,,,,>),
            typeof(WeakWrapper<,,,,,,,,,,,,>),
            typeof(WeakWrapper<,,,,,,,,,,,,,>),
            typeof(WeakWrapper<,,,,,,,,,,,,,,>),
            typeof(WeakWrapper<,,,,,,,,,,,,,,,>),
            typeof(WeakWrapper<,,,,,,,,,,,,,,,,>),
        };
        public static Delegate Weak(this Delegate odel, Type ddeltype)
        {
            var miodel = odel.GetType().GetMethod("Invoke", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var opars = miodel.GetParameters();
            var oparcnt = opars.Length;
            var upars = new Type[oparcnt + 1];
            upars[0] = DelegateCurry.GetCompatible‌UnderlyingType(miodel.ReturnType);
            for (int i = 0; i < oparcnt; ++i)
            {
                upars[i + 1] = DelegateCurry.GetCompatible‌UnderlyingType(opars[i].ParameterType);
            }
            var gtype = _Wrappers[oparcnt];
            var wtype = gtype.MakeGenericType(upars);
            var wrapper = UnsafeEx.GetUninitializedObject(wtype) as WeakWrapper;
            wrapper.SetDelegate(odel);
            string mname = miodel.ReturnType == typeof(void) ? "InvokeNoReturn" : "Invoke";
            var miwrapper = wtype.GetMethod(mname, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return UnsafeEx.CreateDelegateUnsafe(ddeltype, wrapper, miwrapper);
        }
        public static Delegate Weak(this Delegate odel)
        {
            return Weak(odel, odel.GetType());
        }
        public static TDel Weak<TDel>(this TDel odel) where TDel : Delegate
        {
            return (TDel)Weak(odel, typeof(TDel));
        }

        public abstract class WeakEvent
        {
            internal List<WeakWrapper> _Listeners = new List<WeakWrapper>();
            internal abstract WeakWrapper Wrap(Delegate handler);

            public void RemoveDead()
            {
                RemoveDead(false);
            }
            protected void RemoveDead(bool inplace)
            {
                List<WeakWrapper> newlist = inplace ? _Listeners : null;
                for (int i = _Listeners.Count - 1; i >= 0; --i)
                {
                    var wrapper = _Listeners[i];
                    if (!wrapper.IsAlive)
                    {
                        if (newlist == null)
                        {
                            newlist = new List<WeakWrapper>(_Listeners.Capacity);
                            newlist.AddRange(_Listeners);
                        }
                        newlist.RemoveAt(i);
                    }
                }
                if (newlist != null)
                {
                    _Listeners = newlist;
                }
            }

            public void AddListener(Delegate handler)
            {
                var oldlist = _Listeners;
                RemoveDead();
                List<WeakWrapper> newlist = _Listeners;
                if (oldlist == _Listeners)
                {
                    newlist = new List<WeakWrapper>(_Listeners.Capacity);
                    newlist.AddRange(_Listeners);
                }

                if (handler.Target is WeakWrapper wrapper)
                {
                }
                else
                {
                    wrapper = Wrap(handler);
                }
                wrapper._RethrowWeakRefDeadException = true;
                newlist.Add(wrapper);

                if (oldlist == _Listeners)
                {
                    _Listeners = newlist;
                }
            }
            public void RemoveListener(Delegate handler)
            {
                var tar = handler.Target;
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (tar == (object)handler)
                    {
                        tar = null;
                    }
                }
                MethodBase method = handler.Method;
                if (tar is WeakWrapper wrapper)
                {
                    (_, tar, method) = wrapper.GetDelegateInfo();
                }
                List<WeakWrapper> newlist = null;
                int removedcnt = 0;
                for (int i = 0; i < _Listeners.Count; ++i)
                {
                    (var alive, var otar, var omethod) = _Listeners[i].GetDelegateInfo();
                    if (!alive)
                    {
                        if (newlist == null)
                        {
                            newlist = new List<WeakWrapper>(_Listeners.Capacity);
                            newlist.AddRange(_Listeners);
                        }
                        newlist.RemoveAt(i - removedcnt);
                        ++removedcnt;
                        continue;
                    }
                    else if (otar == tar && omethod == method)
                    {
                        if (newlist == null)
                        {
                            newlist = new List<WeakWrapper>(_Listeners.Capacity);
                            newlist.AddRange(_Listeners);
                        }
                        newlist.RemoveAt(i - removedcnt);
                        break;
                    }
                }
                if (newlist != null)
                {
                    _Listeners = newlist;
                }
                RemoveDead(newlist != null);
            }
            internal void RemoveListener(WeakWrapper wrapper)
            {
                List<WeakWrapper> newlist = null;
                for (int i = 0; i < _Listeners.Count; ++i)
                {
                    if (_Listeners[i] == wrapper)
                    {
                        //if (newlist == null)
                        {
                            newlist = new List<WeakWrapper>(_Listeners.Capacity);
                            newlist.AddRange(_Listeners);
                        }
                        newlist.RemoveAt(i);
                        break;
                    }
                }
                if (newlist != null)
                {
                    _Listeners = newlist;
                }
                RemoveDead(newlist != null);
            }
        }
        public class WeakEvent<R> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R>(handler);
            }
            public void AddListener(Action act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent()
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R> wrapper = listeners[i] as WeakWrapper<R>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn();
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1>(handler);
            }
            public void AddListener(Action<U1> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1> wrapper = listeners[i] as WeakWrapper<R, U1>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2>(handler);
            }
            public void AddListener(Action<U1, U2> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2> wrapper = listeners[i] as WeakWrapper<R, U1, U2>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3>(handler);
            }
            public void AddListener(Action<U1, U2, U3> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7, U8> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7, U8> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, U8, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7, u8);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7, u8, u9);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
        public class WeakEvent<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : WeakEvent
        {
            protected static bool _ShouldReturn = typeof(R) != typeof(VoidReturn);
            internal override WeakWrapper Wrap(Delegate handler)
            {
                return new WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(handler);
            }
            public void AddListener(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> act)
            {
                AddListener((Delegate)act);
            }
            public void AddListener(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> func)
            {
                AddListener((Delegate)func);
            }
            public R TrigEvent(U1 u1, U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15, U16 u16)
            {
                RemoveDead();
                R rv = default(R);
                var listeners = _Listeners;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> wrapper = listeners[i] as WeakWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>;
                    if (wrapper != null)
                    {
                        try
                        {
                            if (_ShouldReturn)
                            {
                                wrapper.TryInvoke(ref rv, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                            }
                            else
                            {
                                wrapper.InvokeNoReturn(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                            }
                        }
                        catch (WeakRefDeadException)
                        {
                            RemoveListener(wrapper);
                        }
                    }
                }
                return rv;
            }
        }
    }
}
