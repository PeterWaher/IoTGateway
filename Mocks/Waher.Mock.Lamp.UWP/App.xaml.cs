using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Waher.Events;
using Waher.Events.XMPP;
using Waher.Networking;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.BitsOfBinary;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.Interoperability;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Sensor;
using Waher.Things;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Mock.Lamp.UWP
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
				Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
				Microsoft.ApplicationInsights.WindowsCollectors.Session);
			this.InitializeComponent();
			this.Suspending += this.OnSuspending;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (!(Window.Current.Content is Frame rootFrame))
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				rootFrame.NavigationFailed += this.OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content is null)
			{
				// When the navigation stack isn't restored navigate to the first page,
				// configuring the new page by passing required information as a navigation
				// parameter
				rootFrame.Navigate(typeof(MainPage), e.Arguments);
			}
			// Ensure the current window is active
			Window.Current.Activate();

			this.StartActuator();
		}

		private const int MaxRecordsPerPeriod = 500;

		private XmppClient xmppClient = null;
		private Timer sampleTimer = null;
		private SensorServer sensorServer = null;
		private ControlServer controlServer = null;
		private BobClient bobClient = null;
		private ChatServer chatServer = null;
		private InteroperabilityServer interoperabilityServer;
		private ThingRegistryClient thingRegistryClient = null;
		private ProvisioningClient provisioningClient = null;
		private bool connected = false;
		private bool immediateReconnect;
		private bool registered = false;
		private static string ownerJid = null;
		private static string qrCodeUrl = null;
		private static string key = null;
		private static MetaDataTag[] metaData = null;

		private async void StartActuator()
		{
			try
			{
				Log.Informational("Starting application.");

				XmppCredentials Credentials = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config",
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
					typeof(App).GetTypeInfo().Assembly);

				Log.Informational("Connecting to XMPP server.");

				this.xmppClient = new XmppClient(Credentials, "en", typeof(App).GetTypeInfo().Assembly);

				if (Credentials.Sniffer && !(MainPage.Sniffer is null))
					this.xmppClient.Add(MainPage.Sniffer);

				if (!string.IsNullOrEmpty(Credentials.Events))
					Log.Register(new XmppEventSink("XMPP Event Sink", this.xmppClient, Credentials.Events, false));

				if (!string.IsNullOrEmpty(Credentials.ThingRegistry))
				{
					this.thingRegistryClient = new ThingRegistryClient(this.xmppClient, Credentials.ThingRegistry);

					this.thingRegistryClient.Claimed += (sender, e) =>
					{
						ownerJid = e.JID;
						Log.Informational("Thing has been claimed.", ownerJid, new KeyValuePair<string, object>("Public", e.IsPublic));
						return this.RaiseOwnershipChanged();
					};

					this.thingRegistryClient.Disowned += (sender, e) =>
					{
						Log.Informational("Thing has been disowned.", ownerJid);
						ownerJid = string.Empty;
						this.Register();    // Will call this.OwnershipChanged() after successful registration.
						return Task.CompletedTask;
					};

					this.thingRegistryClient.Removed += (sender, e) =>
					{
						Log.Informational("Thing has been removed from the public registry.", ownerJid);
						return Task.CompletedTask;
					};
				}

				if (!string.IsNullOrEmpty(Credentials.Provisioning))
					this.provisioningClient = new ProvisioningClient(this.xmppClient, Credentials.Provisioning);

				Timer ConnectionTimer = new Timer(async (P) =>
				{
					try
					{
						if (this.xmppClient.State == XmppState.Offline || this.xmppClient.State == XmppState.Error || this.xmppClient.State == XmppState.Authenticating)
						{
							Log.Informational("Reconnecting.");
							await this.xmppClient.Reconnect();
						}
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}, null, 60000, 60000);

				this.xmppClient.OnStateChanged += async (sender, NewState) =>
				{
					Log.Informational(NewState.ToString());

					switch (NewState)
					{
						case XmppState.Connected:
							this.connected = true;

							if (!this.registered && !(this.thingRegistryClient is null))
								this.Register();
							break;

						case XmppState.Offline:
							this.immediateReconnect = this.connected;
							this.connected = false;

							if (this.immediateReconnect)
								await this.xmppClient.Reconnect();
							break;
					}
				};

				this.xmppClient.OnPresenceSubscribe += async (sender, e) =>
				{
					Log.Informational("Subscription request received from " + e.From + ".");

					await e.Accept();     // TODO: Provisioning

					RosterItem Item = this.xmppClient.GetRosterItem(e.FromBareJID);
					if (Item is null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
						await this.xmppClient.RequestPresenceSubscription(e.FromBareJID);

					await this.xmppClient.SetPresence(Availability.Chat);
				};

				this.xmppClient.OnPresenceUnsubscribe += async (sender, e) =>
				{
					Log.Informational("Unsubscription request received from " + e.From + ".");
					await e.Accept();
				};

				this.xmppClient.OnRosterItemUpdated += (sender, e) =>
				{
					if (e.State == SubscriptionState.None && e.PendingSubscription != PendingSubscription.Subscribe)
						this.xmppClient.RemoveRosterItem(e.BareJid);

					return Task.CompletedTask;
				};

				bool SwitchOn = false;

				this.sensorServer = new SensorServer(this.xmppClient, this.provisioningClient, false);
				this.sensorServer.OnExecuteReadoutRequest += (Sender, Request) =>
				{
					DateTime Now = DateTime.Now;

					Log.Informational("Readout requested", string.Empty, Request.Actor);

					Request.ReportFields(true, new BooleanField(ThingReference.Empty, Now, "Lamp", SwitchOn, FieldType.Momentary, FieldQoS.AutomaticReadout));

					return Task.CompletedTask;
				};

				this.controlServer = new ControlServer(this.xmppClient,
					new BooleanControlParameter("Lamp", "Control", "Lamp switch on.", "If checked, lamp is turned on.",
						(Node) => Task.FromResult<bool?>(SwitchOn),
						(Node, Value) =>
						{
							SwitchOn = Value;
							Log.Informational("Lamp turned " + (SwitchOn ? "ON" : "OFF"));
							this.UpdateMainWindow(SwitchOn);
							return Task.CompletedTask;
						}));

				this.bobClient = new BobClient(this.xmppClient, Path.Combine(Path.GetTempPath(), "BitsOfBinary"));
				this.chatServer = new ChatServer(this.xmppClient, this.bobClient, this.sensorServer, this.controlServer, this.provisioningClient);

				this.interoperabilityServer = new InteroperabilityServer(this.xmppClient);
				this.interoperabilityServer.OnGetInterfaces += (sender, e) =>
				{
					e.Add("XMPP.IoT.Actuator.Lamp");
					return Task.CompletedTask;
				};

				await this.xmppClient.Connect();
			}
			catch (Exception ex)
			{
				Log.Emergency(ex);

				MessageDialog Dialog = new MessageDialog(ex.Message, "Error");
				await Dialog.ShowAsync();
			}
		}

		private async void UpdateMainWindow(bool LampSwitch)
		{
			MainPage MainPage = MainPage.Instance;

			await MainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				((TextBlock)MainPage.FindName("Lamp")).Text = LampSwitch ? "ON" : "OFF";
			});
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();

			if (!(this.sampleTimer is null))
			{
				this.sampleTimer.Dispose();
				this.sampleTimer = null;
			}

			if (!(this.interoperabilityServer is null))
			{
				this.interoperabilityServer.Dispose();
				this.interoperabilityServer = null;
			}

			if (!(this.chatServer is null))
			{
				this.chatServer.Dispose();
				this.chatServer = null;
			}

			if (!(this.bobClient is null))
			{
				this.bobClient.Dispose();
				this.bobClient = null;
			}

			if (!(this.controlServer is null))
			{
				this.controlServer.Dispose();
				this.controlServer = null;
			}

			if (!(this.sensorServer is null))
			{
				this.sensorServer.Dispose();
				this.sensorServer = null;
			}

			if (!(this.provisioningClient is null))
			{
				this.provisioningClient.Dispose();
				this.provisioningClient = null;
			}

			if (!(this.thingRegistryClient is null))
			{
				this.thingRegistryClient.Dispose();
				this.thingRegistryClient = null;
			}

			if (!(this.xmppClient is null))
			{
				this.xmppClient.DisposeAsync().Wait();  // TODO: Avoid blocking calls.
				this.xmppClient = null;
			}

			Log.Terminate();

			deferral.Complete();
		}

		private void Register()
		{
			key = Guid.NewGuid().ToString().Replace("-", string.Empty);

			// For info on tag names, see: http://xmpp.org/extensions/xep-0347.html#tags
			metaData = new MetaDataTag[]
			{
				new MetaDataStringTag("KEY", key),
				new MetaDataStringTag("CLASS", "Lamp Actuator"),
				new MetaDataStringTag("MAN", "waher.se"),
				new MetaDataStringTag("MODEL", "Waher.Mock.Lamp.UWP"),
				new MetaDataStringTag("PURL", "https://github.com/PeterWaher/IoTGateway/tree/master/Mocks/Waher.Mock.Lamp.UWP"),
				new MetaDataNumericTag("V",1.0)
			};

			qrCodeUrl = SimpleXmppConfiguration.GetQRCodeURL(this.thingRegistryClient.EncodeAsIoTDiscoURI(metaData), 400, 400);

			this.thingRegistryClient.RegisterThing(metaData, async (sender2, e2) =>
			{
				if (e2.Ok)
				{
					this.registered = true;

					if (e2.IsClaimed)
						ownerJid = e2.OwnerJid;
					else
						ownerJid = string.Empty;

					await this.RaiseOwnershipChanged();
				}

			}, null);
		}

		private Task RaiseOwnershipChanged()
		{
			return OwnershipChanged.Raise(this, EventArgs.Empty);
		}

		public static event EventHandler OwnershipChanged = null;

		public static string OwnerJid
		{
			get { return ownerJid; }
		}

		public static string QrCodeUrl
		{
			get { return qrCodeUrl; }
		}
	}
}
