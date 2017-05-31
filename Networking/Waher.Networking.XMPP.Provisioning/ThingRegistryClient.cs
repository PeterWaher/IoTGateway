using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Networking.XMPP.Provisioning.SearchOperators;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Delegate for registration callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void RegistrationEventHandler(object Sender, RegistrationEventArgs e);

	/// <summary>
	/// Delegate for update callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void UpdateEventHandler(object Sender, UpdateEventArgs e);

	/// <summary>
	/// Delegate for node result callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void NodeResultEventHandler(object Sender, NodeResultEventArgs e);

	/// <summary>
	/// Delegate for node events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void NodeEventHandler(object Sender, NodeEventArgs e);

	/// <summary>
	/// Delegate for claimed events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ClaimedEventHandler(object Sender, ClaimedEventArgs e);

	/// <summary>
	/// Delegate for search result event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SearchResultEventHandler(object Sender, SearchResultEventArgs e);

	/// <summary>
	/// Implements an XMPP thing registry client interface.
	/// 
	/// The interface is defined in XEP-0347:
	/// http://xmpp.org/extensions/xep-0347.html
	/// </summary>
	public class ThingRegistryClient : IDisposable
	{
		private XmppClient client;
		private string thingRegistryAddress;

		/// <summary>
		/// urn:xmpp:iot:discovery
		/// </summary>
		public const string NamespaceDiscovery = "urn:xmpp:iot:discovery";

		/// <summary>
		/// Implements an XMPP provisioning client interface.
		/// 
		/// The interface is defined in XEP-0324:
		/// http://xmpp.org/extensions/xep-0324.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ThingRegistryAddress">Thing Registry XMPP address.</param>
		public ThingRegistryClient(XmppClient Client, string ThingRegistryAddress)
		{
			this.client = Client;
			this.thingRegistryAddress = ThingRegistryAddress;

			this.client.RegisterIqSetHandler("claimed", NamespaceDiscovery, this.ClaimedHandler, true);
			this.client.RegisterIqSetHandler("removed", NamespaceDiscovery, this.RemovedHandler, false);
			this.client.RegisterIqSetHandler("disowned", NamespaceDiscovery, this.DisownedHandler, false);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterIqSetHandler("claimed", NamespaceDiscovery, this.ClaimedHandler, true);
			this.client.UnregisterIqSetHandler("removed", NamespaceDiscovery, this.RemovedHandler, false);
			this.client.UnregisterIqSetHandler("disowned", NamespaceDiscovery, this.DisownedHandler, false);
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// Thing Registry XMPP address.
		/// </summary>
		public string ThingRegistryAddress
		{
			get { return this.thingRegistryAddress; }
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="Update"/> to update its meta-data in the Thing Registry, if the meta-data
		/// has changed.
		/// </summary>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		public void RegisterThing(MetaDataTag[] MetaDataTags, RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(false, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="Update"/> to update its meta-data in the Thing Registry, if the meta-data
		/// has changed.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		public void RegisterThing(string NodeId, MetaDataTag[] MetaDataTags, RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(false, NodeId, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="Update"/> to update its meta-data in the Thing Registry, if the meta-data
		/// has changed.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		public void RegisterThing(string NodeId, string SourceId, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(false, NodeId, SourceId, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="Update"/> to update its meta-data in the Thing Registry, if the meta-data
		/// has changed.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="CacheType">Cache Type of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		public void RegisterThing(string NodeId, string SourceId, string CacheType, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(false, NodeId, SourceId, CacheType, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="Update"/> to update its meta-data in the Thing Registry, if the meta-data
		/// has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		public void RegisterThing(bool SelfOwned, MetaDataTag[] MetaDataTags, RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(SelfOwned, string.Empty, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="Update"/> to update its meta-data in the Thing Registry, if the meta-data
		/// has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		public void RegisterThing(bool SelfOwned, string NodeId, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(SelfOwned, NodeId, string.Empty, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="Update"/> to update its meta-data in the Thing Registry, if the meta-data
		/// has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		public void RegisterThing(bool SelfOwned, string NodeId, string SourceId, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			this.RegisterThing(SelfOwned, NodeId, SourceId, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Registers a thing in the Thing Registry. Only things that does not have an owner can register with the Thing Registry.
		/// Things that have an owner should call <see cref="Update"/> to update its meta-data in the Thing Registry, if the meta-data
		/// has changed.
		/// </summary>
		/// <param name="SelfOwned">If the thing is owned by itself.</param>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="CacheType">Cache Type of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void RegisterThing(bool SelfOwned, string NodeId, string SourceId, string CacheType, MetaDataTag[] MetaDataTags,
			RegistrationEventHandler Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<register xmlns='");
			Request.Append(NamespaceDiscovery);

			this.AddNodeInfo(Request, NodeId, SourceId, CacheType);

			if (SelfOwned)
				Request.Append("' selfOwned='true");

			Request.Append("'>");

			this.AddTags(Request, MetaDataTags);

			Request.Append("</register>");

			this.client.SendIqSet(this.thingRegistryAddress, Request.ToString(), (sender, e) =>
			{
				if (Callback != null)
				{
					XmlElement E = e.FirstElement;
					string OwnerJid = string.Empty;
					bool IsPublic = false;

					if (e.Ok && E != null && E.LocalName == "claimed" && E.NamespaceURI == NamespaceDiscovery)
					{
						OwnerJid = XML.Attribute(E, "jid");
						IsPublic = XML.Attribute(E, "public", false);
					}

					RegistrationEventArgs e2 = new RegistrationEventArgs(e, State, OwnerJid, IsPublic);

					try
					{
						Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, null);
		}

		private void AddNodeInfo(StringBuilder Request, string NodeId, string SourceId, string CacheType)
		{
			if (!string.IsNullOrEmpty(NodeId))
			{
				Request.Append("' nodeId='");
				Request.Append(XML.Encode(NodeId));
			}

			if (!string.IsNullOrEmpty(SourceId))
			{
				Request.Append("' sourceId='");
				Request.Append(XML.Encode(SourceId));
			}

			if (!string.IsNullOrEmpty(CacheType))
			{
				Request.Append("' cacheType='");
				Request.Append(XML.Encode(CacheType));
			}
		}

		private void AddTags(StringBuilder Request, MetaDataTag[] MetaDataTags)
		{
			foreach (MetaDataTag Tag in MetaDataTags)
			{
				if (Tag is MetaDataStringTag)
				{
					Request.Append("<str name='");
					Request.Append(XML.Encode(Tag.Name));
					Request.Append("' value='");
					Request.Append(XML.Encode((string)Tag.Value));
					Request.Append("'/>");
				}
				else if (Tag is MetaDataNumericTag)
				{
					Request.Append("<num name='");
					Request.Append(XML.Encode(Tag.Name));
					Request.Append("' value='");
					Request.Append(CommonTypes.Encode((double)Tag.Value));
					Request.Append("'/>");
				}
			}
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

			this.AddTags(Request, MetaDataTags);

			Request.Append("</mine>");

			this.client.SendIqSet(this.thingRegistryAddress, Request.ToString(), (sender, e) =>
			{
				if (Callback != null)
				{
					XmlElement E = e.FirstElement;
					string NodeJid = string.Empty;
					ThingReference Node = ThingReference.Empty;

					if (e.Ok && E != null && E.LocalName == "claimed" && E.NamespaceURI == NamespaceDiscovery)
					{
						string NodeId = XML.Attribute(E, "nodeId");
						string SourceId = XML.Attribute(E, "sourceId");
						string CacheType = XML.Attribute(E, "cacheType");
						NodeJid = XML.Attribute(E, "jid");

						if (!string.IsNullOrEmpty(NodeId) || !string.IsNullOrEmpty(SourceId) || !string.IsNullOrEmpty(CacheType))
							Node = new ThingReference(NodeId, SourceId, CacheType);
					}

					NodeResultEventArgs e2 = new NodeResultEventArgs(e, State, NodeJid, Node);

					try
					{
						Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, null);
		}

		private void ClaimedHandler(object Sender, IqEventArgs e)
		{
			XmlElement E = e.Query;
			string OwnerJid = XML.Attribute(E, "jid");
			string NodeId = XML.Attribute(E, "nodeId");
			string SourceId = XML.Attribute(E, "sourceId");
			string CacheType = XML.Attribute(E, "cacheType");
			bool Public = XML.Attribute(E, "public", false);
			ThingReference Node;

			if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(CacheType))
				Node = ThingReference.Empty;
			else
				Node = new ThingReference(NodeId, SourceId, CacheType);

			ClaimedEventArgs e2 = new ClaimedEventArgs(e, Node, OwnerJid, Public);
			ClaimedEventHandler h = this.Claimed;
			if (h != null)
			{
				try
				{
					h(this, e2);
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
		public void Remove(string ThingJid, IqResultEventHandler Callback, object State)
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
		public void Remove(string ThingJid, string NodeId, IqResultEventHandler Callback, object State)
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
		public void Remove(string ThingJid, string NodeId, string SourceId, IqResultEventHandler Callback, object State)
		{
			this.Remove(ThingJid, NodeId, SourceId, string.Empty, Callback, State);
		}

		/// <summary>
		/// Removes a publicly claimed thing from the thing registry, so that it does not appear in search results.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="CacheType">Optional Cache Type of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Remove(string ThingJid, string NodeId, string SourceId, string CacheType, IqResultEventHandler Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<remove xmlns='");
			Request.Append(NamespaceDiscovery);

			Request.Append("' jid='");
			Request.Append(XML.Encode(ThingJid));

			this.AddNodeInfo(Request, NodeId, SourceId, CacheType);

			Request.Append("'/>");

			this.client.SendIqSet(this.thingRegistryAddress, Request.ToString(), (sender, e) =>
			{
				if (Callback != null)
				{
					try
					{
						Callback(this, e);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, null);
		}

		private void RemovedHandler(object Sender, IqEventArgs e)
		{
			XmlElement E = e.Query;
			string NodeId = XML.Attribute(E, "nodeId");
			string SourceId = XML.Attribute(E, "sourceId");
			string CacheType = XML.Attribute(E, "cacheType");
			ThingReference Node;

			if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(CacheType))
				Node = ThingReference.Empty;
			else
				Node = new ThingReference(NodeId, SourceId, CacheType);

			NodeEventArgs e2 = new NodeEventArgs(e, Node);
			NodeEventHandler h = this.Removed;
			if (h != null)
			{
				try
				{
					h(this, e2);
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
		/// Things that do not have an owner should call <see cref="Register"/> to update its meta-data in the Thing Registry.
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
		/// Things that do not have an owner should call <see cref="Register"/> to update its meta-data in the Thing Registry.
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
		/// Things that do not have an owner should call <see cref="Register"/> to update its meta-data in the Thing Registry.
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
		/// Things that do not have an owner should call <see cref="Register"/> to update its meta-data in the Thing Registry.
		/// 
		/// Note: Meta information updated in this way will only overwrite tags provided in the request, and leave other tags previously 
		/// reported as is.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="CacheType">Cache Type of thing, if behind a concentrator.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void UpdateThing(string NodeId, string SourceId, string CacheType, MetaDataTag[] MetaDataTags,
			UpdateEventHandler Callback, object State)
		{
			this.UpdateThing(NodeId, SourceId, CacheType, string.Empty, MetaDataTags, Callback, State);
		}

		/// <summary>
		/// Updates the meta-data about a thing in the Thing Registry. Only public things that have an owner can update its meta-data.
		/// Things that do not have an owner should call <see cref="Register"/> to update its meta-data in the Thing Registry.
		/// 
		/// Note: Meta information updated in this way will only overwrite tags provided in the request, and leave other tags previously 
		/// reported as is.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="CacheType">Cache Type of thing, if behind a concentrator.</param>
		/// <param name="ThingJid">JID of thing. Required if an owner wants to update the meta-data about one of its things. Leave empty,
		/// if the thing wants to update its own meta-data.</param>
		/// <param name="MetaDataTags">Meta-data tags to register with the registry.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void UpdateThing(string NodeId, string SourceId, string CacheType, string ThingJid, MetaDataTag[] MetaDataTags,
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

			this.AddNodeInfo(Request, NodeId, SourceId, CacheType);

			Request.Append("'>");

			this.AddTags(Request, MetaDataTags);

			Request.Append("</update>");

			this.client.SendIqSet(this.thingRegistryAddress, Request.ToString(), (sender, e) =>
			{
				if (Callback != null)
				{
					XmlElement E = e.FirstElement;
					bool Disowned = false;

					if (e.Ok && E != null && E.LocalName == "disowned" && E.NamespaceURI == NamespaceDiscovery)
						Disowned = true;

					UpdateEventArgs e2 = new UpdateEventArgs(e, State, Disowned);

					try
					{
						Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, null);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void Unregister(IqResultEventHandler Callback, object State)
		{
			this.Unregister(string.Empty, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void Unregister(string NodeId, IqResultEventHandler Callback, object State)
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
		public void Unregister(string NodeId, string SourceId, IqResultEventHandler Callback, object State)
		{
			this.Unregister(NodeId, SourceId, string.Empty, Callback, State);
		}

		/// <summary>
		/// Unregisters a thing from the thing registry.
		/// </summary>
		/// <param name="NodeId">Node ID of thing, if behind a concentrator.</param>
		/// <param name="SourceId">Source ID of thing, if behind a concentrator.</param>
		/// <param name="CacheType">Cache Type of thing, if behind a concentrator.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object passed on to callback method.</param>
		public void Unregister(string NodeId, string SourceId, string CacheType, IqResultEventHandler Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<unregister xmlns='");
			Request.Append(NamespaceDiscovery);

			this.AddNodeInfo(Request, NodeId, SourceId, CacheType);

			Request.Append("'/>");

			this.client.SendIqSet(this.thingRegistryAddress, Request.ToString(), (sender, e) =>
			{
				if (Callback != null)
				{
					try
					{
						Callback(this, e);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, null);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Disown(string ThingJid, IqResultEventHandler Callback, object State)
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
		public void Disown(string ThingJid, string NodeId, IqResultEventHandler Callback, object State)
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
		public void Disown(string ThingJid, string NodeId, string SourceId, IqResultEventHandler Callback, object State)
		{
			this.Disown(ThingJid, NodeId, SourceId, string.Empty, Callback, State);
		}

		/// <summary>
		/// Disowns a thing, so that it can be claimed by another.
		/// </summary>
		/// <param name="ThingJid">JID of thing to disown.</param>
		/// <param name="NodeId">Optional Node ID of thing.</param>
		/// <param name="SourceId">Optional Source ID of thing.</param>
		/// <param name="CacheType">Optional Cache Type of thing.</param>
		/// <param name="Callback">Method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Disown(string ThingJid, string NodeId, string SourceId, string CacheType, IqResultEventHandler Callback, object State)
		{
			StringBuilder Request = new StringBuilder();

			Request.Append("<disown xmlns='");
			Request.Append(NamespaceDiscovery);

			Request.Append("' jid='");
			Request.Append(XML.Encode(ThingJid));

			this.AddNodeInfo(Request, NodeId, SourceId, CacheType);

			Request.Append("'/>");

			this.client.SendIqSet(this.thingRegistryAddress, Request.ToString(), (sender, e) =>
			{
				if (Callback != null)
				{
					try
					{
						Callback(this, e);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, null);
		}

		private void DisownedHandler(object Sender, IqEventArgs e)
		{
			XmlElement E = e.Query;
			string NodeId = XML.Attribute(E, "nodeId");
			string SourceId = XML.Attribute(E, "sourceId");
			string CacheType = XML.Attribute(E, "cacheType");
			ThingReference Node;

			if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(CacheType))
				Node = ThingReference.Empty;
			else
				Node = new ThingReference(NodeId, SourceId, CacheType);

			NodeEventArgs e2 = new NodeEventArgs(e, Node);
			NodeEventHandler h = this.Disowned;
			if (h != null)
			{
				try
				{
					h(this, e2);
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
				List<SearchResultThing> Things = new List<SearchResultThing>();
				List<MetaDataTag> MetaData = new List<MetaDataTag>();
				ThingReference Node;
				XmlElement E = e.FirstElement;
				XmlElement E2, E3;
				string Jid;
				string OwnerJid;
				string NodeId;
				string SourceId;
				string CacheType;
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
							OwnerJid = XML.Attribute(E2, "owner");
							NodeId = XML.Attribute(E2, "nodeId");
							SourceId = XML.Attribute(E2, "sourceId");
							CacheType = XML.Attribute(E2, "cacheType");

							if (string.IsNullOrEmpty(NodeId) && string.IsNullOrEmpty(SourceId) && string.IsNullOrEmpty(CacheType))
								Node = ThingReference.Empty;
							else
								Node = new ThingReference(NodeId, SourceId, CacheType);

							MetaData.Clear();
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								E3 = N2 as XmlElement;
								if (E3 == null)
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

							Things.Add(new SearchResultThing(Jid, OwnerJid, Node, MetaData.ToArray()));
						}
					}
				}

				if (Callback != null)
				{
					SearchResultEventArgs e2 = new SearchResultEventArgs(e, State, Offset, MaxCount, More, Things.ToArray());

					try
					{
						Callback(this, e2);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, null);
		}

		/// <summary>
		/// Generates an IOTDISCO URI from the meta-data provided in <paramref name="MetaData"/>.
		/// 
		/// For more information about the IOTDISCO URI scheme, see:
		/// <see cref="http://www.iana.org/assignments/uri-schemes/prov/iotdisco.pdf"/>
		/// </summary>
		/// <param name="MetaData">Meta-data to encode.</param>
		/// <returns>IOTDISCO URI encoding the meta-data.</returns>
		public string EncodeAsIoTDiscoURI(params MetaDataTag[] MetaData)
		{
			StringBuilder Result = new StringBuilder("iotdisco:");
			bool First = true;

			foreach (MetaDataTag Tag in MetaData)
			{
				if (First)
					First = false;
				else
					Result.Append(';');

				if (Tag is MetaDataNumericTag)
					Result.Append('#');

				Result.Append(Tag.Name);
				Result.Append('=');
				Result.Append(Tag.StringValue);
			}

			return Result.ToString();
		}

	}
}
