using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Provisioning.Events
{
	/// <summary>
	/// Event arguments for CanControl events.
	/// </summary>
	public class CanControlEventArgs : NodeQuestionEventArgs
	{
		private readonly ProvisioningClient provisioningClient;
		private readonly string[] parameters;

		/// <summary>
		/// Event arguments for CanControl events.
		/// </summary>
		/// <param name="ProvisioningClient">XMPP Provisioning Client used.</param>
		/// <param name="e">Message with request.</param>
		public CanControlEventArgs(ProvisioningClient ProvisioningClient, MessageEventArgs e)
			: base(ProvisioningClient, e)
		{
			this.provisioningClient = ProvisioningClient;
			List<string> Parameters = null;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "p")
				{
					if (Parameters is null)
						Parameters = new List<string>();

					Parameters.Add(XML.Attribute(E, "n"));
				}
			}

			if (!(Parameters is null))
				this.parameters = Parameters.ToArray();
		}

		/// <summary>
		/// Provisioning client
		/// </summary>
		public ProvisioningClient ProvisioningClient => this.provisioningClient;

		/// <summary>
		/// Any parameter name specifications of the original request.
		/// </summary>
		public string[] Parameters => this.parameters;

		/// <summary>
		/// Accept control operations from this remote JID.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptAllFromJID(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, "<fromJid/>", Callback, State);
		}

		/// <summary>
		/// Accept control operations from all entities of the remote domain.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptAllFromDomain(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, "<fromDomain/>", Callback, State);
		}

		/// <summary>
		/// Accept control operations from this service.
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
		/// Accept control operations from this device.
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
		/// Accept control operations from this user.
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
		/// Accept partial control operations from this remote JID.
		/// </summary>
		/// <param name="Parameters">Parameters that can be controlled.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromJID(string[] Parameters, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialParameters(Parameters) + "<fromJid/>", Callback, State);
		}

		/// <summary>
		/// Accept partial control operations from all entities of the remote domain.
		/// </summary>
		/// <param name="Parameters">Parameters that can be controlled.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromDomain(string[] Parameters, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialParameters(Parameters) + "<fromDomain/>", Callback, State);
		}

		/// <summary>
		/// Accept partial control operations from this service.
		/// </summary>
		/// <param name="Parameters">Parameters that can be controlled.</param>
		/// <param name="ServiceToken">Which service token is to be accepted.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromService(string[] Parameters, string ServiceToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialParameters(Parameters) + "<fromService token='" + XML.Encode(ServiceToken) + "'/>", Callback, State);
		}

		/// <summary>
		/// Accept partial control operations from this device.
		/// </summary>
		/// <param name="Parameters">Parameters that can be controlled.</param>
		/// <param name="DeviceToken">Which device token is to be accepted.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromDevice(string[] Parameters, string DeviceToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialParameters(Parameters) + "<fromDevice token='" + XML.Encode(DeviceToken) + "'/>", Callback, State);
		}

		/// <summary>
		/// Accept partial control operations from this user.
		/// </summary>
		/// <param name="Parameters">Parameters that can be controlled.</param>
		/// <param name="UserToken">Which user token is to be accepted.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> AcceptPartialFromUser(string[] Parameters, string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, this.PartialParameters(Parameters) + "<fromUser token='" + XML.Encode(UserToken) + "'/>", Callback, State);
		}

		private string PartialParameters(string[] Names)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<partial>");

			foreach (string Name in Names)
			{
				Xml.Append("<p n='");
				Xml.Append(XML.Encode(Name));
				Xml.Append("</p>");
			}

			Xml.Append("</partial>");

			return Xml.ToString();
		}

		/// <summary>
		/// Reject control operations from this remote JID.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> RejectAllFromJID(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, "<fromJid/>", Callback, State);
		}

		/// <summary>
		/// Reject control operations from all entities of the remote domain.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> RejectAllFromDomain(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, "<fromDomain/>", Callback, State);
		}

		/// <summary>
		/// Reject control operations from this service.
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
		/// Reject control operations from this device.
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
		/// Reject control operations from this user.
		/// </summary>
		/// <param name="UserToken">Which user token is to be rejected.</param>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public Task<bool> RejectAllFromUser(string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, "<fromUser token='" + XML.Encode(UserToken) + "'/>", Callback, State);
		}

		private async Task<bool> Respond(bool CanControl, string RuleXml, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<canControlRule xmlns='");
			Xml.Append(ProvisioningClient.NamespaceProvisioningOwnerCurrent);
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(this.JID));
			Xml.Append("' remoteJid='");
			Xml.Append(XML.Encode(this.RemoteJID));
			Xml.Append("' key='");
			Xml.Append(XML.Encode(this.Key));
			Xml.Append("' result='");
			Xml.Append(CommonTypes.Encode(CanControl));
			Xml.Append("'>");
			Xml.Append(RuleXml);
			Xml.Append("</canControlRule>");

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
