namespace Tjs2.Sharpen
{
    internal class AccessController
	{
		public static T DoPrivileged<T> (PrivilegedAction<T> action)
		{
			return action.Run ();
		}
	}
}
