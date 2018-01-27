using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.XMPP.BitsOfBinary;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Provisioning;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.App
{
	public class ChatModule : IModule
	{
		private ChatServer chatServer = null;
		private BobClient bobClient = null;

		public ChatModule()
		{
		}

		public WaitHandle Start()
		{
			ConcentratorServer ConcentratorServer = null;
			ProvisioningClient ProvisioningClient = null;

			if (Types.TryGetModuleParameter("Concentrator", out object Obj))
				ConcentratorServer = Obj as ConcentratorServer;

			if (Types.TryGetModuleParameter("Provisioning", out Obj))
				ProvisioningClient = Obj as ProvisioningClient;

			this.bobClient = new BobClient(Gateway.XmppClient, Path.Combine(Gateway.AppDataFolder, "BoB"));
			this.chatServer = new ChatServer(Gateway.XmppClient, this.bobClient, ConcentratorServer, ProvisioningClient);

			return null;
		}

		public void Stop()
		{
			this.chatServer?.Dispose();
			this.chatServer = null;

			this.bobClient?.Dispose();
			this.bobClient = null;
		}
	}
}
