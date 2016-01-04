using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Sensor;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents a simple XMPP actuator.
	/// </summary>
	public class XmppActuator : XmppContact 
	{
		private bool isSensor;

		public XmppActuator(TreeNode Parent, RosterItem RosterItem, bool IsSensor)
			: base(Parent, RosterItem)
		{
			this.isSensor = IsSensor;
		}

		public override string TypeName
		{
			get { return "Actuator"; }
		}

		public override bool CanReadSensorData
		{
			get
			{
				return this.isSensor;
			}
		}

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

	}
}
