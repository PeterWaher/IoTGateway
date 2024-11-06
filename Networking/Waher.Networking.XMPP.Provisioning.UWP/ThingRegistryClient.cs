using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.Provisioning.Events;
using Waher.Networking.XMPP.Provisioning.SearchOperators;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Things;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Implements an XMPP thing registry client interface.
	/// 
	/// The interface is defined in XEP-0347:
	/// http://xmpp.org/extensions/xep-0347.html
	/// </summary>
	public class ThingRegistryClient : XmppExtension
	{
		private readonly string thingRegistryAddress;

		/// <summary>
		/// urn:ieee:iot:disco:1.0
		/// </summary>
		public const string NamespaceDiscoveryIeeeV1 = "urn:ieee:iot:disco:1.0";

		/// <summary>
		/// urn:nf:iot:disco:1.0
		/// </summary>
		public const string NamespaceDiscoveryNeuroFoundationV1 = "urn:nf:iot:disco:1.0";

		/// <summary>
		/// Current namespace for IoT Discovery
		/// </summary>
		public const string NamespaceDiscoveryCurrent = NamespaceDiscoveryNeuroFoundationV1;

		/// <summary>
		/// Namespaces supported for discovery.
		/// </summary>
		public static readonly string[] NamespacesDiscovery = new string[]
		{
			NamespaceDiscoveryIeeeV1,
			NamespaceDiscoveryNeuroFoundationV1
		};

		/// <summary>
		/// Implements an XMPP provisioning client interface.
		/// 
		/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
		/// https://neuro-foundation.io
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ThingRegistryAddress">Thing Registry XMPP address.</param>
		public ThingRegistryClient(XmppClient Client, string ThingRegistryAddress)
			: base(Client)
		{
			this.thingRegistryAddress = ThingRegistryAddress;

			#region Neuro-Foundation V1

			this.client.RegisterIqSetHandler("claimed", NamespaceDiscoveryNeuroFoundationV1, this.ClaimedHandler, true);
			this.client.RegisterIqSetHandler("removed", NamespaceDiscoveryNeuroFoundationV1, this.RemovedHandler, false);
			this.client.RegisterIqSetHandler("disowned", NamespaceDiscoveryNeuroFoundationV1, this.DisownedHandler, false);

			#endregion

			#region IEEE V1

			this.client.RegisterIqSetHandler("claimed", NamespaceDiscoveryIeeeV1, this.ClaimedHandler, true);
			this.client.RegisterIqSetHandler("removed", NamespaceDiscoveryIeeeV1, this.RemovedHandler, false);
			this.client.RegisterIqSetHandler("disowned", NamespaceDiscoveryIeeeV1, this.DisownedHandler, false);

			#endregion
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			base.Dispose();

			#region Neuro-Foundation V1

			this.client.UnregisterIqSetHandler("claimed", NamespaceDiscoveryNeuroFoundationV1, this.ClaimedHandler, true);
			this.client.UnregisterIqSetHandler("removed", NamespaceDiscoveryNeuroFoundationV1, this.RemovedHandler, false);
			this.client.UnregisterIqSetHandler("disowned", NamespaceDiscoveryNeuroFoundationV1, this.DisownedHandler, false);

			#endregion

			#region IEEE V1

			this.client.UnregisterIqSetHandler("claimed", NamespaceDiscoveryIeeeV1, this.ClaimedHandler, true);
			this.client.UnregisterIqSetHandler("removed", NamespaceDiscoveryIeeeV1, this.RemovedHandler, false);
			this.client.UnregisterIqSetHandler("disowned", NamespaceDiscoveryIeeeV1, this.DisownedHandler, false);

			#endregion
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0347" };

		/// <summary>
		/// Thing Registry XMPP address.
		/// </summary>
		public string ThingRegistryAddress => this.thingRegistryAddress;

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(MetaDataTag[], EventHandlerAsync{UpdateEventArgs}, object)"/> to update 
		/// its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task RegisterThing(MetaDataTag[] MetaDataTags, EventHandlerAsync<RegistrationEventArgs> Callback, object State)
		{
			return this.RegisterThing(false, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, MetaDataTag[], EventHandlerAsync{UpdateEventArgs}, object)"/> to 
		/// update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task RegisterThing(string NodeId, MetaDataTag[] MetaDataTags, EventHandlerAsync<RegistrationEventArgs> Callback, object State)
		{
			return this.RegisterThing(false, NodeId, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, string, MetaDataTag[], EventHandlerAsync{UpdateEventArgs}, object)"/> to 
		/// update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task RegisterThing(string NodeId, string SourceId, MetaDataTag[] MetaDataTags,
			EventHandlerAsync<RegistrationEventArgs> Callback, object State)
		{
			return this.RegisterThing(false, NodeId, SourceId, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, string, string, MetaDataTag[], EventHandlerAsync{UpdateEventArgs}, object)"/> 
		/// to update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Partition">Partition of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task RegisterThing(string NodeId, string SourceId, string Partition, MetaDataTag[] MetaDataTags,
			EventHandlerAsync<RegistrationEventArgs> Callback, object State)
		{
			return this.RegisterThing(false, NodeId, SourceId, Partition, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(MetaDataTag[], EventHandlerAsync{UpdateEventArgs}, object)"/> to 
		/// update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task RegisterThing(bool SelfOwned, MetaDataTag[] MetaDataTags, EventHandlerAsync<RegistrationEventArgs> Callback, object State)
		{
			return this.RegisterThing(SelfOwned, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, MetaDataTag[], EventHandlerAsync{UpdateEventArgs}, object)"/> 
		/// to update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task RegisterThing(bool SelfOwned, string NodeId, MetaDataTag[] MetaDataTags,
			EventHandlerAsync<RegistrationEventArgs> Callback, object State)
		{
			return this.RegisterThing(SelfOwned, NodeId, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, string, MetaDataTag[], EventHandlerAsync{UpdateEventArgs}, object)"/> 
		/// to update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task RegisterThing(bool SelfOwned, string NodeId, string SourceId, MetaDataTag[] MetaDataTags,
			EventHandlerAsync<RegistrationEventArgs> Callback, object State)
		{
			return this.RegisterThing(SelfOwned, NodeId, SourceId, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, string, string, MetaDataTag[], EventHandlerAsync{UpdateEventArgs}, object)"/> 
		/// to update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Partition">Partition of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task RegisterThing(bool SelfOwned, string NodeId, string SourceId, string Partition, MetaDataTag[] MetaDataTags,
			EventHandlerAsync<RegistrationEventArgs> Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<register xmlns='");
			Request.Append(NamespaceDiscoveryCurrent);

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			if (SelfOwned)
				Request.Append("' selfOwned='true");

			Request.Append("'>");

			string RegistryAddress = this.AddTags(Request, MetaDataTags, this.thingRegistryAddress);

			Request.Append("</register>");

			return this.client.SendIqSet(RegistryAddress, Request.ToString(), async (sender, e) =>
			{
				if (!(Callback is null))
				{
					XmlElement E = e.FirstElement;
					string OwnerJid = string.Empty;
					bool IsPublic = false;

					if (e.Ok && !(E is null) && E.LocalName == "claimed")
					{
						OwnerJid = XML.Attribute(E, "jid");
						IsPublic = XML.Attribute(E, "public", false);

						if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(Partition) &&
							this.client.TryGetExtension(out ProvisioningClient ProvisioningClient))
						{
							ProvisioningClient.OwnerJid = OwnerJid;
						}
					}

					await Callback.Raise(this, new RegistrationEventArgs(e, State, OwnerJid, IsPublic));
				}
			}, State);
		}

		private void AddNodeInfo(StringBuilder Request, string NodeId, string SourceId, string Partition)
		{
			if (!string.IsNullOrEmpty(NodeId))
			{
				Request.Append("' id='");
				Request.Append(XML.Encode(NodeId));
			}

			if (!string.IsNullOrEmpty(SourceId))
			{
				Request.Append("' src='");
				Request.Append(XML.Encode(SourceId));
			}

			if (!string.IsNullOrEmpty(Partition))
			{
				Request.Append("' pt='");
				Request.Append(XML.Encode(Partition));
			}
		}

		private string AddTags(StringBuilder Request, MetaDataTag[] MetaDataTags, string RegistryAddress)
		{
			foreach (MetaDataTag Tag in MetaDataTags)
			{
				if (Tag is MetaDataStringTag)
				{
					if (Tag.Name == "R")
						RegistryAddress = Tag.StringValue;
					else
					{
						Request.Append("<str name='");
						Request.Append(XML.Encode(Tag.Name));
						Request.Append("' value='");
						Request.Append(XML.Encode(Tag.StringValue));
						Request.Append("'/>");
					}
				}
				else if (Tag is MetaDataNumericTag)
				{
					Request.Append("<num name='");
					Request.Append(XML.Encode(Tag.Name));
					Request.Append("' value='");
					Request.Append(Tag.StringValue);
					Request.Append("'/>");
				}
			}

			return RegistryAddress;
		}

		/// <summary>
		/// Claims a thing.
		/// </summary>
		/// <param name="MetaDataTags">Meta-data describing the thing.</param>
		/// <param name="Callback">Method to call when response to claim is returned.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		public Task Mine(MetaDataTag[] MetaDataTags, EventHandlerAsync<NodeResultEventArgs> Callback, object State)
		{
			return this.Mine(true, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Claims a thing.
		/// </summary>
		/// <param name="Public">If the thing should be left as a public thing that is searchable.</param>
		/// <param name="MetaDataTags">Meta-data describing the thing.</param>
		/// <param name="Callback">Method to call when response to claim is returned.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		public Task Mine(bool Public, MetaDataTag[] MetaDataTags, EventHandlerAsync<NodeResultEventArgs> Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<mine xmlns='");
			Request.Append(NamespaceDiscoveryCurrent);
			Request.Append("' public='");
			Request.Append(CommonTypes.Encode(Public));
			Request.Append("'>");

			string RegistryAddress = this.AddTags(Request, MetaDataTags, this.thingRegistryAddress);

			Request.Append("</mine>");

			return this.client.SendIqSet(RegistryAddress, Request.ToString(), async (sender, e) =>
			{
				if (!(Callback is null))
				{
					XmlElement E = e.FirstElement;
					string NodeJid = string.Empty;
					ThingReference Node = ThingReference.Empty;

					if (e.Ok && !(E is null) && E.LocalName == "claimed")
					{
						string NodeId = XML.Attribute(E, "id");
						string SourceId = XML.Attribute(E, "src");
						string Partition = XML.Attribute(E, "pt");
						NodeJid = XML.Attribute(E, "jid");

						if (!string.IsNullOrEmpty(NodeId) || !string.IsNullOrEmpty(SourceId) || !string.IsNullOrEmpty(Partition))
							Node = new ThingReference(NodeId, SourceId, Partition);
					}

					await Callback.Raise(this, new NodeResultEventArgs(e, State, NodeJid, Node));
				}
			}, State);
		}

		private async Task ClaimedHandler(object Sender, IqEventArgs e)
		{
			XmlElement E = e.Query;
			string OwnerJid = XML.Attribute(E, "jid");
			string NodeId = XML.Attribute(E, "id");
			string SourceId = XML.Attribute(E, "src");
			string Partition = XML.Attribute(E, "pt");
			bool Public = XML.Attribute(E, "public", false);
			ThingReference Node;

			if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(Partition))
			{
				Node = ThingReference.Empty;

				if (this.client.TryGetExtension(out ProvisioningClient ProvisioningClient))
					ProvisioningClient.OwnerJid = OwnerJid;
			}
			else
				Node = new ThingReference(NodeId, SourceId, Partition);

			ClaimedEventArgs e2 = new ClaimedEventArgs(e, Node, OwnerJid, Public);
			await this.Claimed.Raise(this, e2);

			await e.IqResult(string.Empty);
		}

		/// <summary>
		/// Event raised when a node has been claimed.
		/// </summary>
		public event EventHandlerAsync<ClaimedEventArgs> Claimed = null;

		/// <summary>
		/// Removes a publicly claimed thing from the thing registry, so that it does not appear in search results.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Remove(string ThingJid, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Remove(ThingJid, string.Empty, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Removes a publicly claimed thing from the thing registry, so that it does not appear in search results.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Remove(string ThingJid, string NodeId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Remove(ThingJid, NodeId, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Removes a publicly claimed thing from the thing registry, so that it does not appear in search results.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Remove(string ThingJid, string NodeId, string SourceId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Remove(ThingJid, NodeId, SourceId, string.Empty, Callback, State);
		}

		/// <summary>
		/// Removes a publicly claimed thing from the thing registry, so that it does not appear in search results.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="Partition">Optional Partition of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Remove(string ThingJid, string NodeId, string SourceId, string Partition, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Remove(this.thingRegistryAddress, ThingJid, NodeId, SourceId, Partition, Callback, State);
		}

		/// <summary>
		/// Removes a publicly claimed thing from the thing registry, so that it does not appear in search results.
		/// </summary>
		/// <param name="RegistryJid">JID of registry service.</param>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="Partition">Optional Partition of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Remove(string RegistryJid, string ThingJid, string NodeId, string SourceId, string Partition, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<remove xmlns='");
			Request.Append(NamespaceDiscoveryCurrent);

			Request.Append("' jid='");
			Request.Append(XML.Encode(ThingJid));

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			Request.Append("'/>");

			return this.client.SendIqSet(RegistryJid, Request.ToString(), (sender, e) => Callback.Raise(this, e), State);
		}

		private async Task RemovedHandler(object Sender, IqEventArgs e)
		{
			XmlElement E = e.Query;
			string NodeId = XML.Attribute(E, "id");
			string SourceId = XML.Attribute(E, "src");
			string Partition = XML.Attribute(E, "pt");
			ThingReference Node;

			if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(Partition))
				Node = ThingReference.Empty;
			else
				Node = new ThingReference(NodeId, SourceId, Partition);

			NodeEventArgs e2 = new NodeEventArgs(e, Node);
			await this.Removed.Raise(this, e2);

			await e.IqResult(string.Empty);
		}

		/// <summary>
		/// Event raised when a node has been removed from the registry.
		/// </summary>
		public event EventHandlerAsync<NodeEventArgs> Removed = null;

		/// <summary>
		/// Updates the meta-data about a thing in the Thing Registry. Only public things that have an owner can update its meta-data.
		/// Things that do not have an owner should call <see cref="RegisterThing(MetaDataTag[], EventHandlerAsync{RegistrationEventArgs}, object)"/> to 
		/// update its meta-data in the Thing Registry.
		/// 
		/// Note: Meta information updated in this way will only overwrite tags provided in the request, and leave other tags previously 
		/// reported as is.
		/// </summary>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task UpdateThing(MetaDataTag[] MetaDataTags, EventHandlerAsync<UpdateEventArgs> Callback, object State)
		{
			return this.UpdateThing(string.Empty, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Updates the meta-data about a thing in the Thing Registry. Only public things that have an owner can update its meta-data.
		/// Things that do not have an owner should call <see cref="RegisterThing(string, MetaDataTag[], EventHandlerAsync{RegistrationEventArgs}, object)"/> to 
		/// update its meta-data in the Thing Registry.
		/// 
		/// Note: Meta information updated in this way will only overwrite tags provided in the request, and leave other tags previously 
		/// reported as is.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task UpdateThing(string NodeId, MetaDataTag[] MetaDataTags, EventHandlerAsync<UpdateEventArgs> Callback, object State)
		{
			return this.UpdateThing(NodeId, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Updates the meta-data about a thing in the Thing Registry. Only public things that have an owner can update its meta-data.
		/// Things that do not have an owner should call <see cref="RegisterThing(string, string, MetaDataTag[], EventHandlerAsync{RegistrationEventArgs}, object)"/> 
		/// to update its meta-data in the Thing Registry.
		/// 
		/// Note: Meta information updated in this way will only overwrite tags provided in the request, and leave other tags previously 
		/// reported as is.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task UpdateThing(string NodeId, string SourceId, MetaDataTag[] MetaDataTags, EventHandlerAsync<UpdateEventArgs> Callback, object State)
		{
			return this.UpdateThing(NodeId, SourceId, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Updates the meta-data about a thing in the Thing Registry. Only public things that have an owner can update its meta-data.
		/// Things that do not have an owner should call <see cref="RegisterThing(string, string, string, MetaDataTag[], EventHandlerAsync{RegistrationEventArgs}, object)"/> to 
		/// update its meta-data in the Thing Registry.
		/// 
		/// Note: Meta information updated in this way will only overwrite tags provided in the request, and leave other tags previously 
		/// reported as is.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Partition">Partition of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task UpdateThing(string NodeId, string SourceId, string Partition, MetaDataTag[] MetaDataTags,
			EventHandlerAsync<UpdateEventArgs> Callback, object State)
		{
			return this.UpdateThing(NodeId, SourceId, Partition, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Allows an owner to update the meta-data about one of its things in the Thing Registry.
		/// 
		/// Note: Meta information updated in this way will only overwrite tags provided in the request, and leave other tags previously 
		/// reported as is.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Partition">Partition of thing, if behind a concentrator.</param>
		/// <param name="ThingJid">JID of thing. Required if an owner wants to update the meta-data about one of its things. Leave empty,
		/// if the thing wants to update its own meta-data.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task UpdateThing(string NodeId, string SourceId, string Partition, string ThingJid, MetaDataTag[] MetaDataTags,
			EventHandlerAsync<UpdateEventArgs> Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<update xmlns='");
			Request.Append(NamespaceDiscoveryCurrent);

			if (!string.IsNullOrEmpty(ThingJid))
			{
				Request.Append("' jid='");
				Request.Append(XML.Encode(ThingJid));
			}

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			Request.Append("'>");

			string RegistryAddress = this.AddTags(Request, MetaDataTags, this.thingRegistryAddress);

			Request.Append("</update>");

			return this.client.SendIqSet(RegistryAddress, Request.ToString(), async (sender, e) =>
			{
				XmlElement E = e.FirstElement;
				bool Disowned = false;

				if (e.Ok && !(E is null) && E.LocalName == "disowned")
				{
					Disowned = true;

					if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(Partition) &&
						this.client.TryGetExtension(out ProvisioningClient ProvisioningClient))
					{
						ProvisioningClient.OwnerJid = string.Empty;
					}
				}

				await Callback.Raise(this, new UpdateEventArgs(e, State, Disowned));

			}, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task Unregister(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Unregister(string.Empty, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task Unregister(string NodeId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Unregister(NodeId, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task Unregister(string NodeId, string SourceId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Unregister(NodeId, SourceId, string.Empty, Callback, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Partition">Partition of thing, if behind a concentrator.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task Unregister(string NodeId, string SourceId, string Partition, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Unregister(this.thingRegistryAddress, NodeId, SourceId, Partition, Callback, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="RegistryJid">JID of registry service.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Partition">Partition of thing, if behind a concentrator.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public Task Unregister(string RegistryJid, string NodeId, string SourceId, string Partition, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<unregister xmlns='");
			Request.Append(NamespaceDiscoveryCurrent);

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			Request.Append("'/>");

			return this.client.SendIqSet(RegistryJid, Request.ToString(), (sender, e) => Callback.Raise(this, e), State);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Disown(string ThingJid, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Disown(ThingJid, string.Empty, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Disown(string ThingJid, string NodeId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Disown(ThingJid, NodeId, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Disown(string ThingJid, string NodeId, string SourceId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Disown(ThingJid, NodeId, SourceId, string.Empty, Callback, State);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="Partition">Optional Partition of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Disown(string ThingJid, string NodeId, string SourceId, string Partition, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Disown(this.thingRegistryAddress, ThingJid, NodeId, SourceId, Partition, Callback, State);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="RegistryJid">JID of registry service.</param>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="Partition">Optional Partition of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Disown(string RegistryJid, string ThingJid, string NodeId, string SourceId, string Partition, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<disown xmlns='");
			Request.Append(NamespaceDiscoveryCurrent);

			Request.Append("' jid='");
			Request.Append(XML.Encode(ThingJid));

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			Request.Append("'/>");

			return this.client.SendIqSet(RegistryJid, Request.ToString(), (sender, e) => Callback.Raise(this, e), State);
		}

		private async Task DisownedHandler(object Sender, IqEventArgs e)
		{
			XmlElement E = e.Query;
			string NodeId = XML.Attribute(E, "id");
			string SourceId = XML.Attribute(E, "src");
			string Partition = XML.Attribute(E, "pt");
			ThingReference Node;

			if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(Partition))
			{
				Node = ThingReference.Empty;

				if (this.client.TryGetExtension(out ProvisioningClient ProvisioningClient))
					ProvisioningClient.OwnerJid = string.Empty;
			}
			else
				Node = new ThingReference(NodeId, SourceId, Partition);

			NodeEventArgs e2 = new NodeEventArgs(e, Node);
			await this.Disowned.Raise(this, e2);

			await e.IqResult(string.Empty);
		}

		/// <summary>
		/// Event raised when a node has been disowned.
		/// </summary>
		public event EventHandlerAsync<NodeEventArgs> Disowned = null;

		/// <summary>
		/// Searches for publically available things in the thing registry.
		/// </summary>
		/// <param name="Offset">Search offset.</param>
		/// <param name="MaxCount">Maximum number of things to return.</param>
		/// <param name="SearchOperators">Search operators to use in search.</param>
		/// <param name="Callback">Method to call when result has been received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Search(int Offset, int MaxCount, SearchOperator[] SearchOperators, EventHandlerAsync<SearchResultEventArgs> Callback, object State)
		{
			return this.Search(this.thingRegistryAddress, Offset, MaxCount, SearchOperators, Callback, State);
		}

		/// <summary>
		/// Searches for publically available things in the thing registry.
		/// </summary>
		/// <param name="RegistryJid">JID of registry service.</param>
		/// <param name="Offset">Search offset.</param>
		/// <param name="MaxCount">Maximum number of things to return.</param>
		/// <param name="SearchOperators">Search operators to use in search.</param>
		/// <param name="Callback">Method to call when result has been received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Search(string RegistryJid, int Offset, int MaxCount, SearchOperator[] SearchOperators, EventHandlerAsync<SearchResultEventArgs> Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<search xmlns='");
			Request.Append(NamespaceDiscoveryCurrent);
			Request.Append("' offset='");
			Request.Append(Offset.ToString());
			Request.Append("' maxCount='");
			Request.Append(MaxCount.ToString());
			Request.Append("'>");

			foreach (SearchOperator Operator in SearchOperators)
				Operator.Serialize(Request);

			Request.Append("</search>");

			return this.client.SendIqGet(RegistryJid, Request.ToString(), (sender, e) =>
			{
				ParseResultSet(Offset, MaxCount, this, e, Callback, State);
				return Task.CompletedTask;
			}, State);
		}

		internal static void ParseResultSet(int Offset, int MaxCount, object Sender, IqResultEventArgs e, EventHandlerAsync<SearchResultEventArgs> Callback, object State)
		{
			List<SearchResultThing> Things = new List<SearchResultThing>();
			List<MetaDataTag> MetaData = new List<MetaDataTag>();
			ThingReference Node;
			XmlElement E = e.FirstElement;
			XmlElement E2, E3;
			string Jid;
			string NodeId;
			string SourceId;
			string Partition;
			string Name;
			bool More = false;

			if (e.Ok && !(E is null) && E.LocalName == "found")
			{
				More = XML.Attribute(E, "more", false);

				foreach (XmlNode N in E.ChildNodes)
				{
					E2 = N as XmlElement;
					if (E2.LocalName == "thing")
					{
						Jid = XML.Attribute(E2, "jid");
						NodeId = XML.Attribute(E2, "id");
						SourceId = XML.Attribute(E2, "src");
						Partition = XML.Attribute(E2, "pt");

						if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(Partition))
							Node = ThingReference.Empty;
						else
							Node = new ThingReference(NodeId, SourceId, Partition);

						MetaData.Clear();
						foreach (XmlNode N2 in E2.ChildNodes)
						{
							E3 = N2 as XmlElement;
							if (E3 is null)
								continue;

							Name = XML.Attribute(E3, "name");

							switch (E3.LocalName)
							{
								case "str":
									MetaData.Add(new MetaDataStringTag(Name, XML.Attribute(E3, "value")));
									break;

								case "num":
									MetaData.Add(new MetaDataNumericTag(Name, XML.Attribute(E3, "value", 0.0)));
									break;
							}
						}

						Things.Add(new SearchResultThing(Jid, Node, MetaData.ToArray()));
					}
				}
			}

			if (!(Callback is null))
			{
				SearchResultEventArgs e2 = new SearchResultEventArgs(e, State, Offset, MaxCount, More, Things.ToArray());

				try
				{
					Callback(Sender, e2);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		/// <summary>
		/// Generates an IOTDISCO URI from the meta-data provided in <paramref name="MetaData"/>.
		/// 
		/// For more information about the IOTDISCO URI scheme, see: http://www.iana.org/assignments/uri-schemes/prov/iotdisco.pdf
		/// </summary>
		/// <param name="MetaData">Meta-data to encode.</param>
		/// <returns>IOTDISCO URI encoding the meta-data.</returns>
		public string EncodeAsIoTDiscoURI(params MetaDataTag[] MetaData)
		{
			StringBuilder Result = new StringBuilder("iotdisco:");
			bool First = true;
			bool ContainsR = false;


			foreach (MetaDataTag Tag in MetaData)
			{
				if (First)
					First = false;
				else
					Result.Append(';');

				if (Tag is MetaDataNumericTag)
					Result.Append('#');

				Result.Append(Uri.EscapeDataString(Tag.Name));
				Result.Append('=');
				Result.Append(Uri.EscapeDataString(Tag.StringValue));

				if (!ContainsR && Tag.Name == "R")
					ContainsR = true;
			}

			if (!ContainsR)
			{
				if (!First)
					Result.Append(';');

				Result.Append("R=");
				Result.Append(Uri.EscapeDataString(this.thingRegistryAddress));
			}

			return Result.ToString();
		}

		/// <summary>
		/// Decodes an IoTDisco Claim URI (subset of all possible IoTDisco URIs).
		/// </summary>
		/// <param name="DiscoUri">IoTDisco URI</param>
		/// <returns>Meta data tags.</returns>
		public static MetaDataTag[] DecodeIoTDiscoClaimURI(string DiscoUri)
		{
			if (TryDecodeIoTDiscoClaimURI(DiscoUri, out MetaDataTag[] Tags))
				return Tags;
			else
				throw new ArgumentException("URI does not conform to the iotdisco URI scheme for claiming things.", nameof(DiscoUri));
		}

		/// <summary>
		/// Tries to decode an IoTDisco Claim URI (subset of all possible IoTDisco URIs).
		/// </summary>
		/// <param name="DiscoUri">IoTDisco URI</param>
		/// <param name="Tags">Decoded meta data tags.</param>
		/// <returns>If <paramref name="DiscoUri"/> was successfully decoded.</returns>
		public static bool TryDecodeIoTDiscoClaimURI(string DiscoUri, out MetaDataTag[] Tags)
		{
			List<MetaDataTag> Result = new List<MetaDataTag>();

			foreach (SearchOperator Op in DecodeIoTDiscoURI(DiscoUri))
			{
				if (Op is StringTagEqualTo S)
					Result.Add(new MetaDataStringTag(S.Name, S.Value));
				else if (Op is NumericTagEqualTo N)
					Result.Add(new MetaDataNumericTag(N.Name, N.Value));
				else
				{
					Tags = null;
					return false;
				}
			}

			Tags = Result.ToArray();

			return true;
		}

		/// <summary>
		/// Decodes an IoTDisco URI.
		/// </summary>
		/// <param name="DiscoUri">IoTDisco URI</param>
		/// <returns>Search operators.</returns>
		public static IEnumerable<SearchOperator> DecodeIoTDiscoURI(string DiscoUri)
		{
			if (TryDecodeIoTDiscoURI(DiscoUri, out IEnumerable<SearchOperator> Result))
				return Result;
			else
				throw new ArgumentException("URI does not conform to the iotdisco URI scheme.", nameof(DiscoUri));
		}

		/// <summary>
		/// Decodes an IoTDisco URI.
		/// </summary>
		/// <param name="DiscoUri">IoTDisco URI</param>
		/// <param name="Operators">Search operators.</param>
		/// <returns>If the URI could be parsed.</returns>
		public static bool TryDecodeIoTDiscoURI(string DiscoUri, out IEnumerable<SearchOperator> Operators)
		{
			Dictionary<string, SearchOperator> OperatorsByName = new Dictionary<string, SearchOperator>(StringComparer.CurrentCultureIgnoreCase);
			StringBuilder sb = new StringBuilder();
			Operator Operator = Operator.Equals;
			string Name = null;
			bool Numeric = false;
			char Wildcard = (char)0;
			int State = 0;

			Operators = null;

			foreach (char ch in DiscoUri)
			{
				switch (State)
				{
					case 0:
						if (ch == 'i' || ch == 'I')
							State++;
						else
							return false;
						break;

					case 1:
						if (ch == 'o' || ch == 'O')
							State++;
						else
							return false;
						break;

					case 2:
						if (ch == 't' || ch == 'T')
							State++;
						else
							return false;
						break;

					case 3:
						if (ch == 'd' || ch == 'D')
							State++;
						else
							return false;
						break;

					case 4:
						if (ch == 'i' || ch == 'I')
							State++;
						else
							return false;
						break;

					case 5:
						if (ch == 's' || ch == 'S')
							State++;
						else
							return false;
						break;

					case 6:
						if (ch == 'c' || ch == 'C')
							State++;
						else
							return false;
						break;

					case 7:
						if (ch == 'o' || ch == 'O')
							State++;
						else
							return false;
						break;

					case 8:
						if (ch == ':')
							State++;
						else
							return false;
						break;

					case 9:     // Tag Name, first character
						if (ch == '#')
						{
							Numeric = true;
							State++;
						}
						else if (ch == ';')
							return false;
						else
						{
							sb.Append(ch);
							Numeric = false;
							State++;
						}
						break;

					case 10:     // Tag Name, not first character
						switch (ch)
						{
							case '=':
								Operator = Operator.Equals;
								State += 5;
								break;

							case '<':
								State++;
								break;

							case '>':
								State += 2;
								break;

							case '~':
								Operator = Operator.Mask;
								State += 3;
								break;

							case '\\':
								State += 4;
								break;

							default:
								sb.Append(ch);
								break;
						}
						break;

					case 11:    // <
						switch (ch)
						{
							case '=':
								Operator = Operator.LessOrEqual;
								State += 4;
								break;

							case '>':
								Operator = Operator.NotEquals;
								State += 4;
								break;

							default:
								Operator = Operator.Less;
								Name = sb.ToString();
								sb.Clear();
								sb.Append(ch);
								State += 5;
								break;
						}
						break;

					case 12:    // >
						if (ch == '=')
						{
							Operator = Operator.GreaterOrEqual;
							State += 3;
						}
						else
						{
							Operator = Operator.Greater;
							Name = sb.ToString();
							sb.Clear();
							sb.Append(ch);
							State += 4;
						}
						break;

					case 13:    // ~
						Wildcard = ch;
						State += 2;
						break;

					case 14:    // \
						sb.Append(ch);
						State -= 4;
						break;

					case 15:    // First character of value
						Name = sb.ToString();
						sb.Clear();

						switch (ch)
						{
							case ';':
								if (AddOperator(Name, sb.ToString(), Numeric, Operator, Wildcard, OperatorsByName))
								{
									sb.Clear();
									State -= 6;
								}
								else
									return false;
								break;

							case '\\':
								State += 2;
								break;

							default:
								sb.Append(ch);
								State++;
								break;
						}
						break;

					case 16:    // Rest of characters of value
						switch (ch)
						{
							case ';':
								if (AddOperator(Name, sb.ToString(), Numeric, Operator, Wildcard, OperatorsByName))
								{
									sb.Clear();
									State -= 7;
								}
								else
									return false;
								break;

							case '\\':
								State++;
								break;

							default:
								sb.Append(ch);
								break;
						}
						break;

					case 17:    // \
						sb.Append(ch);
						State--;
						break;
				}

				if (State < 0)
					break;
			}

			switch (State)
			{
				case 15:
					if (!AddOperator(sb.ToString(), string.Empty, Numeric, Operator, Wildcard, OperatorsByName))
						return false;
					break;

				case 16:
					if (!AddOperator(Name, sb.ToString(), Numeric, Operator, Wildcard, OperatorsByName))
						return false;
					break;

				default:
					return false;
			}

			Operators = OperatorsByName.Values;

			return true;
		}

		private static bool AddOperator(string Name, string Value, bool Numeric, Operator Operator, char Wildcard, Dictionary<string, SearchOperator> Operators)
		{
			Name = Uri.UnescapeDataString(Name);
			Value = Uri.UnescapeDataString(Value);

			if (Numeric)
			{
				if (!CommonTypes.TryParse(Value, out double NumericValue))
					return false;

				switch (Operator)
				{
					case Operator.Equals:
						if (Operators.ContainsKey(Name))
							return false;
						else
							Operators[Name] = new NumericTagEqualTo(Name, NumericValue);
						break;

					case Operator.NotEquals:
						if (Operators.ContainsKey(Name))
							return false;
						else
							Operators[Name] = new NumericTagNotEqualTo(Name, NumericValue);
						break;

					case Operator.Less:
						if (Operators.TryGetValue(Name, out SearchOperator Op))
						{
							if (Op is NumericTagGreaterThan Gt)
							{
								if (NumericValue < Gt.Value)
									Operators[Name] = new NumericTagNotInRange(Name, NumericValue, true, Gt.Value, true);
								else
									Operators[Name] = new NumericTagInRange(Name, Gt.Value, false, NumericValue, false);
							}
							else if (Op is NumericTagGreaterThanOrEqualTo GtE)
							{
								if (NumericValue < GtE.Value)
									Operators[Name] = new NumericTagNotInRange(Name, NumericValue, true, GtE.Value, false);
								else
									Operators[Name] = new NumericTagInRange(Name, GtE.Value, true, NumericValue, false);
							}
							else
								return false;
						}
						else
							Operators[Name] = new NumericTagLesserThan(Name, NumericValue);
						break;

					case Operator.LessOrEqual:
						if (Operators.TryGetValue(Name, out Op))
						{
							if (Op is NumericTagGreaterThan Gt)
							{
								if (NumericValue < Gt.Value)
									Operators[Name] = new NumericTagNotInRange(Name, NumericValue, false, Gt.Value, true);
								else
									Operators[Name] = new NumericTagInRange(Name, Gt.Value, false, NumericValue, true);
							}
							else if (Op is NumericTagGreaterThanOrEqualTo GtE)
							{
								if (NumericValue < GtE.Value)
									Operators[Name] = new NumericTagNotInRange(Name, NumericValue, false, GtE.Value, false);
								else
									Operators[Name] = new NumericTagInRange(Name, GtE.Value, true, NumericValue, true);
							}
							else
								return false;
						}
						else
							Operators[Name] = new NumericTagLesserThanOrEqualTo(Name, NumericValue);
						break;

					case Operator.Greater:
						if (Operators.TryGetValue(Name, out Op))
						{
							if (Op is NumericTagLesserThan Lt)
							{
								if (NumericValue < Lt.Value)
									Operators[Name] = new NumericTagInRange(Name, NumericValue, false, Lt.Value, false);
								else
									Operators[Name] = new NumericTagNotInRange(Name, Lt.Value, true, NumericValue, true);
							}
							else if (Op is NumericTagLesserThanOrEqualTo LtE)
							{
								if (NumericValue < LtE.Value)
									Operators[Name] = new NumericTagInRange(Name, NumericValue, false, LtE.Value, true);
								else
									Operators[Name] = new NumericTagNotInRange(Name, LtE.Value, false, NumericValue, true);
							}
							else
								return false;
						}
						else
							Operators[Name] = new NumericTagGreaterThan(Name, NumericValue);
						break;

					case Operator.GreaterOrEqual:
						if (Operators.TryGetValue(Name, out Op))
						{
							if (Op is NumericTagLesserThan Lt)
							{
								if (NumericValue < Lt.Value)
									Operators[Name] = new NumericTagInRange(Name, NumericValue, true, Lt.Value, false);
								else
									Operators[Name] = new NumericTagNotInRange(Name, Lt.Value, true, NumericValue, false);
							}
							else if (Op is NumericTagLesserThanOrEqualTo LtE)
							{
								if (NumericValue < LtE.Value)
									Operators[Name] = new NumericTagInRange(Name, NumericValue, true, LtE.Value, true);
								else
									Operators[Name] = new NumericTagNotInRange(Name, LtE.Value, false, NumericValue, false);
							}
							else
								return false;
						}
						else
							Operators[Name] = new NumericTagGreaterThanOrEqualTo(Name, NumericValue);
						break;

					default:
						return false;
				}
			}
			else
			{
				switch (Operator)
				{
					case Operator.Equals:
						if (Operators.ContainsKey(Name))
							return false;
						else
							Operators[Name] = new StringTagEqualTo(Name, Value);
						break;

					case Operator.NotEquals:
						if (Operators.ContainsKey(Name))
							return false;
						else
							Operators[Name] = new StringTagNotEqualTo(Name, Value);
						break;

					case Operator.Less:
						if (Operators.TryGetValue(Name, out SearchOperator Op))
						{
							if (Op is StringTagGreaterThan Gt)
							{
								if (string.Compare(Value, Gt.Value) < 0)
									Operators[Name] = new StringTagNotInRange(Name, Value, true, Gt.Value, true);
								else
									Operators[Name] = new StringTagInRange(Name, Gt.Value, false, Value, false);
							}
							else if (Op is StringTagGreaterThanOrEqualTo GtE)
							{
								if (string.Compare(Value, GtE.Value) < 0)
									Operators[Name] = new StringTagNotInRange(Name, Value, true, GtE.Value, false);
								else
									Operators[Name] = new StringTagInRange(Name, GtE.Value, true, Value, false);
							}
							else
								return false;
						}
						else
							Operators[Name] = new StringTagLesserThan(Name, Value);
						break;

					case Operator.LessOrEqual:
						if (Operators.TryGetValue(Name, out Op))
						{
							if (Op is StringTagGreaterThan Gt)
							{
								if (string.Compare(Value, Gt.Value) < 0)
									Operators[Name] = new StringTagNotInRange(Name, Value, false, Gt.Value, true);
								else
									Operators[Name] = new StringTagInRange(Name, Gt.Value, false, Value, true);
							}
							else if (Op is StringTagGreaterThanOrEqualTo GtE)
							{
								if (string.Compare(Value, GtE.Value) < 0)
									Operators[Name] = new StringTagNotInRange(Name, Value, false, GtE.Value, false);
								else
									Operators[Name] = new StringTagInRange(Name, GtE.Value, true, Value, true);
							}
							else
								return false;
						}
						else
							Operators[Name] = new StringTagLesserThanOrEqualTo(Name, Value);
						break;

					case Operator.Greater:
						if (Operators.TryGetValue(Name, out Op))
						{
							if (Op is StringTagLesserThan Lt)
							{
								if (string.Compare(Value, Lt.Value) < 0)
									Operators[Name] = new StringTagInRange(Name, Value, false, Lt.Value, false);
								else
									Operators[Name] = new StringTagNotInRange(Name, Lt.Value, true, Value, true);
							}
							else if (Op is StringTagLesserThanOrEqualTo LtE)
							{
								if (string.Compare(Value, LtE.Value) < 0)
									Operators[Name] = new StringTagInRange(Name, Value, false, LtE.Value, true);
								else
									Operators[Name] = new StringTagNotInRange(Name, LtE.Value, false, Value, true);
							}
							else
								return false;
						}
						else
							Operators[Name] = new StringTagGreaterThan(Name, Value);
						break;

					case Operator.GreaterOrEqual:
						if (Operators.TryGetValue(Name, out Op))
						{
							if (Op is StringTagLesserThan Lt)
							{
								if (string.Compare(Value, Lt.Value) < 0)
									Operators[Name] = new StringTagInRange(Name, Value, true, Lt.Value, false);
								else
									Operators[Name] = new StringTagNotInRange(Name, Lt.Value, true, Value, false);
							}
							else if (Op is StringTagLesserThanOrEqualTo LtE)
							{
								if (string.Compare(Value, LtE.Value) < 0)
									Operators[Name] = new StringTagInRange(Name, Value, true, LtE.Value, true);
								else
									Operators[Name] = new StringTagNotInRange(Name, LtE.Value, false, Value, false);
							}
							else
								return false;
						}
						else
							Operators[Name] = new StringTagGreaterThanOrEqualTo(Name, Value);
						break;

					case Operator.Mask:
						if (Operators.ContainsKey(Name))
							return false;
						else
							Operators[Name] = new StringTagMask(Name, Value, new string(Wildcard, 1));
						break;

					default:
						return false;
				}
			}

			return true;
		}

		private enum Operator
		{
			Equals,
			NotEquals,
			Less,
			LessOrEqual,
			Greater,
			GreaterOrEqual,
			Mask
		}

		/// <summary>
		/// Checks if a URI is a claim URI.
		/// </summary>
		/// <param name="DiscoUri">IoTDisco URI</param>
		/// <returns>If <paramref name="DiscoUri"/> is a claim URI.</returns>
		public static bool IsIoTDiscoClaimURI(string DiscoUri)
		{
			if (!TryDecodeIoTDiscoURI(DiscoUri, out IEnumerable<SearchOperator> Operators))
				return false;
			else
				return IsIoTDiscoClaimURI(Operators);
		}

		/// <summary>
		/// Checks if a URI is a claim URI.
		/// </summary>
		/// <param name="Operators">Tag operators in URI</param>
		/// <returns>If <paramref name="Operators"/> is a claim URI.</returns>
		public static bool IsIoTDiscoClaimURI(IEnumerable<SearchOperator> Operators)
		{
			bool HasKey = false;
			bool HasRegistry = false;

			foreach (SearchOperator Op in Operators)
			{
				if (Op is StringTagEqualTo S)
				{
					switch (S.Name.ToUpper())
					{
						case "KEY":
							HasKey = true;
							break;

						case "R":
							HasRegistry = true;
							break;
					}
				}
				else if (!(Op is NumericTagEqualTo))
					return false;
			}

			return HasKey && HasRegistry;
		}

		/// <summary>
		/// Checks if a URI is a search URI.
		/// </summary>
		/// <param name="DiscoUri">IoTDisco URI</param>
		/// <returns>If <paramref name="DiscoUri"/> is a search URI.</returns>
		public static bool IsIoTDiscoSearchURI(string DiscoUri)
		{
			if (!TryDecodeIoTDiscoURI(DiscoUri, out IEnumerable<SearchOperator> Operators))
				return false;
			else
				return IsIoTDiscoSearchURI(Operators);
		}

		/// <summary>
		/// Checks if a URI is a search URI.
		/// </summary>
		/// <param name="Operators">Tag operators in URI</param>
		/// <returns>If <paramref name="Operators"/> is a search URI.</returns>
		public static bool IsIoTDiscoSearchURI(IEnumerable<SearchOperator> Operators)
		{
			return IsIoTDiscoSearchURI(Operators, false);
		}

		/// <summary>
		/// Checks if a URI is a search URI.
		/// </summary>
		/// <param name="Operators">Tag operators in URI</param>
		/// <param name="RequireRegistry">If a registry is required.</param>
		/// <returns>If <paramref name="Operators"/> is a search URI.</returns>
		public static bool IsIoTDiscoSearchURI(IEnumerable<SearchOperator> Operators, bool RequireRegistry)
		{
			bool HasRegistry = false;

			foreach (SearchOperator Op in Operators)
			{
				if (Op is StringTagEqualTo S)
				{
					switch (S.Name.ToUpper())
					{
						case "KEY":
						case "JID":
						case "NID":
						case "SID":
						case "PT":
							return false;

						case "R":
							HasRegistry = true;
							break;
					}
				}
			}

			return !RequireRegistry || HasRegistry;
		}

		/// <summary>
		/// Checks if a URI is a direct reference URI.
		/// </summary>
		/// <param name="DiscoUri">IoTDisco URI</param>
		/// <returns>If <paramref name="DiscoUri"/> is a search URI.</returns>
		public static bool IsIoTDiscoDirectURI(string DiscoUri)
		{
			if (!TryDecodeIoTDiscoURI(DiscoUri, out IEnumerable<SearchOperator> Operators))
				return false;
			else
				return IsIoTDiscoDirectURI(Operators);
		}

		/// <summary>
		/// Checks if a URI is a direct reference URI.
		/// </summary>
		/// <param name="Operators">Tag operators in URI</param>
		/// <returns>If <paramref name="Operators"/> is a direct reference URI.</returns>
		public static bool IsIoTDiscoDirectURI(IEnumerable<SearchOperator> Operators)
		{
			bool HasJid = false;

			foreach (SearchOperator Op in Operators)
			{
				if (Op is StringTagEqualTo S)
				{
					switch (S.Name.ToUpper())
					{
						case "JID":
							HasJid = true;
							break;

						case "NID":
						case "SID":
						case "PT":
						default:
							break;
					}
				}
				else if (!(Op is NumericTagEqualTo))
					return false;
			}

			return HasJid;
		}

		#region Finding Thing Registry

		/// <summary>
		/// Finds the thing registry servicing a device.
		/// </summary>
		/// <param name="DeviceBareJid">Device Bare JID</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void FindThingRegistry(string DeviceBareJid, EventHandlerAsync<ServiceEventArgs> Callback, object State)
		{
			this.client.FindComponent(DeviceBareJid, NamespacesDiscovery, Callback, State);
		}

		/// <summary>
		/// Finds the thing registry servicing a device.
		/// </summary>
		/// <param name="DeviceBareJid">Device Bare JID</param>
		/// <returns>Thing Registry, if found.</returns>
		public async Task<string> FindThingRegistryAsync(string DeviceBareJid)
		{
			KeyValuePair<string, string> P = await this.client.FindComponentAsync(DeviceBareJid, NamespacesDiscovery);
			return P.Key;
		}

		#endregion
	}
}
