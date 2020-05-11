using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Sensor;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Model.Things
{
	/// <summary>
	/// Represents a simple XMPP actuator.
	/// </summary>
	public class XmppActuator : XmppContact
	{
		private readonly bool isSensor;
		private readonly bool suportsEvents;

		public XmppActuator(TreeNode Parent, XmppClient Client, string BareJid, bool IsSensor, bool SupportsEventSubscripton)
			: base(Parent, Client, BareJid)
		{
			this.isSensor = IsSensor;
			this.suportsEvents = SupportsEventSubscripton;
		}

		public override string TypeName
		{
			get { return "Actuator"; }
		}

		public override bool CanReadSensorData => this.isSensor;
		public override bool CanSubscribeToSensorData => this.suportsEvents;

		public override SensorDataClientRequest StartSensorDataMomentaryReadout()
		{
			if (this.isSensor)
			{
				XmppAccountNode XmppAccountNode = this.XmppAccountNode;
				SensorClient SensorClient;

				if (XmppAccountNode != null && (SensorClient = XmppAccountNode.SensorClient) != null)
					return SensorClient.RequestReadout(this.RosterItem.LastPresenceFullJid, FieldType.Momentary);
				else
					return null;
			}
			else
				throw new NotSupportedException();
		}

		public override SensorDataClientRequest StartSensorDataFullReadout()
		{
			if (this.isSensor)
			{
				XmppAccountNode XmppAccountNode = this.XmppAccountNode;
				SensorClient SensorClient;

				if (XmppAccountNode != null && (SensorClient = XmppAccountNode.SensorClient) != null)
					return SensorClient.RequestReadout(this.RosterItem.LastPresenceFullJid, FieldType.All);
				else
					return null;
			}
			else
				throw new NotSupportedException();
		}

		public override SensorDataSubscriptionRequest SubscribeSensorDataMomentaryReadout(FieldSubscriptionRule[] Rules)
		{
			if (this.isSensor)
			{
				XmppAccountNode XmppAccountNode = this.XmppAccountNode;
				SensorClient SensorClient;

				if (XmppAccountNode != null && (SensorClient = XmppAccountNode.SensorClient) != null)
				{
					return SensorClient.Subscribe(this.RosterItem.LastPresenceFullJid, FieldType.Momentary, Rules,
						Duration.FromSeconds(1), Duration.FromMinutes(1), false);
				}
				else
					return null;
			}
			else
				throw new NotSupportedException();
		}

		public override bool CanConfigure => true;

		public override void GetConfigurationForm(DataFormResultEventHandler Callback, object State)
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			ControlClient ControlClient;

			if (XmppAccountNode != null && (ControlClient = XmppAccountNode.ControlClient) != null)
				ControlClient.GetForm(this.RosterItem.LastPresenceFullJid, "en", Callback, State);
			else
				throw new NotSupportedException();
		}

	}
}
