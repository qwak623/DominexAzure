namespace Utils;

// todo do i need this - revisit this design
public static class ThreadSafeRandom
{
	private static readonly Random _global = new Random();
	[ThreadStatic] private static Random _local;

	static ThreadSafeRandom()
	{
		//_local = _global;
		//return; // zdeterministicteni

		if (_local == null)
		{
			lock (_global)
			{
				if (_local == null)
				{
					int seed = _global.Next();
					_local = new Random(seed);
				}
			}
		}
	}

	public static int Next() => _local.Next();

	public static int Next(int n) => _local.Next(n);

	public static int NextSign() => _local.Next(2) * 2 - 1;

	public static double NextDouble() => _local.NextDouble();
}