namespace Tjs2.Sharpen
{
    internal interface Future<T>
	{
		bool Cancel (bool mayInterruptIfRunning);
		T Get ();
	}
}
