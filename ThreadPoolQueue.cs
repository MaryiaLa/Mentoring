using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessingQueue
{
	/// <summary>
	/// Implements <see cref="IProcessingQueue" /> using custom user threads: <see cref="System.Threading.ThreadPool" />.
	/// More requirements are written for the interface.
	/// </summary>
	internal class ThreadPoolQueue : IProcessingQueue
	{
		private Dictionary<int, WaitHandle> handlers;

		/// <summary>
		/// The constructor.
		/// </summary>
		public ThreadPoolQueue()
		{
			handlers = new Dictionary<int, WaitHandle>();
			ThreadPool.SetMinThreads(5, 0);
			ThreadPool.SetMaxThreads(5, 0);
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

				ThreadPool.QueueUserWorkItem(state =>
				{
					itemToProcess();
					handle.Set();
				});
			}
			else
			{
				if (handlers.TryGetValue(dependsOnId.Value, out var handlerToDependOn))
				{
					var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
					handlers.Add(id, handle);

					ThreadPool.QueueUserWorkItem(state =>
					{
						handlerToDependOn.WaitOne();
						itemToProcess();
						handle.Set();
					});
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
