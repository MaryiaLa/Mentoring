using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessingQueue
{
	/// <summary>
	/// Implements <see cref="IProcessingQueue" /> using custom user threads: <see cref="System.Threading.Thread" />.
	/// More requirements are written for the interface.
	/// </summary>
	internal class ThreadQueue : IProcessingQueue
	{
		private Dictionary<int, Thread> threads;
		private static Semaphore semaphore;
		private Dictionary<int, WaitHandle> handlers;

		/// <summary>
		/// The constructor.
		/// </summary>
		public ThreadQueue()
		{
			threads = new Dictionary<int, Thread>();
			semaphore = new Semaphore(5, 5);
			handlers = new Dictionary<int, WaitHandle>();
		}

		/// <summary>
		/// Adds action that should be processed on a separate thread
		/// </summary>
		/// <param name="id">The identifier of the action.</param>
		/// <param name="dependsOnId">If not null then it's the id of the action that this action depends on. Null means no dependency. The action with Id = dependsOnId has been already added to the queue (if not then ignore the dependency). </param>
		/// <param name="itemToProcess">The action to process. Time-consuming. Uses CPU.</param>
		public void Enqueue(int id, int? dependsOnId, Action itemToProcess)
		{
			if (dependsOnId == null)
			{
				var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
				handlers.Add(id, handle);

				Thread thread = new Thread(() =>
				{
					semaphore.WaitOne();
					itemToProcess();
					semaphore.Release();
					handle.Set();
				});
				threads.Add(id, thread);
				thread.Start();
			}
			else
			{
				if (threads.TryGetValue(dependsOnId.Value, out var threadToDependOn) && handlers.TryGetValue(dependsOnId.Value, out var handlerToDependOn))
				{
					var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
					handlers.Add(id, handle);

					Thread thread = new Thread(() =>
					{
						handlerToDependOn.WaitOne();
						semaphore.WaitOne();
						itemToProcess();
						semaphore.Release();
						handle.Set();
					});
					threads.Add(id, thread);
					thread.Start();
				}
			}
		}

		/// <summary>
		/// Waits till all actions will be processed completely.
		/// </summary>
		public void WaitAll()
		{
			WaitHandle.WaitAll(handlers.Values.ToArray());
		}
	}
}
