using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.Deployment.WindowsInstaller;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.IoTGateway.Installers
{
	public partial class CustomActions
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

				using (XmppClient Client = new XmppClient(XmppBroker, 5222, string.Empty, string.Empty, "en", typeof(CustomActions).Assembly))
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
								case XmppState.StreamOpened:
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

						Client.Connect();

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

				if (!int.TryParse(Session["XMPPPORT"], out int XmppPort) || XmppPort <= 0 || XmppPort > 65535)
				{
					Session["XmppPortOk"] = "0";
					Log(Session, "Invalid port number.");
				}
				else
				{
					Log(Session, "XMPP broker to validate: " + XmppBroker + ":" + XmppPort.ToString());

					using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, string.Empty, string.Empty, "en", typeof(CustomActions).Assembly))
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
									case XmppState.StreamOpened:
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

							Client.Connect();

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
					using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, XmppAccountName, XmppPassword1, "en", typeof(CustomActions).Assembly))
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
									case XmppState.StreamOpened:
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

							Client.Connect();

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

				using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, XmppAccountName, XmppPassword1, "en", typeof(CustomActions).Assembly))
				{
					if (clp.TryGetValue(XmppBroker, out KeyValuePair<string, string> Signature))
						Client.AllowRegistration(Signature.Key, Signature.Value);
					else
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
								case XmppState.StreamOpened:
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

						Client.Connect();

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

				using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, XmppAccountName, XmppPassword1, "en", typeof(CustomActions).Assembly))
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
								case XmppState.StreamOpened:
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

						Client.Connect();

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
				Xml.AppendLine("<SimpleXmppConfiguration xmlns='http://waher.se/Schema/SimpleXmppConfiguration.xsd'>");

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
		public static ActionResult InstallAndStartService(Session Session)
		{
			Session.Log("Installing service.");
			try
			{
				string DisplayName = Session["SERVICEDISPLAYNAME"];
				string Description = Session["SERVICEDESCRIPTION"];
				string InstallDir = Session["INSTALLDIR"];

				if (!InstallDir.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					InstallDir += Path.DirectorySeparatorChar;

				Session.Log("Service Display Name: " + DisplayName);
				Session.Log("Service Description: " + Description);
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

				ProcessStartInfo ProcessInformation = new ProcessStartInfo()
				{
					FileName = InstallDir + "Waher.IotGateway.Svc.exe",
					Arguments = "-install -displayname \"" + DisplayName + "\" -description \"" + Description + "\" -start AutoStart -immediate",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					WorkingDirectory = InstallDir,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden
				};

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
				else
				{
					if (!P.StandardError.EndOfStream)
						Session.Log(P.StandardError.ReadToEnd());

					if (!P.StandardOutput.EndOfStream)
						Session.Log(P.StandardOutput.ReadToEnd());

					if (P.ExitCode != 0)
						throw new Exception("Installation failed. Exit code: " + P.ExitCode.ToString());
				}

				Session.Log("Service installed and started.");

				return WaitAllModulesStarted(Session);
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
				string InstallDir = Session["INSTALLDIR"];

				if (!InstallDir.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					InstallDir += Path.DirectorySeparatorChar;

				Session.Log("Working folder: " + InstallDir);

				ProcessStartInfo ProcessInformation = new ProcessStartInfo()
				{
					FileName = InstallDir + "Waher.IotGateway.Svc.exe",
					Arguments = "-uninstall",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					WorkingDirectory = InstallDir,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden
				};

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
				else
				{
					if (!P.StandardError.EndOfStream)
						Session.Log(P.StandardError.ReadToEnd());

					if (!P.StandardOutput.EndOfStream)
						Session.Log(P.StandardOutput.ReadToEnd());

					if (P.ExitCode != 0)
						Session.Log("Uninstallation failed. Exit code: " + P.ExitCode.ToString());
					else
						Session.Log("Service uninstalled.");
				}
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

				ProcessStartInfo ProcessInformation = new ProcessStartInfo()
				{
					FileName = "net",
					Arguments = "start \"IoT Gateway Service\"",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					WorkingDirectory = InstallDir,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden
				};

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
				else
				{
					if (!P.StandardError.EndOfStream)
						Session.Log(P.StandardError.ReadToEnd());

					if (!P.StandardOutput.EndOfStream)
						Session.Log(P.StandardOutput.ReadToEnd());

					if (P.ExitCode != 0)
						throw new Exception("Service start failed. Exit code: " + P.ExitCode.ToString());
				}

				Session.Log("Service started.");

				return WaitAllModulesStarted(Session);
			}
			catch (Exception ex)
			{
				Session.Log("Unable to start service. Error reported: " + ex.Message);
				//return ActionResult.Failure;
				return ActionResult.Success;
			}
		}

		[CustomAction]
		public static ActionResult StopService(Session Session)
		{
			Session.Log("Stopping service.");
			try
			{
				string InstallDir = Session["INSTALLDIR"];

				ProcessStartInfo ProcessInformation = new ProcessStartInfo()
				{
					FileName = "net",
					Arguments = "stop \"IoT Gateway Service\"",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					WorkingDirectory = InstallDir,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden
				};

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
				else
				{
					if (!P.StandardError.EndOfStream)
						Session.Log(P.StandardError.ReadToEnd());

					if (!P.StandardOutput.EndOfStream)
						Session.Log(P.StandardOutput.ReadToEnd());

					if (P.ExitCode != 0)
						Session.Log("Stopping service failed. Exit code: " + P.ExitCode.ToString());
					else
						Session.Log("Service stopped.");
				}
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
				else
				{
					if (!P.StandardError.EndOfStream)
						Session.Log(P.StandardError.ReadToEnd());

					if (!P.StandardOutput.EndOfStream)
						Session.Log(P.StandardOutput.ReadToEnd());

					if (P.ExitCode != 0)
						Session.Log("Stopping http service failed. Exit code: " + P.ExitCode.ToString());
					else
						Session.Log("Service stopped.");
				}

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
				else 
				{
					if (!P.StandardError.EndOfStream)
						Session.Log(P.StandardError.ReadToEnd());

					if (!P.StandardOutput.EndOfStream)
						Session.Log(P.StandardOutput.ReadToEnd());

					if (P.ExitCode != 0)
						Session.Log("Disabling http service failed. Exit code: " + P.ExitCode.ToString());
					else
						Session.Log("Service disabled.");
				}
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

				string Port = string.Empty;
				string Protocol = "http";

				try
				{
					string s = File.ReadAllText(AppDataFolder + "Ports.txt");
					string[] Rows = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					SortedDictionary<int, bool> Ports = new SortedDictionary<int, bool>();

					foreach (string Row in Rows)
					{
						if (int.TryParse(Row, out int i))
							Ports[i] = true;
					}

					if (!Ports.ContainsKey(80))
					{
						if (Ports.ContainsKey(8080))
							Port = ":8080";
						else if (Ports.ContainsKey(8081))
							Port = ":8081";
						else if (Ports.ContainsKey(8082))
							Port = ":8082";
						else if (Ports.ContainsKey(443))
							Protocol = "https";
						else if (Ports.ContainsKey(8088))
						{
							Protocol = "https";
							Port = ":8088";
						}
					}
				}
				catch (Exception ex)
				{
					Session.Log("Unable to get opened ports. Error reported: " + ex.Message);
				}

				StartPage = Protocol + "://localhost" + Port + "/" + StartPage;
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
					string Xml = File.ReadAllText(FilePath);
					if (Xml.Contains("http://waher.se/SimpleXmppConfiguration.xsd"))
					{
						Xml = Xml.Replace("http://waher.se/SimpleXmppConfiguration.xsd", "http://waher.se/Schema/SimpleXmppConfiguration.xsd");
						File.WriteAllText(FilePath, Xml);
					}

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
				else
				{
					if (!P.StandardError.EndOfStream)
						Session.Log(P.StandardError.ReadToEnd());

					if (!P.StandardOutput.EndOfStream)
						Session.Log(P.StandardOutput.ReadToEnd());

					if (P.ExitCode != 0)
						throw new Exception("Installation failed. Exit code: " + P.ExitCode.ToString());
				}

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
				else
				{
					if (!P.StandardError.EndOfStream)
						Session.Log(P.StandardError.ReadToEnd());

					if (!P.StandardOutput.EndOfStream)
						Session.Log(P.StandardOutput.ReadToEnd());

					if (P.ExitCode != 0)
						throw new Exception("Failed to start service. Exit code: " + P.ExitCode.ToString());
				}

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

		[CustomAction]
		public static ActionResult BeforeUninstallEvent(Session Session)
		{
			Session.Log("Sending BeforeUninstall event.");
			try
			{
				using (ServiceController ServiceController = new ServiceController("IoT Gateway Service"))
				{
					ServiceController.ExecuteCommand(128);
				}
			}
			catch (Exception ex)
			{
				Session.Log("Unable to send event. The following error was reported: " + ex.Message);
			}

			return ActionResult.Success;
		}

		[CustomAction]
		public static ActionResult InstallManifest(Session Session)
		{
			string ManifestFile = Path.Combine(Session["INSTALLDIR"], Session["ManifestFile"]);
			string ServerApplication = Path.Combine(Session["INSTALLDIR"], "Waher.IoTGateway.Svc.dll");
			string ProgramDataFolder = Session["APPDATADIR"];

			Session.Log("Installing module: " + ManifestFile);
			Session.Log("Server application: " + ServerApplication);
			Session.Log("Program data folder: " + ProgramDataFolder);
			
			try
			{
				Install(Session, ManifestFile, ServerApplication, ProgramDataFolder);
				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log(ex.Message);
				return ActionResult.Failure;
			}
		}

		#region From Waher.Utility.Install

		private static void Install(Session Session, string ManifestFile, string ServerApplication, string ProgramDataFolder)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(ManifestFile))
				throw new Exception("Missing manifest file.");

			if (string.IsNullOrEmpty(ServerApplication))
				throw new Exception("Missing server application.");

			if (string.IsNullOrEmpty(ProgramDataFolder))
			{
				ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "IoT Gateway");
				Session.Log("Using default program data folder: " + ProgramDataFolder);
			}

			if (!File.Exists(ServerApplication))
				throw new Exception("Server application not found: " + ServerApplication);

			Session.Log("Getting assembly name of server.");
			AssemblyName ServerName = AssemblyName.GetAssemblyName(ServerApplication);
			Session.Log("Server assembly name: " + ServerName.ToString());

			string DepsJsonFileName;

			int i = ServerApplication.LastIndexOf('.');
			if (i < 0)
				DepsJsonFileName = ServerApplication;
			else
				DepsJsonFileName = ServerApplication.Substring(0, i);

			DepsJsonFileName += ".deps.json";

			Session.Log("deps.json file name: " + DepsJsonFileName);

			if (!File.Exists(DepsJsonFileName))
				throw new Exception("Invalid server executable. No corresponding deps.json file found.");

			Session.Log("Opening " + DepsJsonFileName);

			string s = File.ReadAllText(DepsJsonFileName);

			Session.Log("Parsing " + DepsJsonFileName);

			Dictionary<string, object> Deps = JSON.Parse(s) as Dictionary<string, object>;
			if (Deps == null)
				throw new Exception("Invalid deps.json file. Unable to install.");

			Session.Log("Loading manifest file.");

			XmlDocument Manifest = new XmlDocument();
			Manifest.Load(ManifestFile);

			Session.Log("Validating manifest file.");

			XmlElement Module = Manifest["Module"];
			string SourceFolder = Path.GetDirectoryName(ManifestFile);
			string AppFolder = Path.GetDirectoryName(ServerApplication);

			Session.Log("Source folder: " + SourceFolder);
			Session.Log("App folder: " + AppFolder);

			foreach (XmlNode N in Module.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "Assembly")
				{
					string FileName = XML.Attribute(E, "fileName");
					string SourceFileName = Path.Combine(SourceFolder, FileName);

					if (CopyFileIfNewer(Session, SourceFileName, Path.Combine(AppFolder, FileName), true))
					{
						if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
						{
							string PdbFileName = FileName.Substring(0, FileName.Length - 4) + ".pdb";
							if (File.Exists(PdbFileName))
								CopyFileIfNewer(Session, Path.Combine(SourceFolder, PdbFileName), Path.Combine(AppFolder, PdbFileName), false);
						}
					}

					Assembly A = Assembly.LoadFrom(SourceFileName);
					AssemblyName AN = A.GetName();

					if (Deps != null && Deps.TryGetValue("targets", out object Obj) && Obj is Dictionary<string, object> Targets)
					{
						foreach (KeyValuePair<string, object> P in Targets)
						{
							if (P.Value is Dictionary<string, object> Target)
							{
								foreach (KeyValuePair<string, object> P2 in Target)
								{
									if (P2.Key.StartsWith(ServerName.Name + "/") &&
										P2.Value is Dictionary<string, object> App &&
										App.TryGetValue("dependencies", out object Obj2) &&
										Obj2 is Dictionary<string, object> Dependencies)
									{
										Dependencies[AN.Name] = AN.Version.ToString();
										break;
									}
								}

								Dictionary<string, object> Dependencies2 = new Dictionary<string, object>();

								foreach (AssemblyName Dependency in A.GetReferencedAssemblies())
									Dependencies2[Dependency.Name] = Dependency.Version.ToString();

								Dictionary<string, object> Runtime = new Dictionary<string, object>()
									{
										{ Path.GetFileName(SourceFileName), new Dictionary<string,object>() }
									};

								Target[AN.Name + "/" + AN.Version.ToString()] = new Dictionary<string, object>()
									{
										{ "dependencies", Dependencies2 },
										{ "runtime", Runtime }
									};
							}
						}
					}

					if (Deps != null && Deps.TryGetValue("libraries", out object Obj3) && Obj3 is Dictionary<string, object> Libraries)
					{
						foreach (KeyValuePair<string, object> P in Libraries)
						{
							if (P.Key.StartsWith(AN.Name + "/"))
							{
								Libraries.Remove(P.Key);
								break;
							}
						}

						Libraries[AN.Name + "/" + AN.Version.ToString()] = new Dictionary<string, object>()
								{
									{ "type", "project" },
									{ "serviceable", false },
									{ "sha512", string.Empty }
								};
					}

				}
			}

			if (SourceFolder == AppFolder)
				Session.Log("Skipping copying of content. Source and application folders the same. Assuming content files are located where they should be.");
			else
				CopyContent(Session, SourceFolder, AppFolder, ProgramDataFolder, Module);

			Session.Log("Encoding JSON");
			s = JSON.Encode(Deps, true);

			Session.Log("Writing " + DepsJsonFileName);
			File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);
		}

		private static bool CopyFileIfNewer(Session Session, string From, string To, bool OnlyIfNewer)
		{
			if (From == To)
				return false;

			if (!File.Exists(From))
				throw new Exception("File not found: " + From);

			if (OnlyIfNewer && File.Exists(To))
			{
				DateTime ToTP = File.GetCreationTimeUtc(To);
				DateTime FromTP = File.GetCreationTimeUtc(From);

				if (ToTP >= FromTP)
					return false;
			}

			Session.Log("Copying " + From + " to " + To);
			File.Copy(From, To, true);

			return true;
		}

		private static void CopyContent(Session Session, string SourceFolder, string AppFolder, string DataFolder, XmlElement Parent)
		{
			foreach (XmlNode N in Parent.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "Content":
							string FileName = XML.Attribute(E, "fileName");

							Session.Log("Content file: " + FileName);

							if (!string.IsNullOrEmpty(DataFolder) && !Directory.Exists(DataFolder))
							{
								Session.Log("Creating folder " + DataFolder + ".");
								Directory.CreateDirectory(DataFolder);
							}

							CopyFileIfNewer(Session, Path.Combine(SourceFolder, FileName), Path.Combine(DataFolder, FileName), true);
							break;

						case "Folder":
							string Name = XML.Attribute(E, "name");

							string SourceFolder2 = Path.Combine(SourceFolder, Name);
							string AppFolder2 = Path.Combine(AppFolder, Name);
							string DataFolder2 = Path.Combine(DataFolder, Name);

							Session.Log("Folder: " + Name,
								new KeyValuePair<string, object>("Source", SourceFolder2),
								new KeyValuePair<string, object>("App", AppFolder2),
								new KeyValuePair<string, object>("Data", DataFolder2));

							CopyContent(Session, SourceFolder2, AppFolder2, DataFolder2, E);
							break;
					}
				}
			}
		}

		#endregion

	}
}
