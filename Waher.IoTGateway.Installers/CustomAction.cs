using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Deployment.WindowsInstaller;
using Waher.Networking.Sniffers;
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

					Client.Add(new SessionSniffer(Session));

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

						Client.Add(new SessionSniffer(Session));

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

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Validation of XMPP broker failed. Error reported: " + ex.Message);
				Session["Log"] = "Validation of XMPP broker failed. Error reported: " + ex.Message;
				return ActionResult.Failure;
			}
		}

	}
}
