using System;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Sensor;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Model.Things
{
	/// <summary>
	/// Represents a simple XMPP sensor.
	/// </summary>
	public class XmppSensor : XmppContact
	{
		private readonly bool suportsEvents;

		public XmppSensor(TreeNode Parent, XmppClient Client, string BareJid, bool SupportsEventSubscripton, bool SupportsRdp)
			: base(Parent, Client, BareJid, SupportsRdp)
		{
			this.suportsEvents = SupportsEventSubscripton;
		}

		public override string TypeName
		{
			get { return "Sensor"; }
		}

		public override bool CanReadSensorData => true;
		public override bool CanSubscribeToSensorData => this.suportsEvents;

		public override async Task<SensorDataClientRequest> StartSensorDataMomentaryReadout()
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			SensorClient SensorClient;

			if (XmppAccountNode is not null && !((SensorClient = XmppAccountNode.SensorClient) is null))
				return await SensorClient.RequestReadout(this.RosterItem.LastPresenceFullJid, FieldType.Momentary);
			else
				return null;
		}

		public override async Task<SensorDataClientRequest> StartSensorDataFullReadout()
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			SensorClient SensorClient;

			if (XmppAccountNode is not null && !((SensorClient = XmppAccountNode.SensorClient) is null))
				return await SensorClient.RequestReadout(this.RosterItem.LastPresenceFullJid, FieldType.All);
			else
				throw new NotSupportedException();
		}

		public override async Task<SensorDataSubscriptionRequest> SubscribeSensorDataMomentaryReadout(FieldSubscriptionRule[] Rules)
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			SensorClient SensorClient;

			if (XmppAccountNode is not null && !((SensorClient = XmppAccountNode.SensorClient) is null))
			{
				return await SensorClient.Subscribe(this.RosterItem.LastPresenceFullJid, FieldType.Momentary, Rules,
					Duration.FromSeconds(1), Duration.FromMinutes(1), false);
			}
			else
				return null;
		}

	}
}
