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
using Waher.Networking.XMPP.Sensor;

namespace Waher.Mock.Temperature
{
	/// <summary>
	/// Mock temperature sensor providing simulated temperature information through an XMPP sensor interface.
	/// </summary>
	public class Program
	{
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

				xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config", "mock.temp01", string.Empty);

				using (XmppClient Client = new XmppClient(
					xmppConfiguration.Host,
					xmppConfiguration.Port,
					xmppConfiguration.Account,
					xmppConfiguration.Password,
					"en"))
				{
					if (xmppConfiguration.TrustServer)
						Client.TrustServer = true;

					Client.AllowRegistration();

					if (xmppConfiguration.Sniffer)
						Client.Add(new ConsoleOutSniffer());

					if (!string.IsNullOrEmpty(xmppConfiguration.Events))
						Log.Register(new XmppEventSink("XMPP Event Sink", Client, xmppConfiguration.Events, false));

					Timer Timer = new Timer((P) =>
					{
						if (Client.State == XmppState.Offline || Client.State == XmppState.Error)
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

					SensorServer SensorServer = new SensorServer(Client);
					SensorServer.OnExecuteReadoutRequest += (Sender, Request) =>
					{
						Log.Informational("Readout requested", string.Empty, Request.RemoteJID);

						DateTime Now = DateTime.Now;
						double x = (Now - new DateTime(2015, 1, 1)).TotalDays;
						double AverageTemp = 5 - 25 * Math.Cos(2 * Math.PI * x / 365.25);
						double DailyVariation = -5 * Math.Cos(2 * Math.PI * x - 7.0 / 24);
						double WeeklyWeatherVariation = 3 * Math.Cos(2 * Math.PI * x / 7);
						double CloudVariation = 0.5 * Math.Cos(2 * Math.PI * x * 10);
						double MeasurementError = 0.2 * Math.Cos(2 * Math.PI * x * 100);
						double Temp = AverageTemp + DailyVariation + WeeklyWeatherVariation + CloudVariation + MeasurementError;

						Temp = Math.Round(Temp * 10) * 0.1;

						Request.ReportFields(true, new QuantityField(ThingReference.Empty, Now, "Temperature", Temp, 1, "°C", 
							FieldType.Momentary, FieldQoS.AutomaticReadout));
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
	}
}
