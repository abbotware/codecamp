<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

public class Record
{
	public long Version { get; set; }
	public long Sub { get; set; }
	public long Add { get; set; }
}

void Main()
{
	var producer = 1;
	var consumer = 3;
	var MAX = 10000000;
	var queue = new int[MAX];
	var database = new Record();

	//Spin Up Consumer(s)
	Parallel.For(0, consumer, (i) =>
	{
		Task.Factory.StartNew(() =>
		{
			var id = i;
			var count = i;
			var step = consumer;
			var location = count % MAX;
			var work = 0;
			
			while (true)
			{
				//Wait for Item to work on!
				do
				{
					work = Volatile.Read(ref queue[location]);
				} while (work == 0);
	
				lock (database)
				{
					database.Add = database.Add + work;
					database.Sub = database.Sub - work;
					database.Version = database.Version + 1;
				}

				count += step;
				location = count % MAX;
			}
		});

	});

	//Spin Up producer(s)
	Parallel.For(0, producer, (i) =>
	{
		Task.Factory.StartNew(() =>
		{
			var id = i;
			var count = i;
			var step = producer;
			var location = count % MAX;
			var rng = new Random();

			while (true)
			{
				if (Volatile.Read(ref queue[location]) != 0)
				{
					//$"[{id}] Queue Full".Dump();
					//Thread.Sleep(2000);
				}

				queue[location] = rng.Next();

				count += step;
				location = count % MAX;
			}
		});
	});

	while (true)
	{
		Thread.Sleep(1000);
		var current = Volatile.Read(ref database);
		$"{current.Version}  {current.Add} == {current.Sub}".Dump();
	}

}