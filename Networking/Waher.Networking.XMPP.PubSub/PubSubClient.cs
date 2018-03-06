using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
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
				Callback?.Invoke(this, new NodeEventArgs(Name, e));
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
				
				Callback?.Invoke(this, new ConfigurationEventArgs(Name, Configuration, 
					new DataFormEventArgs(Form, e)));

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
				Callback?.Invoke(this, new NodeEventArgs(Name, e));
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
				Callback?.Invoke(this, new NodeEventArgs(Name, e));
			}, State);
		}

		#endregion

	}
}
