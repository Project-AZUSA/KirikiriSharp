/*
 * TJS2 CSharp
 */

using System.Reflection;
using Tjs2.Engine;

namespace Tjs2.NativeApi.Java
{
	public class NativeJavaClassConstructor : NativeJavaClassMethod
	{
		/// <exception cref="TjsException"></exception>
		public NativeJavaClassConstructor(MethodInfo m, int classID) : base(m, classID)
		{
		}
	}
}
