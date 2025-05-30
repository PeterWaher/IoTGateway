﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
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
using Microsoft.Maker.Serial;
using Microsoft.Maker.RemoteWiring;
using Waher.Events;
using Waher.Events.XMPP;
using Waher.Mock;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.BitsOfBinary;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Control;
using Waher.Things.ControlParameters;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.Provisioning;

namespace Waher.Service.GPIO
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
		private ThingRegistryClient thingRegistryClient = null;
		private ProvisioningClient provisioningClient = null;
		private bool connected = false;
		private bool immediateReconnect;
		private bool registered = false;
		private static string ownerJid = null;
		private static string qrCodeUrl = null;
		private static string key = null;
		private static MetaDataTag[] metaData;
		private GpioController gpio = null;
		private UsbSerial arduinoUsb = null;
		private RemoteDevice arduino = null;
		private SortedDictionary<int, KeyValuePair<GpioPin, KeyValuePair<TextBlock, TextBlock>>> gpioPins = new SortedDictionary<int, KeyValuePair<GpioPin, KeyValuePair<TextBlock, TextBlock>>>();
		private readonly SortedDictionary<string, KeyValuePair<TextBlock, TextBlock>> arduinoPins = new SortedDictionary<string, KeyValuePair<TextBlock, TextBlock>>();

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

					this.thingRegistryClient.Claimed += (Sender, e) =>
					{
						ownerJid = e.JID;
						Log.Informational("Thing has been claimed.", ownerJid, new KeyValuePair<string, object>("Public", e.IsPublic));
						return this.RaiseOwnershipChanged();
					};

					this.thingRegistryClient.Disowned += (Sender, e) =>
					{
						Log.Informational("Thing has been disowned.", ownerJid);
						ownerJid = string.Empty;
						this.Register();    // Will call this.OwnershipChanged() after successful registration.

						return Task.CompletedTask;
					};

					this.thingRegistryClient.Removed += (Sender, e) =>
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

				this.xmppClient.OnPresenceSubscribe += async (Sender, e) =>
				{
					Log.Informational("Subscription request received from " + e.From + ".");

					await e.Accept();     // TODO: Provisioning

					RosterItem Item = this.xmppClient.GetRosterItem(e.FromBareJID);
					if (Item is null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
						await this.xmppClient.RequestPresenceSubscription(e.FromBareJID);

					await this.xmppClient.SetPresence(Availability.Chat);
				};

				this.xmppClient.OnPresenceUnsubscribe += (Sender, e) =>
				{
					Log.Informational("Unsubscription request received from " + e.From + ".");
					return e.Accept();
				};

				this.xmppClient.OnRosterItemUpdated += (Sender, e) =>
				{
					if (e.State == SubscriptionState.None && e.PendingSubscription != PendingSubscription.Subscribe)
						this.xmppClient.RemoveRosterItem(e.BareJid);

					return Task.CompletedTask;
				};

				this.gpio = GpioController.GetDefault();
				if (!(this.gpio is null))
				{
					int c = this.gpio.PinCount;
					int i;

					for (i = 0; i < c; i++)
					{
						if (this.gpio.TryOpenPin(i, GpioSharingMode.Exclusive, out GpioPin Pin, out GpioOpenStatus Status) && Status == GpioOpenStatus.PinOpened)
						{
							this.gpioPins[i] = new KeyValuePair<GpioPin, KeyValuePair<TextBlock, TextBlock>>(Pin,
								MainPage.Instance.AddPin("GPIO" + i.ToString(), Pin.GetDriveMode(), Pin.Read().ToString()));

							Pin.ValueChanged += async (Sender, e) =>
							{
								if (!this.gpioPins.TryGetValue(Sender.PinNumber, out KeyValuePair<GpioPin, KeyValuePair<TextBlock, TextBlock>> P))
									return;

								PinState Value = e.Edge == GpioPinEdge.FallingEdge ? PinState.LOW : PinState.HIGH;

								if (this.sensorServer.HasSubscriptions(ThingReference.Empty))
								{
									DateTime TP = DateTime.Now;
									string s = "GPIO" + Sender.PinNumber.ToString();

									await this.sensorServer.NewMomentaryValues(
										new EnumField(ThingReference.Empty, TP, s, Value, FieldType.Momentary, FieldQoS.AutomaticReadout));
								}

								await P.Value.Value.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => P.Value.Value.Text = Value.ToString());
							};
						}
					}
				}

				DeviceInformationCollection Devices = await Microsoft.Maker.Serial.UsbSerial.listAvailableDevicesAsync();
				foreach (DeviceInformation DeviceInfo in Devices)
				{
					if (DeviceInfo.IsEnabled && DeviceInfo.Name.StartsWith("Arduino"))
					{
						Log.Informational("Connecting to " + DeviceInfo.Name);
						this.arduinoUsb = new UsbSerial(DeviceInfo);
						this.arduinoUsb.ConnectionEstablished += () =>
						{
							Log.Informational("USB connection established.");
						};

						this.arduino = new RemoteDevice(this.arduinoUsb);
						this.arduino.DeviceReady += async () =>
						{
							Log.Informational("Device ready.");

							Dictionary<int, bool> DisabledPins = new Dictionary<int, bool>();
							Dictionary<string, KeyValuePair<Enum, string>> Values = new Dictionary<string, KeyValuePair<Enum, string>>();
							PinMode Mode;
							PinState State;
							string s;
							ushort Value;

							foreach (byte PinNr in this.arduino.DeviceHardwareProfile.DisabledPins)
								DisabledPins[PinNr] = true;

							foreach (byte PinNr in this.arduino.DeviceHardwareProfile.AnalogPins)
							{
								if (DisabledPins.ContainsKey(PinNr))
									continue;

								s = "A" + (PinNr - this.arduino.DeviceHardwareProfile.AnalogOffset).ToString();
								if (this.arduino.DeviceHardwareProfile.isAnalogSupported(PinNr))
									this.arduino.pinMode(s, PinMode.ANALOG);

								Mode = this.arduino.getPinMode(s);
								Value = this.arduino.analogRead(s);

								Values[s] = new KeyValuePair<Enum, string>(Mode, Value.ToString());
							}

							foreach (byte PinNr in this.arduino.DeviceHardwareProfile.DigitalPins)
							{
								if (DisabledPins.ContainsKey(PinNr) || (PinNr > 6 && PinNr != 13))  // Not sure why this limitation is necessary. Without it, my Arduino board (or the Microsoft Firmata library) stops providing me with pin update events.
									continue;

								if (PinNr == 13)
								{
									this.arduino.pinMode(13, PinMode.OUTPUT);    // Onboard LED.
									this.arduino.digitalWrite(13, PinState.HIGH);
								}
								else
								{
									if (this.arduino.DeviceHardwareProfile.isDigitalInputSupported(PinNr))
										this.arduino.pinMode(PinNr, PinMode.INPUT);
								}

								s = "D" + PinNr.ToString();
								Mode = this.arduino.getPinMode(PinNr);
								State = this.arduino.digitalRead(PinNr);

								Values[s] = new KeyValuePair<Enum, string>(Mode, State.ToString());
							}

							await MainPage.Instance.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
							{
								lock (this.arduinoPins)
								{
									foreach (KeyValuePair<string, KeyValuePair<Enum, string>> P in Values)
										this.arduinoPins[P.Key] = MainPage.Instance.AddPin(P.Key, P.Value.Key, P.Value.Value);
								}
							});

							this.SetupControlServer();
						};

						this.arduino.AnalogPinUpdated += async (pin, value) =>
						{
							KeyValuePair<TextBlock, TextBlock> P;
							DateTime TP = DateTime.Now;

							lock (this.arduinoPins)
							{
								if (!this.arduinoPins.TryGetValue(pin, out P))
									return;
							}

							if (this.sensorServer.HasSubscriptions(ThingReference.Empty))
							{
								await this.sensorServer.NewMomentaryValues(
									new Int32Field(ThingReference.Empty, TP, pin + ", Raw",
										value, FieldType.Momentary, FieldQoS.AutomaticReadout),
									new QuantityField(ThingReference.Empty, TP, pin,
										value / 10.24, 2, "%", FieldType.Momentary, FieldQoS.AutomaticReadout));
							}

							await P.Value.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => P.Value.Text = value.ToString());
						};

						this.arduino.DigitalPinUpdated += async (pin, value) =>
						{
							KeyValuePair<TextBlock, TextBlock> P;
							DateTime TP = DateTime.Now;
							string s = "D" + pin.ToString();

							lock (this.arduinoPins)
							{
								if (!this.arduinoPins.TryGetValue("D" + pin.ToString(), out P))
									return;
							}

							if (this.sensorServer.HasSubscriptions(ThingReference.Empty))
							{
								await this.sensorServer.NewMomentaryValues(
									new EnumField(ThingReference.Empty, TP, s, value, FieldType.Momentary, FieldQoS.AutomaticReadout));
							}

							await P.Value.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => P.Value.Text = value.ToString());
						};

						this.arduinoUsb.ConnectionFailed += message =>
						{
							Log.Error("USB connection failed: " + message);
						};

						this.arduinoUsb.ConnectionLost += message =>
						{
							Log.Error("USB connection lost: " + message);
						};

						this.arduinoUsb.begin(57600, SerialConfig.SERIAL_8N1);
						break;
					}
				}

				this.sensorServer = new SensorServer(this.xmppClient, this.provisioningClient, true);
				this.sensorServer.OnExecuteReadoutRequest += (Sender, Request) =>
				{
					DateTime Now = DateTime.Now;
					LinkedList<Field> Fields = new LinkedList<Field>();
					DateTime TP = DateTime.Now;
					string s;
					bool ReadMomentary = Request.IsIncluded(FieldType.Momentary);
					bool ReadStatus = Request.IsIncluded(FieldType.Status);

					Log.Informational("Readout requested", string.Empty, Request.Actor);

					foreach (KeyValuePair<GpioPin, KeyValuePair<TextBlock, TextBlock>> Pin in this.gpioPins.Values)
					{
						if (ReadMomentary && Request.IsIncluded(s = "GPIO" + Pin.Key.PinNumber.ToString()))
						{
							Fields.AddLast(new EnumField(ThingReference.Empty, TP, s,
								Pin.Key.Read(), FieldType.Momentary, FieldQoS.AutomaticReadout));
						}

						if (ReadStatus && Request.IsIncluded(s = "GPIO" + Pin.Key.PinNumber.ToString() + ", Mode"))
						{
							Fields.AddLast(new EnumField(ThingReference.Empty, TP, s,
								Pin.Key.GetDriveMode(), FieldType.Status, FieldQoS.AutomaticReadout));
						}
					}

					if (!(this.arduinoPins is null))
					{
						foreach (KeyValuePair<string, KeyValuePair<TextBlock, TextBlock>> Pin in this.arduinoPins)
						{
							byte i;

							if (ReadMomentary && Request.IsIncluded(s = Pin.Key))
							{
								if (s.StartsWith("D") && byte.TryParse(s.Substring(1), out i))
								{
									Fields.AddLast(new EnumField(ThingReference.Empty, TP, s,
										this.arduino.digitalRead(i), FieldType.Momentary, FieldQoS.AutomaticReadout));
								}
								else
								{
									ushort Raw = this.arduino.analogRead(s);
									double Percent = Raw / 10.24;

									Fields.AddLast(new Int32Field(ThingReference.Empty, TP, s + ", Raw",
										Raw, FieldType.Momentary, FieldQoS.AutomaticReadout));

									Fields.AddLast(new QuantityField(ThingReference.Empty, TP, s,
										Percent, 2, "%", FieldType.Momentary, FieldQoS.AutomaticReadout));
								}
							}

							if (ReadStatus && Request.IsIncluded(s = Pin.Key + ", Mode"))
							{
								if (s.StartsWith("D") && byte.TryParse(s.Substring(1), out i))
								{
									Fields.AddLast(new EnumField(ThingReference.Empty, TP, s,
										this.arduino.getPinMode(i), FieldType.Status, FieldQoS.AutomaticReadout));
								}
								else
								{
									Fields.AddLast(new EnumField(ThingReference.Empty, TP, s,
										this.arduino.getPinMode(s), FieldType.Status, FieldQoS.AutomaticReadout));
								}
							}
						}
					}

					return Request.ReportFields(true, Fields);
				};

				if (this.arduino is null)
					this.SetupControlServer();

				await this.xmppClient.Connect();
			}
			catch (Exception ex)
			{
				Log.Emergency(ex);

				MessageDialog Dialog = new MessageDialog(ex.Message, "Error");
				await Dialog.ShowAsync();
			}
		}

		private void SetupControlServer()
		{
			List<ControlParameter> Parameters = new List<ControlParameter>();

			foreach (KeyValuePair<GpioPin, KeyValuePair<TextBlock, TextBlock>> Pin in this.gpioPins.Values)
			{
				string s = Pin.Key.PinNumber.ToString();

				Parameters.Add(new BooleanControlParameter("GPIO" + s, "GPIO", "GPIO " + s + ".", "If the GPIO output should be high (checked) or low (unchecked).",
					(Node) => Task.FromResult<bool?>(Pin.Key.Read() == GpioPinValue.High),
					(Node, Value) =>
					{
						Pin.Key.Write(Value ? GpioPinValue.High : GpioPinValue.Low);
						Log.Informational("GPIO " + s + " turned " + (Value ? "HIGH" : "LOW"));
						return Task.CompletedTask;
					}));

				List<string> Options = new List<string>();

				foreach (GpioPinDriveMode Option in Enum.GetValues(typeof(GpioPinDriveMode)))
				{
					if (Pin.Key.IsDriveModeSupported(Option))
						Options.Add(Option.ToString());
				}

				if (Options.Count > 0)
				{
					Parameters.Add(new StringControlParameter("GPIO" + s + "Mode", "GPIO Mode", "GPIO " + s + " Drive Mode:",
						"The drive mode of the underlying hardware for the corresponding GPIO pin.", Options.ToArray(),
						(Node) => Task.FromResult<string>(Pin.Key.GetDriveMode().ToString()),
						(Node, Value) =>
						{
							GpioPinDriveMode Mode = (GpioPinDriveMode)Enum.Parse(typeof(GpioPinDriveMode), Value);

							Pin.Key.SetDriveMode(Mode);
							Log.Informational("GPIO " + s + " drive mode set to " + Value);

							return Task.CompletedTask;
						}));
				}
			}

			if (!(this.arduinoPins is null))
			{
				KeyValuePair<string, KeyValuePair<TextBlock, TextBlock>>[] ArduinoPins;

				lock (this.arduinoPins)
				{
					ArduinoPins = new KeyValuePair<string, KeyValuePair<TextBlock, TextBlock>>[this.arduinoPins.Count];
					this.arduinoPins.CopyTo(ArduinoPins, 0);
				}

				foreach (KeyValuePair<string, KeyValuePair<TextBlock, TextBlock>> Pin in ArduinoPins)
				{
					List<string> Options = new List<string>();
					byte Capabilities;

					if (Pin.Key.StartsWith("D"))
					{
						Parameters.Add(new BooleanControlParameter(Pin.Key, "Arduino D/O", "Arduino Digital Output on " + Pin.Key.ToString() + ".",
							"If the Arduino digitial output should be high (checked) or low (unchecked).",
							(Node) => Task.FromResult<bool?>(this.arduino.digitalRead(byte.Parse(Pin.Key.Substring(1))) == PinState.HIGH),
							(Node, Value) =>
							{
								this.arduino.digitalWrite(byte.Parse(Pin.Key.Substring(1)), Value ? PinState.HIGH : PinState.LOW);
								Log.Informational("Arduino " + Pin.Key + " turned " + (Value ? "HIGH" : "LOW"));
								return Task.CompletedTask;
							}));

						Capabilities = this.arduino.DeviceHardwareProfile.getPinCapabilitiesBitmask(byte.Parse(Pin.Key.Substring(1)));
					}
					else
					{
						Capabilities = this.arduino.DeviceHardwareProfile.getPinCapabilitiesBitmask((uint)(byte.Parse(Pin.Key.Substring(1)) +
							this.arduino.DeviceHardwareProfile.AnalogOffset));
					}

					if ((Capabilities & (byte)PinCapability.ANALOG) != 0)
						Options.Add("ANALOG");

					if ((Capabilities & (byte)PinCapability.I2C) != 0)
						Options.Add("I2C");

					if ((Capabilities & (byte)PinCapability.INPUT) != 0)
						Options.Add("INPUT");

					if ((Capabilities & (byte)PinCapability.INPUT_PULLUP) != 0)
						Options.Add("PULLUP");

					if ((Capabilities & (byte)PinCapability.OUTPUT) != 0)
						Options.Add("OUTPUT");

					if ((Capabilities & (byte)PinCapability.PWM) != 0)
						Options.Add("PWM");

					if ((Capabilities & (byte)PinCapability.SERVO) != 0)
						Options.Add("SERVO");

					if (Options.Count > 0)
					{
						Parameters.Add(new StringControlParameter(Pin.Key + "Mode", "Arduino Mode", "Pin " + Pin.Key + " Drive Mode:",
							"The drive mode of the underlying hardware for the corresponding Arduino pin.", Options.ToArray(),
							(Node) => Task.FromResult<string>(Pin.Key.StartsWith("D") ? this.arduino.getPinMode(byte.Parse(Pin.Key.Substring(1))).ToString() : this.arduino.getPinMode(Pin.Key).ToString()),
							(Node, Value) =>
							{
								PinMode Mode = (PinMode)Enum.Parse(typeof(PinMode), Value);

								if (Pin.Key.StartsWith("D"))
									this.arduino.pinMode(byte.Parse(Pin.Key.Substring(1)), Mode);
								else
									this.arduino.pinMode(Pin.Key, Mode);

								Log.Informational("Arduino " + Pin.Key + " drive mode set to " + Value);

								return Task.CompletedTask;
							}));
					}
				}
			}

			this.controlServer = new ControlServer(this.xmppClient, Parameters.ToArray());
			this.bobClient = new BobClient(this.xmppClient, Path.Combine(Path.GetTempPath(), "BitsOfBinary"));
			this.chatServer = new ChatServer(this.xmppClient, this.bobClient, this.sensorServer, this.controlServer, this.provisioningClient);
		}

		private async Task UpdateMainWindow(bool LampSwitch)
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
		void OnNavigationFailed(object Sender, NavigationFailedEventArgs e)
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
		private void OnSuspending(object Sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();

			if (!(this.arduino is null))
			{
				this.arduino.digitalWrite(13, PinState.LOW);
				this.arduino.pinMode(13, PinMode.INPUT);    // Onboard LED.

				this.arduino.Dispose();
				this.arduino = null;
			}

			if (!(this.arduinoUsb is null))
			{
				this.arduinoUsb.end();
				this.arduinoUsb.Dispose();
				this.arduinoUsb = null;
			}

			if (!(this.gpioPins is null))
			{
				foreach (KeyValuePair<GpioPin, KeyValuePair<TextBlock, TextBlock>> Pin in this.gpioPins.Values)
					Pin.Key.Dispose();

				this.gpioPins = null;
			}

			if (!(this.sampleTimer is null))
			{
				this.sampleTimer.Dispose();
				this.sampleTimer = null;
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
				new MetaDataStringTag("CLASS", "PLC"),
				new MetaDataStringTag("MAN", "waher.se"),
				new MetaDataStringTag("MODEL", "Waher.Service.GPIO"),
				new MetaDataStringTag("PURL", "https://github.com/PeterWaher/IoTGateway/tree/master/Services/Waher.Service.GPIO"),
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

		public static event EventHandlerAsync OwnershipChanged = null;

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
