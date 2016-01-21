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
	/// Represents a simple XMPP sensor.
	/// </summary>
	public class XmppSensor : XmppContact
	{
		public XmppSensor(TreeNode Parent, RosterItem RosterItem)
			: base(Parent, RosterItem)
		{
		}

		public override string TypeName
		{
			get { return "Sensor"; }
		}

		public override bool CanReadSensorData
		{
			get
			{
				return true;
			}
		}

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

	}
}
