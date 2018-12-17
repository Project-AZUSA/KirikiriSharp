namespace Tjs2.Sharpen
{
    internal interface ExecutorService : Executor
	{
		bool AwaitTermination (long n, TimeUnit unit);
		void Shutdown ();
		Future<T> Submit<T> (Callable<T> ob);
	}
}
