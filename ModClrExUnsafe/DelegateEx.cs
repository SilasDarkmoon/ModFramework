using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Mod.LowLevel
{
    public static class DelegateEx
    {
    }

    public interface IUnsafeDelegate
    {
        void Bind(MethodInfo method);
        MethodInfo Method { get; }
    }
    public class UnsafeAction : IUnsafeDelegate
    {
        public MethodInfo Method => throw new NotImplementedException();

        public void Bind(MethodInfo method)
        {
            throw new NotImplementedException();
        }
    }
}
