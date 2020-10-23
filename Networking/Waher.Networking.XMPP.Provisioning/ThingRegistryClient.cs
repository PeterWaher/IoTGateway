using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Things;
using Waher.Networking.XMPP.Provisioning.SearchOperators;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Delegate for registration callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task RegistrationEventHandler(object Sender, RegistrationEventArgs e);

	/// <summary>
	/// Delegate for update callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task UpdateEventHandler(object Sender, UpdateEventArgs e);

	/// <summary>
	/// Delegate for node result callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task NodeResultEventHandler(object Sender, NodeResultEventArgs e);

	/// <summary>
	/// Delegate for node events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task NodeEventHandler(object Sender, NodeEventArgs e);

	/// <summary>
	/// Delegate for claimed events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task ClaimedEventHandler(object Sender, ClaimedEventArgs e);

	/// <summary>
	/// Delegate for search result event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task SearchResultEventHandler(object Sender, SearchResultEventArgs e);

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
		public const string NamespaceDiscovery = "urn:ieee:iot:disco:1.0";

		/// <summary>
		/// Implements an XMPP provisioning client interface.
		/// 
		/// The interface is defined in the IEEE XMPP IoT extensions:
		/// https://gitlab.com/IEEE-SA/XMPPI/IoT
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ThingRegistryAddress">Thing Registry XMPP address.</param>
		public ThingRegistryClient(XmppClient Client, string ThingRegistryAddress)
			: base(Client)
		{
			this.thingRegistryAddress = ThingRegistryAddress;

			this.client.RegisterIqSetHandler("claimed", NamespaceDiscovery, this.ClaimedHandler, true);
			this.client.RegisterIqSetHandler("removed", NamespaceDiscovery, this.RemovedHandler, false);
			this.client.RegisterIqSetHandler("disowned", NamespaceDiscovery, this.DisownedHandler, false);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.client.UnregisterIqSetHandler("claimed", NamespaceDiscovery, this.ClaimedHandler, true);
			this.client.UnregisterIqSetHandler("removed", NamespaceDiscovery, this.RemovedHandler, false);
			this.client.UnregisterIqSetHandler("disowned", NamespaceDiscovery, this.DisownedHandler, false);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0347" };

		/// <summary>
		/// Thing Registry XMPP address.
		/// </summary>
		public string ThingRegistryAddress
		{
			get { return this.thingRegistryAddress; }
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(MetaDataTag[], UpdateEventHandler, object)"/> to update 
		/// its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterThing(MetaDataTag[] MetaDataTags, RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(false, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, MetaDataTag[], UpdateEventHandler, object)"/> to 
		/// update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterThing(string NodeId, MetaDataTag[] MetaDataTags, RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(false, NodeId, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, string, MetaDataTag[], UpdateEventHandler, object)"/> to 
		/// update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterThing(string NodeId, string SourceId, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(false, NodeId, SourceId, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, string, string, MetaDataTag[], UpdateEventHandler, object)"/> 
		/// to update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Partition">Partition of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterThing(string NodeId, string SourceId, string Partition, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(false, NodeId, SourceId, Partition, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(MetaDataTag[], UpdateEventHandler, object)"/> to 
		/// update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterThing(bool SelfOwned, MetaDataTag[] MetaDataTags, RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(SelfOwned, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, MetaDataTag[], UpdateEventHandler, object)"/> 
		/// to update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterThing(bool SelfOwned, string NodeId, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(SelfOwned, NodeId, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, string, MetaDataTag[], UpdateEventHandler, object)"/> 
		/// to update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterThing(bool SelfOwned, string NodeId, string SourceId, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(SelfOwned, NodeId, SourceId, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="UpdateThing(string, string, string, MetaDataTag[], UpdateEventHandler, object)"/> 
		/// to update its meta-data in the Thing Registry, if the meta-data has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Partition">Partition of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterThing(bool SelfOwned, string NodeId, string SourceId, string Partition, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<register xmlns='");
			Request.Append(NamespaceDiscovery);

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			if (SelfOwned)
				Request.Append("' selfOwned='true");

			Request.Append("'>");

			string RegistryAddress = this.AddTags(Request, MetaDataTags, this.thingRegistryAddress);

			Request.Append("</register>");

			this.client.SendIqSet(RegistryAddress, Request.ToString(), async (sender, e) =>
			{
				if (!(Callback is null))
				{
					XmlElement E = e.FirstElement;
					string OwnerJid = string.Empty;
					bool IsPublic = false;

					if (e.Ok && E != null && E.LocalName == "claimed" && E.NamespaceURI == NamespaceDiscovery)
					{
						OwnerJid = XML.Attribute(E, "jid");
						IsPublic = XML.Attribute(E, "public", false);

						if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(Partition) &&
							this.client.TryGetExtension(typeof(ProvisioningClient), out IXmppExtension Extension) &&
							Extension is ProvisioningClient ProvisioningClient)
						{
							ProvisioningClient.OwnerJid = OwnerJid;
						}
					}

					RegistrationEventArgs e2 = new RegistrationEventArgs(e, State, OwnerJid, IsPublic);

					try
					{
						await Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
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
		public void Mine(MetaDataTag[] MetaDataTags, NodeResultEventHandler Callback, object State)
		{
			this.Mine(true, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Claims a thing.
		/// </summary>
		/// <param name="Public">If the thing should be left as a public thing that is searchable.</param>
		/// <param name="MetaDataTags">Meta-data describing the thing.</param>
		/// <param name="Callback">Method to call when response to claim is returned.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		public void Mine(bool Public, MetaDataTag[] MetaDataTags, NodeResultEventHandler Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<mine xmlns='");
			Request.Append(NamespaceDiscovery);
			Request.Append("' public='");
			Request.Append(CommonTypes.Encode(Public));
			Request.Append("'>");

			string RegistryAddress = this.AddTags(Request, MetaDataTags, this.thingRegistryAddress);

			Request.Append("</mine>");

			this.client.SendIqSet(RegistryAddress, Request.ToString(), async (sender, e) =>
			{
				if (!(Callback is null))
				{
					XmlElement E = e.FirstElement;
					string NodeJid = string.Empty;
					ThingReference Node = ThingReference.Empty;

					if (e.Ok && E != null && E.LocalName == "claimed" && E.NamespaceURI == NamespaceDiscovery)
					{
						string NodeId = XML.Attribute(E, "id");
						string SourceId = XML.Attribute(E, "src");
						string Partition = XML.Attribute(E, "pt");
						NodeJid = XML.Attribute(E, "jid");

						if (!string.IsNullOrEmpty(NodeId) || !string.IsNullOrEmpty(SourceId) || !string.IsNullOrEmpty(Partition))
							Node = new ThingReference(NodeId, SourceId, Partition);
					}

					NodeResultEventArgs e2 = new NodeResultEventArgs(e, State, NodeJid, Node);

					try
					{
						await Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
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

				if (this.client.TryGetExtension(typeof(ProvisioningClient), out IXmppExtension Extension) &&
					Extension is ProvisioningClient ProvisioningClient)
				{
					ProvisioningClient.OwnerJid = OwnerJid;
				}
			}
			else
				Node = new ThingReference(NodeId, SourceId, Partition);

			ClaimedEventArgs e2 = new ClaimedEventArgs(e, Node, OwnerJid, Public);
			ClaimedEventHandler h = this.Claimed;
			if (!(h is null))
			{
				try
				{
					await h(this, e2);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			e.IqResult(string.Empty);
		}

		/// <summary>
		/// Event raised when a node has been claimed.
		/// </summary>
		public event ClaimedEventHandler Claimed = null;

		/// <summary>
		/// Removes a publicly claimed thing from the thing registry, so that it does not appear in search results.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Remove(string ThingJid, IqResultEventHandlerAsync Callback, object State)
		{
			this.Remove(ThingJid, string.Empty, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Removes a publicly claimed thing from the thing registry, so that it does not appear in search results.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Remove(string ThingJid, string NodeId, IqResultEventHandlerAsync Callback, object State)
		{
			this.Remove(ThingJid, NodeId, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Removes a publicly claimed thing from the thing registry, so that it does not appear in search results.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Remove(string ThingJid, string NodeId, string SourceId, IqResultEventHandlerAsync Callback, object State)
		{
			this.Remove(ThingJid, NodeId, SourceId, string.Empty, Callback, State);
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
		public void Remove(string ThingJid, string NodeId, string SourceId, string Partition, IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<remove xmlns='");
			Request.Append(NamespaceDiscovery);

			Request.Append("' jid='");
			Request.Append(XML.Encode(ThingJid));

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			Request.Append("'/>");

			this.client.SendIqSet(this.thingRegistryAddress, Request.ToString(), async (sender, e) =>
			{
				if (!(Callback is null))
				{
					try
					{
						await Callback(this, e);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, State);
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
			NodeEventHandler h = this.Removed;
			if (!(h is null))
			{
				try
				{
					await h(this, e2);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			e.IqResult(string.Empty);
		}

		/// <summary>
		/// Event raised when a node has been removed from the registry.
		/// </summary>
		public event NodeEventHandler Removed = null;

		/// <summary>
		/// Updates the meta-data about a thing in the Thing Registry. Only public things that have an owner can update its meta-data.
		/// Things that do not have an owner should call <see cref="RegisterThing(MetaDataTag[], RegistrationEventHandler, object)"/> to 
		/// update its meta-data in the Thing Registry.
		/// 
		/// Note: Meta information updated in this way will only overwrite tags provided in the request, and leave other tags previously 
		/// reported as is.
		/// </summary>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void UpdateThing(MetaDataTag[] MetaDataTags, UpdateEventHandler Callback, object State)
		{
			this.UpdateThing(string.Empty, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Updates the meta-data about a thing in the Thing Registry. Only public things that have an owner can update its meta-data.
		/// Things that do not have an owner should call <see cref="RegisterThing(string, MetaDataTag[], RegistrationEventHandler, object)"/> to 
		/// update its meta-data in the Thing Registry.
		/// 
		/// Note: Meta information updated in this way will only overwrite tags provided in the request, and leave other tags previously 
		/// reported as is.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void UpdateThing(string NodeId, MetaDataTag[] MetaDataTags, UpdateEventHandler Callback, object State)
		{
			this.UpdateThing(NodeId, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Updates the meta-data about a thing in the Thing Registry. Only public things that have an owner can update its meta-data.
		/// Things that do not have an owner should call <see cref="RegisterThing(string, string, MetaDataTag[], RegistrationEventHandler, object)"/> 
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
		public void UpdateThing(string NodeId, string SourceId, MetaDataTag[] MetaDataTags, UpdateEventHandler Callback, object State)
		{
			this.UpdateThing(NodeId, SourceId, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Updates the meta-data about a thing in the Thing Registry. Only public things that have an owner can update its meta-data.
		/// Things that do not have an owner should call <see cref="RegisterThing(string, string, string, MetaDataTag[], RegistrationEventHandler, object)"/> to 
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
		public void UpdateThing(string NodeId, string SourceId, string Partition, MetaDataTag[] MetaDataTags,
			UpdateEventHandler Callback, object State)
		{
			this.UpdateThing(NodeId, SourceId, Partition, string.Empty, MetaDataTags, Callback, State);
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
		public void UpdateThing(string NodeId, string SourceId, string Partition, string ThingJid, MetaDataTag[] MetaDataTags,
			UpdateEventHandler Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<update xmlns='");
			Request.Append(NamespaceDiscovery);

			if (!string.IsNullOrEmpty(ThingJid))
			{
				Request.Append("' jid='");
				Request.Append(XML.Encode(ThingJid));
			}

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			Request.Append("'>");

			string RegistryAddress = this.AddTags(Request, MetaDataTags, this.thingRegistryAddress);

			Request.Append("</update>");

			this.client.SendIqSet(RegistryAddress, Request.ToString(), async (sender, e) =>
			{
				if (!(Callback is null))
				{
					XmlElement E = e.FirstElement;
					bool Disowned = false;

					if (e.Ok && E != null && E.LocalName == "disowned" && E.NamespaceURI == NamespaceDiscovery)
					{
						Disowned = true;

						if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(Partition) &&
							this.client.TryGetExtension(typeof(ProvisioningClient), out IXmppExtension Extension) &&
							Extension is ProvisioningClient ProvisioningClient)
						{
							ProvisioningClient.OwnerJid = string.Empty;
						}
					}

					UpdateEventArgs e2 = new UpdateEventArgs(e, State, Disowned);

					try
					{
						await Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void Unregister(IqResultEventHandlerAsync Callback, object State)
		{
			this.Unregister(string.Empty, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void Unregister(string NodeId, IqResultEventHandlerAsync Callback, object State)
		{
			this.Unregister(NodeId, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void Unregister(string NodeId, string SourceId, IqResultEventHandlerAsync Callback, object State)
		{
			this.Unregister(NodeId, SourceId, string.Empty, Callback, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="Partition">Partition of thing, if behind a concentrator.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void Unregister(string NodeId, string SourceId, string Partition, IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<unregister xmlns='");
			Request.Append(NamespaceDiscovery);

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			Request.Append("'/>");

			this.client.SendIqSet(this.thingRegistryAddress, Request.ToString(), async (sender, e) =>
			{
				if (!(Callback is null))
				{
					try
					{
						await Callback(this, e);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, State);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Disown(string ThingJid, IqResultEventHandlerAsync Callback, object State)
		{
			this.Disown(ThingJid, string.Empty, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Disown(string ThingJid, string NodeId, IqResultEventHandlerAsync Callback, object State)
		{
			this.Disown(ThingJid, NodeId, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Disown(string ThingJid, string NodeId, string SourceId, IqResultEventHandlerAsync Callback, object State)
		{
			this.Disown(ThingJid, NodeId, SourceId, string.Empty, Callback, State);
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
		public void Disown(string ThingJid, string NodeId, string SourceId, string Partition, IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<disown xmlns='");
			Request.Append(NamespaceDiscovery);

			Request.Append("' jid='");
			Request.Append(XML.Encode(ThingJid));

			this.AddNodeInfo(Request, NodeId, SourceId, Partition);

			Request.Append("'/>");

			this.client.SendIqSet(this.thingRegistryAddress, Request.ToString(), async (sender, e) =>
			{
				if (!(Callback is null))
				{
					try
					{
						await Callback(this, e);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, State);
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

				if (this.client.TryGetExtension(typeof(ProvisioningClient), out IXmppExtension Extension) &&
					Extension is ProvisioningClient ProvisioningClient)
				{
					ProvisioningClient.OwnerJid = string.Empty;
				}
			}
			else
				Node = new ThingReference(NodeId, SourceId, Partition);

			NodeEventArgs e2 = new NodeEventArgs(e, Node);
			NodeEventHandler h = this.Disowned;
			if (!(h is null))
			{
				try
				{
					await h(this, e2);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			e.IqResult(string.Empty);
		}

		/// <summary>
		/// Event raised when a node has been disowned.
		/// </summary>
		public event NodeEventHandler Disowned = null;

		/// <summary>
		/// Searches for publically available things in the thing registry.
		/// </summary>
		/// <param name="Offset">Search offset.</param>
		/// <param name="MaxCount">Maximum number of things to return.</param>
		/// <param name="SearchOperators">Search operators to use in search.</param>
		/// <param name="Callback">Method to call when result has been received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Search(int Offset, int MaxCount, SearchOperator[] SearchOperators, SearchResultEventHandler Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<search xmlns='");
			Request.Append(NamespaceDiscovery);
			Request.Append("' offset='");
			Request.Append(Offset.ToString());
			Request.Append("' maxCount='");
			Request.Append(MaxCount.ToString());
			Request.Append("'>");

			foreach (SearchOperator Operator in SearchOperators)
				Operator.Serialize(Request);

			Request.Append("</search>");

			this.client.SendIqGet(this.thingRegistryAddress, Request.ToString(), (sender, e) =>
			{
				ParseResultSet(Offset, MaxCount, this, e, Callback, State);
				return Task.CompletedTask;
			}, State);
		}

		internal static void ParseResultSet(int Offset, int MaxCount, object Sender, IqResultEventArgs e, SearchResultEventHandler Callback, object State)
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

			if (e.Ok && E != null && E.LocalName == "found" && E.NamespaceURI == NamespaceDiscovery)
			{
				More = XML.Attribute(E, "more", false);

				foreach (XmlNode N in E.ChildNodes)
				{
					E2 = N as XmlElement;
					if (E2.LocalName == "thing" && E2.NamespaceURI == NamespaceDiscovery)
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
					Log.Critical(ex);
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

			if (!First)
				Result.Append(';');

			if (!ContainsR)
			{
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

			foreach (SearchOperator Op in Operators)
			{
				if (Op is StringTagEqualTo S)
				{
					if (!HasKey && S.Name.ToUpper() == "KEY")
						HasKey = true;
				}
				else if (!(Op is NumericTagEqualTo))
					return false;
			}

			return HasKey;
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
				return !IsIoTDiscoClaimURI(Operators);
		}

	}
}
