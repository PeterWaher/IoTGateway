﻿using System;
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
using Waher.Networking.XMPP.Interoperability;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.Provisioning;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Mock.Temperature.UWP
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

			this.StartSensor();
		}

		private const int MaxRecordsPerPeriod = 500;

		private XmppClient xmppClient = null;
		private Timer sampleTimer = null;
		private SensorServer sensorServer = null;
		private BobClient bobClient = null;
		private ChatServer chatServer = null;
		private InteroperabilityServer interoperabilityServer;
		private ThingRegistryClient thingRegistryClient = null;
		private ProvisioningClient provisioningClient = null;
		private bool connected = false;
		private bool immediateReconnect;
		private bool registered = false;
		private static string key = null;
		private static string ownerJid = null;
		private static string qrCodeUrl = null;
		private static MetaDataTag[] metaData = null;

		private async void StartSensor()
		{
			try
			{
				Log.Informational("Starting application.");

				XmppCredentials Credentials = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config",
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
					typeof(App).Assembly);

				Log.Informational("Connecting to XMPP server.");

				this.xmppClient = new XmppClient(Credentials, "en", typeof(App).Assembly);

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

				LinkedList<DayHistoryRecord> DayHistoricalValues = new LinkedList<DayHistoryRecord>();
				LinkedList<MinuteHistoryRecord> MinuteHistoricalValues = new LinkedList<MinuteHistoryRecord>();
				DateTime SampleTime = DateTime.Now;
				DateTime PeriodStart = SampleTime.Date;
				DateTime Now;
				DateTime MinTime = SampleTime;
				DateTime MaxTime = SampleTime;
				double CurrentTemperature = this.ReadTemp();
				double MinTemp = CurrentTemperature;
				double MaxTemp = CurrentTemperature;
				double SumTemp = CurrentTemperature;
				int NrTemp = 1;
				int NrDayRecords = 0;
				int NrMinuteRecords = 0;
				SemaphoreSlim SampleSynch = new SemaphoreSlim(1);

				this.sampleTimer = new Timer(async (P) =>
				{
					try
					{
						await SampleSynch.WaitAsync();
						try
						{
							Now = DateTime.Now;

							if (Now.Date != PeriodStart.Date)
							{
								DayHistoryRecord Rec = new DayHistoryRecord(PeriodStart.Date, PeriodStart.Date.AddDays(1).AddMilliseconds(-1),
									MinTemp, MaxTemp, SumTemp / NrTemp);

								DayHistoricalValues.AddFirst(Rec);

								if (NrDayRecords < MaxRecordsPerPeriod)
									NrDayRecords++;
								else
									DayHistoricalValues.RemoveLast();

								// TODO: Persistence

								PeriodStart = Now.Date;
								SumTemp = 0;
								NrTemp = 0;
							}

							CurrentTemperature = this.ReadTemp();

							if (Now.Minute != SampleTime.Minute)
							{
								MinuteHistoryRecord Rec = new MinuteHistoryRecord(Now, CurrentTemperature);

								MinuteHistoricalValues.AddFirst(Rec);

								if (NrMinuteRecords < MaxRecordsPerPeriod)
									NrMinuteRecords++;
								else
									MinuteHistoricalValues.RemoveLast();

								// TODO: Persistence
							}

							SampleTime = Now;

							if (CurrentTemperature < MinTemp)
							{
								MinTemp = CurrentTemperature;
								MinTime = SampleTime;
							}

							if (CurrentTemperature > MaxTemp)
							{
								MaxTemp = CurrentTemperature;
								MaxTime = SampleTime;
							}

							SumTemp += CurrentTemperature;
							NrTemp++;
						}
						finally
						{
							SampleSynch.Release();
						}

						if (this.sensorServer.HasSubscriptions(ThingReference.Empty))
						{
							await this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, SampleTime, "Temperature",
								CurrentTemperature, 1, "°C", FieldType.Momentary, FieldQoS.AutomaticReadout));
						}

						this.UpdateMainWindow(CurrentTemperature, MinTemp, MaxTemp, SumTemp / NrTemp);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}

				}, null, 1000 - PeriodStart.Millisecond, 1000);

				this.sensorServer = new SensorServer(this.xmppClient, this.provisioningClient, true);
				this.sensorServer.OnExecuteReadoutRequest += async (Sender, Request) =>
				{
					Log.Informational("Readout requested by " + Request.From, string.Empty, Request.Actor);

					List<Field> Fields = new List<Field>();
					bool IncludeTemp = Request.IsIncluded("Temperature");
					bool IncludeTempMin = Request.IsIncluded("Temperature, Min");
					bool IncludeTempMax = Request.IsIncluded("Temperature, Max");
					bool IncludeTempAvg = Request.IsIncluded("Temperature, Average");
					bool IncludePeak = Request.IsIncluded(FieldType.Peak);
					bool IncludeComputed = Request.IsIncluded(FieldType.Computed);

					await SampleSynch.WaitAsync();
					try
					{
						if (IncludeTemp && Request.IsIncluded(FieldType.Momentary))
						{
							Fields.Add(new QuantityField(ThingReference.Empty, SampleTime, "Temperature", CurrentTemperature, 1, "°C",
								FieldType.Momentary, FieldQoS.AutomaticReadout));
						}

						if (IncludePeak)
						{
							if (IncludeTempMin)
							{
								Fields.Add(new QuantityField(ThingReference.Empty, MinTime, "Temperature, Min", MinTemp, 1, "°C",
									FieldType.Peak, FieldQoS.AutomaticReadout));
							}

							if (IncludeTempMax)
							{
								Fields.Add(new QuantityField(ThingReference.Empty, MaxTime, "Temperature, Max", MaxTemp, 1, "°C",
									FieldType.Peak, FieldQoS.AutomaticReadout));
							}
						}

						if (IncludeTempAvg && IncludeComputed)
						{
							Fields.Add(new QuantityField(ThingReference.Empty, SampleTime, "Temperature, Average", SumTemp / NrTemp, 2, "°C",
								FieldType.Computed, FieldQoS.AutomaticReadout));
						}

						if (Request.IsIncluded(FieldType.Historical))
						{
							foreach (DayHistoryRecord Rec in DayHistoricalValues)
							{
								if (!Request.IsIncluded(Rec.PeriodStart))
									continue;

								if (Fields.Count >= 100)
								{
									await Request.ReportFields(false, Fields);
									Fields.Clear();
								}

								if (IncludePeak)
								{
									if (IncludeTempMin)
									{
										Fields.Add(new QuantityField(ThingReference.Empty, Rec.PeriodStart, "Temperature, Min", Rec.MinTemperature, 1, "°C",
											FieldType.Peak | FieldType.Historical, FieldQoS.AutomaticReadout));
									}

									if (IncludeTempMax)
									{
										Fields.Add(new QuantityField(ThingReference.Empty, Rec.PeriodStart, "Temperature, Max", Rec.MaxTemperature, 1, "°C",
											FieldType.Peak | FieldType.Historical, FieldQoS.AutomaticReadout));
									}
								}

								if (IncludeTempAvg && IncludeComputed)
								{
									Fields.Add(new QuantityField(ThingReference.Empty, Rec.PeriodStart, "Temperature, Average", Rec.AverageTemperature, 1, "°C",
										FieldType.Computed | FieldType.Historical, FieldQoS.AutomaticReadout));
								}
							}

							foreach (MinuteHistoryRecord Rec in MinuteHistoricalValues)
							{
								if (!Request.IsIncluded(Rec.Timestamp))
									continue;

								if (IncludeTemp)
								{
									if (Fields.Count >= 100)
									{
										await Request.ReportFields(false, Fields);
										Fields.Clear();
									}

									Fields.Add(new QuantityField(ThingReference.Empty, Rec.Timestamp, "Temperature", Rec.Temperature, 1, "°C",
										FieldType.Historical, FieldQoS.AutomaticReadout));
								}
							}
						}
					}
					finally
					{
						SampleSynch.Release();
					}

					await Request.ReportFields(true, Fields);
				};

				this.bobClient = new BobClient(this.xmppClient, Path.Combine(Path.GetTempPath(), "BitsOfBinary"));
				this.chatServer = new ChatServer(this.xmppClient, this.bobClient, this.sensorServer, this.provisioningClient);

				this.interoperabilityServer = new InteroperabilityServer(this.xmppClient);
				this.interoperabilityServer.OnGetInterfaces += (Sender, e) =>
				{
					e.Add("XMPP.IoT.Sensor.Temperature",
						"XMPP.IoT.Sensor.Temperature.History",
						"XMPP.IoT.Sensor.Temperature.Average",
						"XMPP.IoT.Sensor.Temperature.Average.History",
						"XMPP.IoT.Sensor.Temperature.Min",
						"XMPP.IoT.Sensor.Temperature.Min.History",
						"XMPP.IoT.Sensor.Temperature.Max",
						"XMPP.IoT.Sensor.Temperature.Max.History");

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

		private double ReadTemp()
		{
			DateTime Now = DateTime.Now;
			double x = (Now - new DateTime(2015, 1, 1)).TotalDays;
			double AverageTemp = 5 - 25 * Math.Cos(2 * Math.PI * x / 365.25);
			double DailyVariation = -5 * Math.Cos(2 * Math.PI * x - 7.0 / 24);
			double WeeklyWeatherVariation = 3 * Math.Cos(2 * Math.PI * x / 7);
			double CloudVariation = 0.5 * Math.Cos(2 * Math.PI * x * 10);
			double MeasurementError = 0.2 * Math.Cos(2 * Math.PI * x * 100);
			double Temp = AverageTemp + DailyVariation + WeeklyWeatherVariation + CloudVariation + MeasurementError;

			return Math.Round(Temp * 10) * 0.1;
		}

		private async void UpdateMainWindow(double Temp, double MinTemp, double MaxTemp, double AvgTemp)
		{
			MainPage MainPage = MainPage.Instance;

			await MainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				((TextBlock)MainPage.FindName("Temperature")).Text = Temp.ToString("F1") + " °C";
				((TextBlock)MainPage.FindName("MinTemperature")).Text = MinTemp.ToString("F1") + " °C";
				((TextBlock)MainPage.FindName("MaxTemperature")).Text = MaxTemp.ToString("F1") + " °C";
				((TextBlock)MainPage.FindName("AvgTemperature")).Text = AvgTemp.ToString("F2") + " °C";
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
				new MetaDataStringTag("CLASS", "Temperature Sensor"),
				new MetaDataStringTag("MAN", "waher.se"),
				new MetaDataStringTag("MODEL", "Waher.Mock.Temperature.UWP"),
				new MetaDataStringTag("PURL", "https://github.com/PeterWaher/IoTGateway/tree/master/Mocks/Waher.Mock.Temperature.UWP"),
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
