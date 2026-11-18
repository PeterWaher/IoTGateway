using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Matter
{
	/// <summary>
	/// Module starting Matter controllers in the metering topology when the gateway starts, and stopping them when
	/// the gateway stops.
	/// </summary>
	[Singleton]
	public class MatterModule : IModule
	{
		/// <summary>
		/// Module starting Matter controllers in the metering topology when the gateway starts, and stopping them when
		/// the gateway stops.
		/// </summary>
		public MatterModule()
		{
		}

		/// <summary>
		/// Starts the module.
		/// </summary>
		public async Task Start()
		{
			try
			{
				await this.CheckNode(MeteringTopology.Root);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async Task CheckNode(INode Node)
		{
			foreach (INode Child in await Node.ChildNodes)
			{
				if (Child is MatterControllerNode Controller)
				{
					try
					{
						await Controller.StartAsync();
					}
					catch (Exception ex)
					{
						await Controller.LogErrorAsync("Start", "Unable to start Matter controller: " + ex.Message);
					}
				}
				else if (Child is NodeCollection)
					await this.CheckNode(Child);
			}
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			return MatterControllerNode.StopAllAsync();
		}
	}
}
