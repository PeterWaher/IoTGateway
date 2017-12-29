using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Mock;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence;
using Waher.Persistence.Files;

namespace Waher.IoTGateway.Console
{
	/// <summary>
	/// A console application version of the IoT gateway. It's easy to use and experiment with.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				System.Console.ForegroundColor = ConsoleColor.White;

				System.Console.Out.WriteLine("Welcome to the Internet of Things Gateway server application.");
				System.Console.Out.WriteLine(new string('-', 79));
				System.Console.Out.WriteLine("This server application will help you manage IoT devices and");
				System.Console.Out.WriteLine("create dynamic content that you can publish on the Internet.");
				System.Console.Out.WriteLine("It also provides programming interfaces (API) which allow you");
				System.Console.Out.WriteLine("to dynamically and securely interact with the devices and the");
				System.Console.Out.WriteLine("content you publish.");

				Log.Register(new ConsoleEventSink(false));
				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					if (e.IsTerminating)
					{
						using (StreamWriter w = File.CreateText(Path.Combine(Gateway.AppDataFolder, "UnhandledException.txt")))
						{
							w.Write("Type: ");

							if (e.ExceptionObject != null)
								w.WriteLine(e.ExceptionObject.GetType().FullName);
							else
								w.WriteLine("null");

							w.Write("Time: ");
							w.WriteLine(DateTime.Now.ToString());

							w.WriteLine();
							if (e.ExceptionObject is Exception ex)
							{
								w.WriteLine(ex.Message);
								w.WriteLine();
								w.WriteLine(ex.StackTrace);
							}
							else
							{
								if (e.ExceptionObject != null)
									w.WriteLine(e.ExceptionObject.ToString());

								w.WriteLine();
								w.WriteLine(Environment.StackTrace);
							}

							w.Flush();
						}
					}

					if (e.ExceptionObject is Exception ex2)
						Log.Critical(ex2);
					else if (e.ExceptionObject != null)
						Log.Critical(e.ExceptionObject.ToString());
					else
						Log.Critical("Unexpected null exception thrown.");
				};

				Gateway.GetDatabaseProvider += GetDatabase;
				Gateway.GetXmppClientCredentials += GetXmppClientCredentials;
				Gateway.XmppCredentialsUpdated += XmppCredentialsUpdated;
				Gateway.RegistrationSuccessful += RegistrationSuccessful;

				if (!Gateway.Start(true))
				{
					System.Console.Out.WriteLine();
					System.Console.Out.WriteLine("Gateway being started in another process.");
					return;
				}

				ManualResetEvent Done = new ManualResetEvent(false);
				System.Console.CancelKeyPress += (sender, e) => Done.Set();

				try
				{
					SetConsoleCtrlHandler((ControlType) =>
					{
						switch (ControlType)
						{
							case CtrlTypes.CTRL_BREAK_EVENT:
							case CtrlTypes.CTRL_CLOSE_EVENT:
							case CtrlTypes.CTRL_C_EVENT:
							case CtrlTypes.CTRL_SHUTDOWN_EVENT:
								Done.Set();
								break;

							case CtrlTypes.CTRL_LOGOFF_EVENT:
								break;
						}

						return true;
					}, true);
				}
				catch (Exception)
				{
					Log.Error("Unable to register CTRL-C control handler.");
				}

				while (!Done.WaitOne(1000))
					;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				Gateway.Stop();
				Log.Terminate();
			}
		}

		private static IDatabaseProvider GetDatabase(XmlElement DatabaseConfig)
		{
			if (!CommonTypes.TryParse(DatabaseConfig.Attributes["encrypted"].Value, out bool Encrypted))
				Encrypted = true;

			return new FilesProvider(Gateway.AppDataFolder + DatabaseConfig.Attributes["folder"].Value,
				DatabaseConfig.Attributes["defaultCollectionName"].Value,
				int.Parse(DatabaseConfig.Attributes["blockSize"].Value),
				int.Parse(DatabaseConfig.Attributes["blocksInCache"].Value),
				int.Parse(DatabaseConfig.Attributes["blobBlockSize"].Value), Encoding.UTF8,
				int.Parse(DatabaseConfig.Attributes["timeoutMs"].Value),
				Encrypted, false, true);
		}

		private static XmppCredentials GetXmppClientCredentials(string XmppConfigFileName)
		{
			XmppCredentials Result = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog(XmppConfigFileName,
				Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
				Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
				typeof(Gateway).Assembly);

			return Result;
		}

		private static void XmppCredentialsUpdated(string XmppConfigFileName, XmppCredentials Credentials)
		{
			SimpleXmppConfiguration.SaveSimpleXmppConfiguration(XmppConfigFileName, Credentials);
		}

		private static void RegistrationSuccessful(MetaDataTag[] MetaData, RegistrationEventArgs e)
		{
			if (!e.IsClaimed)
			{
				SimpleXmppConfiguration.PrintQRCode(ThingRegistryClient.EncodeAsIoTDiscoURI(MetaData));

				string ClaimUrl = ThingRegistryClient.EncodeAsIoTDiscoURI(MetaData);
				string FilePath = Path.Combine(Gateway.AppDataFolder, "Gateway.iotdisco");

				Log.Informational("Registration successful.");
				Log.Informational(ClaimUrl, new KeyValuePair<string, object>("Path", FilePath));

				File.WriteAllText(FilePath, ClaimUrl);
			}
		}

		#region unmanaged

		// https://msdn.microsoft.com/en-us/library/windows/desktop/ms686016(v=vs.85).aspx
		// https://msdn.microsoft.com/en-us/library/windows/desktop/ms683242(v=vs.85).aspx

		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
		public delegate bool HandlerRoutine(CtrlTypes CtrlType);

		public enum CtrlTypes
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		#endregion
	}
}
