using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Networking;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Mock
{
	/// <summary>
	/// Class containing information about a simple XMPP connection.
	/// </summary>
	public class SimpleXmppConfiguration
	{
		private static readonly XmlSchema schema = XML.LoadSchema("Waher.Mock.Schema.SimpleXmppConfiguration.xsd", Assembly.GetExecutingAssembly());
		private const string expectedRootElement = "SimpleXmppConfiguration";
		private const string expectedNamespace = "http://waher.se/SimpleXmppConfiguration.xsd";

		/// <summary>
		/// Default XMPP Server.
		/// </summary>
		public const string DefaultXmppServer = "thingk.me";

		/// <summary>
		/// Default XMPP Server port.
		/// </summary>
		public const int DefaultPort = 5222;

		private string host;
		private string account;
		private string password;
		private string passwordType;
		private string thingRegistry;
		private string provisioning;
		private string events;
		private bool sniffer;
		private bool trustServer;
		private int port;

		private SimpleXmppConfiguration()
		{
		}

		/// <summary>
		/// Class containing information about a simple XMPP connection.
		/// </summary>
		/// <param name="FileName">Name of file containing configuration.</param>
		public SimpleXmppConfiguration(string FileName)
		{
			XmlDocument Xml = new XmlDocument();
			Xml.Load(FileName);

			XML.Validate(FileName, Xml, expectedRootElement, expectedNamespace, schema);

			this.Init(Xml.DocumentElement);
		}

		/// <summary>
		/// Class containing information about a simple XMPP connection.
		/// </summary>
		/// <param name="Xml">XML Document containing information.</param>
		public SimpleXmppConfiguration(XmlDocument Xml)
		{
			XML.Validate(string.Empty, Xml, expectedRootElement, expectedNamespace, schema);

			this.Init(Xml.DocumentElement);
		}

		/// <summary>
		/// Class containing information about a simple XMPP connection.
		/// </summary>
		/// <param name="Xml">XML element containing information.</param>
		public SimpleXmppConfiguration(XmlElement Xml)
		{
			this.Init(Xml);
		}

		private void Init(XmlElement E)
		{
			foreach (XmlNode N in E.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "Host":
						this.host = N.InnerText;
						break;

					case "Port":
						this.port = int.Parse(N.InnerText);
						break;

					case "Account":
						this.account = N.InnerText;
						break;

					case "Password":
						this.password = N.InnerText;
						this.passwordType = XML.Attribute((XmlElement)N, "type");
						break;

					case "ThingRegistry":
						this.thingRegistry = N.InnerText;
						break;

					case "Provisioning":
						this.provisioning = N.InnerText;
						break;

					case "Events":
						this.events = N.InnerText;
						break;

					case "Sniffer":
						if (!CommonTypes.TryParse(N.InnerText, out this.sniffer))
							this.sniffer = false;
						break;
				}
			}
		}

		/// <summary>
		/// Host name of XMPP server.
		/// </summary>
		public string Host { get { return this.host; } }

		/// <summary>
		/// Name of account on XMPP server to connect to.
		/// </summary>
		public string Account { get { return this.account; } }

		/// <summary>
		/// Password of account.
		/// </summary>
		public string Password { get { return this.password; } }

		/// <summary>
		/// Password type of account.
		/// </summary>
		public string PasswordType { get { return this.passwordType; } }

		/// <summary>
		/// JID of Thing Registry to use. Leave blank if no thing registry is to be used.
		/// </summary>
		public string ThingRegistry { get { return this.thingRegistry; } }

		/// <summary>
		/// JID of Provisioning Server to use. Leave blank if no thing registry is to be used.
		/// </summary>
		public string Provisioning { get { return this.provisioning; } }

		/// <summary>
		/// JID of entity to whom events should be sent. Leave blank if events are not to be forwarded.
		/// </summary>
		public string Events { get { return this.events; } }

		/// <summary>
		/// If a sniffer is to be used ('true' or 'false'). If 'true', network communication will be output to the console.
		/// </summary>
		public bool Sniffer { get { return this.sniffer; } }

		/// <summary>
		/// Port number to use when connecting to XMPP server.
		/// </summary>
		public int Port { get { return this.port; } }

		/// <summary>
		/// If the server certificate should be trusted automatically ('true'), or if a certificate validation should be done to 
		/// test the validity of the server ('false').
		/// </summary>
		public bool TrustServer { get { return this.trustServer; } }

		/// <summary>
		/// Loads simple XMPP configuration from an XML file. If file does not exist, or is not valid, a console dialog with the user is performed,
		/// to get the relevant information, test it, and create the corresponding XML file for later use.
		/// </summary>
		/// <param name="FileName">Name of configuration file.</param>
		/// <param name="DefaultAccountName">Default account name.</param>
		/// <param name="DefaultPassword">Default password.</param>
		/// <param name="FormSignatureKey">Form signature key, if form signatures (XEP-0348) is to be used during registration.</param>
		/// <param name="FormSignatureSecret">Form signature secret, if form signatures (XEP-0348) is to be used during registration.</param>
		/// <returns>Simple XMPP configuration.</returns>
		public static SimpleXmppConfiguration GetConfigUsingSimpleConsoleDialog(string FileName, string DefaultAccountName, string DefaultPassword,
			string FormSignatureKey, string FormSignatureSecret)
		{
			try
			{
				return new SimpleXmppConfiguration("xmpp.config");
			}
			catch (Exception)
			{
				ConsoleColor FgBak = Console.ForegroundColor;
				SimpleXmppConfiguration Config = new SimpleXmppConfiguration();
				string s;
				string Default;
				bool Ok;

				Config.host = DefaultXmppServer;
				Config.port = DefaultPort;
				Config.account = DefaultAccountName;
				Config.password = DefaultPassword;

				do
				{
					Console.ForegroundColor = ConsoleColor.Yellow;

					Console.Out.WriteLine();
					Console.Out.WriteLine("To setup a connection to the XMPP network, please answer the following");
					Console.Out.WriteLine("questions:");

					Default = Config.host;
					Console.Out.WriteLine();
					Console.Out.WriteLine("What XMPP server do you want to use? Press ENTER to use " + Default);

					do
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.Out.Write("XMPP Server: ");
						Config.host = Console.In.ReadLine();
						if (string.IsNullOrEmpty(Config.host))
							Config.host = Default;

						Console.ForegroundColor = ConsoleColor.Green;
						Console.Out.WriteLine();
						Console.Out.WriteLine("You've selected to use '" + Config.host + "'. Is this correct? [y/n]");
						s = Console.In.ReadLine();
						Console.Out.WriteLine();
					}
					while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

					Default = Config.port.ToString();
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Out.WriteLine("What port do you want to connect to? Press ENTER to use " + Default);

					do
					{
						Console.ForegroundColor = ConsoleColor.White;

						do
						{
							Console.Out.Write("Port Number: ");
							s = Console.In.ReadLine();
							if (string.IsNullOrEmpty(s))
								s = Default;
						}
						while (!int.TryParse(s, out Config.port) || Config.port < 1 || Config.port > 65535);

						Console.ForegroundColor = ConsoleColor.Green;
						Console.Out.WriteLine();
						Console.Out.WriteLine("You've selected to use '" + Config.port.ToString() + "'. Is this correct? [y/n]");
						s = Console.In.ReadLine();
						Console.Out.WriteLine();
					}
					while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

					Default = Config.account;
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Out.WriteLine("What account to you want to connect with? If the account does not exist,");
					Console.Out.WriteLine("but the server supports In-band registration, the account will be created.");

					if (!string.IsNullOrEmpty(Default))
						Console.Out.WriteLine("Press ENTER to use " + Default);

					do
					{
						Console.ForegroundColor = ConsoleColor.White;
						do
						{
							Console.Out.Write("Account: ");
							Config.account = Console.In.ReadLine();
							if (string.IsNullOrEmpty(Config.account))
								Config.account = Default;
						}
						while (string.IsNullOrEmpty(Config.account));

						Console.ForegroundColor = ConsoleColor.Green;
						Console.Out.WriteLine();
						Console.Out.WriteLine("You've selected to use '" + Config.account + "'. Is this correct? [y/n]");
						s = Console.In.ReadLine();
						Console.Out.WriteLine();
					}
					while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

					Default = Config.password;
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Out.WriteLine("What password goes with the account? Remember that the configuration will,");
					Console.Out.Write("be stored in a simple text file along with the application. ");

					if (!string.IsNullOrEmpty(Default))
						Console.Out.WriteLine("Press ENTER to use " + Default);

					do
					{
						Console.ForegroundColor = ConsoleColor.White;
						do
						{
							Console.Out.Write("Password: ");
							Config.password = Console.In.ReadLine();
							if (string.IsNullOrEmpty(Config.password))
								Config.password = Default;
						}
						while (string.IsNullOrEmpty(Config.password));

						Console.ForegroundColor = ConsoleColor.Green;
						Console.Out.WriteLine();
						Console.Out.WriteLine("You've selected to use '" + Config.password + "'. Is this correct? [y/n]");
						s = Console.In.ReadLine();
						Console.Out.WriteLine();
					}
					while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

					Default = Config.events;
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Out.WriteLine("Do you want to trust the server? It's better not to trust the server.");
					Console.Out.WriteLine("This will force the server certificate to be validated, and the server");
					Console.Out.WriteLine("trusted only if the certificate is valid. If there's a valid problem with");
					Console.Out.WriteLine("the server certificate however, you might need to trust the server to");
					Console.Out.WriteLine("be able to proceed.");

					do
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.Out.Write("Trust server [y/n]? ");

						s = Console.In.ReadLine();
						Config.trustServer = s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase);
					}
					while (!Config.trustServer && !s.StartsWith("n", StringComparison.InvariantCultureIgnoreCase));

					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Out.WriteLine("I will now try to connect to the server to see if the information");
					Console.Out.WriteLine("provided is correct.");

					using (XmppClient Client = new XmppClient(Config.host, Config.port, Config.account, Config.password, "en"))
					{
						Client.AllowRegistration(FormSignatureKey, FormSignatureSecret);

						if (Config.trustServer)
							Client.TrustServer = true;

						ManualResetEvent Connected = new ManualResetEvent(false);
						ManualResetEvent Failure = new ManualResetEvent(false);

						Client.OnStateChanged += (sender, NewState) =>
						{
							Console.Out.WriteLine(NewState.ToString());

							switch (NewState)
							{
								case XmppState.Connected:
									Console.ForegroundColor = ConsoleColor.Green;
									Console.Out.WriteLine("Connection successful.");
									Config.password = Client.PasswordHash;
									Config.passwordType = Client.PasswordHashMethod;
									Connected.Set();
									break;

								case XmppState.Error:
									Console.ForegroundColor = ConsoleColor.Red;
									Console.Out.WriteLine("Connection failed. Please update connection information.");
									Failure.Set();
									break;
							}
						};

						switch (WaitHandle.WaitAny(new WaitHandle[] { Connected, Failure }, 20000))
						{
							case 0:
								Ok = true;
								break;

							case 1:
							default:
								Ok = false;
								break;
						}

						if (Ok)
						{
							Console.ForegroundColor = ConsoleColor.Yellow;
							Console.Out.WriteLine("Checking server capabilities.");

							ServiceItemsDiscoveryEventArgs e = Client.ServiceItemsDiscovery(Client.Domain, 10000);

							foreach (Item Item in e.Items)
							{
								Console.Out.WriteLine("Checking " + Item.JID + ".");
								ServiceDiscoveryEventArgs e2 = Client.ServiceDiscovery(Item.JID, 10000);

								if (e2.Features.ContainsKey("urn:xmpp:iot:discovery") && string.IsNullOrEmpty(Config.thingRegistry))
								{
									Console.Out.WriteLine("Thing registry found.");
									Config.thingRegistry = Item.JID;
								}
								else
									Config.thingRegistry = string.Empty;

								if (e2.Features.ContainsKey("urn:xmpp:iot:provisioning") && string.IsNullOrEmpty(Config.provisioning))
								{
									Console.Out.WriteLine("Provisioning server found.");
									Config.provisioning = Item.JID;
								}
								else
									Config.provisioning = string.Empty;

								if (e2.Features.ContainsKey("urn:xmpp:eventlog") && string.IsNullOrEmpty(Config.events))
								{
									Console.Out.WriteLine("Event log found.");
									Config.events = Item.JID;
								}
								else
									Config.events = string.Empty;
							}

						}
					}
				}
				while (!Ok);

				Default = Config.thingRegistry;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Out.WriteLine("What thing registry do you want to use? The use of a thing registry is optional.");

				if (string.IsNullOrEmpty(Default))
					Console.Out.WriteLine("Press ENTER to not use a thing registry.");
				else
					Console.Out.WriteLine("Press ENTER to use " + Default);

				do
				{
					Console.ForegroundColor = ConsoleColor.White;
					Console.Out.Write("Thing Registry: ");
					Config.thingRegistry = Console.In.ReadLine();
					if (string.IsNullOrEmpty(Config.thingRegistry))
						Config.thingRegistry = Default;

					Console.ForegroundColor = ConsoleColor.Green;
					Console.Out.WriteLine();
					if (string.IsNullOrEmpty(Config.thingRegistry))
						Console.Out.WriteLine("You've selected to not use a thing registry. Is this correct? [y/n]");
					else
						Console.Out.WriteLine("You've selected to use '" + Config.thingRegistry + "'. Is this correct? [y/n]");
					s = Console.In.ReadLine();
					Console.Out.WriteLine();
				}
				while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

				Default = Config.provisioning;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Out.WriteLine("What provisioning server do you want to use? The use of a provisioning server");
				Console.Out.WriteLine("is optional.");

				if (string.IsNullOrEmpty(Default))
					Console.Out.WriteLine("Press ENTER to not use a provisioning server.");
				else
					Console.Out.WriteLine("Press ENTER to use " + Default);

				do
				{
					Console.ForegroundColor = ConsoleColor.White;
					Console.Out.Write("Provisioning Server: ");
					Config.provisioning = Console.In.ReadLine();
					if (string.IsNullOrEmpty(Config.provisioning))
						Config.provisioning = Default;

					Console.ForegroundColor = ConsoleColor.Green;
					Console.Out.WriteLine();
					if (string.IsNullOrEmpty(Config.provisioning))
						Console.Out.WriteLine("You've selected to not use a provisioning server. Is this correct? [y/n]");
					else
						Console.Out.WriteLine("You've selected to use '" + Config.provisioning + "'. Is this correct? [y/n]");
					s = Console.In.ReadLine();
					Console.Out.WriteLine();
				}
				while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

				Default = Config.events;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Out.WriteLine("What event log do you want to use? The use of a event logs is optional.");

				if (string.IsNullOrEmpty(Default))
					Console.Out.WriteLine("Press ENTER to not use an event log.");
				else
					Console.Out.WriteLine("Press ENTER to use " + Default);

				do
				{
					Console.ForegroundColor = ConsoleColor.White;
					Console.Out.Write("Event Log: ");
					Config.events = Console.In.ReadLine();
					if (string.IsNullOrEmpty(Config.events))
						Config.events = Default;

					Console.ForegroundColor = ConsoleColor.Green;
					Console.Out.WriteLine();
					if (string.IsNullOrEmpty(Config.events))
						Console.Out.WriteLine("You've selected to not use an event log. Is this correct? [y/n]");
					else
						Console.Out.WriteLine("You've selected to use '" + Config.events + "'. Is this correct? [y/n]");
					s = Console.In.ReadLine();
					Console.Out.WriteLine();
				}
				while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

				Default = Config.events;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Out.WriteLine("Do you want to use a sniffer? If you use a sniffer, XMPP network");
				Console.Out.WriteLine("communication will be Output to the console in real-time. This can");
				Console.Out.WriteLine("come in handy when debugging network communication.");

				do
				{
					Console.ForegroundColor = ConsoleColor.White;
					Console.Out.Write("Sniffer [y/n]? ");

					s = Console.In.ReadLine();
					Config.sniffer = s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase);
				}
				while (!Config.sniffer && !s.StartsWith("n", StringComparison.InvariantCultureIgnoreCase));

				Console.Out.WriteLine();
				Console.ForegroundColor = FgBak;

				StringBuilder Xml = new StringBuilder();

				Xml.AppendLine("<?xml version='1.0' encoding='utf-8'?>");
				Xml.AppendLine("<SimpleXmppConfiguration xmlns='http://waher.se/SimpleXmppConfiguration.xsd'>");

				Xml.Append("\t<Host>");
				Xml.Append(XML.Encode(Config.host));
				Xml.AppendLine("</Host>");

				Xml.Append("\t<Port>");
				Xml.Append(Config.port.ToString());
				Xml.AppendLine("</Port>");

				Xml.Append("\t<Account>");
				Xml.Append(XML.Encode(Config.account));
				Xml.AppendLine("</Account>");

				Xml.Append("\t<Password type=\"");
				Xml.Append(XML.Encode(Config.passwordType));
				Xml.Append("\">");
				Xml.Append(XML.Encode(Config.password));
				Xml.AppendLine("</Password>");

				Xml.Append("\t<ThingRegistry>");
				Xml.Append(XML.Encode(Config.thingRegistry));
				Xml.AppendLine("</ThingRegistry>");

				Xml.Append("\t<Provisioning>");
				Xml.Append(XML.Encode(Config.provisioning));
				Xml.AppendLine("</Provisioning>");

				Xml.Append("\t<Events>");
				Xml.Append(XML.Encode(Config.events));
				Xml.AppendLine("</Events>");

				Xml.Append("\t<Sniffer>");
				Xml.Append(CommonTypes.Encode(Config.sniffer));
				Xml.AppendLine("</Sniffer>");

				Xml.Append("\t<TrustServer>");
				Xml.Append(CommonTypes.Encode(Config.trustServer));
				Xml.AppendLine("</TrustServer>");

				Xml.AppendLine("</SimpleXmppConfiguration>");

				File.WriteAllText(FileName, Xml.ToString(), Encoding.UTF8);

				return Config;
			}
		}

		public XmppClient GetClient(string Language)
		{
			if (string.IsNullOrEmpty(this.passwordType))
				return new XmppClient(this.host, this.port, this.account, this.password, "en");
			else
				return new XmppClient(this.host, this.port, this.account, this.password, this.passwordType, "en");
		}

	}
}
