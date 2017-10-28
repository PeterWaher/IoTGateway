using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Sensor;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents a simple XMPP sensor.
	/// </summary>
	public class XmppSensor : XmppContact
	{
		private bool suportsEvents;

		public XmppSensor(TreeNode Parent, XmppClient Client, string BareJid, bool SupportsEventSubscripton)
			: base(Parent, Client, BareJid)
		{
			this.suportsEvents = SupportsEventSubscripton;
		}

		public override string TypeName
		{
			get { return "Sensor"; }
		}

		public override bool CanReadSensorData => true;
		public override bool CanSubscribeToSensorData => this.suportsEvents;

		public override SensorDataClientRequest StartSensorDataMomentaryReadout()
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			SensorClient SensorClient;

			if (XmppAccountNode != null && (SensorClient = XmppAccountNode.SensorClient) != null)
				return SensorClient.RequestReadout(this.RosterItem.LastPresenceFullJid, FieldType.Momentary);
			else
				return null;
		}

		public override SensorDataClientRequest StartSensorDataFullReadout()
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			SensorClient SensorClient;

			if (XmppAccountNode != null && (SensorClient = XmppAccountNode.SensorClient) != null)
				return SensorClient.RequestReadout(this.RosterItem.LastPresenceFullJid, FieldType.All);
			else
				throw new NotSupportedException();
		}

		public override SensorDataSubscriptionRequest SubscribeSensorDataMomentaryReadout(FieldSubscriptionRule[] Rules)
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			SensorClient SensorClient;

			if (XmppAccountNode != null && (SensorClient = XmppAccountNode.SensorClient) != null)
			{
				return SensorClient.Subscribe(this.RosterItem.LastPresenceFullJid, FieldType.Momentary, Rules,
					new Duration(false, 0, 0, 0, 0, 0, 1), new Duration(false, 0, 0, 0, 0, 1, 0), false);
			}
			else
				return null;
		}

	}
}
