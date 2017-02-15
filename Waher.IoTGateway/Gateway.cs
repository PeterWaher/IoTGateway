using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Files;
using Waher.Events.WindowsEventLog;
using Waher.Events.XMPP;
using Waher.Mock;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.WebService.Script;

namespace Waher.IoTGateway
{
	/// <summary>
	/// Login result.
	/// </summary>
	public enum LoginResult
	{
		/// <summary>
		/// Unable to connect to server.
		/// </summary>
		UnableToConnect,

		/// <summary>
		/// Login credentials invalid.
		/// </summary>
		InvalidLogin,

		/// <summary>
		/// Login successful
		/// </summary>
		Successful
	}

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
		private static FilesProvider databaseProvider;
		private static ClientEvents clientEvents = null;
		private static string ownerJid = null;
		private static string appDataFolder;
		private static string xmppConfigFileName;
		private static bool registered = false;
		private static bool connected = false;
		private static bool immediateReconnect;

		#region Life Cycle

		/// <summary>
		/// Starts the gateway.
		/// </summary>
		/// <param name="ConsoleOutput">If console output is permitted.</param>
		public static bool Start(bool ConsoleOutput)
		{
			Semaphore StartingServer = new Semaphore(1, 1, "Waher.IoTGateway");
			if (!StartingServer.WaitOne(1000))
				return false; // Being started in another process.

			try
			{
				if (!ConsoleOutput)
					Log.Register(new WindowsEventLog("IoTGateway", "IoTGateway", 512));

				appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				if (!appDataFolder.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					appDataFolder += Path.DirectorySeparatorChar;

				appDataFolder += "IoT Gateway" + Path.DirectorySeparatorChar;

				Log.Register(new XmlFileEventSink("XML File Event Sink",
					appDataFolder + "Events" + Path.DirectorySeparatorChar + "Event Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
					appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "EventXmlToHtml.xslt", 7));

				Log.Informational("Server starting up.");

				string RootFolder = appDataFolder + "Root" + Path.DirectorySeparatorChar;
				if (!Directory.Exists(RootFolder))
				{
					appDataFolder = string.Empty;
					RootFolder = "Root" + Path.DirectorySeparatorChar;
				}

				Waher.Script.Types.SetModuleParameter("AppData", appDataFolder);
				Waher.Script.Types.SetModuleParameter("Root", RootFolder);

				xmppConfigFileName = appDataFolder + "xmpp.config";

				if (ConsoleOutput)
				{
					xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog(xmppConfigFileName,
						Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
						Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
						FormSignatureKey, FormSignatureSecret);
				}
				else
					xmppConfiguration = new SimpleXmppConfiguration(xmppConfigFileName);

				xmppClient = xmppConfiguration.GetClient("en");
				xmppClient.AllowRegistration(FormSignatureKey, FormSignatureSecret);
				xmppClient.OnValidateSender += XmppClient_OnValidateSender;
				Waher.Script.Types.SetModuleParameter("XMPP", xmppClient);

				if (xmppConfiguration.Sniffer)
				{
					ISniffer Sniffer;

					if (ConsoleOutput)
					{
						Sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount);
						xmppClient.Add(Sniffer);
					}

					Sniffer = new XmlFileSniffer(appDataFolder + "XMPP" + Path.DirectorySeparatorChar +
						"XMPP Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
						appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "SnifferXmlToHtml.xslt",
						7, BinaryPresentationMethod.ByteCount);
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

				DateTime Now = DateTime.Now;
				int MsToNext = 60000 - (Now.Second * 1000 + Now.Millisecond);

				connectionTimer = new Timer(CheckConnection, null, MsToNext, 60000);
				xmppClient.OnStateChanged += XmppClient_OnStateChanged;
				xmppClient.OnPresenceSubscribe += XmppClient_OnPresenceSubscribe;
				xmppClient.OnPresenceUnsubscribe += XmppClient_OnPresenceUnsubscribe;
				xmppClient.OnRosterItemUpdated += XmppClient_OnRosterItemUpdated;

				certificate = new X509Certificate2("certificate.pfx", "testexamplecom");    // TODO: Make certificate parameters configurable
				webServer = new HttpServer(new int[] { 80, 8080, 8081, 8082 }, new int[] { 443, 8088 }, certificate);
				Waher.Script.Types.SetModuleParameter("HTTP", webServer);

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
				webServer.Register(clientEvents = new ClientEvents());

				HttpFolderResource.AllowTypeConversion();

				httpxServer = new HttpxServer(xmppClient, webServer, 4096);
				Waher.Script.Types.SetModuleParameter("HTTPX", HttpxProxy);
				Waher.Script.Types.SetModuleParameter("HTTPXS", httpxServer);

				if (xmppConfiguration.Sniffer)
				{
					ISniffer Sniffer;

					Sniffer = new XmlFileSniffer(appDataFolder + "HTTP" + Path.DirectorySeparatorChar +
						"HTTP Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
						appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "SnifferXmlToHtml.xslt",
						7, BinaryPresentationMethod.ByteCount);
					webServer.Add(Sniffer);
				}

				//Database.Register(new MongoDBProvider("IoTGateway", "Default"));
				databaseProvider = new FilesProvider(appDataFolder + "Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true, false);
				Database.Register(databaseProvider);  // TODO: Make configurable.

				Waher.Script.Types.GetRootNamespaces();     // Will trigger a load of modules, if not loaded already.
			}
			catch (Exception ex)
			{
				StartingServer.Release();
				StartingServer.Dispose();

				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			Task.Run(() =>
			{
				try
				{
					Waher.Script.Types.StartAllModules(int.MaxValue);
				}
				finally
				{
					StartingServer.Release();
					StartingServer.Dispose();
				}

				/*
				Task<string> Task = databaseProvider.ExportXml(true);
				Task.Wait();
				File.WriteAllText(appDataFolder + "Start.xml", Task.Result);
				*/
			});

			return true;
		}

		/// <summary>
		/// Stops the gateway.
		/// </summary>
		public static void Stop()
		{
			IDisposable Disposable;

			Log.Informational("Server shutting down.");

			/*
			if (databaseProvider != null)
			{
				Task<string> Task = databaseProvider.ExportXml(true);
				Task.Wait();
				File.WriteAllText(appDataFolder + "Stop.xml", Task.Result);
			}
			*/

			if (httpxServer != null)
			{
				httpxServer.Dispose();
				httpxServer = null;
			}

			if (xmppClient != null)
			{
				using (ManualResetEvent OfflineSent = new ManualResetEvent(false))
				{
					xmppClient.SetPresence(Availability.Offline, string.Empty, (sender, e) => OfflineSent.Set());
					OfflineSent.WaitOne(1000);
				}

				foreach (ISniffer Sniffer in xmppClient.Sniffers)
				{
					XmppClient.Remove(Sniffer);

					Disposable = Sniffer as IDisposable;
					if (Disposable != null)
						Disposable.Dispose();
				}

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
				foreach (ISniffer Sniffer in webServer.Sniffers)
				{
					webServer.Remove(Sniffer);

					Disposable = Sniffer as IDisposable;
					if (Disposable != null)
						Disposable.Dispose();
				}

				webServer.Dispose();
				webServer = null;
			}

			clientEvents = null;
		}

		#endregion

		#region XMPP

		private static void XmppClient_OnValidateSender(object Sender, ValidateSenderEventArgs e)
		{
			RosterItem Item;
			string BareJid = e.FromBareJID.ToLower();

			if (string.IsNullOrEmpty(BareJid) || (xmppClient != null && BareJid == xmppClient.Domain.ToLower()))
				e.Accept();

			else if (BareJid.IndexOf('@') > 0 && (xmppClient == null || (Item = xmppClient.GetRosterItem(BareJid)) == null ||
				(Item.State != SubscriptionState.Both && Item.State != SubscriptionState.From)))
			{
				e.Reject();
			}
		}

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

			EventHandler h = MinuteTick;
			if (h != null)
			{
				try
				{
					h(null, new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised every minute.
		/// </summary>
		public static EventHandler MinuteTick = null;

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

					if (immediateReconnect && xmppClient != null)
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
			if (Item.State == SubscriptionState.None && Item.PendingSubscription != PendingSubscription.Subscribe)
				xmppClient.RemoveRosterItem(Item.BareJid);
		}

		/// <summary>
		/// Performs a login operation on the main XMPP account, on the main XMPP account domain.
		/// </summary>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <param name="RemoteEndPoint">Remote End-Point.</param>
		/// <returns>If the login-operation was successful or not.</returns>
		public static async Task<LoginResult> DoMainXmppLogin(string UserName, string Password, string RemoteEndPoint)
		{
			if (xmppClient == null || xmppClient.UserName != UserName)
			{
				Log.Notice("Invalid login.", UserName, RemoteEndPoint, "LoginFailure", EventLevel.Minor);
				return LoginResult.InvalidLogin;
			}

			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			int Result = -1;
			string PasswordHash;
			string PasswordHashMethod;
			bool Connected = false;

			using (XmppClient Client = new XmppClient(xmppClient.Host, xmppClient.Port, UserName, Password, "en"))
			{
				Client.AllowCramMD5 = xmppClient.AllowCramMD5;
				Client.AllowDigestMD5 = xmppClient.AllowDigestMD5;
				Client.AllowPlain = xmppClient.AllowPlain;
				Client.AllowScramSHA1 = xmppClient.AllowScramSHA1;
				Client.AllowEncryption = xmppClient.AllowEncryption;

				Client.OnStateChanged += (sender, NewState) =>
				{
					switch (NewState)
					{
						case XmppState.StreamNegotiation:
							Connected = true;
							break;

						case XmppState.Binding:
							Done.Set();
							break;

						case XmppState.Error:
							Error.Set();
							break;
					}
				};

				await Task.Run(() => Result = WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000));

				PasswordHash = Client.PasswordHash;
				PasswordHashMethod = Client.PasswordHashMethod;
			}

			if (Result != 0)
			{
				if (Connected)
				{
					Log.Notice("Invalid login.", UserName, RemoteEndPoint, "LoginFailure", EventLevel.Minor);
					return LoginResult.InvalidLogin;
				}
				else
				{
					if ((RemoteEndPoint.StartsWith("[::1]:") || RemoteEndPoint.StartsWith("127.0.0.1:")) &&
						UserName == xmppConfiguration.Account && Password == xmppConfiguration.Password &&
						string.IsNullOrEmpty(xmppConfiguration.PasswordType))
					{
						Log.Notice("Successful login. Connection to XMPP broker down. Credentials matched configuration and connection made from same machine.", UserName, RemoteEndPoint, "Login", EventLevel.Minor);
						return LoginResult.Successful;
					}
					else
					{
						Log.Notice("Unable to connect to XMPP broker.", UserName, RemoteEndPoint, "LoginFailure", EventLevel.Minor);
						return LoginResult.UnableToConnect;
					}
				}
			}

			Log.Informational("Successful login.", UserName, RemoteEndPoint, "Login", EventLevel.Minor);

			if (xmppClient.State != XmppState.Connected &&
				(xmppClient.PasswordHash != PasswordHash || xmppClient.PasswordHashMethod != PasswordHashMethod))
			{
				Log.Notice("XMPP credentials updated.", UserName, RemoteEndPoint, "CredentialsUpdated", EventLevel.Minor);

				xmppClient.Reconnect(UserName, PasswordHash, PasswordHashMethod);

				xmppConfiguration.Account = UserName;
				xmppConfiguration.Password = PasswordHash;
				xmppConfiguration.PasswordType = PasswordHashMethod;

				xmppConfiguration.SaveSimpleXmppConfiguration(xmppConfigFileName);
			}

			return LoginResult.Successful;
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
