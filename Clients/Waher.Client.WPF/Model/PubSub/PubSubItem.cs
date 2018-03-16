using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.PubSub;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Things;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;
using Waher.Client.WPF.Dialogs;

namespace Waher.Client.WPF.Model.PubSub
{
	/// <summary>
	/// Represents a node in a Publish/Subscribe service.
	/// </summary>
	public class PubSubItem : TreeNode
	{
		private DisplayableParameters parameters;
		private XmlDocument xml;
		private DateTime? published = null;
		private string jid = null;
		private string node = null;
		private string itemId = null;
		private string payload = null;
		private string publisher = null;
		private string title = null;
		private string summary = null;
		private string link = null;

		public PubSubItem(TreeNode Parent, string Jid, string Node, string ItemId, string Payload, string Publisher)
			: base(Parent)
		{
			XmlElement E;

			this.jid = Jid;
			this.node = Node;
			this.itemId = ItemId;
			this.payload = Payload;
			this.publisher = Publisher;

			this.xml = new XmlDocument();

			try
			{
				this.xml.LoadXml(Payload);

				if (this.xml != null && (E = this.xml.DocumentElement) != null)
				{
					if (E.LocalName == "entry" && E.NamespaceURI == "http://www.w3.org/2005/Atom")
					{
						foreach (XmlNode N in E.ChildNodes)
						{
							if (N is XmlElement E2)
							{
								switch (E2.LocalName.ToLower())
								{
									case "title":
										this.title = E2.InnerText;
										break;

									case "summary":
										this.summary = E2.InnerText;
										break;

									case "published":
										if (XML.TryParse(E2.InnerText, out DateTime TP))
											this.published = TP;
										break;

									case "link":
										this.link = XML.Attribute(E2, "href");
										break;
								}
							}
						}
					}
				}

			}
			catch (Exception)
			{
				this.xml = null;	// Not XML payload.
			}

			List<Parameter> Parameters = new List<Parameter>();

			if (!string.IsNullOrEmpty(this.jid))
				Parameters.Add(new StringParameter("JID", "JID", this.jid));

			if (!string.IsNullOrEmpty(this.node))
				Parameters.Add(new StringParameter("Node", "Node", this.node));

			if (!string.IsNullOrEmpty(this.publisher))
				Parameters.Add(new StringParameter("Publisher", "Publisher", this.publisher));

			if (!string.IsNullOrEmpty(this.title))
				Parameters.Add(new StringParameter("Title", "Title", this.title));

			if (published != null)
				Parameters.Add(new DateTimeParameter("Published", "Published", this.published.Value));

			this.parameters = new DisplayableParameters(Parameters.ToArray());
		}

		public override string Key => this.itemId;
		public override string Header => this.itemId;
		public override string ToolTip => "Item ID " + this.itemId;
		public override bool CanRecycle => false;
		public override DisplayableParameters DisplayableParameters => this.parameters;

		public override string TypeName
		{
			get
			{
				return "Publish/Subscribe Item";
			}
		}

		public override ImageSource ImageResource
		{
			get
			{
				return XmppAccountNode.box;
			}
		}

		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}


		public PubSubService Service
		{
			get
			{
				TreeNode Loop = this.Parent;

				while (Loop != null)
				{
					if (Loop is PubSubService PubSubService)
						return PubSubService;

					Loop = Loop.Parent;
				}

				return null;
			}
		}
		public override bool CanAddChildren => false;   
		public override bool CanDelete => false;    // TODO
		public override bool CanEdit => !string.IsNullOrEmpty(this.link);

		public override void Edit()
		{
			if (!string.IsNullOrEmpty(this.link))
			{
				try
				{
					System.Diagnostics.Process.Start(this.link);
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox("Unable to open link: " + ex.Message);
				}
			}
		}

	}
}
