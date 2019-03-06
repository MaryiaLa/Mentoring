using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessingQueue
{
	class Program
	{
		static void Main(string[] args)
		{
			var queue = CreateThreadPoolQueue(); // CreateThreadQueue(); // CreateTaskQueue();
			TestQueue(queue);
			Console.ReadLine();
		}

		private static IProcessingQueue CreateThreadQueue()
		{
			return new ThreadQueue();
		}

		private static IProcessingQueue CreateThreadPoolQueue()
		{
			return new ThreadPoolQueue();
		}

		private static IProcessingQueue CreateTaskQueue()
		{
			return new TaskQueue();
		}

		/// <summary>
		/// Tests the queue. It's just an example. Real test will take into account dependencies and other requirements
		/// </summary>
		/// <param name="queue">The queue.</param>
		private static void TestQueue(IProcessingQueue queue)
		{
			Stopwatch timer = new Stopwatch();
			timer.Start();
			try
			{
				queue.Enqueue(1, null, () => { Console.WriteLine("start task 1"); Thread.Sleep(1000); Console.WriteLine("end task 1"); });
				queue.Enqueue(2, null, () => { Console.WriteLine("start task 2"); Thread.Sleep(1000); Console.WriteLine("end task 2"); });
				queue.Enqueue(3, null, () => { Console.WriteLine("start task 3"); Thread.Sleep(1000); Console.WriteLine("end task 3"); });
				queue.Enqueue(4, null, () => { Console.WriteLine("start task 4"); Thread.Sleep(1000); Console.WriteLine("end task 4"); });
				queue.Enqueue(5, null, () => { Console.WriteLine("start task 5"); Thread.Sleep(1000); Console.WriteLine("end task 5"); });
				queue.Enqueue(6, null, () => { Console.WriteLine("start task 6"); Thread.Sleep(1000); Console.WriteLine("end task 6"); });

				// Dependent actions below: 7 depends on 6, 8 on 6, and 9 on 7.
				queue.Enqueue(7, 6, () => { Console.WriteLine("start task 7"); Thread.Sleep(1000); Console.WriteLine("end task 7"); });
				queue.Enqueue(8, 6, () => { Console.WriteLine("start task 8"); Thread.Sleep(1000); Console.WriteLine("end task 8"); });
				queue.Enqueue(9, 7, () => { Console.WriteLine("start task 9"); Thread.Sleep(1000); Console.WriteLine("end task 9"); });
				queue.WaitAll();
				Console.WriteLine("NO EXCEPTION");
			}
			catch (Exception ex)
			{
				Console.WriteLine("EXCEPTION: " + ex.Message);
			}
			finally
			{
				timer.Stop();
				Console.WriteLine("Elapsed time: " + timer.Elapsed);
			}
		}
	}
}
