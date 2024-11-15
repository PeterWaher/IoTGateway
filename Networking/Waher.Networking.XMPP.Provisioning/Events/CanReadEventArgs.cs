using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Provisioning.Events
{
	/// <summary>
	/// Event arguments for CanRead events.
	/// </summary>
	public class CanReadEventArgs : NodeQuestionEventArgs
	{
		private readonly ProvisioningClient provisioningClient;
		private readonly FieldType fieldTypes;
		private readonly string[] fields;

		/// <summary>
		/// Event arguments for CanRead events.
		/// </summary>
		/// <param name="ProvisioningClient">XMPP Provisioning Client used.</param>
		/// <param name="e">Message with request.</param>
		public CanReadEventArgs(ProvisioningClient ProvisioningClient, MessageEventArgs e)
			: base(ProvisioningClient, e)
		{
			this.provisioningClient = ProvisioningClient;
			this.fieldTypes = (FieldType)0;

			foreach (XmlAttribute Attr in e.Content.Attributes)
			{
				switch (Attr.LocalName)
				{
					case "all":
						if (CommonTypes.TryParse(Attr.Value, out bool b) && b)
							this.fieldTypes |= FieldType.All;
						break;

					case "m":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							this.fieldTypes |= FieldType.Momentary;
						break;

					case "p":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							this.fieldTypes |= FieldType.Peak;
						break;

					case "s":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							this.fieldTypes |= FieldType.Status;
						break;

					case "c":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							this.fieldTypes |= FieldType.Computed;
						break;

					case "i":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							this.fieldTypes |= FieldType.Identity;
						break;

					case "h":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							this.fieldTypes |= FieldType.Historical;
						break;
				}
			}

			List<string> Fields = null;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "f")
				{
					if (Fields is null)
						Fields = new List<string>();

					Fields.Add(XML.Attribute(E, "n"));
				}
			}

			if (!(Fields is null))
				this.fields = Fields.ToArray();
			else
				this.fields = null;
		}

		/// <summary>
		/// Provisioning client
		/// </summary>
		public ProvisioningClient ProvisioningClient => this.provisioningClient;

		/// <summary>
		/// Any field specifications of the original request. If null, all fields are requested.
		/// </summary>
		public string[] Fields => this.fields;

		/// <summary>
		/// Any field types available in the original request.
		/// </summary>
		public FieldType FieldTypes => this.fieldTypes;

		/// <summary>
		/// Accept readouts from this remote JID.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptAllFromJID(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, "<fromJid/>", Callback, State);
		}

		/// <summary>
		/// Accept readouts from all entities of the remote domain.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptAllFromDomain(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, "<fromDomain/>", Callback, State);
		}

		/// <summary>
		/// Accept readouts from this service.
		/// </summary>
		/// <param name="ServiceToken">Which service token is to be accepted.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptAllFromService(string ServiceToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, "<fromService token='" +XML.Encode(ServiceToken) + "'/>", Callback, State);
		}

		/// <summary>
		/// Accept readouts from this device.
		/// </summary>
		/// <param name="DeviceToken">Which device token is to be accepted.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptAllFromDevice(string DeviceToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, "<fromDevice token='" + XML.Encode(DeviceToken) + "'/>", Callback, State);
		}

		/// <summary>
		/// Accept readouts from this user.
		/// </summary>
		/// <param name="UserToken">Which user token is to be accepted.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptAllFromUser(string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, "<fromUser token='" + XML.Encode(UserToken) + "'/>", Callback, State);
		}

		/// <summary>
		/// Accept partial readouts from this remote JID.
		/// </summary>
		/// <param name="Fields">Fields that can be read.</param>
		/// <param name="FieldTypes">Field categories that can be read.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromJID(string[] Fields, FieldType FieldTypes, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialFields(Fields, FieldTypes) + "<fromJid/>", Callback, State);
		}

		/// <summary>
		/// Accept partial readouts from all entities of the remote domain.
		/// </summary>
		/// <param name="Fields">Fields that can be read.</param>
		/// <param name="FieldTypes">Field categories that can be read.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromDomain(string[] Fields, FieldType FieldTypes, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialFields(Fields, FieldTypes) + "<fromDomain/>", Callback, State);
		}

		/// <summary>
		/// Accept partial readouts from this service.
		/// </summary>
		/// <param name="Fields">Fields that can be read.</param>
		/// <param name="FieldTypes">Field categories that can be read.</param>
		/// <param name="ServiceToken">Which service token is to be accepted.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromService(string[] Fields, FieldType FieldTypes, string ServiceToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialFields(Fields, FieldTypes) + "<fromService token='" + XML.Encode(ServiceToken) + "'/>", Callback, State);
		}

		/// <summary>
		/// Accept partial readouts from this device.
		/// </summary>
		/// <param name="Fields">Fields that can be read.</param>
		/// <param name="FieldTypes">Field categories that can be read.</param>
		/// <param name="DeviceToken">Which device token is to be accepted.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromDevice(string[] Fields, FieldType FieldTypes, string DeviceToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialFields(Fields, FieldTypes) + "<fromDevice token='" + XML.Encode(DeviceToken) + "'/>", Callback, State);
		}

		/// <summary>
		/// Accept partial readouts from this user.
		/// </summary>
		/// <param name="Fields">Fields that can be read.</param>
		/// <param name="FieldTypes">Field categories that can be read.</param>
		/// <param name="UserToken">Which user token is to be accepted.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromUser(string[] Fields, FieldType FieldTypes, string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialFields(Fields, FieldTypes) + "<fromUser token='" + XML.Encode(UserToken) + "'/>", Callback, State);
		}

		private string PartialFields(string[] Names, FieldType FieldTypes)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<partial");

			if (FieldTypes == FieldType.All)
				Xml.Append(" all='true'");
			else
			{
				if ((FieldTypes & FieldType.Momentary) != 0)
					Xml.Append(" m='true'");

				if ((FieldTypes & FieldType.Identity) != 0)
					Xml.Append(" i='true'");

				if ((FieldTypes & FieldType.Status) != 0)
					Xml.Append(" s='true'");

				if ((FieldTypes & FieldType.Computed) != 0)
					Xml.Append(" c='true'");

				if ((FieldTypes & FieldType.Peak) != 0)
					Xml.Append(" p='true'");

				if ((FieldTypes & FieldType.Historical) != 0)
					Xml.Append(" h='true'");
			}

			Xml.Append(">");

			foreach (string Name in Names)
			{
				Xml.Append("<f n='");
				Xml.Append(XML.Encode(Name));
				Xml.Append("</f>");
			}

			Xml.Append("</partial>");

			return Xml.ToString();
		}

		/// <summary>
		/// Reject readouts from this remote JID.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> RejectAllFromJID(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, "<fromJid/>", Callback, State);
		}

		/// <summary>
		/// Reject readouts from all entities of the remote domain.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> RejectAllFromDomain(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, "<fromDomain/>", Callback, State);
		}

		/// <summary>
		/// Reject readouts from this service.
		/// </summary>
		/// <param name="ServiceToken">Which service token is to be rejected.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> RejectAllFromService(string ServiceToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, "<fromService token='" + XML.Encode(ServiceToken) + "'/>", Callback, State);
		}

		/// <summary>
		/// Reject readouts from this device.
		/// </summary>
		/// <param name="DeviceToken">Which device token is to be rejected.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> RejectAllFromDevice(string DeviceToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, "<fromDevice token='" + XML.Encode(DeviceToken) + "'/>", Callback, State);
		}

		/// <summary>
		/// Reject readouts from this user.
		/// </summary>
		/// <param name="UserToken">Which user token is to be rejected.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> RejectAllFromUser(string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, "<fromUser token='" + XML.Encode(UserToken) + "'/>", Callback, State);
		}

		private async Task<bool> Respond(bool CanRead, string RuleXml, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<canReadRule xmlns='");
			Xml.Append(ProvisioningClient.NamespaceProvisioningOwnerCurrent);
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(this.JID));
			Xml.Append("' remoteJid='");
			Xml.Append(XML.Encode(this.RemoteJID));
			Xml.Append("' key='");
			Xml.Append(XML.Encode(this.Key));
			Xml.Append("' result='");
			Xml.Append(CommonTypes.Encode(CanRead));
			Xml.Append("'>");
			Xml.Append(RuleXml);
			Xml.Append("</canRaedRule>");

			RosterItem Item = this.Client.Client[this.FromBareJID];
			if (Item.HasLastPresence && Item.LastPresence.IsOnline)
			{
				await this.Client.Client.SendIqSet(Item.LastPresenceFullJid, Xml.ToString(), Callback, State);
				return true;
			}
			else
				return false;
		}

	}
}
