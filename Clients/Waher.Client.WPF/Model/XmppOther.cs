using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using Waher.Networking.XMPP;
using Waher.Content.Markdown;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents an unspecialized XMPP contact.
	/// </summary>
	public class XmppOther : XmppContact
	{
		public XmppOther(TreeNode Parent, XmppClient Client, string BareJid)
			: base(Parent, Client, BareJid)
		{
		}

		public override string TypeName
		{
			get { return "Other"; }
		}

		public override bool CanReadSensorData
		{
			get
			{
				XmppAccountNode Node = this.XmppAccountNode;
				if (Node == null)
					return false;

				XmppClient Client = Node.Client;
				RosterItem Item = Client[this.BareJID];

				return Item != null && Item.HasLastPresence && Item.LastPresence.IsOnline;
			}
		}

		public override SensorDataClientRequest StartSensorDataFullReadout()
		{
			return this.DoReadout(Things.SensorData.FieldType.All);
		}

		public override SensorDataClientRequest StartSensorDataMomentaryReadout()
		{
			return this.DoReadout(Things.SensorData.FieldType.Momentary);
		}

		private SensorDataClientRequest DoReadout(Things.SensorData.FieldType Types)
		{
			XmppClient Client = this.XmppAccountNode.Client;
			string Id = Guid.NewGuid().ToString();

			RosterItem Item = Client[this.BareJID];
			string Jid = Item.LastPresenceFullJid;

			CustomSensorDataClientRequest Request = new CustomSensorDataClientRequest(Id, string.Empty, string.Empty, null,
				Types, null, DateTime.MinValue, DateTime.MaxValue, DateTime.Now, string.Empty, string.Empty, string.Empty);

			Request.Accept(false);
			Request.Started();

			Client.SendServiceDiscoveryRequest(Jid, (sender, e) =>
			{
				if (e.Ok)
				{
					List<Things.SensorData.Field> Fields = new List<Things.SensorData.Field>();
					DateTime Now = DateTime.Now;

					foreach (KeyValuePair<string, bool> Feature in e.Features)
					{
						Fields.Add(new Things.SensorData.BooleanField(Things.ThingReference.Empty, Now,
							Feature.Key, Feature.Value, Things.SensorData.FieldType.Momentary, Things.SensorData.FieldQoS.AutomaticReadout));
					}

					bool VersionDone = false;

					if ((Types & Things.SensorData.FieldType.Identity) != 0)
					{
						foreach (Identity Identity in e.Identities)
						{
							Fields.Add(new Things.SensorData.StringField(Things.ThingReference.Empty, Now,
								Identity.Type, Identity.Category + (string.IsNullOrEmpty(Identity.Name) ? string.Empty : " (" + Identity.Name + ")"),
								Things.SensorData.FieldType.Identity,
								Things.SensorData.FieldQoS.AutomaticReadout));
						}

						if (e.HasFeature(XmppClient.NamespaceSoftwareVersion))
						{
							Client.SendSoftwareVersionRequest(Jid, (sender2, e2) =>
							{
								Now = DateTime.Now;

								if (e2.Ok)
								{
									Request.LogFields(new Things.SensorData.Field[]
									{
										new Things.SensorData.StringField(Things.ThingReference.Empty, Now, "Client, Name", e2.Name,
											Things.SensorData.FieldType.Identity, Things.SensorData.FieldQoS.AutomaticReadout),
										new Things.SensorData.StringField(Things.ThingReference.Empty, Now, "Client, OS", e2.OS,
											Things.SensorData.FieldType.Identity, Things.SensorData.FieldQoS.AutomaticReadout),
										new Things.SensorData.StringField(Things.ThingReference.Empty, Now, "Client, Version", e2.Version,
											Things.SensorData.FieldType.Identity, Things.SensorData.FieldQoS.AutomaticReadout),
									});
								}
								else
								{
									Request.LogErrors(new Things.ThingError[]
									{
										new Things.ThingError(Things.ThingReference.Empty, Now, "Unable to read software version.")
									});
								}

								VersionDone = true;

								if (VersionDone)
									Request.Done();

							}, null);
						}
						else
							VersionDone = true;
					}
					else
						VersionDone = true;

					Request.LogFields(Fields);

					if (VersionDone)
						Request.Done();
				}
				else
					Request.Fail("Unable to perform a service discovery.");
			}, null);

			return Request;
		}

	}
}
