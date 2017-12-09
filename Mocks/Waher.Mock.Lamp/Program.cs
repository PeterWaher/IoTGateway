using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Waher.Events;
using Waher.Events.Console;
using Waher.Events.XMPP;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.BitsOfBinary;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Control;
using Waher.Things.ControlParameters;
using Waher.Networking.XMPP.Interoperability;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.Provisioning;

namespace Waher.Mock.Lamp
{
	/// <summary>
	/// Mock lamp switch using XMPP.
	/// </summary>
	public class Program
	{
		private const string FormSignatureKey = "";		// Form signature key, if form signatures (XEP-0348) is to be used during registration.
		private const string FormSignatureSecret = "";	// Form signature secret, if form signatures (XEP-0348) is to be used during registration.

		private static SimpleXmppConfiguration xmppConfiguration;
		private static ThingRegistryClient thingRegistryClient = null;
		private static string ownerJid = null;
		private static bool registered = false;

		static void Main(string[] args)
		{
			try
			{
				Console.ForegroundColor = ConsoleColor.White;

				Console.Out.WriteLine("Welcome to the Mock Temperature sensor application.");
				Console.Out.WriteLine(new string('-', 79));
				Console.Out.WriteLine("This application will simulate an outside temperature sensor.");
				Console.Out.WriteLine("Values will be published over XMPP using the interface defined in the IEEE XMPP IoT extensions.");
				Console.Out.WriteLine("You can also chat with the sensor.");

				Log.Register(new ConsoleEventSink());
				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config",
					Guid.NewGuid().ToString().Replace("-", string.Empty),	// Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),	// Default password.
					FormSignatureKey, FormSignatureSecret, typeof(Program).Assembly);

				using (XmppClient Client = xmppConfiguration.GetClient("en", typeof(Program).Assembly, false))
				{
					Client.AllowRegistration(FormSignatureKey, FormSignatureSecret);

					if (xmppConfiguration.Sniffer)
						Client.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.PadWithSpaces));

					if (!string.IsNullOrEmpty(xmppConfiguration.Events))
						Log.Register(new XmppEventSink("XMPP Event Sink", Client, xmppConfiguration.Events, false));

					if (!string.IsNullOrEmpty(xmppConfiguration.ThingRegistry))
					{
						thingRegistryClient = new ThingRegistryClient(Client, xmppConfiguration.ThingRegistry);

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
						ProvisioningClient = new ProvisioningClient(Client, xmppConfiguration.Provisioning);

					Timer ConnectionTimer = new Timer((P) =>
					{
						if (Client.State == XmppState.Offline || Client.State == XmppState.Error || Client.State == XmppState.Authenticating)
						{
							try
							{
								Client.Reconnect();
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
						}
					}, null, 60000, 60000);

					bool Connected = false;
					bool ImmediateReconnect;

					Client.OnStateChanged += (sender, NewState) =>
					{
						switch (NewState)
						{
							case XmppState.Connected:
								Connected = true;

								if (!registered && thingRegistryClient != null)
									Register();
								break;

							case XmppState.Offline:
								ImmediateReconnect = Connected;
								Connected = false;

								if (ImmediateReconnect)
									Client.Reconnect();
								break;
						}
					};

					Client.OnPresenceSubscribe += (sender, e) =>
					{
						e.Accept();     // TODO: Provisioning

						RosterItem Item = Client.GetRosterItem(e.FromBareJID);
						if (Item == null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
							Client.RequestPresenceSubscription(e.FromBareJID);

						Client.SetPresence(Availability.Chat);
					};

					Client.OnPresenceUnsubscribe += (sender, e) =>
					{
						e.Accept();
					};

					Client.OnRosterItemUpdated += (sender, e) =>
					{
						if (e.State == SubscriptionState.None && e.PendingSubscription != PendingSubscription.Subscribe)
							Client.RemoveRosterItem(e.BareJid);
					};

					bool SwitchOn = false;

					SensorServer SensorServer = new SensorServer(Client, ProvisioningClient, false);
					SensorServer.OnExecuteReadoutRequest += (Sender, Request) =>
					{
						DateTime Now = DateTime.Now;

						Log.Informational("Readout requested", string.Empty, Request.Actor);

						Request.ReportFields(true, new BooleanField(ThingReference.Empty, Now, "Lamp", SwitchOn, FieldType.Momentary, FieldQoS.AutomaticReadout));
					};

					ControlServer ControlServer = new ControlServer(Client,
						new BooleanControlParameter("Lamp", "Control", "Lamp switch on.", "If checked, lamp is turned on.",
							(Node) => SwitchOn,
							(Node, Value) =>
							{
								SwitchOn = Value;
								Log.Informational(Environment.NewLine + Environment.NewLine + "Lamp turned " + (SwitchOn ? "ON" : "OFF") + Environment.NewLine + Environment.NewLine);
							}));

					BobClient BobClient = new BobClient(Client, Path.Combine(Path.GetTempPath(), "BitsOfBinary"));
					ChatServer ChatServer = new ChatServer(Client, BobClient, SensorServer, ControlServer, ProvisioningClient);

					InteroperabilityServer InteroperabilityServer = new InteroperabilityServer(Client);
					InteroperabilityServer.OnGetInterfaces += (sender, e) =>
					{
						e.Add("XMPP.IoT.Actuator.Lamp");
					};

					Client.Connect();

					while (true)
						Thread.Sleep(1000);
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Out.WriteLine(ex.Message);
			}
			finally
			{
				Log.Terminate();
			}
		}

		private static void Register()
		{
			string Key = Guid.NewGuid().ToString().Replace("-", string.Empty);

			// For info on tag names, see: http://xmpp.org/extensions/xep-0347.html#tags
			MetaDataTag[] MetaData = new MetaDataTag[]
			{
				new MetaDataStringTag("KEY", Key),
				new MetaDataStringTag("CLASS", "Lamp Actuator"),
				new MetaDataStringTag("MAN", "waher.se"),
				new MetaDataStringTag("MODEL", "Waher.Mock.Lamp"),
				new MetaDataStringTag("PURL", "https://github.com/PeterWaher/IoTGateway/tree/master/Mocks/Waher.Mock.Lamp"),
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
						SimpleXmppConfiguration.PrintQRCode(ThingRegistryClient.EncodeAsIoTDiscoURI(MetaData));
					}
				}
			}, null);
		}

	}
}
