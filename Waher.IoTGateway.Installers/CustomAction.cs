using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Deployment.WindowsInstaller;
using Waher.Networking.XMPP;

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

		[CustomAction]
		public static ActionResult ValidateBroker(Session Session)
		{
			Session.Log("Validating XMPP broker.");
			Session["Log"] = "Validating XMPP broker.";
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				Session.Log("XMPP broker to validate: " + XmppBroker);
				Session["Log"] = "XMPP broker to validate: " + XmppBroker;

				using (XmppClient Client = new XmppClient(XmppBroker, 5222, string.Empty, string.Empty, "en"))
				{
					ManualResetEvent Done = new ManualResetEvent(false);
					ManualResetEvent Fail = new ManualResetEvent(false);
					bool Connected = false;

					using (SessionSniffer Sniffer = new SessionSniffer(Session))
					{
						Client.Add(Sniffer);

						Client.OnStateChanged += (Sender, NewState) =>
						{
							Session["Log"] = "New state: " + NewState.ToString();

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

						if (WaitHandle.WaitAny(new WaitHandle[] { Done, Fail }, 5000) < 0 || !Connected)
						{
							Session["XmppBrokerOk"] = "0";
							Session.Log("Broker not reached. Domain name not OK.");
							Session["Log"] = "Broker not reached. Domain name not OK.";
						}
						else
						{
							Session["XmppBrokerOk"] = "1";

							if (Done.WaitOne(0))
							{
								Session["XmppPortRequired"] = "0";
								Session["XMPPPORT"] = "5222";
								Session.Log("Broker reached on default port (5222).");
								Session["Log"] = "Broker reached on default port (5222).";
							}
							else
							{
								Session["XmppPortRequired"] = "1";
								Session.Log("Broker reached, but XMPP service not available on default port (5222).");
								Session["Log"] = "Broker reached, but XMPP service not available on default port (5222).";
							}
						}
					}
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Validation of XMPP broker failed. Error reported: " + ex.Message);
				Session["Log"] = "Validation of XMPP broker failed. Error reported: " + ex.Message;
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult ValidatePort(Session Session)
		{
			Session.Log("Validating XMPP broker.");
			Session["Log"] = "Validating XMPP broker.";
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				int XmppPort;

				if (!int.TryParse(Session["XMPPPORT"], out XmppPort) || XmppPort <= 0 || XmppPort > 65535)
				{
					Session["XmppPortOk"] = "0";
					Session.Log("Invalid port number.");
					Session["Log"] = "Invalid port number.";
				}
				else
				{
					Session.Log("XMPP broker to validate: " + XmppBroker + ":" + XmppPort.ToString());
					Session["Log"] = "XMPP broker to validate: " + XmppBroker + ":" + XmppPort.ToString();

					using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, string.Empty, string.Empty, "en"))
					{
						ManualResetEvent Done = new ManualResetEvent(false);
						ManualResetEvent Fail = new ManualResetEvent(false);
						bool Connected = false;

						using (SessionSniffer Sniffer = new SessionSniffer(Session))
						{
							Client.Add(Sniffer);

							Client.OnStateChanged += (Sender, NewState) =>
						{
							Session["Log"] = "New state: " + NewState.ToString();

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

							if (WaitHandle.WaitAny(new WaitHandle[] { Done, Fail }, 5000) < 0 || !Connected)
							{
								Session["XmppPortOk"] = "0";
								Session.Log("Broker not reached. Domain name not OK.");
								Session["Log"] = "Broker not reached. Domain name not OK.";
							}
							else
							{
								if (Done.WaitOne(0))
								{
									Session["XmppPortOk"] = "1";
									Session.Log("Broker reached.");
									Session["Log"] = "Broker reached.";
								}
								else
								{
									Session["XmppPortOk"] = "0";
									Session.Log("Broker reached, but XMPP service not available on port.");
									Session["Log"] = "Broker reached, but XMPP service not available on port.";
								}
							}
						}
					}
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Validation of XMPP broker port failed. Error reported: " + ex.Message);
				Session["Log"] = "Validation of XMPP broker port failed. Error reported: " + ex.Message;
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult ValidateAccount(Session Session)
		{
			Session.Log("Validating XMPP account.");
			Session["Log"] = "Validating XMPP account.";
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				int XmppPort = int.Parse(Session["XMPPPORT"]);
				string XmppAccountName = Session["XMPPACCOUNTNAME"];
				string XmppPassword1 = Session["XMPPPASSWORD1"];
				string XmppPassword2 = Session["XMPPPASSWORD2"];

				if (XmppPassword1 != XmppPassword2)
				{
					Session.Log("Passwords not equal.");
					Session["Log"] = "Passwords not equal.";
					Session["XmppAccountOk"] = "-2";
				}
				else
				{
					using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, XmppAccountName, XmppPassword1, "en"))
					{
						ManualResetEvent Done = new ManualResetEvent(false);
						ManualResetEvent Fail = new ManualResetEvent(false);
						bool Connected = false;

						using (SessionSniffer Sniffer = new SessionSniffer(Session))
						{
							Client.Add(Sniffer);

							Client.OnStateChanged += (Sender, NewState) =>
						{
							Session["Log"] = "New state: " + NewState.ToString();

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
								Session.Log("Broker not reached, or user not authenticated within the time allotted.");
								Session["Log"] = "Broker not reached, or user not authenticated within the time allotted.";
							}
							else
							{
								if (Done.WaitOne(0))
								{
									Session["XmppAccountOk"] = "1";
									Session.Log("Account found and user authenticated.");
									Session["Log"] = "Account found and user authenticated.";
								}
								else
								{
									if (Client.CanRegister)
									{
										Session["XmppAccountOk"] = "-1";
										Session.Log("User not authenticated. Server supports In-band registration.");
										Session["Log"] = "User not authenticated. Server supports In-band registration.";
									}
									else
									{
										Session["XmppAccountOk"] = "0";
										Session.Log("User not authenticated.");
										Session["Log"] = "User not authenticated.";
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
				Session.Log("Validation of XMPP account failed. Error reported: " + ex.Message);
				Session["Log"] = "Validation of XMPP account failed. Error reported: " + ex.Message;
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult CreateAccount(Session Session)
		{
			Session.Log("Creating XMPP account.");
			Session["Log"] = "Creating XMPP account.";
			try
			{
				string XmppBroker = Session["XMPPBROKER"];
				int XmppPort = int.Parse(Session["XMPPPORT"]);
				string XmppAccountName = Session["XMPPACCOUNTNAME"];
				string XmppPassword1 = Session["XMPPPASSWORD1"];

				using (XmppClient Client = new XmppClient(XmppBroker, XmppPort, XmppAccountName, XmppPassword1, "en"))
				{
					Client.AllowRegistration();

					ManualResetEvent Done = new ManualResetEvent(false);
					ManualResetEvent Fail = new ManualResetEvent(false);
					bool Connected = false;

					using (SessionSniffer Sniffer = new SessionSniffer(Session))
					{
						Client.Add(Sniffer);

						Client.OnStateChanged += (Sender, NewState) =>
					{
						Session["Log"] = "New state: " + NewState.ToString();

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
							Session.Log("Broker not reached, or user not authenticated within the time allotted.");
							Session["Log"] = "Broker not reached, or user not authenticated within the time allotted.";
						}
						else
						{
							if (Done.WaitOne(0))
							{
								Session["XmppAccountOk"] = "1";
								Session.Log("Account created.");
								Session["Log"] = "Account created.";
							}
							else
							{
								Session["XmppAccountOk"] = "0";
								Session.Log("Unable to create account.");
								Session["Log"] = "Unable to create account.";
							}
						}
					}
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Creation of XMPP account failed. Error reported: " + ex.Message);
				Session["Log"] = "Creation of XMPP account failed. Error reported: " + ex.Message;
				return ActionResult.Failure;
			}
		}

	}
}
