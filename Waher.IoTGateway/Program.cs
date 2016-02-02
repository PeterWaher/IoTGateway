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
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.HTTP;

namespace Waher.IoTGateway
{
	/// <summary>
	/// A console application version of the IoT gateway. It's easy to use and experiment with.
	/// </summary>
	class Program
	{
		private const string FormSignatureKey = "";		// Form signature key, if form signatures (XEP-0348) is to be used during registration.
		private const string FormSignatureSecret = "";	// Form signature secret, if form signatures (XEP-0348) is to be used during registration.
		private const int MaxRecordsPerPeriod = 500;

		private static SimpleXmppConfiguration xmppConfiguration;

		static void Main(string[] args)
		{
			XmppClient XmppClient = null;
			Timer ConnectionTimer = null;
			X509Certificate2 Certificate = null;
			HttpServer WebServer = null;
			bool Connected = false;
			bool ImmediateReconnect;

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

				xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config",
					Guid.NewGuid().ToString().Replace("-", string.Empty),	// Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),	// Default password.
					FormSignatureKey, FormSignatureSecret);

				XmppClient = xmppConfiguration.GetClient("en");
			
				if (xmppConfiguration.TrustServer)
					XmppClient.TrustServer = true;

				XmppClient.AllowRegistration(FormSignatureKey, FormSignatureSecret);

				if (xmppConfiguration.Sniffer)
					XmppClient.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount));

				if (!string.IsNullOrEmpty(xmppConfiguration.Events))
					Log.Register(new XmppEventSink("XMPP Event Sink", XmppClient, xmppConfiguration.Events, false));

				ConnectionTimer = new Timer((P) =>
				{
					if (XmppClient.State == XmppState.Offline || XmppClient.State == XmppState.Error || XmppClient.State == XmppState.Authenticating)
					{
						try
						{
							XmppClient.Reconnect();
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
				}, null, 60000, 60000);

				XmppClient.OnStateChanged += (sender, NewState) =>
				{
					switch (NewState)
					{
						case XmppState.Connected:
							Connected = true;

							break;

						case XmppState.Offline:
							ImmediateReconnect = Connected;
							Connected = false;

							if (ImmediateReconnect)
								XmppClient.Reconnect();
							break;
					}
				};

				XmppClient.OnPresenceSubscribe += (sender, e) =>
				{
					e.Accept();		// TODO: Provisioning
					XmppClient.SetPresence(Availability.Chat);
				};

				XmppClient.OnPresenceUnsubscribe += (sender, e) =>
				{
					e.Accept();
				};

				Certificate = new X509Certificate2("certificate.pfx", "testexamplecom");	// TODO: Make certificate parameters configurable
				WebServer = new HttpServer(new int[] { 80, 8080, 8081, 8082 }, new int[] { 443, 8088 }, Certificate);

				new MarkdownSettings();	// Loads the assembly.

				HttpFolderResource HttpFolderResource;

				WebServer.Register(new HttpFolderResource("/Graphics", "Graphics", true, true, true));	// TODO: Add authentication mechanisms for PUT & DELETE.
				WebServer.Register(HttpFolderResource = new HttpFolderResource(string.Empty, "Root", true, true, true));	// TODO: Add authentication mechanisms for PUT & DELETE.
				HttpFolderResource.AllowTypeConversion();

				while (true)
					Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Out.WriteLine(ex.Message);
			}
			finally
			{
				Log.Informational("Server shutting down.");

				if (XmppClient != null)
					XmppClient.Dispose();

				if (ConnectionTimer != null)
					ConnectionTimer.Dispose();

				if (WebServer != null)
					WebServer.Dispose();
			}
		}
	}
}
