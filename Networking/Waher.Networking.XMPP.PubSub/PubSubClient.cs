using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Client managing communication with a Publish/Subscribe component.
	/// https://xmpp.org/extensions/xep-0060.html
	/// </summary>
	public class PubSubClient : XmppExtension
	{
		/// <summary>
		/// http://jabber.org/protocol/pubsub
		/// </summary>
		public const string NamespacePubSub = "http://jabber.org/protocol/pubsub";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#owner
		/// </summary>
		public const string NamespacePubSubOwner = "http://jabber.org/protocol/pubsub#owner";

		private string componentAddress;

		/// <summary>
		/// Client managing communication with a Publish/Subscribe component.
		/// https://xmpp.org/extensions/xep-0060.html
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		/// <param name="ComponentAddress">Address to the Publish/Subscribe component.</param>
		public PubSubClient(XmppClient Client, string ComponentAddress)
			: base(Client)
		{
			this.componentAddress = ComponentAddress;
		}

		/// <summary>
		/// Publish/Subscribe component address.
		/// </summary>
		public string ComponentAddress
		{
			get { return this.componentAddress; }
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0060" };

		#region Create Node

		/// <summary>
		/// Creates a node on the server.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void CreateNode(string Name, NodeEventHandler Callback, object State)
		{
			this.CreateNode(Name, (DataForm)null, Callback, State);
		}

		/// <summary>
		/// Creates a node on the server.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Configuration">Node configuration.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void CreateNode(string Name, NodeConfiguration Configuration,
			NodeEventHandler Callback, object State)
		{
			this.CreateNode(Name, Configuration.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Creates a node on the server.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Configuration">Node configuration.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void CreateNode(string Name, DataForm Configuration,
			NodeEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><create node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'/>");

			if (Configuration != null)
			{
				Xml.Append("<configure>");
				Configuration.SerializeSubmit(Xml);
				Xml.Append("</configure>");
			}

			Xml.Append("</pubsub>");

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				try
				{
					Callback?.Invoke(this, new NodeEventArgs(Name, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}, State);
		}

		#endregion

		#region Configure Node

		/// <summary>
		/// Gets the configuration for a node.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetNodeConfiguration(string Name, ConfigurationEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><configure node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'/>");
			Xml.Append("</pubsub>");

			this.client.SendIqGet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				XmlElement E;
				DataForm Form = null;
				NodeConfiguration Configuration = null;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "pubsub" &&
					E.NamespaceURI == NamespacePubSubOwner)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "configure" &&
							XML.Attribute(E2, "node") == Name)
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "x")
								{
									Form = new DataForm(this.client, E3, this.ConfigureNode, this.CancelConfiguration, e.From, e.To)
									{
										State = Name
									};
								}
							}
						}
					}
				}

				if (Form == null)
					e.Ok = false;
				else
					Configuration = new NodeConfiguration(Form);

				try
				{
					Callback?.Invoke(this, new ConfigurationEventArgs(Name, Configuration,
						new DataFormEventArgs(Form, e)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, State);
		}

		private void ConfigureNode(object Sender, DataForm Form)
		{
			string Name = (string)Form.State;
			this.ConfigureNode(Name, Form, null, null);
		}

		private void CancelConfiguration(object Sender, DataForm Form)
		{
			string Name = (string)Form.State;
			this.CancelNodeConfiguration(Name, null, null);
		}

		/// <summary>
		/// Configures a publish/subscribe node.
		/// </summary>
		/// <param name="Name">Node name</param>
		/// <param name="Configuration">Configuration</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public void ConfigureNode(string Name, NodeConfiguration Configuration,
			IqResultEventHandler Callback, object State)
		{
			this.ConfigureNode(Name, Configuration.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Configures a publish/subscribe node.
		/// </summary>
		/// <param name="Name">Node name</param>
		/// <param name="Configuration">Configuration</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public void ConfigureNode(string Name, DataForm Configuration,
			IqResultEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><configure node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'>");

			Configuration?.SerializeSubmit(Xml);

			Xml.Append("</configure></pubsub>");

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				try
				{
					Callback?.Invoke(this, new NodeEventArgs(Name, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}, State);
		}

		/// <summary>
		/// Cancels a node configuration.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public void CancelNodeConfiguration(string Name, IqResultEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><configure node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'><x xmlns='");
			Xml.Append(XmppClient.NamespaceData);
			Xml.Append("' type='cancel'/></configure></pubsub>");

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				try
				{
					Callback?.Invoke(this, new NodeEventArgs(Name, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}, State);
		}

		/// <summary>
		/// Gets the default configuration for a node.
		/// </summary>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetDefaultNodeConfiguration(ConfigurationEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><default/>");
			Xml.Append("</pubsub>");

			this.client.SendIqGet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				XmlElement E;
				DataForm Form = null;
				NodeConfiguration Configuration = null;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "pubsub" &&
					E.NamespaceURI == NamespacePubSubOwner)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "default")
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "x")
									Form = new DataForm(this.client, E3, this.ConfigureNode, this.CancelConfiguration, e.From, e.To);
							}
						}
					}
				}

				if (Form == null)
					e.Ok = false;
				else
					Configuration = new NodeConfiguration(Form);

				try
				{
					Callback?.Invoke(this, new ConfigurationEventArgs(string.Empty, Configuration,
						new DataFormEventArgs(Form, e)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, State);
		}

		/// <summary>
		/// Deletes a node.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public void DeleteNode(string Name, NodeEventHandler Callback, object State)
		{
			this.DeleteNode(Name, null, Callback, State);
		}

		/// <summary>
		/// Deletes a node.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="RedirectUrl">Redirection URL.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public void DeleteNode(string Name, string RedirectUrl, NodeEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><delete node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'>");

			if (!string.IsNullOrEmpty(RedirectUrl))
			{
				Xml.Append("<redirect uri='");
				Xml.Append(XML.Encode(RedirectUrl));
				Xml.Append("'/>");
			}

			Xml.Append("</delete></pubsub>");

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				try
				{
					Callback?.Invoke(this, new NodeEventArgs(Name, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}, State);
		}

		#endregion

		#region Subscribe to node

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Subscribe(string NodeName, SubscriptionEventHandler Callback, object State)
		{
			this.Subscribe(NodeName, null, (DataForm)null, Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Subscribe(string NodeName, string Jid, SubscriptionEventHandler Callback, object State)
		{
			this.Subscribe(NodeName, Jid, (DataForm)null, Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Options">Subscription options.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Subscribe(string NodeName, SubscriptionOptions Options,
			SubscriptionEventHandler Callback, object State)
		{
			this.Subscribe(NodeName, null, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Options">Subscription options.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Subscribe(string NodeName, DataForm Options,
			SubscriptionEventHandler Callback, object State)
		{
			this.Subscribe(NodeName, null, Options, Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Options">Subscription options.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Subscribe(string NodeName, string Jid, SubscriptionOptions Options,
			SubscriptionEventHandler Callback, object State)
		{
			this.Subscribe(NodeName, Jid, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Options">Subscription options.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Subscribe(string NodeName, string Jid, DataForm Options,
			SubscriptionEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			if (string.IsNullOrEmpty(Jid))
				Jid = this.client.BareJID;

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><subscribe node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(Jid));
			Xml.Append("'/>");

			if (Options != null)
			{
				Xml.Append("<options node='");
				Xml.Append(XML.Encode(NodeName));
				Xml.Append("' jid='");
				Xml.Append(XML.Encode(Jid));
				Xml.Append("'>");
				Options.SerializeSubmit(Xml);
				Xml.Append("</options>");
			}

			Xml.Append("</pubsub>");

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				SubscriptionOptions Options2 = null;
				OptionsAvailability Availability = OptionsAvailability.Unknown;
				NodeSubscriptionStatus Status = NodeSubscriptionStatus.none;
				DateTime Expires = DateTime.MaxValue;
				DataForm Form = null;
				XmlElement E;
				string SubscriptionId = string.Empty;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "pubsub" && E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.NamespaceURI == NamespacePubSub)
						{
							switch (E2.LocalName)
							{
								case "subscription":
									if (XML.Attribute(E2, "node") != NodeName)
									{
										e.Ok = false;
										break;
									}

									if (XML.Attribute(E2, "jid") != Jid)
									{
										e.Ok = false;
										break;
									}

									Status = (NodeSubscriptionStatus)XML.Attribute(E2, "subscription", NodeSubscriptionStatus.none);
									Expires = XML.Attribute(E2, "expiry", DateTime.MaxValue);
									SubscriptionId = XML.Attribute(E2, "subid");

									foreach (XmlNode N2 in E2.ChildNodes)
									{
										if (N2 is XmlElement E3)
										{
											switch (E3.LocalName)
											{
												case "subscribe-options":
													Availability = OptionsAvailability.Supported;
													foreach (XmlNode N3 in E3.ChildNodes)
													{
														if (N3.LocalName == "required")
															Availability = OptionsAvailability.Required;
													}
													break;

												case "options":
													Availability = OptionsAvailability.Supported;
													foreach (XmlNode N3 in E3.ChildNodes)
													{
														if (N3 is XmlElement E4 && E4.LocalName == "x")
														{
															Form = new DataForm(this.client, E4, this.SubmitSubscribeOptions, this.CancelSubscribeOptions, e.From, e.To)
															{
																State = new object[] { NodeName, Jid, SubscriptionId }
															};
															Options2 = new SubscriptionOptions(Form);
														}
													}
													break;
											}
										}
									}
									break;

								case "options":
									Availability = OptionsAvailability.Supported;
									foreach (XmlNode N3 in E2.ChildNodes)
									{
										if (N3 is XmlElement E4 && E4.LocalName == "x")
										{
											Form = new DataForm(this.client, E4, this.SubmitSubscribeOptions, this.CancelSubscribeOptions, e.From, e.To)
											{
												State = new object[] { NodeName, Jid, SubscriptionId }
											};
											Options2 = new SubscriptionOptions(Form);
										}
									}
									break;
							}
						}
					}
				}
				else
					e.Ok = false;

				try
				{
					Callback?.Invoke(this, new SubscriptionEventArgs(NodeName, Jid, SubscriptionId,
						Options2, Availability, Expires, new DataFormEventArgs(Form, e)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, null);
		}

		private void SubmitSubscribeOptions(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			string NodeName = (string)P[0];
			string Jid = (string)P[1];
			string SubscrriptionId = (string)P[2];

			this.SetSubscriptionOptions(NodeName, Jid, SubscrriptionId, Form, null, null);
		}

		private void CancelSubscribeOptions(object Sender, DataForm Form)
		{
			// Do nothing.
		}

		#endregion

		#region Subscription options

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetSubscriptionOptions(string NodeName, 
			SubscriptionOptionsEventHandler Callback, object State)
		{
			this.GetSubscriptionOptions(NodeName, null, null, Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetSubscriptionOptions(string NodeName, string Jid, 
			SubscriptionOptionsEventHandler Callback, object State)
		{
			this.GetSubscriptionOptions(NodeName, Jid, null, Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetSubscriptionOptions(string NodeName, string Jid, string SubscriptionId,
			SubscriptionOptionsEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			if (string.IsNullOrEmpty(Jid))
				Jid = this.client.BareJID;

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><options node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(Jid));

			if (!string.IsNullOrEmpty(SubscriptionId))
			{
				Xml.Append("' subid='");
				Xml.Append(XML.Encode(SubscriptionId));
			}

			Xml.Append("'/></pubsub>");

			this.client.SendIqGet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				XmlElement E;
				DataForm Form = null;
				SubscriptionOptions Options = null;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "pubsub" &&
					E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "options" &&
							XML.Attribute(E2, "node") == NodeName &&
							XML.Attribute(E2, "jid") == Jid)
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "x")
								{
									Form = new DataForm(this.client, E3, this.SubmitSubscribeOptions, this.CancelSubscribeOptions, e.From, e.To)
									{
										State = new object[] { NodeName, Jid }
									};
								}
							}
						}
					}
				}

				if (Form == null)
					e.Ok = false;
				else
					Options = new SubscriptionOptions(Form);

				try
				{
					Callback?.Invoke(this, new SubscriptionOptionsEventArgs(NodeName, Jid, Options,
						new DataFormEventArgs(Form, e)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, null);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SetSubscriptionOptions(string NodeName, SubscriptionOptions Options,
			SubscriptionOptionsEventHandler Callback, object State)
		{
			this.SetSubscriptionOptions(NodeName, null, null, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SetSubscriptionOptions(string NodeName, string Jid, SubscriptionOptions Options,
			SubscriptionOptionsEventHandler Callback, object State)
		{
			this.SetSubscriptionOptions(NodeName, Jid, null, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SetSubscriptionOptions(string NodeName, DataForm Options,
			SubscriptionOptionsEventHandler Callback, object State)
		{
			this.SetSubscriptionOptions(NodeName, null, null, Options, Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SetSubscriptionOptions(string NodeName, string Jid, 
			DataForm Options, SubscriptionOptionsEventHandler Callback, object State)
		{
			this.SetSubscriptionOptions(NodeName, Jid, null, Options, Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SetSubscriptionOptions(string NodeName, string Jid, string SubscriptionId, SubscriptionOptions Options,
			SubscriptionOptionsEventHandler Callback, object State)
		{
			this.SetSubscriptionOptions(NodeName, Jid, SubscriptionId, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SetSubscriptionOptions(string NodeName, string Jid, string SubscriptionId, 
			DataForm Options, SubscriptionOptionsEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			if (string.IsNullOrEmpty(Jid))
				Jid = this.client.BareJID;

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><options node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(Jid));

			if (!string.IsNullOrEmpty(SubscriptionId))
			{
				Xml.Append("' subid='");
				Xml.Append(XML.Encode(SubscriptionId));
			}

			Xml.Append("'>");
			Options.SerializeSubmit(Xml);
			Xml.Append("</options></pubsub>");

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				try
				{
					Callback?.Invoke(this, new SubscriptionOptionsEventArgs(NodeName, Jid,
						new SubscriptionOptions(Options), new DataFormEventArgs(Options, e)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, null);
		}

		/// <summary>
		/// Gets default subscription options.
		/// </summary>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetDefaultSubscriptionOptions(SubscriptionOptionsEventHandler Callback, object State)
		{
			this.GetDefaultSubscriptionOptions(null, Callback, State);
		}

		/// <summary>
		/// Gets default subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetDefaultSubscriptionOptions(string NodeName, SubscriptionOptionsEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><default");

			if (!string.IsNullOrEmpty(NodeName))
			{
				Xml.Append(" node ='");
				Xml.Append(XML.Encode(NodeName));
				Xml.Append('\'');
			}

			Xml.Append("/></pubsub>");

			this.client.SendIqGet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				XmlElement E;
				DataForm Form = null;
				SubscriptionOptions Options = null;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "pubsub" &&
					E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "default" &&
							XML.Attribute(E2, "node", NodeName) == NodeName)
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "x")
									Form = new DataForm(this.client, E3, null, null, e.From, e.To);
							}
						}
					}
				}

				if (Form == null)
					e.Ok = false;
				else
					Options = new SubscriptionOptions(Form);

				try
				{
					Callback?.Invoke(this, new SubscriptionOptionsEventArgs(NodeName, null, Options,
						new DataFormEventArgs(Form, e)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, null);
		}

		#endregion

		#region Unsubscribe from node

		/// <summary>
		/// Unsubscribes from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Unsubscribe(string NodeName, SubscriptionEventHandler Callback, object State)
		{
			this.Unsubscribe(NodeName, null, null, Callback, State);
		}

		/// <summary>
		/// Unsubscribes from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Unsubscribe(string NodeName, string Jid, SubscriptionEventHandler Callback, object State)
		{
			this.Unsubscribe(NodeName, Jid, null, Callback, State);
		}

		/// <summary>
		/// Unsubscribes from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Unsubscribe(string NodeName, string Jid, string SubscriptionId,
			SubscriptionEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			if (string.IsNullOrEmpty(Jid))
				Jid = this.client.BareJID;

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><unsubscribe node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(Jid));

			if (!string.IsNullOrEmpty(SubscriptionId))
			{
				Xml.Append("' subid='");
				Xml.Append(XML.Encode(SubscriptionId));
			}

			Xml.Append("'/></pubsub>");

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), (sender, e) =>
			{
				NodeSubscriptionStatus Status = NodeSubscriptionStatus.none;
				DateTime Expires = DateTime.MaxValue;
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "pubsub" && E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.NamespaceURI == NamespacePubSub)
						{
							switch (E2.LocalName)
							{
								case "subscription":
									if (XML.Attribute(E2, "node") != NodeName)
									{
										e.Ok = false;
										break;
									}

									if (XML.Attribute(E2, "jid") != Jid)
									{
										e.Ok = false;
										break;
									}

									if (!string.IsNullOrEmpty(SubscriptionId) && 
										XML.Attribute(E2, "subid", SubscriptionId) != SubscriptionId)
									{
										e.Ok = false;
										break;
									}

									Status = (NodeSubscriptionStatus)XML.Attribute(E2, "subscription", NodeSubscriptionStatus.none);
									Expires = XML.Attribute(E2, "expiry", DateTime.MaxValue);
									SubscriptionId = XML.Attribute(E2, "subid");
									break;
							}
						}
					}
				}
				else
					e.Ok = false;

				try
				{
					Callback?.Invoke(this, new SubscriptionEventArgs(NodeName, Jid, SubscriptionId,
						null, OptionsAvailability.Unknown, Expires, new DataFormEventArgs(Form, e)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, null);
		}

		#endregion

	}
}
