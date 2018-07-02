using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Synchronization;

namespace Waher.Utility.AnalyzeClock
{
	class Program
	{
		/// <summary>
		/// Analyzes the difference between the clock on the local machine with the clock on
		/// another machine, connected to the XMPP network, and compatible with the IEEE
		/// XMPP IoT extensions.
		/// 
		/// Command line switches:
		/// 
		/// -h HOST               XMPP Server host name.
		/// -p PORT               XMPP Port number, if different from 5222
		/// -a ACCOUNT            XMPP Account name to use when connecting to the server.
		/// -pwd PASSWORD         PASSWORD to use when authenticating with the server.
		/// -i INTERVAL           Interval (in milliseconds) used to check clocks.
		/// -j JID                JID of clock source to monitor.
		///                       Default=5000.
		/// -r RECORDS            Number of measurements to collect.
		/// -n HISTORY            Number of records in history. Averages are calculated
		///                       on records in this history. Default=100
		/// -w WINDOW             Filter window size. The window is used to detect
		///                       and eliminate bad measurements. Default=16
		/// -s SPIKE_POS          Spike position. Where spikes are detected, in
		///                       window. Default=6
		/// -sw SPIKE_WIDTH       Spike width. Number of measurements in a row that can
		///                       constitute a spike. Default=3
		/// -o OUTPUT_FILE        File name of report file.
		/// -enc ENCODING         Text encoding. Default=UTF-8
		/// -t                    XSLT transform to use.
		/// -?                    Help.
		/// </summary>
		static int Main(string[] args)
		{
			try
			{
				Encoding Encoding = Encoding.UTF8;
				string OutputFileName = null;
				string XsltPath = null;
				string Host = null;
				string Account = null;
				string Password = null;
				string Jid = null;
				string s;
				int Port = 5222;
				int Records = 0;
				int Interval = 5000;
				int History = 100;
				int Window = 16;
				int SpikePos = 6;
				int SpikeWidth = 3;
				int i = 0;
				int c = args.Length;
				bool Help = false;

				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-o":
							if (i >= c)
								throw new Exception("Missing output file name.");

							if (string.IsNullOrEmpty(OutputFileName))
								OutputFileName = args[i++];
							else
								throw new Exception("Only one output file name allowed.");
							break;

						case "-h":
							if (i >= c)
								throw new Exception("Missing host name.");

							if (string.IsNullOrEmpty(Host))
								Host = args[i++];
							else
								throw new Exception("Only one host name allowed.");
							break;

						case "-p":
							if (i >= c)
								throw new Exception("Missing port number.");

							if (!int.TryParse(args[i++], out Port) || Port <= 0 || Port > 65535)
								throw new Exception("Invalid port number.");
							break;

						case "-j":
							if (i >= c)
								throw new Exception("Missing JID.");

							if (string.IsNullOrEmpty(Jid))
								Jid = args[i++];
							else
								throw new Exception("Only one JID allowed.");
							break;

						case "-i":
							if (i >= c)
								throw new Exception("Missing interval.");

							if (!int.TryParse(args[i++], out Interval) || Interval < 1000)
								throw new Exception("Invalid interval.");
							break;

						case "-r":
							if (i >= c)
								throw new Exception("Missing number of records to collect.");

							if (!int.TryParse(args[i++], out Records) || Records <= 0)
								throw new Exception("Invalid number of records to collect.");
							break;

						case "-n":
							if (i >= c)
								throw new Exception("Missing number of history records.");

							if (!int.TryParse(args[i++], out History) || History <= 0)
								throw new Exception("Invalid number of history records.");
							break;

						case "-w":
							if (i >= c)
								throw new Exception("Missing window size.");

							if (!int.TryParse(args[i++], out Window) || Window <= 0)
								throw new Exception("Invalid window size.");
							break;

						case "-s":
							if (i >= c)
								throw new Exception("Missing spike position.");

							if (!int.TryParse(args[i++], out SpikePos) || SpikePos <= 0)
								throw new Exception("Invalid spike position.");
							break;

						case "-sw":
							if (i >= c)
								throw new Exception("Missing spike width.");

							if (!int.TryParse(args[i++], out SpikeWidth) || SpikeWidth <= 0)
								throw new Exception("Invalid spike width.");
							break;

						case "-a":
							if (i >= c)
								throw new Exception("Missing account name.");

							if (string.IsNullOrEmpty(Account))
								Account = args[i++];
							else
								throw new Exception("Only one account name allowed.");
							break;

						case "-pwd":
							if (i >= c)
								throw new Exception("Missing password.");

							if (string.IsNullOrEmpty(Password))
								Password = args[i++];
							else
								throw new Exception("Only one password allowed.");
							break;

						case "-enc":
							if (i >= c)
								throw new Exception("Text encoding missing.");

							Encoding = Encoding.GetEncoding(args[i++]);
							break;

						case "-t":
							if (i >= c)
								throw new Exception("XSLT transform missing.");

							XsltPath = args[i++];
							break;

						case "-?":
							Help = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (Help || c == 0)
				{
					Console.Out.WriteLine("Analyzes the difference between the clock on the local machine with the clock on");
					Console.Out.WriteLine("another machine, connected to the XMPP network, and compatible with the IEEE");
					Console.Out.WriteLine("XMPP IoT extensions.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-h HOST               XMPP Server host name.");
					Console.Out.WriteLine("-p PORT               XMPP Port number, if different from 5222");
					Console.Out.WriteLine("-a ACCOUNT            XMPP Account name to use when connecting to the server.");
					Console.Out.WriteLine("-pwd PASSWORD         PASSWORD to use when authenticating with the server.");
					Console.Out.WriteLine("-j JID                JID of clock source to monitor.");
					Console.Out.WriteLine("-i INTERVAL           Interval (in milliseconds) used to check clocks.");
					Console.Out.WriteLine("                      Default=5000.");
					Console.Out.WriteLine("-r RECORDS            Number of measurements to collect.");
					Console.Out.WriteLine("-n HISTORY            Number of records in history. Averages are calculated");
					Console.Out.WriteLine("                      on records in this history. Default=100");
					Console.Out.WriteLine("-w WINDOW             Filter window size. The window is used to detect");
					Console.Out.WriteLine("                      and eliminate bad measurements. Default=16");
					Console.Out.WriteLine("-s SPIKE_POS          Spike position. Where spikes are detected, in");
					Console.Out.WriteLine("                      window. Default=6");
					Console.Out.WriteLine("-sw SPIKE_WIDTH       Spike width. Number of measurements in a row that can");
					Console.Out.WriteLine("                      constitute a spike. Default=3");
					Console.Out.WriteLine("-o OUTPUT_FILE        File name of report file.");
					Console.Out.WriteLine("-enc ENCODING         Text encoding. Default=UTF-8");
					Console.Out.WriteLine("-t                    XSLT transform to use.");
					Console.Out.WriteLine("-?                    Help.");
					return 0;
				}

				if (string.IsNullOrEmpty(Host))
					throw new Exception("No host name specified.");

				if (string.IsNullOrEmpty(Account))
					throw new Exception("No account name specified.");

				if (string.IsNullOrEmpty(Password))
					throw new Exception("No password specified.");

				if (string.IsNullOrEmpty(Jid))
					throw new Exception("No clock source JID specified.");

				if (Records <= 0)
					throw new Exception("Number of records to collect not specified.");

				if (string.IsNullOrEmpty(OutputFileName))
					throw new Exception("No output filename specified.");

				XmppCredentials Credentials = new XmppCredentials()
				{
					Host = Host,
					Port = Port,
					Account = Account,
					Password = Password,
					AllowCramMD5 = true,
					AllowEncryption = true,
					AllowDigestMD5 = true,
					AllowPlain = true,
					AllowScramSHA1 = true,
					AllowRegistration = false
				};

				using (XmppClient Client = new XmppClient(Credentials, "en", typeof(Program).Assembly))
				{
					ManualResetEvent Done = new ManualResetEvent(false);
					ManualResetEvent Error = new ManualResetEvent(false);

					Client.OnStateChanged += (sender, NewState) =>
					{
						Console.Out.WriteLine(NewState.ToString());

						switch (NewState)
						{
							case XmppState.Connected:
								Done.Set();
								break;

							case XmppState.Error:
							case XmppState.Offline:
								Error.Set();
								break;
						}
					};

					Client.Connect();

					i = WaitHandle.WaitAny(new WaitHandle[] { Done, Error });
					if (i == 1)
						throw new Exception("Unable to connect to broker.");

					Done.Reset();

					using (StreamWriter f = File.CreateText(OutputFileName))
					{
						XmlWriterSettings Settings = new XmlWriterSettings()
						{
							Encoding = Encoding,
							Indent = true,
							IndentChars = "\t",
							NewLineChars = Console.Out.NewLine,
							OmitXmlDeclaration = false,
							WriteEndDocumentOnClose = true
						};

						using (XmlWriter w = XmlWriter.Create(f, Settings))
						{
							w.WriteStartDocument();

							if (!string.IsNullOrEmpty(XsltPath))
								w.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + XML.Encode(XsltPath) + "\"");

							w.WriteStartElement("ClockStatistics", "http://waher.se/Schema/Networking/ClockStatistics.xsd");

							w.WriteStartElement("Parameters");
							w.WriteAttributeString("clientJid", Client.BareJID);
							w.WriteAttributeString("sourceJid", Jid);
							w.WriteAttributeString("records", Records.ToString());
							w.WriteAttributeString("interval", Interval.ToString());
							w.WriteAttributeString("history", History.ToString());
							w.WriteAttributeString("window", Window.ToString());
							w.WriteAttributeString("spikePos", SpikePos.ToString());
							w.WriteAttributeString("spikeWidth", SpikeWidth.ToString());
							w.WriteAttributeString("hfFreq", System.Diagnostics.Stopwatch.Frequency.ToString());
							w.WriteEndElement();

							w.WriteStartElement("Samples");

							using (SynchronizationClient SynchClient = new SynchronizationClient(Client))
							{
								SynchClient.OnUpdated += (sender, e) =>
								{
									DateTime TP = DateTime.Now;

									w.WriteStartElement("Sample");
									w.WriteAttributeString("timestamp", XML.Encode(TP));

									w.WriteAttributeString("rawLatencyMs", CommonTypes.Encode(SynchClient.RawLatency100Ns * 1e-4));
									w.WriteAttributeString("spikeLatencyRemoved", CommonTypes.Encode(SynchClient.LatencySpikeRemoved));
									w.WriteAttributeString("rawDifferenceMs", CommonTypes.Encode(SynchClient.RawClockDifference100Ns * 1e-4));
									w.WriteAttributeString("spikeDifferenceRemoved", CommonTypes.Encode(SynchClient.ClockDifferenceSpikeRemoved));
									w.WriteAttributeString("filteredLatencyMs", CommonTypes.Encode(SynchClient.FilteredLatency100Ns * 1e-4));
									w.WriteAttributeString("filteredDifferenceMs", CommonTypes.Encode(SynchClient.FilteredClockDifference100Ns * 1e-4));
									w.WriteAttributeString("avgLatencyMs", CommonTypes.Encode(SynchClient.AvgLatency100Ns * 1e-4));
									w.WriteAttributeString("avgDifferenceMs", CommonTypes.Encode(SynchClient.AvgClockDifference100Ns * 1e-4));

									if (SynchClient.RawLatencyHf.HasValue)
									{
										w.WriteAttributeString("rawLatencyHf", SynchClient.RawLatencyHf.Value.ToString());
										w.WriteAttributeString("spikeLatencyHfRemoved", CommonTypes.Encode(SynchClient.LatencyHfSpikeRemoved));
									}

									if (SynchClient.RawClockDifferenceHf.HasValue)
									{
										w.WriteAttributeString("rawDifferenceHf", SynchClient.RawClockDifferenceHf.Value.ToString());
										w.WriteAttributeString("spikeDifferenceHfRemoved", CommonTypes.Encode(SynchClient.ClockDifferenceHfSpikeRemoved));
									}

									if (SynchClient.FilteredLatencyHf.HasValue)
										w.WriteAttributeString("filteredLatencyHf", SynchClient.FilteredLatencyHf.ToString());

									if (SynchClient.FilteredClockDifferenceHf.HasValue)
										w.WriteAttributeString("filteredDifferenceHf", SynchClient.FilteredClockDifferenceHf.ToString());

									if (SynchClient.AvgLatencyHf.HasValue)
										w.WriteAttributeString("avgLatencyHf", SynchClient.AvgLatencyHf.ToString());

									if (SynchClient.AvgClockDifferenceHf.HasValue)
										w.WriteAttributeString("avgDifferenceHf", SynchClient.AvgClockDifferenceHf.ToString());

									w.WriteEndElement();

									Console.Out.Write(".");

									if (--Records <= 0)
										Done.Set();
								};

								SynchClient.MonitorClockDifference(Jid, Interval, History, Window, SpikePos, SpikeWidth);

								Done.WaitOne();
							}

							w.WriteEndElement();
							w.WriteEndElement();
							w.WriteEndDocument();

							w.Flush();

							Console.Out.WriteLine();
						}
					}
				}

				return 0;
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
				return -1;
			}
		}
	}
}
