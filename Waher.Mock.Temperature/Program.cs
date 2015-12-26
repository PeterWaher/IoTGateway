using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Waher.Events;
using Waher.Events.Console;
using Waher.Events.XMPP;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Interoperability;
using Waher.Networking.XMPP.Sensor;

namespace Waher.Mock.Temperature
{
	/// <summary>
	/// Mock temperature sensor providing simulated temperature information through an XMPP sensor interface.
	/// </summary>
	public class Program
	{
		private const string FormSignatureKey = "";		// Form signature key, if form signatures (XEP-0348) is to be used during registration.
		private const string FormSignatureSecret = "";	// Form signature secret, if form signatures (XEP-0348) is to be used during registration.
		private const int MaxRecordsPerPeriod = 500;

		private static SimpleXmppConfiguration xmppConfiguration;

		public static void Main(string[] args)
		{
			try
			{
				Console.ForegroundColor = ConsoleColor.White;

				Console.Out.WriteLine("Welcome to the Mock Temperature sensor application.");
				Console.Out.WriteLine(new string('-', 79));
				Console.Out.WriteLine("This application will simulate an outside temperature sensor.");
				Console.Out.WriteLine("Values will be published over XMPP using the interface defined in XEP-0323.");

				Log.Register(new ConsoleEventSink());

				xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config",
					Guid.NewGuid().ToString().Replace("-", string.Empty),	// Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),	// Default password.
					FormSignatureKey, FormSignatureSecret);

				using (XmppClient Client = new XmppClient(
					xmppConfiguration.Host,
					xmppConfiguration.Port,
					xmppConfiguration.Account,
					xmppConfiguration.Password,
					"en"))
				{
					if (xmppConfiguration.TrustServer)
						Client.TrustServer = true;

					Client.AllowRegistration(FormSignatureKey, FormSignatureSecret);

					if (xmppConfiguration.Sniffer)
						Client.Add(new ConsoleOutSniffer());

					if (!string.IsNullOrEmpty(xmppConfiguration.Events))
						Log.Register(new XmppEventSink("XMPP Event Sink", Client, xmppConfiguration.Events, false));

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
					}, null, 60000, 6000);

					bool Connected = false;
					bool ImmediateReconnect;

					Client.OnStateChanged += (sender, NewState) =>
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
									Client.Reconnect();
								break;
						}
					};

					Client.OnPresenceSubscribe += (sender, e) =>
					{
						e.Accept();		// TODO: Provisioning
					};

					Client.OnPresenceUnsubscribe += (sender, e) =>
					{
						e.Accept();
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

					}, null, 1000 - PeriodStart.Millisecond, 1000);

					SensorServer SensorServer = new SensorServer(Client);
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

							if (Request.IsIncluded(FieldType.HistoricalDay))
							{
								foreach (DayHistoryRecord Rec in DayHistoricalValues)
								{
									if (!Request.IsIncluded(Rec.PeriodStart))
										continue;

									if (IncludePeak)
									{
										if (IncludeTempMin)
										{
											Fields.Add(new QuantityField(ThingReference.Empty, Rec.PeriodStart, "Temperature, Min", Rec.MinTemperature, 1, "°C",
												FieldType.Peak | FieldType.HistoricalDay, FieldQoS.AutomaticReadout));
										}

										if (IncludeTempMax)
										{
											Fields.Add(new QuantityField(ThingReference.Empty, Rec.PeriodStart, "Temperature, Max", Rec.MaxTemperature, 1, "°C",
												FieldType.Peak | FieldType.HistoricalDay, FieldQoS.AutomaticReadout));
										}
									}

									if (IncludeTempAvg && IncludeComputed)
									{
										Fields.Add(new QuantityField(ThingReference.Empty, Rec.PeriodStart, "Temperature, Average", Rec.AverageTemperature, 1, "°C",
											FieldType.Computed | FieldType.HistoricalDay, FieldQoS.AutomaticReadout));
									}

									if (Fields.Count >= 100)
									{
										Request.ReportFields(false, Fields);
										Fields.Clear();
									}
								}
							}

							if (Request.IsIncluded(FieldType.HistoricalMinute))
							{
								foreach (MinuteHistoryRecord Rec in MinuteHistoricalValues)
								{
									if (!Request.IsIncluded(Rec.Timestamp))
										continue;

									if (IncludeTemp)
									{
										Fields.Add(new QuantityField(ThingReference.Empty, Rec.Timestamp, "Temperature", Rec.Temperature, 1, "°C",
											FieldType.HistoricalMinute, FieldQoS.AutomaticReadout));
									}

									if (Fields.Count >= 100)
									{
										Request.ReportFields(false, Fields);
										Fields.Clear();
									}
								}
							}

						}

						Request.ReportFields(true, Fields);
					};

					ChatServer ChatServer = new ChatServer(Client, SensorServer);

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
					};

					while (true)
						Thread.Sleep(1000);
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Out.WriteLine(ex.Message);
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

	}
}
