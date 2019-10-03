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
		public WaitHandle Start()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			this.InitBrokers(Done);
			return Done;
		}

		private async void InitBrokers(ManualResetEvent Done)
		{
			try
			{
				await this.CheckNode(MeteringTopology.Root);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				Done.Set();
			}
		}

		private async Task CheckNode(INode Node)
		{
			foreach (INode Child in await MeteringTopology.Root.ChildNodes)
			{
				if (Child is MqttBrokerNode BrokerNode)
					BrokerNode.GetBroker(); // Makes sure it is initialized.
				else if (Child is IpHost IpHost)
					await this.CheckNode(IpHost);
			}
		}

		public void Stop()
		{
		}
	}
}
