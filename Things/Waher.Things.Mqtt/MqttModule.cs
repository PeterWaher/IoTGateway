using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Things.Metering;
using Waher.Things.Ip;

namespace Waher.Things.Mqtt
{
	public class MqttModule : IModule
	{
		public async Task Start()
		{
			try
			{
				await this.CheckNode(MeteringTopology.Root);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task CheckNode(INode Node)
		{
			foreach (INode Child in await Node.ChildNodes)
			{
				if (Child is MqttBrokerNode BrokerNode)
					BrokerNode.GetBroker(); // Makes sure it is initialized.
				else if (Child is IpHost IpHost)
					await this.CheckNode(IpHost);
			}
		}

		public Task Stop()
		{
			return Task.CompletedTask;
		}
	}
}
