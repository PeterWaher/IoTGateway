using System.Threading.Tasks;
using Waher.Jobs.NodeTypes;
using Waher.Runtime.Inventory;
using Waher.Runtime.Timing;
using Waher.Things;

namespace Waher.Jobs
{
	/// <summary>
	/// Module that manages the scheduling and execution of jobs.
	/// </summary>
	public class JobScheduler : IModule
	{
		private static Scheduler scheduler = null;
		private bool disposeScheduler = false;

		/// <summary>
		/// Module that manages the scheduling and execution of jobs.
		/// </summary>
		public JobScheduler()
		{
		}

		/// <summary>
		/// Scheduler used to schedule jobs.
		/// </summary>
		public static Scheduler Scheduler => scheduler;

		/// <summary>
		/// Starts the module.
		/// </summary>
		public async Task Start()
		{
			if (scheduler is null)
			{
				if (Types.TryGetModuleParameter("Scheduler", out scheduler))
					this.disposeScheduler = false;
				else
				{
					scheduler = new Scheduler();
					this.disposeScheduler = true;
				}
			}

			foreach (INode Node in await JobSource.Root.ChildNodes)
			{
				if (Node is Job Job)
					Job.RescheduleJob();
			}
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			if (this.disposeScheduler)
				scheduler?.Dispose();

			scheduler = null;

			return Task.CompletedTask;
		}
	}
}
