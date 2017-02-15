using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Deployment.WindowsInstaller;
using Waher.Content;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.IoTGateway.Installers
{
	public class CustomActions
	{
		[CustomAction]
		public static ActionResult CreateEventSource(Session Session)
		{
			Session.Log("Checking event sources.");

			try
			{
				if (!EventLog.Exists("IoTGateway") || !EventLog.SourceExists("IoTGateway"))
				{
					Session.Log("Creating event source.");
					EventLog.CreateEventSource(new EventSourceCreationData("IoTGateway", "IoTGateway"));
					Session.Log("Event source created.");
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Unable to create event source. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult DeleteEventSource(Session Session)
		{
			Session.Log("Checking event sources.");

			if (EventLog.Exists("IoTGateway"))
			{
				try
				{
					Session.Log("Deleting event log.");
					EventLog.Delete("IoTGateway");
					Session.Log("Event log deleted.");
				}
				catch (Exception ex)
				{
					Session.Log("Unable to delete event log. Error reported: " + ex.Message);
				}
			}

			if (EventLog.SourceExists("IoTGateway"))
			{
				try
				{
					Session.Log("Deleting event source.");
					EventLog.DeleteEventSource("IoTGateway");
					Session.Log("Event source deleted.");
				}
				catch (Exception ex)
				{
					Session.Log("Unable to delete event source. Error reported: " + ex.Message);
					// Ignore.
				}
			}

			return ActionResult.Success;
		}

		private static void Log(Session Session, string Msg)
		{
			Session.Log(Msg);
			Session["Log"] = Msg;
		}

		[CustomAction]
		public static ActionResult ValidateBroker(Session Session)
		{
			Log(Session, "Validating XMPP broker.");
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				Log(Session, "XMPP broker to validate: " + XmppBroker);

				using (XmppClient Client = new XmppClient(XmppBroker, 5222, string.Empty, string.Empty, "en"))
				{
					Client.AllowCramMD5 = true;
					Client.AllowDigestMD5 = true;
					Client.AllowPlain = false;
					Client.AllowScramSHA1 = true;
					Client.AllowEncryption = true;
					Client.TrustServer = true;

					ManualResetEvent Done = new ManualResetEvent(false);
					ManualResetEvent Fail = new ManualResetEvent(false);
					bool Connected = false;

					using (SessionSniffer Sniffer = new SessionSniffer(Session))
					{
						Client.Add(Sniffer);

						Client.OnStateChanged += (Sender, NewState) =>
						{
							Log(Session, "New state: " + NewState.ToString());

							switch (NewState)
							{
								case XmppState.StreamNegotiation:
									Connected = true;
									break;

								case XmppState.Authenticating:
								case XmppState.StartingEncryption:
									Done.Set();
									break;

								case XmppState.Error:
									Fail.Set();
									break;
							}
						};

						if (WaitHandle.WaitAny(new WaitHandle[] { Done, Fail }, 15000) < 0 || !Connected)
						{
							Session["XmppBrokerOk"] = "0";
							Log(Session, "Broker not reached. Domain name not OK.");
						}
						else
						{
							Session["XmppBrokerOk"] = "1";

							if (Done.WaitOne(0))
							{
								Session["XmppPortRequired"] = "0";
								Session["XMPPPORT"] = "5222";
								Log(Session, "Broker reached on default port (5222).");
							}
							else
							{
								Session["XmppPortRequired"] = "1";
								Log(Session, "Broker reached, but XMPP service not available on default port (5222).");
							}
						}
					}
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Log(Session, "Validation of XMPP broker failed. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult ValidatePort(Session Session)
		{
			Log(Session, "Validating XMPP broker.");
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				int XmppPort;

				if (!int.TryParse(Session["XMPPPORT"], out XmppPort) || XmppPort <= 0 || XmppPort > 65535)
				{
					Session["XmppPortOk"] = "0";
					Log(Session, "Invalid port number.");
				}
				else
				{
					Log(Session, "XMPP broker to validate: " + XmppBroker + ":" + XmppPort.ToString());

					using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, string.Empty, string.Empty, "en"))
					{
						Client.AllowCramMD5 = true;
						Client.AllowDigestMD5 = true;
						Client.AllowPlain = false;
						Client.AllowScramSHA1 = true;
						Client.AllowEncryption = true;
						Client.TrustServer = true;

						ManualResetEvent Done = new ManualResetEvent(false);
						ManualResetEvent Fail = new ManualResetEvent(false);
						bool Connected = false;

						using (SessionSniffer Sniffer = new SessionSniffer(Session))
						{
							Client.Add(Sniffer);

							Client.OnStateChanged += (Sender, NewState) =>
						{
							Log(Session, "New state: " + NewState.ToString());

							switch (NewState)
							{
								case XmppState.StreamNegotiation:
									Connected = true;
									break;

								case XmppState.Authenticating:
								case XmppState.StartingEncryption:
									Done.Set();
									break;

								case XmppState.Error:
									Fail.Set();
									break;
							}
						};

							if (WaitHandle.WaitAny(new WaitHandle[] { Done, Fail }, 15000) < 0 || !Connected)
							{
								Session["XmppPortOk"] = "0";
								Log(Session, "Broker not reached. Domain name not OK.");
							}
							else
							{
								if (Done.WaitOne(0))
								{
									Session["XmppPortOk"] = "1";
									Log(Session, "Broker reached.");
								}
								else
								{
									Session["XmppPortOk"] = "0";
									Log(Session, "Broker reached, but XMPP service not available on port.");
								}
							}
						}
					}
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Log(Session, "Validation of XMPP broker port failed. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult ValidateAccount(Session Session)
		{
			Log(Session, "Validating XMPP account.");
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				int XmppPort = int.Parse(Session["XMPPPORT"]);
				string XmppAccountName = Session["XMPPACCOUNTNAME"];
				string XmppPassword1 = Session["XMPPPASSWORD1"];
				string XmppPassword2 = Session["XMPPPASSWORD2"];

				if (XmppPassword1 != XmppPassword2)
				{
					Log(Session, "Passwords not equal.");
					Session["XmppAccountOk"] = "-2";
				}
				else
				{
					using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, XmppAccountName, XmppPassword1, "en"))
					{
						Client.AllowCramMD5 = true;
						Client.AllowDigestMD5 = true;
						Client.AllowPlain = false;
						Client.AllowScramSHA1 = true;
						Client.AllowEncryption = true;
						Client.TrustServer = true;

						ManualResetEvent Done = new ManualResetEvent(false);
						ManualResetEvent Fail = new ManualResetEvent(false);
						bool Connected = false;

						using (SessionSniffer Sniffer = new SessionSniffer(Session))
						{
							Client.Add(Sniffer);

							Client.OnStateChanged += (Sender, NewState) =>
							{
								Log(Session, "New state: " + NewState.ToString());

								switch (NewState)
								{
									case XmppState.StreamNegotiation:
										Connected = true;
										break;

									case XmppState.Connected:
										Done.Set();
										break;

									case XmppState.Error:
										Fail.Set();
										break;
								}
							};

							if (WaitHandle.WaitAny(new WaitHandle[] { Done, Fail }, 15000) < 0 || !Connected)
							{
								Session["XmppAccountOk"] = "0";
								Session["XmppValidCertificte"] = "0";
								Log(Session, "Broker not reached, or user not authenticated within the time allotted.");
							}
							else
							{
								if (Done.WaitOne(0))
								{
									CheckServices(Client, Session);

									Session["XmppAccountOk"] = "1";
									Session["XMPPPASSWORDHASH"] = Client.PasswordHash;
									Session["XMPPPASSWORDHASHMETHOD"] = Client.PasswordHashMethod;

									Log(Session, "Account found and user authenticated.");
								}
								else
								{
									if (Client.CanRegister)
									{
										Session["XmppAccountOk"] = "-1";
										Log(Session, "User not authenticated. Server supports In-band registration.");
									}
									else
									{
										Session["XmppAccountOk"] = "0";
										Log(Session, "User not authenticated.");
									}
								}

								if (Connected)
								{
									if (Client.ServerCertificateValid)
									{
										Log(Session, "Server certificate valid.");
										Session["XmppValidCertificte"] = "1";
									}
									else
									{
										Log(Session, "Server certificate invalid.");
										Session["XmppValidCertificte"] = "0";
									}
								}
							}
						}
					}
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Log(Session, "Validation of XMPP account failed. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		private static void CheckServices(XmppClient Client, Session Session)
		{
			ServiceItemsDiscoveryEventArgs e = Client.ServiceItemsDiscovery(Client.Domain, 15000);

			foreach (Item Item in e.Items)
			{
				Log(Session, "Checking " + Item.JID + ".");

				ServiceDiscoveryEventArgs e2 = Client.ServiceDiscovery(Item.JID, 15000);

				if (e2.Features.ContainsKey("urn:xmpp:iot:discovery"))
				{
					Log(Session, "Thing registry found: " + Item.JID);
					Session["XMPPTHINGREGISTRY"] = Item.JID;
				}
				else
				{
					Log(Session, "No thing registry found.");
					Session["XMPPTHINGREGISTRY"] = string.Empty;
				}

				if (e2.Features.ContainsKey("urn:xmpp:iot:provisioning"))
				{
					Log(Session, "Provisioning server found: " + Item.JID);
					Session["XMPPPROVISIONINGSERVER"] = Item.JID;
				}
				else
				{
					Log(Session, "No provisioning server found.");
					Session["XMPPPROVISIONINGSERVER"] = string.Empty;
				}

				if (e2.Features.ContainsKey("urn:xmpp:eventlog"))
				{
					Log(Session, "Event log found: " + Item.JID);
					Session["XMPPPEVENTLOG"] = Item.JID;
				}
				else
				{
					Log(Session, "No event log found.");
					Session["XMPPPEVENTLOG"] = string.Empty;
				}
			}
		}

		[CustomAction]
		public static ActionResult CreateAccount(Session Session)
		{
			Log(Session, "Creating XMPP account.");
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				int XmppPort = int.Parse(Session["XMPPPORT"]);
				string XmppAccountName = Session["XMPPACCOUNTNAME"];
				string XmppPassword1 = Session["XMPPPASSWORD1"];

				using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, XmppAccountName, XmppPassword1, "en"))
				{
					Client.AllowRegistration();
					Client.AllowCramMD5 = true;
					Client.AllowDigestMD5 = true;
					Client.AllowPlain = false;
					Client.AllowScramSHA1 = true;
					Client.AllowEncryption = true;
					Client.TrustServer = true;

					ManualResetEvent Done = new ManualResetEvent(false);
					ManualResetEvent Fail = new ManualResetEvent(false);
					string ConnectionError = null;
					bool Connected = false;

					using (SessionSniffer Sniffer = new SessionSniffer(Session))
					{
						Client.Add(Sniffer);

						Client.OnConnectionError += (Sender, ex) =>
						{
							ConnectionError = ex.Message;
						};

						Client.OnStateChanged += (Sender, NewState) =>
						{
							Log(Session, "New state: " + NewState.ToString());

							switch (NewState)
							{
								case XmppState.StreamNegotiation:
									Connected = true;
									break;

								case XmppState.Connected:
									Done.Set();
									break;

								case XmppState.Error:
									Fail.Set();
									break;
							}
						};

						if (WaitHandle.WaitAny(new WaitHandle[] { Done, Fail }, 15000) < 0 || !Connected)
						{
							Session["XmppAccountOk"] = "0";
							Session["XMPPACCOUNTERROR"] = "Timeout.";
							Log(Session, "Broker not reached, or user not authenticated within the time allotted.");
						}
						else
						{
							if (Done.WaitOne(0))
							{
								CheckServices(Client, Session);

								Session["XmppAccountOk"] = "1";
								Session["XMPPACCOUNTERROR"] = string.Empty;
								Session["XMPPPASSWORDHASH"] = Client.PasswordHash;
								Session["XMPPPASSWORDHASHMETHOD"] = Client.PasswordHashMethod;

								Log(Session, "Account created.");
							}
							else
							{
								Session["XmppAccountOk"] = "0";
								Log(Session, "Unable to create account.");

								if (!string.IsNullOrEmpty(ConnectionError))
								{
									Log(Session, ConnectionError);
									Session["XMPPACCOUNTERROR"] = ConnectionError;
								}
								else
									Session["XMPPACCOUNTERROR"] = "Unable to create account.";
							}
						}
					}
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Log(Session, "Creation of XMPP account failed. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult NameAccount(Session Session)
		{
			Log(Session, "Naming XMPP account.");
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				int XmppPort = int.Parse(Session["XMPPPORT"]);
				string XmppAccountName = Session["XMPPACCOUNTNAME"];
				string XmppPassword1 = Session["XMPPPASSWORD1"];
				string ReadableName = Session["READABLENAME"];

				Log(Session, "Readable Name: " + ReadableName);

				using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, XmppAccountName, XmppPassword1, "en"))
				{
					Client.AllowCramMD5 = true;
					Client.AllowDigestMD5 = true;
					Client.AllowPlain = false;
					Client.AllowScramSHA1 = true;
					Client.AllowEncryption = true;
					Client.TrustServer = true;

					ManualResetEvent Done = new ManualResetEvent(false);
					ManualResetEvent Fail = new ManualResetEvent(false);
					bool Connected = false;

					using (SessionSniffer Sniffer = new SessionSniffer(Session))
					{
						Client.Add(Sniffer);

						Client.OnStateChanged += (Sender, NewState) =>
						{
							Log(Session, "New state: " + NewState.ToString());

							switch (NewState)
							{
								case XmppState.StreamNegotiation:
									Connected = true;
									break;

								case XmppState.Connected:
									Done.Set();
									break;

								case XmppState.Error:
									Fail.Set();
									break;
							}
						};

						if (WaitHandle.WaitAny(new WaitHandle[] { Done, Fail }, 15000) < 0 || !Connected)
						{
							Session["XmppAccountOk"] = "0";
							Log(Session, "Broker not reached, or user not authenticated within the time allotted.");
						}
						else
						{
							if (Done.WaitOne(0))
							{
								StringBuilder Xml = new StringBuilder();

								Xml.Append("<vCard xmlns='vcard-temp'>");
								Xml.Append("<FN>");
								Xml.Append(XML.Encode(ReadableName));
								Xml.Append("</FN>");
								Xml.Append("<JABBERID>");
								Xml.Append(XML.Encode(Client.BareJID));
								Xml.Append("</JABBERID>");
								Xml.Append("</vCard>");

								Client.IqSet(Client.BareJID, Xml.ToString(), 15000);

								Session["XmppAccountOk"] = "1";
								Log(Session, "Account named.");

								string SupportAccount = Session["SUPPORTACCOUNT"];
								if (!string.IsNullOrEmpty(SupportAccount) &&
									SupportAccount != "unset" &&
									SupportAccount != Client.BareJID &&
									Client.GetRosterItem(SupportAccount) == null)
								{
									Log(Session, "Requesting presence subscripton from support account: " + SupportAccount);

									Done.Reset();
									ManualResetEvent Done2 = new ManualResetEvent(false);

									Client.OnPresenceSubscribed += (sender, e) =>
									{
										if (string.Compare(e.FromBareJID, SupportAccount, true) == 0)
											Done.Set();
									};

									Client.OnPresenceSubscribe += (sender, e) =>
									{
										if (string.Compare(e.FromBareJID, SupportAccount, true) == 0)
										{
											e.Accept();
											Done2.Set();
										}
										else
											e.Decline();
									};

									RosterItem[] Items = Client.Roster;
									if (Items.Length == 1 && Items[0].BareJid == "support@" + XmppBroker)
										Client.RemoveRosterItem(Items[0].BareJid);

									Client.RequestPresenceSubscription(SupportAccount);

									if (WaitHandle.WaitAll(new WaitHandle[] { Done, Done2 }, 15000))
										Log(Session, "Support account added as contact.");
									else if (Done.WaitOne(0))
										Log(Session, "Presence subscription to support account completed. Presence subscrption from support account still wanting.");
									else if (Done2.WaitOne(0))
										Log(Session, "Presence subscription from support account completed. Presence subscrption to support account still wanting.");
									else
										Log(Session, "Unable to add support account as contact.");
								}
							}
							else
							{
								Session["XmppAccountOk"] = "0";
								Log(Session, "Unable to name account.");
							}
						}
					}
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Log(Session, "Naming of XMPP account failed. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		public static string AppDataFolder
		{
			get
			{
				string Result = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				if (!Result.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					Result += Path.DirectorySeparatorChar;

				Result += "IoT Gateway" + Path.DirectorySeparatorChar;
				if (!Directory.Exists(Result))
					Directory.CreateDirectory(Result);

				return Result;
			}
		}

		public static string XmppConfigFileName
		{
			get
			{
				return AppDataFolder + "xmpp.config";
			}
		}

		[CustomAction]
		public static ActionResult CreateXmppConfigFile(Session Session)
		{
			Session.Log("Creating xmpp.config file.");
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				int XmppPort = int.Parse(Session["XMPPPORT"]);
				string XmppAccountName = Session["XMPPACCOUNTNAME"];
				string XmppPasswordHash = Session["XMPPPASSWORDHASH"];
				string XmppPasswordHashMethod = Session["XMPPPASSWORDHASHMETHOD"];
				string XmppThingRegistry = Session["XMPPTHINGREGISTRY"];
				string XmppProvisioningServer = Session["XMPPPROVISIONINGSERVER"];
				string XmppEventLog = Session["XMPPPEVENTLOG"];
				string XmppTrustServer = Session["XMPPTRUSTSERVER"];

				StringBuilder Xml = new StringBuilder();

				Xml.AppendLine("<?xml version='1.0' encoding='utf-8'?>");
				Xml.AppendLine("<SimpleXmppConfiguration xmlns='http://waher.se/SimpleXmppConfiguration.xsd'>");

				Xml.Append("\t<Host>");
				Xml.Append(XML.Encode(XmppBroker));
				Xml.AppendLine("</Host>");

				Xml.Append("\t<Port>");
				Xml.Append(XmppPort.ToString());
				Xml.AppendLine("</Port>");

				Xml.Append("\t<Account>");
				Xml.Append(XML.Encode(XmppAccountName));
				Xml.AppendLine("</Account>");

				Xml.Append("\t<Password type=\"");
				Xml.Append(XML.Encode(XmppPasswordHashMethod));
				Xml.Append("\">");
				Xml.Append(XML.Encode(XmppPasswordHash));
				Xml.AppendLine("</Password>");

				Xml.Append("\t<ThingRegistry>");
				Xml.Append(XML.Encode(XmppThingRegistry));
				Xml.AppendLine("</ThingRegistry>");

				Xml.Append("\t<Provisioning>");
				Xml.Append(XML.Encode(XmppProvisioningServer));
				Xml.AppendLine("</Provisioning>");

				Xml.Append("\t<Events>");
				Xml.Append(XML.Encode(XmppEventLog));
				Xml.AppendLine("</Events>");

				Xml.Append("\t<Sniffer>");
				Xml.Append(CommonTypes.Encode(false));
				Xml.AppendLine("</Sniffer>");

				Xml.Append("\t<TrustServer>");
				Xml.Append(CommonTypes.Encode(XmppTrustServer == "1"));
				Xml.AppendLine("</TrustServer>");

				Xml.AppendLine("\t<AllowCramMD5>true</AllowCramMD5>");
				Xml.AppendLine("\t<AllowDigestMD5>true</AllowDigestMD5>");
				Xml.AppendLine("\t<AllowPlain>false</AllowPlain>");
				Xml.AppendLine("\t<AllowScramSHA1>true</AllowScramSHA1>");
				Xml.AppendLine("\t<AllowEncryption>true</AllowEncryption>");
				Xml.AppendLine("\t<RequestRosterOnStartup>true</RequestRosterOnStartup>");

				Xml.AppendLine("</SimpleXmppConfiguration>");

				File.WriteAllText(XmppConfigFileName, Xml.ToString(), Encoding.UTF8);

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Unable to create xmpp.config file. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult InstallService(Session Session)
		{
			Session.Log("Installing service.");
			try
			{
				string FrameworkFolder = Session["NETFRAMEWORK40FULLINSTALLROOTDIR"];
				string InstallDir = Session["INSTALLDIR"];

				if (!FrameworkFolder.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					FrameworkFolder += Path.DirectorySeparatorChar;

				if (!InstallDir.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					InstallDir += Path.DirectorySeparatorChar;

				Session.Log(".NET framework folder: " + FrameworkFolder);
				Session.Log("Working folder: " + InstallDir);

				if (!File.Exists(XmppConfigFileName))
				{
					Session.Log("xmpp.config file does not exist. Creating file.");

					if (CreateXmppConfigFile(Session) != ActionResult.Success)
					{
						Session.Log("Unable to start service since xmpp.config file could not be created.");
						return ActionResult.Failure;
					}
				}

				string SystemRoot = Environment.GetEnvironmentVariable("SystemRoot");
				string InstallUtil = Path.Combine(SystemRoot, FrameworkFolder + "InstallUtil.exe");

				Session.Log("InstallUtil path: " + InstallUtil);

				ProcessStartInfo ProcessInformation = new ProcessStartInfo();
				ProcessInformation.FileName = InstallUtil;
				ProcessInformation.Arguments = "/LogToConsole=true Waher.IoTGateway.Svc.exe";
				ProcessInformation.UseShellExecute = false;
				ProcessInformation.RedirectStandardError = true;
				ProcessInformation.RedirectStandardOutput = true;
				ProcessInformation.WorkingDirectory = InstallDir;
				ProcessInformation.CreateNoWindow = true;
				ProcessInformation.WindowStyle = ProcessWindowStyle.Hidden;

				Process P = new Process();
				bool Error = false;

				P.ErrorDataReceived += (sender, e) =>
				{
					Error = true;
					Session.Log("ERROR: " + e.Data);
				};

				P.Exited += (sender, e) =>
				{
					Session.Log("Process exited.");
				};

				P.OutputDataReceived += (sender, e) =>
				{
					Session.Log(e.Data);
				};

				P.StartInfo = ProcessInformation;
				P.Start();

				if (!P.WaitForExit(60000) || Error)
					throw new Exception("Timeout. Service did not install properly.");
				else if (P.ExitCode != 0)
					throw new Exception("Installation failed. Exit code: " + P.ExitCode.ToString());

				Session.Log("Service installed.");
				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Unable to install service. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult UninstallService(Session Session)
		{
			Session.Log("Uninstalling service.");
			try
			{
				string FrameworkFolder = Session["NETFRAMEWORK40FULLINSTALLROOTDIR"];
				string InstallDir = Session["INSTALLDIR"];

				if (!FrameworkFolder.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					FrameworkFolder += Path.DirectorySeparatorChar;

				Session.Log(".NET framework folder: " + FrameworkFolder);
				Session.Log("Working folder: " + InstallDir);

				string SystemRoot = Environment.GetEnvironmentVariable("SystemRoot");
				string InstallUtil = Path.Combine(SystemRoot, FrameworkFolder + "InstallUtil.exe");

				Session.Log("InstallUtil path: " + InstallUtil);

				ProcessStartInfo ProcessInformation = new ProcessStartInfo();
				ProcessInformation.FileName = InstallUtil;
				ProcessInformation.Arguments = "/u /LogToConsole=true Waher.IoTGateway.Svc.exe";
				ProcessInformation.UseShellExecute = false;
				ProcessInformation.RedirectStandardError = true;
				ProcessInformation.RedirectStandardOutput = true;
				ProcessInformation.WorkingDirectory = InstallDir;
				ProcessInformation.CreateNoWindow = true;
				ProcessInformation.WindowStyle = ProcessWindowStyle.Hidden;

				Process P = new Process();
				bool Error = false;

				P.ErrorDataReceived += (sender, e) =>
				{
					Error = true;
					Session.Log("ERROR: " + e.Data);
				};

				P.Exited += (sender, e) =>
				{
					Session.Log("Process exited.");
				};

				P.OutputDataReceived += (sender, e) =>
				{
					Session.Log(e.Data);
				};

				P.StartInfo = ProcessInformation;
				P.Start();

				if (!P.WaitForExit(60000) || Error)
					Session.Log("Timeout. Service did not uninstall properly.");
				else if (P.ExitCode != 0)
					Session.Log("Uninstallation failed. Exit code: " + P.ExitCode.ToString());
				else
					Session.Log("Service uninstalled.");
			}
			catch (Exception ex)
			{
				Session.Log("Unable to uninstall service. Error reported: " + ex.Message);
			}

			return ActionResult.Success;
		}

		[CustomAction]
		public static ActionResult StartService(Session Session)
		{
			Session.Log("Starting service.");
			try
			{
				string InstallDir = Session["INSTALLDIR"];

				ProcessStartInfo ProcessInformation = new ProcessStartInfo();
				ProcessInformation.FileName = "net";
				ProcessInformation.Arguments = "start \"IoT Gateway Service\"";
				ProcessInformation.UseShellExecute = false;
				ProcessInformation.RedirectStandardError = true;
				ProcessInformation.RedirectStandardOutput = true;
				ProcessInformation.WorkingDirectory = InstallDir;
				ProcessInformation.CreateNoWindow = true;
				ProcessInformation.WindowStyle = ProcessWindowStyle.Hidden;

				Process P = new Process();
				bool Error = false;

				P.ErrorDataReceived += (sender, e) =>
				{
					Error = true;
					Session.Log("ERROR: " + e.Data);
				};

				P.Exited += (sender, e) =>
				{
					Session.Log("Process exited.");
				};

				P.OutputDataReceived += (sender, e) =>
				{
					Session.Log(e.Data);
				};

				P.StartInfo = ProcessInformation;
				P.Start();

				if (!P.WaitForExit(60000) || Error)
					throw new Exception("Timeout. Service did not start properly.");
				else if (P.ExitCode != 0)
					throw new Exception("Service start failed. Exit code: " + P.ExitCode.ToString());

				Session.Log("Service started.");

				return WaitAllModulesStarted(Session);
			}
			catch (Exception ex)
			{
				Session.Log("Unable to start service. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult StopService(Session Session)
		{
			Session.Log("Stopping service.");
			try
			{
				string InstallDir = Session["INSTALLDIR"];

				ProcessStartInfo ProcessInformation = new ProcessStartInfo();
				ProcessInformation.FileName = "net";
				ProcessInformation.Arguments = "stop \"IoT Gateway Service\"";
				ProcessInformation.UseShellExecute = false;
				ProcessInformation.RedirectStandardError = true;
				ProcessInformation.RedirectStandardOutput = true;
				ProcessInformation.WorkingDirectory = InstallDir;
				ProcessInformation.CreateNoWindow = true;
				ProcessInformation.WindowStyle = ProcessWindowStyle.Hidden;

				Process P = new Process();
				bool Error = false;

				P.ErrorDataReceived += (sender, e) =>
				{
					Error = true;
					Session.Log("ERROR: " + e.Data);
				};

				P.Exited += (sender, e) =>
				{
					Session.Log("Process exited.");
				};

				P.OutputDataReceived += (sender, e) =>
				{
					Session.Log(e.Data);
				};

				P.StartInfo = ProcessInformation;
				P.Start();

				if (!P.WaitForExit(60000) || Error)
					Session.Log("Timeout. Service did not stop properly.");
				else if (P.ExitCode != 0)
					Session.Log("Stopping service failed. Exit code: " + P.ExitCode.ToString());
				else
					Session.Log("Service stopped.");
			}
			catch (Exception ex)
			{
				Session.Log("Unable to stop service. Error reported: " + ex.Message);
			}

			return ActionResult.Success;
		}

		/*[CustomAction]
		public static ActionResult DisableHttpService(Session Session)
		{
			Session.Log("Stopping HTTP service.");
			try
			{
				ProcessStartInfo ProcessInformation = new ProcessStartInfo();
				ProcessInformation.FileName = "net";
				ProcessInformation.Arguments = "stop http /y";
				ProcessInformation.UseShellExecute = false;
				ProcessInformation.RedirectStandardError = true;
				ProcessInformation.RedirectStandardOutput = true;
				ProcessInformation.CreateNoWindow = true;
				ProcessInformation.WindowStyle = ProcessWindowStyle.Hidden;

				Process P = new Process();
				bool Error = false;

				P.ErrorDataReceived += (sender, e) =>
				{
					Error = true;
					Session.Log("ERROR: " + e.Data);
				};

				P.Exited += (sender, e) =>
				{
					Session.Log("Process exited.");
				};

				P.OutputDataReceived += (sender, e) =>
				{
					Session.Log(e.Data);
				};

				P.StartInfo = ProcessInformation;
				P.Start();

				if (!P.WaitForExit(60000) || Error)
					Session.Log("Timeout. HTTP service did not stop properly.");
				else if (P.ExitCode != 0)
					Session.Log("Stopping http service failed. Exit code: " + P.ExitCode.ToString());
				else
					Session.Log("Service stopped.");

				Session.Log("Disabling http service.");

				ProcessInformation = new ProcessStartInfo();
				ProcessInformation.FileName = "sc";
				ProcessInformation.Arguments = "config http start=disabled";
				ProcessInformation.UseShellExecute = false;
				ProcessInformation.RedirectStandardError = true;
				ProcessInformation.RedirectStandardOutput = true;
				ProcessInformation.CreateNoWindow = true;
				ProcessInformation.WindowStyle = ProcessWindowStyle.Hidden;

				P = new Process();
				Error = false;

				P.ErrorDataReceived += (sender, e) =>
				{
					Error = true;
					Session.Log("ERROR: " + e.Data);
				};

				P.Exited += (sender, e) =>
				{
					Session.Log("Process exited.");
				};

				P.OutputDataReceived += (sender, e) =>
				{
					Session.Log(e.Data);
				};

				P.StartInfo = ProcessInformation;
				P.Start();

				if (!P.WaitForExit(60000) || Error)
					Session.Log("Timeout. HTTP service was not disabled properly.");
				else if (P.ExitCode != 0)
					Session.Log("Disabling http service failed. Exit code: " + P.ExitCode.ToString());
				else
					Session.Log("Service disabled.");
			}
			catch (Exception ex)
			{
				Session.Log("Unable to disable http service. Error reported: " + ex.Message);
			}

			return ActionResult.Success;
		}*/

		[CustomAction]
		public static ActionResult OpenLocalhost(Session Session)
		{
			Session.Log("Starting browser.");
			try
			{
				string StartPage = Session["STARTPAGE"];
				if (StartPage == "unset")
					StartPage = string.Empty;

				StartPage = "http://localhost/" + StartPage;
				Session.Log("Start Page: " + StartPage);

				System.Diagnostics.Process.Start(StartPage);
				Session.Log("Browser started.");
			}
			catch (Exception ex)
			{
				Session.Log("Unable to start browser. Error reported: " + ex.Message);
			}

			return ActionResult.Success;
		}

		[CustomAction]
		public static ActionResult CheckXmppConfigExists(Session Session)
		{
			Session.Log("Checking if xmpp.config exists.");
			try
			{
				string FilePath = XmppConfigFileName;

				if (File.Exists(FilePath))
				{
					Session.Log("File exists: " + FilePath);
					Session["XmppConfigExists"] = "1";
				}
				else
				{
					Session.Log("File does not exist: " + FilePath);
					Session["XmppConfigExists"] = "0";
				}
			}
			catch (Exception ex)
			{
				Session.Log("Check if xmpp.config exists. Error reported: " + ex.Message);
			}

			return ActionResult.Success;
		}

		/*
		[CustomAction]
		public static ActionResult StartMongoDB(Session Session)
		{
			Session.Log("Starting MongoDB.");
			try
			{
				if (!Directory.Exists("C:\\data"))
				{
					Session.Log("Creating directory C:\\data");
					Directory.CreateDirectory("C:\\data");
				}

				if (!Directory.Exists("C:\\data\\db"))
				{
					Session.Log("Creating directory C:\\data\\db");
					Directory.CreateDirectory("C:\\data\\db");
				}

				if (!Directory.Exists("C:\\data\\log"))
				{
					Session.Log("Creating directory C:\\data\\log");
					Directory.CreateDirectory("C:\\data\\log");
				}

				if (!File.Exists("C:\\mongodb\\mongod.cfg"))
				{
					Session.Log("Creating file C:\\mongodb\\mongod.cfg");

					File.WriteAllText("C:\\mongodb\\mongod.cfg",
						"systemLog:\r\n" +
						"  destination: file\r\n" +
						"  path: c:\\data\\log\\mongod.log\r\n" +
						"storage:\r\n" +
						"  dbPath: c:\\data\\db\r\n");
				}

				Session.Log("Installing MongoDB service.");

				ProcessStartInfo ProcessInformation = new ProcessStartInfo();
				ProcessInformation.FileName = "C:\\mongodb\\bin\\mongod.exe";
				ProcessInformation.Arguments = "-vvvvv --config \"C:\\mongodb\\mongod.cfg\" --install";
				ProcessInformation.UseShellExecute = false;
				ProcessInformation.RedirectStandardError = true;
				ProcessInformation.RedirectStandardOutput = true;
				ProcessInformation.WorkingDirectory = "C:\\mongodb\\bin";
				ProcessInformation.CreateNoWindow = true;
				ProcessInformation.WindowStyle = ProcessWindowStyle.Hidden;

				Process P = new Process();
				bool Error = false;

				P.ErrorDataReceived += (sender, e) =>
				{
					Error = true;
					Session.Log("ERROR: " + e.Data);
				};

				P.Exited += (sender, e) =>
				{
					Session.Log("Process exited.");
				};

				P.OutputDataReceived += (sender, e) =>
				{
					Session.Log(e.Data);
				};

				P.StartInfo = ProcessInformation;
				P.Start();

				if (!P.WaitForExit(60000) || Error)
					throw new Exception("Timeout. Service did not install properly.");
				else if (P.ExitCode != 0)
					throw new Exception("Installation failed. Exit code: " + P.ExitCode.ToString());

				Session.Log("MongoDB installed.");
				Session.Log("Starting MongoDB.");

				ProcessInformation = new ProcessStartInfo();
				ProcessInformation.FileName = "net";
				ProcessInformation.Arguments = "start MongoDB";
				ProcessInformation.UseShellExecute = false;
				ProcessInformation.RedirectStandardError = true;
				ProcessInformation.RedirectStandardOutput = true;
				ProcessInformation.WorkingDirectory = "C:\\mongodb\\bin";
				ProcessInformation.CreateNoWindow = true;
				ProcessInformation.WindowStyle = ProcessWindowStyle.Hidden;

				P = new Process();
				Error = false;

				P.ErrorDataReceived += (sender, e) =>
				{
					Error = true;
					Session.Log("ERROR: " + e.Data);
				};

				P.Exited += (sender, e) =>
				{
					Session.Log("Process exited.");
				};

				P.OutputDataReceived += (sender, e) =>
				{
					Session.Log(e.Data);
				};

				P.StartInfo = ProcessInformation;
				P.Start();

				if (!P.WaitForExit(60000) || Error)
					throw new Exception("Timeout. Service did not start properly.");
				else if (P.ExitCode != 0)
					throw new Exception("Failed to start service. Exit code: " + P.ExitCode.ToString());

				Session.Log("MongoDB started.");

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Unable to start MongoDB. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}
		*/

		public static ActionResult WaitAllModulesStarted(Session Session)
		{
			Session.Log("Waiting for all modules to start.");
			try
			{
				using (Semaphore StartingServer = new Semaphore(1, 1, "Waher.IoTGateway"))
				{
					if (StartingServer.WaitOne(120000))
					{
						StartingServer.Release();
						Session.Log("All modules started.");
					}
					else
						Session.Log("Modules takes too long to start. Cancelling wait and continuing.");
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Unable to wait for all modules to start. The following error was reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

	}
}
