using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.Brotli;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Security.CallStack;

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
                string InstanceName = string.Empty;
                string Arg;
                bool Error = false;
                bool Help = false;
                int i, c = args.Length;

                for (i = 0; i < c; i++)
                {
                    Arg = args[i];

                    switch (Arg.ToLower())
                    {
                        case "-?":
                            Help = true;
                            break;

                        case "-instance":
                            i++;
                            if (i >= c)
                            {
								ConsoleOut.WriteLine("Missing instance name.");
                                Error = true;
								break;
                            }

                            InstanceName = args[i];
                            break;

                        default:
							ConsoleOut.WriteLine("Unrecognized argument: " + Arg);
							Error = true;
                            break;
                    }
                }

                ConsoleOut.ForegroundColor = ConsoleColor.White;

				ConsoleOut.WriteLine("Welcome to the Internet of Things Gateway server application.");
				ConsoleOut.WriteLine(new string('-', 79));
				ConsoleOut.WriteLine("This server application will help you manage IoT devices and");
				ConsoleOut.WriteLine("create dynamic content that you can publish on the Internet.");
				ConsoleOut.WriteLine("It also provides programming interfaces (API) which allow you");
				ConsoleOut.WriteLine("to dynamically and securely interact with the devices and the");
				ConsoleOut.WriteLine("content you publish.");

                if (Error || Help)
                {
                    Log.Informational("Displaying help.");

                    ConsoleOut.WriteLine();
                    ConsoleOut.WriteLine("Command line switches:");
                    ConsoleOut.WriteLine();
                    ConsoleOut.WriteLine("-?                   Brings this help.");
                    ConsoleOut.WriteLine("-instance INSTANCE   Name of instance. Default is the empty string. Parallel");
                    ConsoleOut.WriteLine("                     instances of the IoT Gateway can execute, provided they");
                    ConsoleOut.WriteLine("                     are given separate instance names.");

                    return;
                }

				Log.RegisterAlertExceptionType(true,
					typeof(OutOfMemoryException),
					typeof(StackOverflowException),
					typeof(AccessViolationException),
					typeof(InsufficientMemoryException),
					typeof(UnauthorizedCallstackException));

				Log.Register(new ConsoleEventSink(false));
				Log.RegisterExceptionToUnnest(typeof(ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				AppDomain.CurrentDomain.UnhandledException += (Sender, e) =>
				{
					if (e.IsTerminating)
					{
						string FileName = Path.Combine(Gateway.AppDataFolder, "UnhandledException.txt");

						if (FileNameTimeSequence.MakeUnique(ref FileName))
						{
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
							Log.Emergency(ex2);
						else if (e.ExceptionObject is not null)
							Log.Emergency(e.ExceptionObject.ToString());
						else
							Log.Emergency("Unexpected null exception thrown.");

						Gateway.Stop().Wait();
						Log.TerminateAsync().Wait();
					}
					else
					{
						if (e.ExceptionObject is Exception ex2)
							Log.Alert(ex2);
						else if (e.ExceptionObject is not null)
							Log.Alert(e.ExceptionObject.ToString());
						else
							Log.Alert("Unexpected null exception thrown.");
					}
				};

				TaskScheduler.UnobservedTaskException += (Sender, e) =>
				{
					Exception ex = Log.UnnestException(e.Exception);
					string StackTrace = Log.CleanStackTrace(ex.StackTrace);

					Log.Alert("Unobserved Task Exception\r\n============================\r\n\r\n" + ex.Message + "\r\n\r\n```\r\n" + StackTrace + "\r\n```");

					e.SetObserved();
				};

				Gateway.GetDatabaseProvider += GetDatabase;
				Gateway.RegistrationSuccessful += RegistrationSuccessful;

				if (!Gateway.Start(true, true, InstanceName).Result)
				{
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("Gateway being started in another process.");
					return;
				}

				ManualResetEvent Done = new(false);
				Gateway.OnTerminate += (Sender, e) =>
				{
					Done.Set();
					return Task.CompletedTask;
				};
				System.Console.CancelKeyPress += async (Sender, e) =>
				{
					try
					{
						e.Cancel = true;
						await Gateway.Terminate();
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				};

				switch (Environment.OSVersion.Platform)
				{
					case PlatformID.Win32S:
					case PlatformID.Win32Windows:
					case PlatformID.Win32NT:
					case PlatformID.WinCE:
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
										Task.Run(async () =>
										{
											try
											{
												await Gateway.Terminate();
											}
											catch (Exception ex)
											{
												Log.Exception(ex);
											}
										});
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
				Log.Exception(ex);
			}
			finally
			{
				Gateway.Stop().Wait(30000);     // TODO: Fail-safe approach
				ConsoleOut.Flush(true);
				Log.TerminateAsync().Wait(30000);
			}
		}

		private async static Task<IDatabaseProvider> GetDatabase(XmlElement DatabaseConfig)
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
				Log.Exception(ex);
			}

			return await FilesProvider.CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize,
				System.Text.Encoding.UTF8, TimeoutMs, Encrypted, Compiled);
		}

		private static async Task RegistrationSuccessful(MetaDataTag[] MetaData, RegistrationEventArgs e)
		{
			if (!e.IsClaimed && Types.TryGetModuleParameter("Registry", out ThingRegistryClient ThingRegistryClient))
			{
				string ClaimUrl = ThingRegistryClient.EncodeAsIoTDiscoURI(MetaData);
				string FilePath = Path.Combine(Gateway.AppDataFolder, "Gateway.iotdisco");

				Log.Informational("Registration successful.");
				Log.Informational(ClaimUrl, new KeyValuePair<string, object>("Path", FilePath));

				await File.WriteAllTextAsync(FilePath, ClaimUrl);
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
