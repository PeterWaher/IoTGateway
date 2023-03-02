using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.XMPP;

namespace Waher.Things.Xmpp.Model
{
	public class XmppBroker : IDisposable
	{
		private readonly XmppBrokerNode node;
		private XmppClient xmppClient;
		private readonly string host;
		private readonly int port;
		private readonly bool tls;
		private readonly string userName;
		private readonly string password;
		private readonly string passwordMechanism;
		private readonly bool trustServer = false;
		private readonly bool allowInsecureMechanisms = false;

		public XmppBroker(XmppBrokerNode Node, string Host, int Port, bool Tls, string UserName, string Password,
			string PasswordMechanism, bool TrustServer, bool AllowInsecureMechanisms)
		{
			this.node = Node;
			this.host = Host;
			this.port = Port;
			this.tls = Tls;
			this.userName = UserName;
			this.password = Password;
			this.passwordMechanism = PasswordMechanism;
			this.trustServer = TrustServer;
			this.allowInsecureMechanisms = AllowInsecureMechanisms;

			this.Open();
		}

		internal XmppClient Client => this.xmppClient;

		private void Open()
		{
			if (string.IsNullOrEmpty(this.passwordMechanism))
			{
				this.xmppClient = new XmppClient(this.host, this.port, this.userName, this.password, "en",
					typeof(XmppBroker).Assembly);
			}
			else
			{
				this.xmppClient = new XmppClient(this.host, this.port, this.userName, this.password, this.passwordMechanism, "en",
					typeof(XmppBroker).Assembly);
			}

			this.xmppClient.TrustServer = this.trustServer;
			this.xmppClient.AllowEncryption = this.tls;
			this.xmppClient.AllowCramMD5 = this.allowInsecureMechanisms;
			this.xmppClient.AllowDigestMD5 = this.allowInsecureMechanisms;
			this.xmppClient.AllowPlain = this.allowInsecureMechanisms;
			this.xmppClient.AllowScramSHA1 = this.allowInsecureMechanisms;

			this.xmppClient.OnStateChanged += this.XmppClient_OnStateChanged;

			this.xmppClient.Connect();
		}

		private void Close()
		{
			if (!(this.xmppClient is null))
			{
				this.xmppClient.OnStateChanged -= this.XmppClient_OnStateChanged;

				this.xmppClient.Dispose();
				this.xmppClient = null;
			}
		}

		public void Dispose()
		{
			this.Close();
		}

		private async Task XmppClient_OnStateChanged(object Sender, XmppState NewState)
		{
			try
			{
				switch (NewState)
				{
					case XmppState.Connected:
						await this.node.RemoveErrorAsync("Offline");
						await this.node.RemoveErrorAsync("Error");
						break;

					case XmppState.Error:
						await this.node.LogErrorAsync("Error", "Connection to broker failed.");
						break;

					case XmppState.Offline:
						await this.node.LogErrorAsync("Offline", "Connection is closed.");
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
