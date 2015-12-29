using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Class representing a normal XMPP account.
	/// </summary>
	public class XmppAccountNode : TreeNode
	{
		private XmppClient client;
		private Timer connectionTimer;
		private string host;
		private int port;
		private string account;
		private string password;
		private bool trustCertificate;
		private bool connected = false;

		/// <summary>
		/// Class representing a normal XMPP account.
		/// </summary>
		/// <param name="Parent">Parent node.</param>
		public XmppAccountNode(TreeNode Parent, string Host, int Port, string Account, string Password, bool TrustCertificate)
			: base(Parent)
		{
			this.host = Host;
			this.port = Port;
			this.account = Account;
			this.password = Password;
			this.trustCertificate = TrustCertificate;

			this.Init();
		}

		private void Init()
		{
			this.client = new XmppClient(this.host, this.port, this.account, this.password, "en");
			this.client.TrustServer = this.trustCertificate;
			this.client.OnStateChanged += new StateChangedEventHandler(client_OnStateChanged);
			this.connectionTimer = new Timer(this.CheckConnection, null, 60000, 60000);
		}

		private void client_OnStateChanged(XmppClient Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					this.connected = true;

					break;

				case XmppState.Offline:
					bool ImmediateReconnect = this.connected;
					this.connected = false;

					if (ImmediateReconnect)
						this.client.Reconnect();
					break;
			}
		}

		public string Host { get { return this.host; } }
		public int Port { get { return this.port; } }
		public string Account { get { return this.account; } }
		public string Password { get { return this.password; } }
		public bool TrustCertificate { get { return this.trustCertificate; } }

		public override string Header
		{
			get { return this.account + "@" + this.host; }
		}

		public override void Dispose()
		{
			base.Dispose();

			if (this.connectionTimer != null)
			{
				this.connectionTimer.Dispose();
				this.connectionTimer = null;
			}

			if (this.client != null)
			{
				this.client.Dispose();
				this.client = null;
			}
		}

		private void CheckConnection(object P)
		{
			if (this.client.State == XmppState.Offline || this.client.State == XmppState.Error || this.client.State == XmppState.Authenticating)
			{
				try
				{
					this.client.Reconnect();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		public override void Write(XmlWriter Output)
		{
			Output.WriteStartElement("XmppAccount");
			Output.WriteAttributeString("host", this.host);
			Output.WriteAttributeString("port", this.port.ToString());
			Output.WriteAttributeString("port", this.port.ToString());
			Output.WriteAttributeString("password", this.password);
			Output.WriteAttributeString("trustCertificate", CommonTypes.Encode(this.trustCertificate));
			Output.WriteEndElement();
		}

		public XmppAccountNode(XmlElement E, TreeNode Parent)
			: base(Parent)
		{
			this.host = XML.Attribute(E, "host");
			this.port = XML.Attribute(E, "port", 5222);
			this.account = XML.Attribute(E, "account");
			this.password = XML.Attribute(E, "password");
			this.trustCertificate = XML.Attribute(E, "trustCertificate", false);
		}

	}
}
