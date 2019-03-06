using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingQueue
{
	/// <summary>
	/// Describes processing queue that:
	/// <para>- processes each item on another thread (not calling): user thread, threadpool and so on</para>
	/// <para>- uses not more that 5 threads at a time. Thus max 5 actions can be being processed simultaneously </para>
	/// <para>- dependent actions should be processed after the action they depend on. I.e., if dependsOnId = 3, then the action should be exceucted after the action with id = 3.</para>
	/// </summary>
	internal interface IProcessingQueue
	{
		/// <summary>
		/// Adds action that should be processed on a separate thread
		/// </summary>
		/// <param name="id">The identifier of the action.</param>
		/// <param name="dependsOnId">If not null then it's the id of the action that this action depends on. Null means no dependency. The action with Id = dependsOnId has been already added to the queue (if not then ignore the dependency). </param>
		/// <param name="itemToProcess">The action to process. Time-consuming. Uses CPU.</param>
		void Enqueue(int id, int? dependsOnId, Action itemToProcess);

		/// <summary>
		/// Waits till all actions will be processed completely.
		/// </summary>
		void WaitAll();
	}
}
