using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Waher.Events;
using Waher.Events.XMPP;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.Control.ParameterTypes;
using Waher.Networking.XMPP.Interoperability;
using Waher.Networking.XMPP.Sensor;

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
            this.Suspending += OnSuspending;
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

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
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

		private const string FormSignatureKey = "";     // Form signature key, if form signatures (XEP-0348) is to be used during registration.
		private const string FormSignatureSecret = "";  // Form signature secret, if form signatures (XEP-0348) is to be used during registration.
		private const int MaxRecordsPerPeriod = 500;

		private XmppClient xmppClient = null;
		private Timer sampleTimer = null;
		private SensorServer sensorServer = null;
		private ControlServer controlServer = null;
		private ChatServer chatServer = null;
		private InteroperabilityServer interoperabilityServer;

		private async void StartActuator()
		{
			try
			{
				Log.Informational("Starting application.");

				SimpleXmppConfiguration xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config",
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
					FormSignatureKey, FormSignatureSecret, typeof(App).GetTypeInfo().Assembly);

				Log.Informational("Connecting to XMPP server.");

				xmppClient = xmppConfiguration.GetClient("en", typeof(App).GetTypeInfo().Assembly);
				xmppClient.AllowRegistration(FormSignatureKey, FormSignatureSecret);

				if (xmppConfiguration.Sniffer && MainPage.Sniffer != null)
					xmppClient.Add(MainPage.Sniffer);

				if (!string.IsNullOrEmpty(xmppConfiguration.Events))
					Log.Register(new XmppEventSink("XMPP Event Sink", xmppClient, xmppConfiguration.Events, false));

				Timer ConnectionTimer = new Timer((P) =>
				{
					if (xmppClient.State == XmppState.Offline || xmppClient.State == XmppState.Error || xmppClient.State == XmppState.Authenticating)
					{
						try
						{
							Log.Informational("Reconnecting.");
							xmppClient.Reconnect();
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
				}, null, 60000, 60000);

				bool Connected = false;
				bool ImmediateReconnect;

				xmppClient.OnStateChanged += (sender, NewState) =>
				{
					Log.Informational(NewState.ToString());

					switch (NewState)
					{
						case XmppState.Connected:
							Connected = true;

							break;

						case XmppState.Offline:
							ImmediateReconnect = Connected;
							Connected = false;

							if (ImmediateReconnect)
								xmppClient.Reconnect();
							break;
					}
				};

				xmppClient.OnPresenceSubscribe += (sender, e) =>
				{
					Log.Informational("Subscription request received from " + e.From + ".");

					e.Accept();     // TODO: Provisioning
					xmppClient.SetPresence(Availability.Chat);
				};

				xmppClient.OnPresenceUnsubscribe += (sender, e) =>
				{
					Log.Informational("Unsubscription request received from " + e.From + ".");
					e.Accept();
				};

				bool SwitchOn = false;

				sensorServer = new SensorServer(xmppClient);
				sensorServer.OnExecuteReadoutRequest += (Sender, Request) =>
				{
					DateTime Now = DateTime.Now;

					Log.Informational("Readout requested", string.Empty, Request.Actor);

					Request.ReportFields(true, new BooleanField(ThingReference.Empty, Now, "Lamp", SwitchOn, FieldType.Momentary, FieldQoS.AutomaticReadout));
				};

				ControlServer ControlServer = new ControlServer(xmppClient,
					new BooleanControlParameter("Lamp", "Control", "Lamp switch on.", "If checked, lamp is turned on.",
						(Node) => SwitchOn,
						(Node, Value) =>
						{
							SwitchOn = Value;
							Log.Informational("Lamp turned " + (SwitchOn ? "ON" : "OFF"));
							UpdateMainWindow(SwitchOn);
						}));

				ChatServer ChatServer = new ChatServer(xmppClient, sensorServer, controlServer);

				InteroperabilityServer InteroperabilityServer = new InteroperabilityServer(xmppClient);
				InteroperabilityServer.OnGetInterfaces += (sender, e) =>
				{
					e.Add("XMPP.IoT.Actuator.Lamp");
				};
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

			if (this.sampleTimer != null)
			{
				this.sampleTimer.Dispose();
				this.sampleTimer = null;
			}

			if (this.xmppClient != null)
			{
				this.xmppClient.Dispose();
				this.xmppClient = null;
			}

			if (this.interoperabilityServer != null)
			{
				this.interoperabilityServer.Dispose();
				this.interoperabilityServer = null;
			}

			if (this.chatServer != null)
			{
				this.chatServer.Dispose();
				this.chatServer = null;
			}

			if (this.controlServer != null)
			{
				this.controlServer.Dispose();
				this.controlServer = null;
			}

			if (this.sensorServer != null)
			{
				this.sensorServer.Dispose();
				this.sensorServer = null;
			}

			Waher.Script.Types.Terminate();
			Waher.Content.Markdown.Model.Multimedia.ImageContent.Terminate();

			deferral.Complete();
        }
    }
}
