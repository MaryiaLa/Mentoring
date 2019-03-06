using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessingQueue
{
	/// <summary>
	/// Implements <see cref="IProcessingQueue" /> using custom user threads: <see cref="System.Threading.Tasks.Task" />.
	/// More requirements are written for the interface.
	/// </summary>
	internal class TaskQueue : IProcessingQueue
	{
		private Dictionary<int, Task> tasks;
		private static Semaphore semaphore;

		/// <summary>
		/// The constructior.
		/// </summary>
		public TaskQueue()
		{
			tasks = new Dictionary<int, Task>();
			semaphore = new Semaphore(5, 5);
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
				Task task = new Task(() =>
				{
					semaphore.WaitOne();
					itemToProcess();
					semaphore.Release();
				});
				tasks.Add(id, task);
				task.Start();
			}
			else
			{
				if (tasks.TryGetValue(dependsOnId.Value, out var taskToDependOn))
				{
					Task task = taskToDependOn.ContinueWith(t =>
					{
						semaphore.WaitOne();
						itemToProcess();
						semaphore.Release();
					});
					tasks.Add(id, task);
				}
			}
		}

		/// <summary>
		/// Waits till all actions will be processed completely.
		/// </summary>
		public void WaitAll()
		{
			Task.WaitAll(tasks.Values.ToArray());
		}
	}
}
