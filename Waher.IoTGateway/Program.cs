using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Events.Console;
using Waher.Events.XMPP;
using Waher.Mock;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence;
using Waher.Persistence.MongoDB;
using Waher.WebService.Script;

namespace Waher.IoTGateway
{
	/// <summary>
	/// A console application version of the IoT gateway. It's easy to use and experiment with.
	/// </summary>
	class Program
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

		static void Main(string[] args)
		{
			try
			{
				Console.ForegroundColor = ConsoleColor.White;

				Console.Out.WriteLine("Welcome to the Internet of Things Gateway server application.");
				Console.Out.WriteLine(new string('-', 79));
				Console.Out.WriteLine("This server application will help you manage IoT devices and");
				Console.Out.WriteLine("create dynamic content that you can publish on the Internet.");
				Console.Out.WriteLine("It also provides programming interfaces (API) which allow you");
				Console.Out.WriteLine("to dynamically and securely interact with the devices and the");
				Console.Out.WriteLine("content you publish.");

				Log.Register(new ConsoleEventSink(false));
				Log.Informational("Server starting up.");

				Database.Register(new MongoDBProvider("IoTGateway", "Default"));

				xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config",
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
					FormSignatureKey, FormSignatureSecret);

				xmppClient = xmppConfiguration.GetClient("en");
				xmppClient.AllowRegistration(FormSignatureKey, FormSignatureSecret);

				ConsoleOutSniffer Sniffer = null;

				if (xmppConfiguration.Sniffer)
					xmppClient.Add(Sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount));

				if (!string.IsNullOrEmpty(xmppConfiguration.Events))
					Log.Register(new XmppEventSink("XMPP Event Sink", xmppClient, xmppConfiguration.Events, false));

				if (!string.IsNullOrEmpty(xmppConfiguration.ThingRegistry))
				{
					thingRegistryClient = new ThingRegistryClient(xmppClient, xmppConfiguration.ThingRegistry);

					thingRegistryClient.Claimed += (sender, e) =>
					{
						ownerJid = e.JID;
						Log.Informational("Thing has been claimed.", ownerJid, new KeyValuePair<string, object>("Public", e.IsPublic));
					};

					thingRegistryClient.Disowned += (sender, e) =>
					{
						Log.Informational("Thing has been disowned.", ownerJid);
						ownerJid = string.Empty;
						Register();
					};

					thingRegistryClient.Removed += (sender, e) =>
					{
						Log.Informational("Thing has been removed from the public registry.", ownerJid);
					};
				}

				ProvisioningClient ProvisioningClient = null;
				if (!string.IsNullOrEmpty(xmppConfiguration.Provisioning))
					ProvisioningClient = new ProvisioningClient(xmppClient, xmppConfiguration.Provisioning);

				connectionTimer = new Timer((P) =>
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
				}, null, 60000, 60000);

				xmppClient.OnStateChanged += (sender, NewState) =>
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
				};

				xmppClient.OnPresenceSubscribe += (sender, e) =>
				{
					e.Accept();     // TODO: Provisioning

					RosterItem Item = xmppClient.GetRosterItem(e.FromBareJID);
					if (Item == null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
						xmppClient.RequestPresenceSubscription(e.FromBareJID);

					xmppClient.SetPresence(Availability.Chat);
				};

				xmppClient.OnPresenceUnsubscribe += (sender, e) =>
				{
					e.Accept();
				};

				xmppClient.OnRosterItemUpdated += (sender, e) =>
				{
					if (e.State == SubscriptionState.None)
						xmppClient.RemoveRosterItem(e.BareJid);
				};

				certificate = new X509Certificate2("certificate.pfx", "testexamplecom");    // TODO: Make certificate parameters configurable
				webServer = new HttpServer(new int[] { 80, 8080, 8081, 8082 }, new int[] { 443, 8088 }, certificate);

				HttpFolderResource HttpFolderResource;
				HttpxProxy HttpxProxy;

				webServer.Register(new HttpFolderResource("/Graphics", "Graphics", false, false, true, false)); // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(new HttpFolderResource("/highlight", "Highlight", false, false, true, false));   // Syntax highlighting library, provided by http://highlightjs.org
				webServer.Register(new ScriptService("/Evaluate"));  // TODO: Add authentication mechanisms. Make service availability pluggable.
				webServer.Register(HttpFolderResource = new HttpFolderResource(string.Empty, "Root", false, false, true, true));    // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(HttpxProxy = new HttpxProxy("/HttpxProxy", xmppClient));
				webServer.Register("/", (req, resp) =>
				{
					throw new TemporaryRedirectException("/Index.md");  // TODO: Make default page configurable.
				});

				httpxServer = new HttpxServer(xmppClient, webServer);
				HttpFolderResource.AllowTypeConversion();

				//if (Sniffer != null)
				//	WebServer.Add(Sniffer);

				Waher.Script.Types.SetModuleParameter("HTTP", webServer);
				Waher.Script.Types.SetModuleParameter("XMPP", xmppClient);
				Waher.Script.Types.SetModuleParameter("HTTPX", HttpxProxy);

				Waher.Script.Types.GetRootNamespaces();     // Will trigger a load of modules, if not loaded already.
				
				while (true)
					Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
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
				new MetaDataStringTag("PURL", "https://github.com/PeterWaher/IoTGateway/tree/master/Waher.IoTGateway"),
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
	}
}
