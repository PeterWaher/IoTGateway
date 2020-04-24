using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Xml;
using System.Xml.Schema;
#if !WINDOWS_UWP
using Gma.QrCodeNet.Encoding.Windows.Render;
using Gma.QrCodeNet.Encoding;
#endif
using Waher.Content;
using Waher.Content.Xml;
#if !WINDOWS_UWP
using Waher.Content.Xsl;
#endif
using Waher.Networking;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Mock
{
	/// <summary>
	/// Class facilitating the setup of a simple XMPP connection.
	/// </summary>
	public static class SimpleXmppConfiguration
	{
#if !WINDOWS_UWP
		private static readonly XmlSchema schema = XSL.LoadSchema("Waher.Mock.Schema.SimpleXmppConfiguration.xsd");
		private const string expectedRootElement = "SimpleXmppConfiguration";
		private const string expectedNamespace = "http://waher.se/Schema/SimpleXmppConfiguration.xsd";
#endif

		/// <summary>
		/// Default XMPP Server.
		/// </summary>
		public const string DefaultXmppServer = "waher.se";


		/// <summary>
		/// Loads XMPP Client credentials from a file.
		/// </summary>
		/// <param name="FileName">Name of file containing configuration.</param>
		public static XmppCredentials Load(string FileName)
		{
			XmlDocument Xml = new XmlDocument();
			using (StreamReader r = File.OpenText(FileName))
			{
				Xml.Load(r);
			}

#if !WINDOWS_UWP
			XSL.Validate(FileName, Xml, expectedRootElement, expectedNamespace, schema);
#endif

			return Init(Xml.DocumentElement);
		}

		/// <summary>
		/// Loads XMPP Client credentials from a stream.
		/// </summary>
		/// <param name="ObjectID">Object ID of XML document. Used in case validation warnings are found during validation.</param>
		/// <param name="File">File containing configuration.</param>
		public static XmppCredentials Load(string ObjectID, Stream File)
        {
            XmlDocument Xml = new XmlDocument();
            Xml.Load(File);

#if !WINDOWS_UWP
            XSL.Validate(ObjectID, Xml, expectedRootElement, expectedNamespace, schema);
#endif

            return Init(Xml.DocumentElement);
        }

		/// <summary>
		/// Loads XMPP Client credentials from an XML Document.
		/// </summary>
		/// <param name="Xml">XML Document containing information.</param>
		public static XmppCredentials Load(XmlDocument Xml)
		{
#if !WINDOWS_UWP
			XSL.Validate(string.Empty, Xml, expectedRootElement, expectedNamespace, schema);
#endif

			return Init(Xml.DocumentElement);
		}

		/// <summary>
		/// Loads XMPP Client credentials from an XML element.
		/// </summary>
		/// <param name="Xml">XML element containing information.</param>
		public static XmppCredentials Load(XmlElement Xml)
		{
			return Init(Xml);
		}

		private static string AssertNotEnter(string s)
		{
			if (s.StartsWith("ENTER ", StringComparison.CurrentCultureIgnoreCase))
				throw new Exception("XMPP configuration not correctly provided.");

			return s;
		}

		private static XmppCredentials Init(XmlElement E)
		{
			XmppCredentials Result = new XmppCredentials();

			foreach (XmlNode N in E.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "Host":
						Result.Host = AssertNotEnter(N.InnerText);
						break;

					case "Port":
						Result.Port = int.Parse(N.InnerText);
						break;

					case "Account":
						Result.Account = AssertNotEnter(N.InnerText);
						break;

					case "Password":
						Result.Password = AssertNotEnter(N.InnerText);
						Result.PasswordType = XML.Attribute((XmlElement)N, "type");
						break;

					case "ThingRegistry":
						Result.ThingRegistry = AssertNotEnter(N.InnerText);
						break;

					case "Provisioning":
						Result.Provisioning = AssertNotEnter(N.InnerText);
						break;

					case "Events":
						Result.Events = AssertNotEnter(N.InnerText);
						break;

					case "Sniffer":
						if (!CommonTypes.TryParse(N.InnerText, out bool b))
							b = false;

						Result.Sniffer = b;
						break;

					case "TrustServer":
						if (!CommonTypes.TryParse(N.InnerText, out b))
							b = false;

						Result.TrustServer = b;
						break;

					case "AllowCramMD5":
						if (!CommonTypes.TryParse(N.InnerText, out b))
							b = false;

						Result.AllowCramMD5 = b;
						break;

					case "AllowDigestMD5":
						if (!CommonTypes.TryParse(N.InnerText, out b))
							b = false;

						Result.AllowDigestMD5 = b;
						break;

					case "AllowPlain":
						if (!CommonTypes.TryParse(N.InnerText, out b))
							b = false;

						Result.AllowPlain = b;
						break;

					case "AllowScramSHA1":
						if (!CommonTypes.TryParse(N.InnerText, out b))
							b = false;

						Result.AllowScramSHA1 = b;
						break;

					case "AllowEncryption":
						if (!CommonTypes.TryParse(N.InnerText, out b))
							b = false;

						Result.AllowEncryption = b;
						break;

					case "RequestRosterOnStartup":
						if (!CommonTypes.TryParse(N.InnerText, out b))
							b = false;

						Result.RequestRosterOnStartup = b;
						break;

					case "AllowRegistration":
						if (!CommonTypes.TryParse(N.InnerText, out b))
							b = false;

						Result.AllowRegistration = b;
						break;

					case "FormSignatureKey":
						Result.FormSignatureKey = AssertNotEnter(N.InnerText);
						break;

					case "FormSignatureSecret":
						Result.FormSignatureSecret = AssertNotEnter(N.InnerText);
						break;
				}
			}

			return Result;
		}

		/// <summary>
		/// Loads simple XMPP configuration from an XML file. If file does not exist, or is not valid, a console dialog with the user is performed,
		/// to get the relevant information, test it, and create the corresponding XML file for later use.
		/// </summary>
		/// <param name="FileName">Name of configuration file.</param>
		/// <param name="DefaultAccountName">Default account name.</param>
		/// <param name="DefaultPassword">Default password.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <returns>Simple XMPP configuration.</returns>
		public static XmppCredentials GetConfigUsingSimpleConsoleDialog(string FileName, string DefaultAccountName, string DefaultPassword,
			Assembly AppAssembly)
		{
			try
			{
				return Load(FileName);
			}
			catch (Exception
#if WINDOWS_UWP
			ex
#endif
			)
			{
				XmppCredentials Credentials = new XmppCredentials();
				bool Ok;

				Credentials.Host = DefaultXmppServer;
				Credentials.Port = XmppCredentials.DefaultPort;
				Credentials.Account = DefaultAccountName;
				Credentials.Password = DefaultPassword;

#if WINDOWS_UWP
				using (XmppClient Client = new XmppClient(Credentials, "en", AppAssembly))
				{
					Client.Connect();

					ManualResetEvent Connected = new ManualResetEvent(false);
					ManualResetEvent Failure = new ManualResetEvent(false);

					Client.OnStateChanged += (sender, NewState) =>
					{
						switch (NewState)
						{
							case XmppState.Connected:
								Credentials.Password = Client.PasswordHash;
								Credentials.PasswordType = Client.PasswordHashMethod;
								Connected.Set();
								break;

							case XmppState.Error:
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
						ServiceItemsDiscoveryEventArgs e = Client.ServiceItemsDiscovery(Client.Domain, 10000);

						foreach (Item Item in e.Items)
						{
							ServiceDiscoveryEventArgs e2 = Client.ServiceDiscovery(Item.JID, 10000);

							if (e2.Features.ContainsKey("urn:ieee:iot:disco:1.0") && string.IsNullOrEmpty(Credentials.ThingRegistry))
								Credentials.ThingRegistry = Item.JID;
							else
								Credentials.ThingRegistry = string.Empty;

							if (e2.Features.ContainsKey("urn:ieee:iot:prov:d:1.0") && string.IsNullOrEmpty(Credentials.Provisioning))
								Credentials.Provisioning = Item.JID;
							else
								Credentials.Provisioning = string.Empty;

							if (e2.Features.ContainsKey("urn:xmpp:eventlog") && string.IsNullOrEmpty(Credentials.Events))
								Credentials.Events = Item.JID;
							else
								Credentials.Events = string.Empty;
						}
					}
					else
						System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
				}
#else
				ConsoleColor FgBak = Console.ForegroundColor;
				string s;
				string Default;

				do
				{
					Console.ForegroundColor = ConsoleColor.Yellow;

					Console.Out.WriteLine();
					Console.Out.WriteLine("To setup a connection to the XMPP network, please answer the following");
					Console.Out.WriteLine("questions:");

					Default = Credentials.Host;
					Console.Out.WriteLine();
					Console.Out.WriteLine("What XMPP server do you want to use? Press ENTER to use " + Default);

					do
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.Out.Write("XMPP Server: ");
						Credentials.Host = Console.In.ReadLine();
						if (string.IsNullOrEmpty(Credentials.Host))
							Credentials.Host = Default;

						Console.ForegroundColor = ConsoleColor.Green;
						Console.Out.WriteLine();
						Console.Out.WriteLine("You've selected to use '" + Credentials.Host + "'. Is this correct? [y/n]");
						s = Console.In.ReadLine();
						Console.Out.WriteLine();
					}
					while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

					Default = Credentials.Port.ToString();
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Out.WriteLine("What port do you want to connect to? Press ENTER to use " + Default);

					do
					{
						Console.ForegroundColor = ConsoleColor.White;
						int Port;

						do
						{
							Console.Out.Write("Port Number: ");
							s = Console.In.ReadLine();
							if (string.IsNullOrEmpty(s))
								s = Default;
						}
						while (!int.TryParse(s, out Port) || Port < 1 || Port > 65535);

						Credentials.Port = Port;

						Console.ForegroundColor = ConsoleColor.Green;
						Console.Out.WriteLine();
						Console.Out.WriteLine("You've selected to use '" + Credentials.Port.ToString() + "'. Is this correct? [y/n]");
						s = Console.In.ReadLine();
						Console.Out.WriteLine();
					}
					while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

					Default = Credentials.Account;
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
							Credentials.Account = Console.In.ReadLine();
							if (string.IsNullOrEmpty(Credentials.Account))
								Credentials.Account = Default;
						}
						while (string.IsNullOrEmpty(Credentials.Account));

						Console.ForegroundColor = ConsoleColor.Green;
						Console.Out.WriteLine();
						Console.Out.WriteLine("You've selected to use '" + Credentials.Account + "'. Is this correct? [y/n]");
						s = Console.In.ReadLine();
						Console.Out.WriteLine();
					}
					while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

					Default = Credentials.Password;
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
							Credentials.Password = Console.In.ReadLine();
							if (string.IsNullOrEmpty(Credentials.Password))
								Credentials.Password = Default;
						}
						while (string.IsNullOrEmpty(Credentials.Password));

						Console.ForegroundColor = ConsoleColor.Green;
						Console.Out.WriteLine();
						Console.Out.WriteLine("You've selected to use '" + Credentials.Password + "'. Is this correct? [y/n]");
						s = Console.In.ReadLine();
						Console.Out.WriteLine();
					}
					while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

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
						Credentials.TrustServer = s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase);
					}
					while (!Credentials.TrustServer && !s.StartsWith("n", StringComparison.InvariantCultureIgnoreCase));

					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Out.WriteLine("Do you want to create an account on the server?");

					do
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.Out.Write("Create account [y/n]? ");

						s = Console.In.ReadLine();
						Credentials.AllowRegistration = s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase);
					}
					while (!Credentials.AllowRegistration && !s.StartsWith("n", StringComparison.InvariantCultureIgnoreCase));

					if (Credentials.AllowRegistration)
					{
						Console.ForegroundColor = ConsoleColor.Yellow;

						Console.Out.WriteLine();
						Console.Out.WriteLine("Some servers require an API key to allow account creation.");
						Console.Out.WriteLine("If so, please enter an API key below.");

						Default = Credentials.FormSignatureKey;
						Console.Out.WriteLine();
						Console.Out.WriteLine("What API Key do you want to use? Press ENTER to use " + Default);

						do
						{
							Console.ForegroundColor = ConsoleColor.White;
							Console.Out.Write("API Key: ");
							Credentials.FormSignatureKey = Console.In.ReadLine();
							if (string.IsNullOrEmpty(Credentials.FormSignatureKey))
								Credentials.FormSignatureKey = Default;

							Console.ForegroundColor = ConsoleColor.Green;
							Console.Out.WriteLine();
							Console.Out.WriteLine("You've selected to use '" + Credentials.FormSignatureKey + "'. Is this correct? [y/n]");
							s = Console.In.ReadLine();
							Console.Out.WriteLine();
						}
						while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

						if (!string.IsNullOrEmpty(Credentials.FormSignatureKey))
						{
							Console.ForegroundColor = ConsoleColor.Yellow;

							Console.Out.WriteLine();
							Console.Out.WriteLine("Please provide the corresponding secret below.");

							Default = Credentials.FormSignatureSecret;
							Console.Out.WriteLine();
							Console.Out.WriteLine("What secret belongs to the API key? Press ENTER to use " + Default);

							do
							{
								Console.ForegroundColor = ConsoleColor.White;
								Console.Out.Write("Secret: ");
								Credentials.FormSignatureSecret = Console.In.ReadLine();
								if (string.IsNullOrEmpty(Credentials.FormSignatureSecret))
									Credentials.FormSignatureSecret = Default;

								Console.ForegroundColor = ConsoleColor.Green;
								Console.Out.WriteLine();
								Console.Out.WriteLine("You've selected to use '" + Credentials.FormSignatureSecret + "'. Is this correct? [y/n]");
								s = Console.In.ReadLine();
								Console.Out.WriteLine();
							}
							while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));
						}
					}

					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Out.WriteLine("I will now try to connect to the server to see if the information");
					Console.Out.WriteLine("provided is correct.");

					using (XmppClient Client = new XmppClient(Credentials, "en", AppAssembly))
					{
						Client.Connect();

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
									Credentials.Password = Client.PasswordHash;
									Credentials.PasswordType = Client.PasswordHashMethod;
									Credentials.AllowRegistration = false;
									Credentials.FormSignatureKey = string.Empty;
									Credentials.FormSignatureSecret = string.Empty;
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

								if (e2.Features.ContainsKey("urn:ieee:iot:disco:1.0") && string.IsNullOrEmpty(Credentials.ThingRegistry))
								{
									Console.Out.WriteLine("Thing registry found.");
									Credentials.ThingRegistry = Item.JID;
								}
								else
									Credentials.ThingRegistry = string.Empty;

								if (e2.Features.ContainsKey("urn:ieee:iot:prov:d:1.0") && string.IsNullOrEmpty(Credentials.Provisioning))
								{
									Console.Out.WriteLine("Provisioning server found.");
									Credentials.Provisioning = Item.JID;
								}
								else
									Credentials.Provisioning = string.Empty;

								if (e2.Features.ContainsKey("urn:xmpp:eventlog") && string.IsNullOrEmpty(Credentials.Events))
								{
									Console.Out.WriteLine("Event log found.");
									Credentials.Events = Item.JID;
								}
								else
									Credentials.Events = string.Empty;
							}

						}
					}
				}
				while (!Ok);

				Default = Credentials.ThingRegistry;
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
					Credentials.ThingRegistry = Console.In.ReadLine();
					if (string.IsNullOrEmpty(Credentials.ThingRegistry))
						Credentials.ThingRegistry = Default;

					Console.ForegroundColor = ConsoleColor.Green;
					Console.Out.WriteLine();
					if (string.IsNullOrEmpty(Credentials.ThingRegistry))
						Console.Out.WriteLine("You've selected to not use a thing registry. Is this correct? [y/n]");
					else
						Console.Out.WriteLine("You've selected to use '" + Credentials.ThingRegistry + "'. Is this correct? [y/n]");
					s = Console.In.ReadLine();
					Console.Out.WriteLine();
				}
				while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

				Default = Credentials.Provisioning;
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
					Credentials.Provisioning = Console.In.ReadLine();
					if (string.IsNullOrEmpty(Credentials.Provisioning))
						Credentials.Provisioning = Default;

					Console.ForegroundColor = ConsoleColor.Green;
					Console.Out.WriteLine();
					if (string.IsNullOrEmpty(Credentials.Provisioning))
						Console.Out.WriteLine("You've selected to not use a provisioning server. Is this correct? [y/n]");
					else
						Console.Out.WriteLine("You've selected to use '" + Credentials.Provisioning + "'. Is this correct? [y/n]");
					s = Console.In.ReadLine();
					Console.Out.WriteLine();
				}
				while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

				Default = Credentials.Events;
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
					Credentials.Events = Console.In.ReadLine();
					if (string.IsNullOrEmpty(Credentials.Events))
						Credentials.Events = Default;

					Console.ForegroundColor = ConsoleColor.Green;
					Console.Out.WriteLine();
					if (string.IsNullOrEmpty(Credentials.Events))
						Console.Out.WriteLine("You've selected to not use an event log. Is this correct? [y/n]");
					else
						Console.Out.WriteLine("You've selected to use '" + Credentials.Events + "'. Is this correct? [y/n]");
					s = Console.In.ReadLine();
					Console.Out.WriteLine();
				}
				while (!s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase));

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Out.WriteLine("Do you want to use a sniffer? If you use a sniffer, XMPP network");
				Console.Out.WriteLine("communication will be Output to the console in real-time. This can");
				Console.Out.WriteLine("come in handy when debugging network communication.");

				do
				{
					Console.ForegroundColor = ConsoleColor.White;
					Console.Out.Write("Sniffer [y/n]? ");

					s = Console.In.ReadLine();
					Credentials.Sniffer = s.StartsWith("y", StringComparison.InvariantCultureIgnoreCase);
				}
				while (!Credentials.Sniffer && !s.StartsWith("n", StringComparison.InvariantCultureIgnoreCase));

				Console.Out.WriteLine();
				Console.ForegroundColor = FgBak;
#endif
				SaveSimpleXmppConfiguration(FileName, Credentials);

				return Credentials;
			}
		}

		/// <summary>
		/// Exports the settings to XML.
		/// </summary>
		/// <param name="Credentials">XMPP Client credentials.</param>
		/// <returns>XML</returns>
		public static string ExportSimpleXmppConfiguration(XmppCredentials Credentials)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.AppendLine("<?xml version='1.0' encoding='utf-8'?>");
			Xml.AppendLine("<SimpleXmppConfiguration xmlns='http://waher.se/Schema/SimpleXmppConfiguration.xsd'>");

			Xml.Append("\t<Host>");
			Xml.Append(XML.Encode(Credentials.Host));
			Xml.AppendLine("</Host>");

			Xml.Append("\t<Port>");
			Xml.Append(Credentials.Port.ToString());
			Xml.AppendLine("</Port>");

			Xml.Append("\t<Account>");
			Xml.Append(XML.Encode(Credentials.Account));
			Xml.AppendLine("</Account>");

			Xml.Append("\t<Password type=\"");
			Xml.Append(XML.Encode(Credentials.PasswordType));
			Xml.Append("\">");
			Xml.Append(XML.Encode(Credentials.Password));
			Xml.AppendLine("</Password>");

			Xml.Append("\t<ThingRegistry>");
			Xml.Append(XML.Encode(Credentials.ThingRegistry));
			Xml.AppendLine("</ThingRegistry>");

			Xml.Append("\t<Provisioning>");
			Xml.Append(XML.Encode(Credentials.Provisioning));
			Xml.AppendLine("</Provisioning>");

			Xml.Append("\t<Events>");
			Xml.Append(XML.Encode(Credentials.Events));
			Xml.AppendLine("</Events>");

			Xml.Append("\t<Sniffer>");
			Xml.Append(CommonTypes.Encode(Credentials.Sniffer));
			Xml.AppendLine("</Sniffer>");

			Xml.Append("\t<TrustServer>");
			Xml.Append(CommonTypes.Encode(Credentials.TrustServer));
			Xml.AppendLine("</TrustServer>");

			Xml.Append("\t<AllowCramMD5>");
			Xml.Append(CommonTypes.Encode(Credentials.AllowCramMD5));
			Xml.AppendLine("</AllowCramMD5>");

			Xml.Append("\t<AllowDigestMD5>");
			Xml.Append(CommonTypes.Encode(Credentials.AllowDigestMD5));
			Xml.AppendLine("</AllowDigestMD5>");

			Xml.Append("\t<AllowPlain>");
			Xml.Append(CommonTypes.Encode(Credentials.AllowPlain));
			Xml.AppendLine("</AllowPlain>");

			Xml.Append("\t<AllowScramSHA1>");
			Xml.Append(CommonTypes.Encode(Credentials.AllowScramSHA1));
			Xml.AppendLine("</AllowScramSHA1>");

			Xml.Append("\t<AllowEncryption>");
			Xml.Append(CommonTypes.Encode(Credentials.AllowEncryption));
			Xml.AppendLine("</AllowEncryption>");

			Xml.Append("\t<RequestRosterOnStartup>");
			Xml.Append(CommonTypes.Encode(Credentials.RequestRosterOnStartup));
			Xml.AppendLine("</RequestRosterOnStartup>");

			Xml.Append("\t<AllowRegistration>");
			Xml.Append(CommonTypes.Encode(Credentials.AllowRegistration));
			Xml.AppendLine("</AllowRegistration>");

			Xml.Append("\t<FormSignatureKey>");
			Xml.Append(XML.Encode(Credentials.FormSignatureKey));
			Xml.AppendLine("</FormSignatureKey>");

			Xml.Append("\t<FormSignatureSecret>");
			Xml.Append(XML.Encode(Credentials.FormSignatureSecret));
			Xml.AppendLine("</FormSignatureSecret>");

			Xml.AppendLine("</SimpleXmppConfiguration>");

			return Xml.ToString();
		}

		/// <summary>
		/// Saves the settings to a file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Credentials">XMPP Client credentials.</param>
		public static void SaveSimpleXmppConfiguration(string FileName, XmppCredentials Credentials)
		{
			File.WriteAllText(FileName, ExportSimpleXmppConfiguration(Credentials), Encoding.UTF8);
		}

		/// <summary>
		/// Gets a new XMPP client using the settings provided in the current object.
		/// </summary>
		/// <param name="Credentials">XMPP Client credentials.</param>
		/// <param name="Language">Primary language.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="Connect">If a connection should be initiated directly.</param>
		/// <returns>XMPP Client object.</returns>
		public static XmppClient GetClient(XmppCredentials Credentials, string Language, Assembly AppAssembly, bool Connect)
		{
			XmppClient Client = new XmppClient(Credentials, Language, AppAssembly);

			if (Connect)
				Client.Connect();

			return Client;
		}

#if !WINDOWS_UWP
		/// <summary>
		/// Prints a QR Code on the console output.
		/// </summary>
		/// <param name="URI">URI to encode.</param>
		public static void PrintQRCode(string URI)
		{
			QrEncoder encoder = new QrEncoder(ErrorCorrectionLevel.M);

			if (encoder.TryEncode(URI, out QrCode qrCode))
			{
				ConsoleColor Bak = Console.BackgroundColor;
				BitMatrix Matrix = qrCode.Matrix;
				int w = Matrix.Width;
				int h = Matrix.Height;
				int x, y;

				for (y = -3; y < h + 3; y++)
				{
					for (x = -3; x < w + 3; x++)
					{
						if (x >= 0 && x < w && y >= 0 && y < h && Matrix[x, y])
						{
							Console.BackgroundColor = ConsoleColor.Black;
							Console.Out.Write("  ");
						}
						else
						{
							Console.BackgroundColor = ConsoleColor.White;
							Console.Out.Write("  ");
						}
					}

					Console.Out.WriteLine();
				}

				Console.BackgroundColor = Bak;
			}
		}
#endif
		/// <summary>
		/// Returns an URL to a Google Chart API QR Code encoding <paramref name="URI"/>.
		/// </summary>
		/// <param name="URI">URI to encode.</param>
		/// <param name="Width">Width of image.</param>
		/// <param name="Height">Height of image.</param>
		/// <returns></returns>
		public static string GetQRCodeURL(string URI, int Width, int Height)
		{
			StringBuilder Result = new StringBuilder();

			Result.Append("https://chart.googleapis.com/chart?cht=qr&chs=");
			Result.Append(Width.ToString());
			Result.Append("x");
			Result.Append(Height.ToString());
			Result.Append("&chl=");
			Result.Append(Uri.EscapeDataString(URI));

			return Result.ToString();
		}

	}
}
