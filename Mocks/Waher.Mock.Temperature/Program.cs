using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Console;
using Waher.Events.XMPP;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.BitsOfBinary;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Interoperability;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.Provisioning;

namespace Waher.Mock.Temperature
{
	/// <summary>
	/// Mock temperature sensor providing simulated temperature information through an XMPP sensor interface.
	/// </summary>
	public class Program
	{
		private const int MaxRecordsPerPeriod = 500;

		private static XmppCredentials credentials;
		private static ThingRegistryClient thingRegistryClient = null;
		private static string ownerJid = null;
		private static bool registered = false;

		public static void Main(string[] _)
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

				credentials = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config",
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
					typeof(Program).Assembly);

				using (XmppClient Client = new XmppClient(credentials, "en", typeof(Program).Assembly))
				{
					if (credentials.Sniffer)
						Client.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.PadWithSpaces));

					if (!string.IsNullOrEmpty(credentials.Events))
						Log.Register(new XmppEventSink("XMPP Event Sink", Client, credentials.Events, false));

					if (!string.IsNullOrEmpty(credentials.ThingRegistry))
					{
						thingRegistryClient = new ThingRegistryClient(Client, credentials.ThingRegistry);

						thingRegistryClient.Claimed += (sender, e) =>
						{
							ownerJid = e.JID;
							Log.Informational("Thing has been claimed.", ownerJid, new KeyValuePair<string, object>("Public", e.IsPublic));
							return Task.CompletedTask;
						};

						thingRegistryClient.Disowned += (sender, e) =>
						{
							Log.Informational("Thing has been disowned.", ownerJid);
							ownerJid = string.Empty;
							Register();
							return Task.CompletedTask;
						};

						thingRegistryClient.Removed += (sender, e) =>
						{
							Log.Informational("Thing has been removed from the public registry.", ownerJid);
							return Task.CompletedTask;
						};
					}

					ProvisioningClient ProvisioningClient = null;
					if (!string.IsNullOrEmpty(credentials.Provisioning))
						ProvisioningClient = new ProvisioningClient(Client, credentials.Provisioning);

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

						return Task.CompletedTask;
					};

					Client.OnPresenceSubscribe += (sender, e) =>
					{
						e.Accept();     // TODO: Provisioning

						RosterItem Item = Client.GetRosterItem(e.FromBareJID);
						if (Item is null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
							Client.RequestPresenceSubscription(e.FromBareJID);

						Client.SetPresence(Availability.Chat);

						return Task.CompletedTask;
					};

					Client.OnPresenceUnsubscribe += (sender, e) =>
					{
						e.Accept();
						return Task.CompletedTask;
					};

					Client.OnRosterItemUpdated += (sender, e) =>
					{
						if (e.State == SubscriptionState.None && e.PendingSubscription != PendingSubscription.Subscribe)
							Client.RemoveRosterItem(e.BareJid);

						return Task.CompletedTask;
					};

					LinkedList<DayHistoryRecord> DayHistoricalValues = new LinkedList<DayHistoryRecord>();
					LinkedList<MinuteHistoryRecord> MinuteHistoricalValues = new LinkedList<MinuteHistoryRecord>();
					DateTime SampleTime = DateTime.Now;
					DateTime PeriodStart = SampleTime.Date;
					DateTime Now;
					DateTime MinTime = SampleTime;
					DateTime MaxTime = SampleTime;
					double CurrentTemperature = ReadTemp();
					double MinTemp = CurrentTemperature;
					double MaxTemp = CurrentTemperature;
					double SumTemp = CurrentTemperature;
					int NrTemp = 1;
					int NrDayRecords = 0;
					int NrMinuteRecords = 0;
					object SampleSynch = new object();

					SensorServer SensorServer = new SensorServer(Client, ProvisioningClient, true);
					SensorServer.OnExecuteReadoutRequest += (Sender, Request) =>
					{
						Log.Informational("Readout requested", string.Empty, Request.Actor);

						List<Field> Fields = new List<Field>();
						bool IncludeTemp = Request.IsIncluded("Temperature");
						bool IncludeTempMin = Request.IsIncluded("Temperature, Min");
						bool IncludeTempMax = Request.IsIncluded("Temperature, Max");
						bool IncludeTempAvg = Request.IsIncluded("Temperature, Average");
						bool IncludePeak = Request.IsIncluded(FieldType.Peak);
						bool IncludeComputed = Request.IsIncluded(FieldType.Computed);

						lock (SampleSynch)
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
										Request.ReportFields(false, Fields);
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
											Request.ReportFields(false, Fields);
											Fields.Clear();
										}

										Fields.Add(new QuantityField(ThingReference.Empty, Rec.Timestamp, "Temperature", Rec.Temperature, 1, "°C",
											FieldType.Historical, FieldQoS.AutomaticReadout));
									}
								}
							}

						}
						
						Request.ReportFields(true, Fields);

						return Task.CompletedTask;
					};

					Timer SampleTimer = new Timer((P) =>
					{
						lock (SampleSynch)
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

							CurrentTemperature = ReadTemp();

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

						if (SensorServer.HasSubscriptions(ThingReference.Empty))
						{
							SensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, SampleTime, "Temperature", 
								CurrentTemperature, 1, "°C", FieldType.Momentary, FieldQoS.AutomaticReadout));
						}

					}, null, 1000 - PeriodStart.Millisecond, 1000);

					BobClient BobClient = new BobClient(Client, Path.Combine(Path.GetTempPath(), "BitsOfBinary"));
					ChatServer ChatServer = new ChatServer(Client, BobClient, SensorServer, ProvisioningClient);

					InteroperabilityServer InteroperabilityServer = new InteroperabilityServer(Client);
					InteroperabilityServer.OnGetInterfaces += (sender, e) =>
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

		private static double ReadTemp()
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

		private static void Register()
		{
			string Key = Guid.NewGuid().ToString().Replace("-", string.Empty);

			// For info on tag names, see: http://xmpp.org/extensions/xep-0347.html#tags
			MetaDataTag[] MetaData = new MetaDataTag[]
			{
				new MetaDataStringTag("KEY", Key),
				new MetaDataStringTag("CLASS", "Temperature Sensor"),
				new MetaDataStringTag("MAN", "waher.se"),
				new MetaDataStringTag("MODEL", "Waher.Mock.Temperature"),
				new MetaDataStringTag("PURL", "https://github.com/PeterWaher/IoTGateway/tree/master/Mocks/Waher.Mock.Temperature"),
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

				return Task.CompletedTask;

			}, null);
		}

	}
}
