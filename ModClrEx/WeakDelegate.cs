using System;
using System.Collections.Generic;
using System.Text;

namespace Mod
{
    public abstract class WeakDelegate : IDisposable
    {
        public WeakDelegate(Delegate del)
        {
        }

        public void Dispose()
        {
        }
    }

    public class WeakDelegate<T>
    {
        public WeakDelegate(Delegate del)
        {
        }
        public WeakDelegate(Action<T> action)
        {

        }

        public void Invoke(T arg)
        {
        }

        public static implicit operator Action<T>(WeakDelegate<T> action)
        {
            return action.Invoke;
        }
    }

}
