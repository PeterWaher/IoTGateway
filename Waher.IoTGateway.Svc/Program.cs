﻿using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Runtime.Inventory;
using Waher.IoTGateway.Svc.ServiceManagement;
using Waher.IoTGateway.Svc.ServiceManagement.Classes;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;
using Waher.IoTGateway.Svc.ServiceManagement.Structures;
using System.Diagnostics;
using Waher.Networking.HTTP.Brotli;
using Waher.Events.Filter;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Waher.IoTGateway.Svc
{
	/// <summary>
	/// IoT Gateway Windows Service Application.
	/// 
	/// Command line switches:
	/// 
	/// -?                   Brings this help.
	/// -install             Installs service in operating system
	/// -uninstall           Uninstalls service from operating system.
	/// -displayname Name    Sets the display name of the service. Default is "IoT Gateway Service".
	/// -description Desc    Sets the textual description of the service. Default is "Windows Service hosting the Waher IoT Gateway.".
	/// -start Mode          Sets the default starting mode of the service. Default is Disabled. Available options are StartOnBoot, StartOnSystemStart, AutoStart, StartOnDemand and Disabled
	/// -immediate           If the service should be started immediately.
	/// -console             Run the service as a console application.
	/// -localsystem         Installed service will run using the Local System account.
	/// -localservice        Installed service will run using the Local Service account (default).
	/// -networkservice      Installed service will run using the Network Service account.
	/// -instance INSTANCE   Name of instance. Default is the empty string. Parallel instances of the IoT Gateway can execute, provided they are given separate instance names.
	/// </summary>
	public class Program
	{
		private static string instanceName = string.Empty;

		public static string InstanceName => instanceName;

		public static int Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				if (e.IsTerminating)
				{
					string FileName = Path.Combine(Gateway.AppDataFolder, "UnhandledException.txt");
					Networking.Sniffers.XmlFileSniffer.MakeUnique(ref FileName);

					using StreamWriter w = File.CreateText(FileName);
					w.Write("Type: ");

					if (e.ExceptionObject is not null)
						w.WriteLine(e.ExceptionObject.GetType().FullName);
					else
						w.WriteLine("null");

					w.Write("Time: ");
					w.WriteLine(DateTime.Now.ToString());

					w.WriteLine();
					if (e.ExceptionObject is Exception ex)
					{
						while (ex is not null)
						{
							w.WriteLine(ex.Message);
							w.WriteLine();
							w.WriteLine(Log.CleanStackTrace(ex.StackTrace));
							w.WriteLine();

							ex = ex.InnerException;
						}
					}
					else
					{
						if (e.ExceptionObject is not null)
							w.WriteLine(e.ExceptionObject.ToString());

						w.WriteLine();
						w.WriteLine(Log.CleanStackTrace(Environment.StackTrace));
					}

					w.Flush();
				}

				if (e.ExceptionObject is Exception ex2)
					Log.Critical(ex2);
				else if (e.ExceptionObject is not null)
					Log.Critical(e.ExceptionObject.ToString());
				else
					Log.Critical("Unexpected null exception thrown.");

				if (e.IsTerminating)
				{
					Gateway.Stop().Wait();
					Log.Terminate();
				}
			};

			AppDomain.CurrentDomain.DomainUnload += (sender, e) =>
			{
				Log.Informational("Unloading domain.");
			};

			AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
			{
				Log.Informational("Exiting process.");
				Log.Terminate();
			};

			TaskScheduler.UnobservedTaskException += (sender, e) =>
			{
				Exception ex = Log.UnnestException(e.Exception);
				string StackTrace = Log.CleanStackTrace(ex.StackTrace);

				Log.Alert("Unobserved Task Exception\r\n============================\r\n\r\n" + ex.Message + "\r\n\r\n```\r\n" + StackTrace + "\r\n```");

				e.SetObserved();
			};

			try
			{
				string ServiceName = "IoT Gateway Service";
				string DisplayName = ServiceName;
				string Description = "Windows Service hosting the Waher IoT Gateway.";
				string Arg;
				ServiceStartType StartType = ServiceStartType.Disabled;
				Win32ServiceCredentials Credentials = Win32ServiceCredentials.LocalService;
				bool Install = false;
				bool Uninstall = false;
				bool Immediate = false;
				bool AsConsole = false;
				bool Error = false;
				bool Help = false;
				int i, c = args.Length;

				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				Log.Informational("Program started.");

				for (i = 0; i < c; i++)
				{
					Arg = args[i];

					switch (Arg.ToLower())
					{
						case "-?":
							Help = true;
							break;

						case "-install":
							Install = true;
							break;

						case "-uninstall":
							Uninstall = true;
							break;

						case "-immediate":
							Immediate = true;
							break;

						case "-console":
							AsConsole = true;
							break;

						case "-displayname":
							i++;
							if (i >= c)
							{
								Error = true;
								break;
							}

							DisplayName = args[i];
							break;

						case "-description":
							i++;
							if (i >= c)
							{
								Error = true;
								break;
							}

							Description = args[i];
							break;

						case "-start":
							i++;
							if (i >= c)
							{
								Error = true;
								break;
							}

							if (!Enum.TryParse(args[i], out StartType))
							{
								Error = true;
								break;
							}
							break;

						case "-localsystem":
							Credentials = Win32ServiceCredentials.LocalSystem;
							break;

						case "-localservice":
							Credentials = Win32ServiceCredentials.LocalService;
							break;

						case "-networkservice":
							Credentials = Win32ServiceCredentials.NetworkService;
							break;

						case "-instance":
							i++;
							if (i >= c)
							{
								Error = true;
								break;
							}

							instanceName = args[i];
							break;

						default:
							Error = true;
							break;
					}
				}

				if (Error || Help)
				{
					Log.Informational("Displaying help.");

					Console.Out.WriteLine("IoT Gateway Windows Service Application.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-?                   Brings this help.");
					Console.Out.WriteLine("-install             Installs service in operating system");
					Console.Out.WriteLine("-uninstall           Uninstalls service from operating system.");
					Console.Out.WriteLine("-displayname Name    Sets the display name of the service. Default is \"IoT ");
					Console.Out.WriteLine("                     Gateway Service\".");
					Console.Out.WriteLine("-description Desc    Sets the textual description of the service. Default is ");
					Console.Out.WriteLine("                     \"Windows Service hosting the Waher IoT Gateway.\".");
					Console.Out.WriteLine("-start Mode          Sets the default starting mode of the service. Default is ");
					Console.Out.WriteLine("                     Disabled. Available options are StartOnBoot, ");
					Console.Out.WriteLine("                     StartOnSystemStart, AutoStart, StartOnDemand and Disabled.");
					Console.Out.WriteLine("-immediate           If the service should be started immediately.");
					Console.Out.WriteLine("-console             Run the service as a console application.");
					Console.Out.WriteLine("-localsystem         Installed service will run using the Local System account.");
					Console.Out.WriteLine("-localservice        Installed service will run using the Local Service account");
					Console.Out.WriteLine("                     (default).");
					Console.Out.WriteLine("-networkservice      Installed service will run using the Network Service");
					Console.Out.WriteLine("                     account.");
					Console.Out.WriteLine("-instance INSTANCE   Name of instance. Default is the empty string. Parallel");
					Console.Out.WriteLine("                     instances of the IoT Gateway can execute, provided they");
					Console.Out.WriteLine("                     are given separate instance names.");

					return -1;
				}

				switch (Environment.OSVersion.Platform)
				{
					case PlatformID.Win32S:
					case PlatformID.Win32Windows:
					case PlatformID.Win32NT:
					case PlatformID.WinCE:
						Log.Register(new EventFilter("Windows Event Log Filter",
							new Waher.Events.WindowsEventLog.WindowsEventLog("IoTGateway" + instanceName),
							EventType.Notice));
						break;
				}

				if (Install && Uninstall)
				{
					Log.Error("Conflicting arguments.");
					Console.Out.Write("Conflicting arguments.");
					return -1;
				}

				if (Install)
				{
					string EventSource = "IoTGateway" + InstanceName;
					if (!EventLog.Exists(EventSource) || !EventLog.SourceExists(EventSource))
					{
						Log.Informational("Creating event source.",
							new KeyValuePair<string, object>("Source", EventSource));

						EventLog.CreateEventSource(new EventSourceCreationData(EventSource, EventSource));
						
						Log.Informational("Event source created.",
							new KeyValuePair<string, object>("Source", EventSource));
					}

					Log.Informational("Installing service.");

					InstallService(ServiceName, InstanceName, DisplayName, Description, StartType, Immediate,
						new ServiceFailureActions(TimeSpan.FromDays(1), null, null, new ScAction[]
						{
							new() { Type = ScActionType.ScActionRestart, Delay = TimeSpan.FromMinutes(1) },
							new() { Type = ScActionType.ScActionRestart, Delay = TimeSpan.FromMinutes(15) },
							new() { Type = ScActionType.ScActionRestart, Delay = TimeSpan.FromHours(1) }
						}), Credentials);

					return 0;
				}
				else if (Uninstall)
				{
					Log.Informational("Uninstalling service.");
					UninstallService(ServiceName, InstanceName);

					try
					{
						string EventSource = "IoTGateway" + InstanceName;
						if (EventLog.Exists(EventSource))
						{
							Log.Informational("Deleting event log.",
								new KeyValuePair<string, object>("Source", EventSource));

							EventLog.Delete(EventSource);

							Log.Informational("Event log deleted.",
								new KeyValuePair<string, object>("Source", EventSource));
						}

						if (EventLog.SourceExists(EventSource))
						{
							Log.Informational("Deleting event source.",
								new KeyValuePair<string, object>("Source", EventSource));

							EventLog.DeleteEventSource(EventSource);

							Log.Informational("Event source deleted.",
								new KeyValuePair<string, object>("Source", EventSource));
						}
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}

					return 0;
				}
				else
				{
					if (AsConsole)
					{
						Log.Informational("Running as console application.");
						RunAsConsole();
					}
					else
					{
						Log.Informational("Running as service application.");
						RunAsService(ServiceName, InstanceName);
					}

					return 1;   // Allows the service to be restarted.
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				Console.Out.WriteLine(ex.Message);
				return 1;
			}
		}

		private static void RunAsService(string ServiceName, string InstanceName)
		{
			try
			{
				Log.Informational("Starting service.",
					new KeyValuePair<string, object>("Service Name", ServiceName),
					new KeyValuePair<string, object>("Instance Name", InstanceName));

				using GatewayService Service = new(ServiceName, InstanceName);
				ServiceBase.Run(Service);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
			}
			finally
			{
				Log.Informational("Service terminated.");
			}
		}

		private static void RunAsConsole()
		{
			try
			{
				Console.ForegroundColor = ConsoleColor.White;

				Console.Out.WriteLine("Welcome to the Internet of Things Gateway server application.");
				Console.Out.WriteLine(new string('-', 79));
				Console.Out.WriteLine("This server application will help you manage IoT devices and");
				Console.Out.WriteLine("create dynamic content that you can publish on the Internet.");
				Console.Out.WriteLine("It also provides programming interfaces (API) which allow you");
				Console.Out.WriteLine("to dynamically and securely interact with the devices and the");
				Console.Out.WriteLine("content you publish.");

				Log.Register(new ConsoleEventSink(false));

				Gateway.GetDatabaseProvider += GetDatabase;
				Gateway.RegistrationSuccessful += RegistrationSuccessful;

				if (!Gateway.Start(true, true, instanceName).Result)
				{
					Console.Out.WriteLine();
					Console.Out.WriteLine("Gateway being started in another process.");
					return;
				}

				ManualResetEvent Done = new(false);
				Gateway.OnTerminate += (sender, e) => Done.Set();
				Console.CancelKeyPress += (sender, e) =>
				{
					e.Cancel = true;
					Gateway.Terminate();
				};

				switch (Environment.OSVersion.Platform)
				{
					case PlatformID.Win32S:
					case PlatformID.Win32Windows:
					case PlatformID.Win32NT:
					case PlatformID.WinCE:
						try
						{
							Win32.SetConsoleCtrlHandler((ControlType) =>
							{
								switch (ControlType)
								{
									case CtrlTypes.CTRL_BREAK_EVENT:
									case CtrlTypes.CTRL_CLOSE_EVENT:
									case CtrlTypes.CTRL_C_EVENT:
									case CtrlTypes.CTRL_SHUTDOWN_EVENT:
										Gateway.Terminate();
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
						break;
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
				Gateway.Stop().Wait();
				Log.Terminate();
			}
		}

		internal async static Task<IDatabaseProvider> GetDatabase(XmlElement DatabaseConfig)
		{
			string Folder = Path.Combine(Gateway.AppDataFolder, XML.Attribute(DatabaseConfig, "folder", "Data"));
			string DefaultCollectionName = XML.Attribute(DatabaseConfig, "defaultCollectionName", "Default");
			int BlockSize = XML.Attribute(DatabaseConfig, "blockSize", 8192);
			int BlocksInCache = XML.Attribute(DatabaseConfig, "blocksInCache", 10000);
			int BlobBlockSize = XML.Attribute(DatabaseConfig, "blobBlockSize", 8192);
			int TimeoutMs = XML.Attribute(DatabaseConfig, "timeoutMs", 3600000);
			bool Encrypted = XML.Attribute(DatabaseConfig, "encrypted", true);
			bool Compiled = XML.Attribute(DatabaseConfig, "compiled", true);

			Types.SetModuleParameter("Data", Folder);

			try
			{
				BrotliContentEncoding.Init(Gateway.AppDataFolder);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return await FilesProvider.CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize,
				System.Text.Encoding.UTF8, TimeoutMs, Encrypted, Compiled);
		}

		internal static async Task RegistrationSuccessful(MetaDataTag[] MetaData, RegistrationEventArgs e)
		{
			if (!e.IsClaimed && Types.TryGetModuleParameter("Registry", out object Obj) && Obj is ThingRegistryClient ThingRegistryClient)
			{
				string ClaimUrl = ThingRegistryClient.EncodeAsIoTDiscoURI(MetaData);
				string FilePath = Path.Combine(Gateway.AppDataFolder, "Gateway.iotdisco");

				Log.Informational("Registration successful.");
				Log.Informational(ClaimUrl, new KeyValuePair<string, object>("Path", FilePath));

				await File.WriteAllTextAsync(FilePath, ClaimUrl);
			}
		}

		private static void InstallService(string ServiceName, string InstanceName, string DisplayName, string Description, 
			ServiceStartType StartType, bool Immediate, ServiceFailureActions FailureActions, Win32ServiceCredentials Credentials)
		{
			ServiceInstaller host = new(ServiceName, InstanceName);
			int i;

			switch (i = host.Install(DisplayName, Description, StartType, Immediate, FailureActions, Credentials))
			{
				case 0:
					Console.Out.WriteLine("Service successfully installed. Service start is pending.");
					break;

				case 1:
					Console.Out.WriteLine("Service successfully installed and started.");
					break;

				case 2:
					Console.Out.WriteLine("Service registration successfully updated. Service start is pending.");
					break;

				case 3:
					Console.Out.WriteLine("Service registration successfully updated. Service started.");
					break;

				default:
					throw new Exception("Unexpected installation result: " + i.ToString());
			}
		}

		private static void UninstallService(string ServiceName, string InstanceName)
		{
			ServiceInstaller host = new(ServiceName, InstanceName);

			if (host.Uninstall())
				Console.Out.WriteLine("Service successfully uninstalled.");
			else
				Console.Out.WriteLine("Service not found. Uninstall not required.");
		}

	}
}

#pragma warning restore CA1416 // Validate platform compatibility
