using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Events.WindowsEventLog;
using Waher.Events.XMPP;
using Waher.Mock;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence;
using Waher.Persistence.MongoDB;
using Waher.Script;
using Waher.WebService.Script;

namespace Waher.IoTGateway
{
	public static class Gateway
	{
		private const string FormSignatureKey = "";     // Form signature key, if form signatures (XEP-0348) is to be used during registration.
		private const string FormSignatureSecret = "";  // Form signature secret, if form signatures (XEP-0348) is to be used during registration.
		private const int MaxRecordsPerPeriod = 500;

		private static SimpleXmppConfiguration xmppConfiguration;
		private static ThingRegistryClient thingRegistryClient = null;
		private static XmppClient xmppClient = null;
		private static Timer connectionTimer = null;
		private static X509Certificate2 certificate = null;
		private static HttpServer webServer = null;
		private static HttpxServer httpxServer = null;
		private static string ownerJid = null;
		private static bool registered = false;
		private static bool connected = false;
		private static bool immediateReconnect;

		/// <summary>
		/// Starts the gateway.
		/// </summary>
		/// <param name="ConsoleOutput">If console output is permitted.</param>
		public static void Start(bool ConsoleOutput)
		{
			if (!ConsoleOutput)
				Log.Register(new WindowsEventLog("IoTGateway", "IoTGateway", 512));

			Log.Informational("Server starting up.");

			Database.Register(new MongoDBProvider("IoTGateway", "Default"));

			string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			if (!AppDataFolder.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
				AppDataFolder += Path.DirectorySeparatorChar;

			AppDataFolder += "IoT Gateway" + Path.DirectorySeparatorChar;
			string RootFolder = AppDataFolder + "Root" + Path.DirectorySeparatorChar;

			if (!Directory.Exists(RootFolder))
			{
				AppDataFolder = string.Empty;
				RootFolder = "Root" + Path.DirectorySeparatorChar;
			}

			string XmppConfigFileName = AppDataFolder + "xmpp.config";

			if (ConsoleOutput)
			{
				xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog(XmppConfigFileName,
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
					FormSignatureKey, FormSignatureSecret);
			}
			else
				xmppConfiguration = new SimpleXmppConfiguration(XmppConfigFileName);

			xmppClient = xmppConfiguration.GetClient("en");
			xmppClient.AllowRegistration(FormSignatureKey, FormSignatureSecret);

			ConsoleOutSniffer Sniffer = null;

			if (xmppConfiguration.Sniffer && ConsoleOutput)
			{
				Sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount);
				xmppClient.Add(Sniffer);
			}

			if (!string.IsNullOrEmpty(xmppConfiguration.Events))
				Log.Register(new XmppEventSink("XMPP Event Sink", xmppClient, xmppConfiguration.Events, false));

			if (!string.IsNullOrEmpty(xmppConfiguration.ThingRegistry))
			{
				thingRegistryClient = new ThingRegistryClient(xmppClient, xmppConfiguration.ThingRegistry);
				thingRegistryClient.Claimed += ThingRegistryClient_Claimed;
				thingRegistryClient.Disowned += ThingRegistryClient_Disowned;
				thingRegistryClient.Removed += ThingRegistryClient_Removed;
			}

			ProvisioningClient ProvisioningClient = null;
			if (!string.IsNullOrEmpty(xmppConfiguration.Provisioning))
				ProvisioningClient = new ProvisioningClient(xmppClient, xmppConfiguration.Provisioning);

			connectionTimer = new Timer(CheckConnection, null, 60000, 60000);
			xmppClient.OnStateChanged += XmppClient_OnStateChanged;
			xmppClient.OnPresenceSubscribe += XmppClient_OnPresenceSubscribe;
			xmppClient.OnPresenceUnsubscribe += XmppClient_OnPresenceUnsubscribe;
			xmppClient.OnRosterItemUpdated += XmppClient_OnRosterItemUpdated;

			certificate = new X509Certificate2("certificate.pfx", "testexamplecom");    // TODO: Make certificate parameters configurable
			webServer = new HttpServer(new int[] { 80, 8080, 8081, 8082 }, new int[] { 443, 8088 }, certificate);

			HttpFolderResource HttpFolderResource;
			HttpxProxy HttpxProxy;

			webServer.Register(new HttpFolderResource("/Graphics", "Graphics", false, false, true, false)); // TODO: Add authentication mechanisms for PUT & DELETE.
			webServer.Register(new HttpFolderResource("/highlight", "Highlight", false, false, true, false));   // Syntax highlighting library, provided by http://highlightjs.org
			webServer.Register(new ScriptService("/Evaluate"));  // TODO: Add authentication mechanisms. Make service availability pluggable.
			webServer.Register(HttpFolderResource = new HttpFolderResource(string.Empty, RootFolder, false, false, true, true));    // TODO: Add authentication mechanisms for PUT & DELETE.
			webServer.Register(HttpxProxy = new HttpxProxy("/HttpxProxy", xmppClient, 4096));
			webServer.Register("/", (req, resp) =>
			{
				throw new TemporaryRedirectException("/Index.md");  // TODO: Make default page configurable.
			});

			httpxServer = new HttpxServer(xmppClient, webServer, 4096);
			HttpFolderResource.AllowTypeConversion();

			//if (Sniffer != null)
			//	webServer.Add(Sniffer);

			Waher.Script.Types.SetModuleParameter("HTTP", webServer);
			Waher.Script.Types.SetModuleParameter("XMPP", xmppClient);
			Waher.Script.Types.SetModuleParameter("HTTPX", HttpxProxy);
			Waher.Script.Types.SetModuleParameter("AppData", AppDataFolder);
			Waher.Script.Types.SetModuleParameter("Root", RootFolder);

			Waher.Script.Types.GetRootNamespaces();     // Will trigger a load of modules, if not loaded already.

			Mutex StartingServer = new Mutex(true, "Waher.IoTGateway");

			Task.Run(() =>
			{
				try
				{
					Waher.Script.Types.WaitAllModulesStarted(int.MaxValue);
				}
				finally
				{
					StartingServer.ReleaseMutex();
					StartingServer.Dispose();
				}
			});
		}

		/// <summary>
		/// Stops the gateway.
		/// </summary>
		public static void Stop()
		{
			Log.Informational("Server shutting down.");

			if (httpxServer != null)
			{
				httpxServer.Dispose();
				httpxServer = null;
			}

			if (xmppClient != null)
			{
				xmppClient.Dispose();
				xmppClient = null;
			}

			if (connectionTimer != null)
			{
				connectionTimer.Dispose();
				connectionTimer = null;
			}

			if (webServer != null)
			{
				webServer.Dispose();
				webServer = null;
			}
		}

		#region XMPP

		private static void CheckConnection(object State)
		{
			if (xmppClient.State == XmppState.Offline || xmppClient.State == XmppState.Error || xmppClient.State == XmppState.Authenticating)
			{
				try
				{
					xmppClient.Reconnect();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		private static void XmppClient_OnStateChanged(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					connected = true;

					if (!registered && thingRegistryClient != null)
						Register();
					break;

				case XmppState.Offline:
					immediateReconnect = connected;
					connected = false;

					if (immediateReconnect)
						xmppClient.Reconnect();
					break;
			}
		}

		private static void XmppClient_OnPresenceSubscribe(object Sender, PresenceEventArgs e)
		{
			e.Accept();     // TODO: Provisioning

			RosterItem Item = xmppClient.GetRosterItem(e.FromBareJID);
			if (Item == null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
				xmppClient.RequestPresenceSubscription(e.FromBareJID);

			xmppClient.SetPresence(Availability.Chat);
		}

		private static void XmppClient_OnPresenceUnsubscribe(object Sender, PresenceEventArgs e)
		{
			e.Accept();
		}

		private static void XmppClient_OnRosterItemUpdated(object Sender, RosterItem Item)
		{
			if (Item.State == SubscriptionState.None)
				xmppClient.RemoveRosterItem(Item.BareJid);
		}

		#endregion

		#region Thing Registry

		private static void ThingRegistryClient_Claimed(object Sender, ClaimedEventArgs e)
		{
			ownerJid = e.JID;
			Log.Informational("Thing has been claimed.", ownerJid, new KeyValuePair<string, object>("Public", e.IsPublic));
		}

		private static void ThingRegistryClient_Disowned(object Sender, NodeEventArgs e)
		{
			Log.Informational("Thing has been disowned.", ownerJid);
			ownerJid = string.Empty;
			Register();
		}

		private static void ThingRegistryClient_Removed(object Sender, NodeEventArgs e)
		{
			Log.Informational("Thing has been removed from the public registry.", ownerJid);
		}

		private static void Register()
		{
			string Key = Guid.NewGuid().ToString().Replace("-", string.Empty);

			// For info on tag names, see: http://xmpp.org/extensions/xep-0347.html#tags
			MetaDataTag[] MetaData = new MetaDataTag[]
			{
				new MetaDataStringTag("KEY", Key),
				new MetaDataStringTag("CLASS", "Gateway"),
				new MetaDataStringTag("MAN", "waher.se"),
				new MetaDataStringTag("MODEL", "Waher.IoTGateway"),
				new MetaDataStringTag("PURL", "https://github.com/PeterWaher/IoTGateway#iotgateway"),
				new MetaDataNumericTag("V",1.0)
			};

			thingRegistryClient.RegisterThing(MetaData, (sender2, e2) =>
			{
				if (e2.Ok)
				{
					registered = true;

					if (e2.IsClaimed)
						ownerJid = e2.OwnerJid;
					else
					{
						ownerJid = string.Empty;
						SimpleXmppConfiguration.PrintQRCode(thingRegistryClient.EncodeAsIoTDiscoURI(MetaData));
					}
				}
			}, null);
		}

		internal static XmppClient XmppClient
		{
			get { return xmppClient; }
		}

		// TODO: Teman: http://mmistakes.github.io/skinny-bones-jekyll/, http://jekyllrb.com/

		#endregion
	}
}
