using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Things.Metering;
using Waher.Things.Ip;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// Module for managing XMPP nodes
	/// </summary>
	[Singleton]
	public class XmppModule : IModule
	{
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
				if (Child is XmppBrokerNode BrokerNode)
					await BrokerNode.GetBroker(); // Makes sure it is initialized.
				else if (Child is IpHost || Child is NodeCollection)
					await this.CheckNode(Child);
			}
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			return Task.CompletedTask;
		}
	}
}
