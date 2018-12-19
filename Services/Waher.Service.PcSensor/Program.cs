using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Events.Console;
using Waher.Events.XMPP;
using Waher.Mock;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.BitsOfBinary;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.Provisioning;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Service.PcSensor
{
	/// <summary>
	/// Application that converts your PC into an IoT sensor, by publishing performace counters as sensor values.
	/// </summary>
	public class Program
	{
		private static XmppCredentials credentials;
		private static ThingRegistryClient thingRegistryClient = null;
		private static string ownerJid = null;
		private static bool registered = false;

		public static void Main(string[] args)
		{
			try
			{
				Console.ForegroundColor = ConsoleColor.White;

				Console.Out.WriteLine("Welcome to the PC Sensor application.");
				Console.Out.WriteLine(new string('-', 79));
				Console.Out.WriteLine("This application will publish performace couters as sensor values.");
				Console.Out.WriteLine("Values will be published over XMPP using the interface defined in the IEEE XMPP IoT extensions.");

				Log.Register(new ConsoleEventSink(false));
				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				credentials = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog("xmpp.config",
					Environment.MachineName,								// Default user name.
					Guid.NewGuid().ToString().Replace("-", string.Empty),	// Default password.
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
					};

					Client.OnPresenceSubscribe += (sender, e) =>
					{
						e.Accept();     // TODO: Provisioning

						RosterItem Item = Client.GetRosterItem(e.FromBareJID);
						if (Item is null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
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

					SortedDictionary<string, string[]> CategoryIncluded = new SortedDictionary<string, string[]>();

					List<string> Instances = new List<string>();
					XmlDocument Doc = new XmlDocument();
					Doc.Load("categories.xml");

					XSL.Validate("categories.xml", Doc, "Categories", "http://waher.se/Schema/PerformanceCounterCategories.xsd",
						XSL.LoadSchema("Waher.Service.PcSensor.Schema.PerformanceCounterCategories.xsd"));

					foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
					{
						if (N.LocalName == "Category")
						{
							XmlElement E = (XmlElement)N;
							string Name = XML.Attribute(E, "name");
							bool Include = XML.Attribute(E, "include", false);

							if (Include)
							{
								Instances.Clear();

								foreach (XmlNode N2 in N.ChildNodes)
								{
									if (N2.LocalName == "Instance")
									{
										E = (XmlElement)N2;
										Instances.Add(XML.Attribute(E, "name"));
									}
								}

								CategoryIncluded[Name] = Instances.ToArray();
							}
							else
								CategoryIncluded[Name] = null;
						}
					}

					SensorServer SensorServer = new SensorServer(Client, ProvisioningClient, false);
					SensorServer.OnExecuteReadoutRequest += (Sender, Request) =>
					{
						Log.Informational("Readout requested", string.Empty, Request.Actor);

						List<Field> Fields = new List<Field>();
						DateTime Now = DateTime.Now;

						Fields.Add(new StringField(ThingReference.Empty, Now, "Machine Name", Environment.MachineName, FieldType.Identity, FieldQoS.AutomaticReadout));
						Fields.Add(new StringField(ThingReference.Empty, Now, "OS Platform", Environment.OSVersion.Platform.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout));
						Fields.Add(new StringField(ThingReference.Empty, Now, "OS Service Pack", Environment.OSVersion.ServicePack, FieldType.Identity, FieldQoS.AutomaticReadout));
						Fields.Add(new StringField(ThingReference.Empty, Now, "OS Version", Environment.OSVersion.VersionString, FieldType.Identity, FieldQoS.AutomaticReadout));
						Fields.Add(new Int32Field(ThingReference.Empty, Now, "Processor Count", Environment.ProcessorCount, FieldType.Status, FieldQoS.AutomaticReadout));

						string[] InstanceNames;
						string FieldName;
						string Unit;
						double Value;
						byte NrDec;
						bool Updated = false;

						foreach (PerformanceCounterCategory Category in PerformanceCounterCategory.GetCategories())
						{
							FieldName = Category.CategoryName;
							lock (CategoryIncluded)
							{
								if (CategoryIncluded.TryGetValue(FieldName, out InstanceNames))
								{
									if (InstanceNames is null)
										continue;
								}
								else
								{
									CategoryIncluded[FieldName] = null;
									Updated = true;
									continue;
								}
							}

							if (Category.CategoryType == PerformanceCounterCategoryType.MultiInstance)
							{
								foreach (string InstanceName in Category.GetInstanceNames())
								{
									if (InstanceNames.Length > 0 && Array.IndexOf<string>(InstanceNames, InstanceName) < 0)
										continue;

									foreach (PerformanceCounter Counter in Category.GetCounters(InstanceName))
									{
										FieldName = Category.CategoryName + ", " + InstanceName + ", " + Counter.CounterName;
										Value = Counter.NextValue();
										GetUnitPrecision(ref FieldName, Value, out NrDec, out Unit);

										if (Fields.Count >= 100)
										{
											Request.ReportFields(false, Fields);
											Fields.Clear();
										}

										Fields.Add(new QuantityField(ThingReference.Empty, Now, FieldName, Value, NrDec, Unit, FieldType.Momentary, FieldQoS.AutomaticReadout));
									}
								}
							}
							else
							{
								foreach (PerformanceCounter Counter in Category.GetCounters())
								{
									FieldName = Category.CategoryName + ", " + Counter.CounterName;
									Value = Counter.NextValue();
									GetUnitPrecision(ref FieldName, Value, out NrDec, out Unit);

									if (Fields.Count >= 100)
									{
										Request.ReportFields(false, Fields);
										Fields.Clear();
									}

									Fields.Add(new QuantityField(ThingReference.Empty, Now, FieldName, Value, NrDec, Unit, FieldType.Momentary, FieldQoS.AutomaticReadout));
								}
							}
						}

						Request.ReportFields(true, Fields);

						if (Updated)
						{
							using (StreamWriter s = File.CreateText("categories.xml"))
							{
								using (XmlWriter w = XmlWriter.Create(s, XML.WriterSettings(true, false)))
								{
									w.WriteStartElement("Categories", "http://waher.se/Schema/PerformanceCounterCategories.xsd");

									lock (CategoryIncluded)
									{
										foreach (KeyValuePair<string, string[]> P in CategoryIncluded)
										{
											w.WriteStartElement("Category");
											w.WriteAttributeString("name", P.Key);
											w.WriteAttributeString("include", CommonTypes.Encode(P.Value != null));

											if (P.Value != null)
											{
												foreach (string InstanceName in P.Value)
												{
													w.WriteStartElement("Instance");
													w.WriteAttributeString("name", P.Key);
													w.WriteEndElement();
												}
											}

											w.WriteEndElement();
										}
									}

									w.WriteEndElement();
									w.Flush();
								}
							}
						}
					};

					BobClient BobClient = new BobClient(Client, Path.Combine(Path.GetTempPath(), "BitsOfBinary"));
					ChatServer ChatServer = new ChatServer(Client, BobClient, SensorServer, ProvisioningClient);

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

		private static void GetUnitPrecision(ref string FieldName, double Value, out byte NrDec, out string Unit)
		{
			NrDec = 0;

			while (Math.Abs(Value - Math.Round(Value)) > 1e-6)
			{
				Value *= 10;
				NrDec++;
			}

			Match M = hasUnit.Match(FieldName);
			if (M.Success)
			{
				Unit = M.Groups["Unit"].Value;
				if (Unit.StartsWith("/"))
					Unit = "1" + Unit;
				else if (Unit.StartsWith("%"))
					Unit = "%";

				switch (Unit.ToLower())
				{
					case "byte":
					case "bytes":
						Unit = "B";
						break;

					case "kilobyte":
					case "kilobytes":
					case "kb":
						Unit = "kB";
						break;

					case "megabyte":
					case "megabytes":
						Unit = "MB";
						break;

					case "gigabyte":
					case "gigabytes":
						Unit = "GB";
						break;

					case "1/sec":
						Unit = "1/s";
						break;
				}
			}
			else
				Unit = string.Empty;
		}

		private static readonly Regex hasUnit = new Regex(@"([(](?'Unit'\w+(/\w+)?)[)]|(?'Unit'/s(ec)?|bytes?|kilobytes?|megabytes?|gigabytes?|B|KB|MB|GB|%[\w\s]*))$",
			RegexOptions.Singleline | RegexOptions.Compiled);

		private static void Register()
		{
			string Key = Guid.NewGuid().ToString().Replace("-", string.Empty);

			// For info on tag names, see: http://xmpp.org/extensions/xep-0347.html#tags
			MetaDataTag[] MetaData = new MetaDataTag[]
			{
				new MetaDataStringTag("KEY", Key),
				new MetaDataStringTag("CLASS", "PC"),
				new MetaDataStringTag("MAN", "waher.se"),
				new MetaDataStringTag("MODEL", "Waher.Service.PcSensor"),
				new MetaDataStringTag("PURL", "https://github.com/PeterWaher/IoTGateway/tree/master/Services/Waher.Service.PcSensor"),
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


	}
}
