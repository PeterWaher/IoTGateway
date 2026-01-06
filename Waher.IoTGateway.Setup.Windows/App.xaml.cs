using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Events.Files;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;

namespace Waher.IoTGateway.Setup.Windows
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		/// <summary>
		/// Name of application.
		/// </summary>
		public const string AppName = "IoTGateway";

		/// <summary>
		/// Displayable Name of application.
		/// </summary>
		public const string AppNameDisplayable = "IoT Gateway™";

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			int i = 0;
			int c = e.Args.Length;

			Types.Initialize(
				typeof(App).Assembly,
				typeof(JSON).Assembly);

			if (c == 0)
			{
				MainWindow MainWindow = new();
				MainWindow.Show();
			}
			else
			{
				string? InstanceName = null;
				string? LogName = null;
				int? Port = null;
				bool Error = false;
				bool Install = false;
				bool Uninstall = false;

				while (i < c)
				{
					switch (e.Args[i++].ToLower())
					{
						case "-i":
						case "-inst":
						case "-install":
							if (InstanceName is null)
							{
								if (i < c)
								{
									InstanceName = e.Args[i++];
									Install = true;
								}
								else
								{
									Error = true;
									Console.Error.WriteLine("Missing instance name.");
								}
							}
							else
							{
								Error = true;
								Console.Error.WriteLine("Instance name already provided.");
							}
							break;

						case "-u":
						case "-uninst":
						case "-uninstall":
							if (InstanceName is null)
							{
								if (i < c)
								{
									InstanceName = e.Args[i++];
									Uninstall = true;
								}
								else
								{
									Error = true;
									Console.Error.WriteLine("Missing instance name.");
								}
							}
							else
							{
								Error = true;
								Console.Error.WriteLine("Instance name already provided.");
							}
							break;

						case "-p":
						case "-port":
							if (Port is null)
							{
								if (i < c)
								{
									if (int.TryParse(e.Args[i++], out int j))
									{
										if (j > 0 && j <= 65535)
											Port = j;
										else
										{
											Error = true;
											Console.Error.WriteLine("Invalid port number.");
										}
									}
									else
									{
										Error = true;
										Console.Error.WriteLine("Missing port number.");
									}
								}
								else
								{
									Error = true;
									Console.Error.WriteLine("Missing port number.");
								}
							}
							else
							{
								Error = true;
								Console.Error.WriteLine("Port number already provided.");
							}
							break;

						case "-l":
						case "-log":
							if (LogName is null)
							{
								if (i < c)
									LogName = e.Args[i++];
								else
								{
									Error = true;
									Console.Error.WriteLine("Missing log name.");
								}
							}
							else
							{
								Error = true;
								Console.Error.WriteLine("Log name already provided.");
							}
							break;

						case "-?":
						case "-h":
						case "-help":
							Console.Out.WriteLine("This tool installs " + AppNameDisplayable +" on your machine");
							Console.Out.WriteLine();
							Console.Out.WriteLine("Syntax: " + AppName + "Setup -inst INSTANCE -port PORT");
							Console.Out.WriteLine();
							Console.Out.WriteLine("Following switches are recognized:");
							Console.Out.WriteLine();
							Console.Out.WriteLine("-i INSTANCE      Defines the instance name of the new installation.");
							Console.Out.WriteLine("-inst INSTANCE   This instance name can be empty, and must in such");
							Console.Out.WriteLine("                 cases be specifies using quotes: \"\"");
							Console.Out.WriteLine("-u INSTANCE      Defines the instance name to uninstall.");
							Console.Out.WriteLine("-uninst INSTANCE This instance name can be empty, and must in such");
							Console.Out.WriteLine("                 cases be specifies using quotes: \"\"");
							Console.Out.WriteLine("-p PORT          Defines the port number to use for accessing content.");
							Console.Out.WriteLine("-port PORT       Access to content is mainly done via the web interface.");
							Console.Out.WriteLine("                 If a port number is not provided, an existing instance");
							Console.Out.WriteLine("                 is updated or repaired.");
							Console.Out.WriteLine("-l FILENAME      Creates a log file using the specified file name.");
							Console.Out.WriteLine("-log FILENAME    The log file will be an XML file using a portable");
							Console.Out.WriteLine("                 event log format.");
							Console.Out.WriteLine("-?, -h, -help:   Shows this help.");
							break;

						default:
							Error = true;
							Console.Error.WriteLine("Unrecognized switch: " + e.Args[i - 1]);
							break;
					}

					if (Error)
						break;
				}

				if (!Error)
				{
					if (InstanceName is null)
					{
						Error = true;
						Console.Error.WriteLine("Missing instance name.");
					}
					else if (Install)
					{
						try
						{
							bool AddConsoleLog = true;

							if (!string.IsNullOrEmpty(LogName))
							{
								XmlFileEventSink EventSink = new("XML File Event Sink", LogName, int.MaxValue);
								Log.Register(EventSink);
								AddConsoleLog = false;
							}

							if (AddConsoleLog)
								Log.Register(new ConsoleEventSink(false, true));

							if (App.Install(InstanceName, Port))
								Error = true;
						}
						catch (Exception ex)
						{
							Error = true;
							Console.Error.WriteLine(ex.Message);
						}
						finally
						{
							Log.TerminateAsync().Wait();
						}
					}
					else if (Uninstall)
					{
						try
						{
							bool AddConsoleLog = true;

							if (!string.IsNullOrEmpty(LogName))
							{
								XmlFileEventSink EventSink = new("XML File Event Sink", LogName, int.MaxValue);
								Log.Register(EventSink);
								AddConsoleLog = false;
							}

							if (AddConsoleLog)
								Log.Register(new ConsoleEventSink(false, true));

							if (App.Uninstall(InstanceName))
								Error = true;
						}
						catch (Exception ex)
						{
							Error = true;
							Console.Error.WriteLine(ex.Message);
						}
						finally
						{
							Log.TerminateAsync().Wait();
						}
					}
				}

				this.Shutdown(Error ? 1 : 0);
			}
		}

		internal static bool IsUserAdministrator()
		{
			try
			{
				WindowsIdentity CurrentUser = WindowsIdentity.GetCurrent();
				WindowsPrincipal CurrentPrincipal = new(CurrentUser);

				return CurrentPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return false;
			}
		}

		internal static int? GetPort(string AppDataFolder)
		{
			try
			{
				string GatewayConfigFileName = Path.Combine(AppDataFolder, "Gateway.config");
				if (!File.Exists(GatewayConfigFileName))
					return null;

				byte[] Bin = File.ReadAllBytes(GatewayConfigFileName);
				string s = Strings.GetString(Bin, Encoding.UTF8);

				int i = s.IndexOf("<Port protocol=\"HTTP\">");
				if (i < 0)
					return null;

				i += 22;
				int j = s.IndexOf("</Port>", i);
				if (j < 0)
					return null;

				if (int.TryParse(s[i..j], out int Port))
					return Port;

				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		internal static bool Install(string InstanceName, int? PortNumber)
		{
			string AppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string Programs = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			string InstallationFolder = Path.Combine(Programs, AppName + InstanceName);
			string ServerApplication = Path.Combine(InstallationFolder, "Waher.IoTGateway.Svc.exe");
			string ProgramDataFolder = Path.Combine(AppData, string.IsNullOrEmpty(InstanceName) ? "IoT Gateway" : "IoT Gateway " + InstanceName);
			bool Errors = false;
			bool NewInstall = PortNumber.HasValue;

			if (NewInstall)
				Log.Informational("Installing instance '" + InstanceName + "', using port " + PortNumber.ToString() + ".");
			else
			{
				Log.Informational("Updating or repairing instance '" + InstanceName + "'.");

				if (StopService(InstanceName))
					return true;

				PortNumber = GetPort(ProgramDataFolder);
			}

			if (!InstallPackage("IoTGateway.package", ServerApplication, ProgramDataFolder, false, false))
				Errors = true;

			// Add other packages here when making custom installers.

			if (PortNumber.HasValue)
			{
				Log.Informational("Generating custom Gateway.config file.");
				try
				{
					using Stream f = GetManifestResourceStream("Gateway.config");
					byte[] Bin = Utility.Install.Program.ReadBin(f, (ulong)f.Length);
					string s = Strings.GetString(Bin, Encoding.UTF8);

					s = s.Replace(
						"<Port protocol=\"HTTP\">80</Port>",
						"<Port protocol=\"HTTP\">" + PortNumber.ToString() + "</Port>");

					File.WriteAllText(Path.Combine(ProgramDataFolder, "Gateway.config"), s);

					Log.Notice("Custom Gateway.config saved.", string.Empty, string.Empty, "FileCopy");
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					Errors = true;
				}
			}

			Log.Informational("Setting folder access rights...", string.Empty, string.Empty, "InstallationStatus");

			SetAccessRights(ProgramDataFolder, ref Errors);
			SetAccessRights(InstallationFolder, ref Errors);

			Log.Informational("Registering Windows Service...", string.Empty, string.Empty, "InstallationStatus");
			try
			{
				string FileName = Path.Combine(InstallationFolder, "Waher.IoTGateway.Svc.exe");
				StringBuilder Arguments = new();

				Arguments.Append("-install -instance \"");
				Arguments.Append(InstanceName);
				Arguments.Append("\" -displayname \"");
				Arguments.Append(AppNameDisplayable);

				if (!string.IsNullOrEmpty(InstanceName))
				{
					Arguments.Append(' ');
					Arguments.Append(InstanceName);
				}

				Arguments.Append("\" -description \"");
				Arguments.Append(AppNameDisplayable);
				Arguments.Append(" service");

				if (string.IsNullOrEmpty(InstanceName))
					Arguments.Append(" default instance");
				else
				{
					Arguments.Append(" instance ");
					Arguments.Append(InstanceName);
				}

				Arguments.Append(".\" -start AutoStart -localservice");

				ProcessStartInfo StartInfo = new()
				{
					FileName = FileName,
					WorkingDirectory = InstallationFolder,
					Arguments = Arguments.ToString(),
					Verb = "runas", // Starts with administrator rights.
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					UseShellExecute = true
				};

				LogProcessStart("Installing Windows Service in process with administrative privileges.", StartInfo);

				Process P = Process.Start(StartInfo)
					?? throw new Exception("Unable to start service installation process.");

				P.WaitForExit();

				int ExitCode = P.ExitCode;

				if (ExitCode != 0)
				{
					Errors = true;
					Log.Error("Unable to install service.", string.Empty, string.Empty, "InstallationStatus",
						new KeyValuePair<string, object>("ExitCode", ExitCode));
				}
				else
					Log.Notice("Windows Service registered...", string.Empty, string.Empty, "InstallationStatus");
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Errors = true;
			}

			Log.Informational("Starting service.", string.Empty, string.Empty, "InstallationStatus");
			try
			{
				string ServiceName = "IoT Gateway Service";
				if (!string.IsNullOrEmpty(InstanceName))
					ServiceName += " " + InstanceName;

				ProcessStartInfo StartInfo = new()
				{
					FileName = "net",
					WorkingDirectory = InstallationFolder,
					Arguments = "start \"" + ServiceName + "\"",
					Verb = "runas", // Starts with administrator rights.
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					UseShellExecute = true
				};

				LogProcessStart("Starting Windows service in process with administrative privileges.", StartInfo);

				Process P = Process.Start(StartInfo)
					?? throw new Exception("Unable to start service.");

				bool Exited = P.WaitForExit(1000);  // Note: Do not wait for process to exit, as it will remain in starting state until service is configured.

				if (Exited)
				{
					int ExitCode = P.ExitCode;

					if (ExitCode != 0)
					{
						Errors = true;
						Log.Error("Unable to start service.", string.Empty, string.Empty, "InstallationStatus",
							new KeyValuePair<string, object>("ExitCode", ExitCode));
					}
					else
						Log.Notice("Windows Service started...", string.Empty, string.Empty, "InstallationStatus");
				}
				else if (NewInstall)
					Log.Notice("Windows Service starting. Needs configuration.", string.Empty, string.Empty, "InstallationStatus");
				else
					Log.Notice("Windows Service starting.", string.Empty, string.Empty, "InstallationStatus");
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Errors = true;
			}

			return Errors;
		}

		internal static void LogProcessStart(string Message, ProcessStartInfo StartInfo)
		{
			Log.Informational(Message,
				new KeyValuePair<string, object>("FileName", StartInfo.FileName),
				new KeyValuePair<string, object>("WorkingDirectory", StartInfo.WorkingDirectory),
				new KeyValuePair<string, object>("Arguments", StartInfo.Arguments),
				new KeyValuePair<string, object>("Verb", StartInfo.Verb),
				new KeyValuePair<string, object>("RedirectStandardError", StartInfo.RedirectStandardError),
				new KeyValuePair<string, object>("RedirectStandardOutput", StartInfo.RedirectStandardOutput),
				new KeyValuePair<string, object>("CreateNoWindow", StartInfo.CreateNoWindow),
				new KeyValuePair<string, object>("WindowStyle", StartInfo.WindowStyle),
				new KeyValuePair<string, object>("UseShellExecute", StartInfo.UseShellExecute));
		}

		internal static bool StopService(string InstanceName)
		{
			string ServiceName = "IoT Gateway Service";
			if (!string.IsNullOrEmpty(InstanceName))
				ServiceName += " " + InstanceName;

			Log.Informational("Checking if service is running.");
			try
			{
				using ServiceController Controller = new(ServiceName);
				if (Controller.Status == ServiceControllerStatus.Stopped)
				{
					Log.Informational("Service is stopped.");
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			Log.Informational("Stopping service.");
			try
			{
				using ServiceController Controller = new(ServiceName);
				Controller.Stop();

				Log.Notice("Windows Service stopped...");
			}
			catch (Exception ex)
			{
				Log.Error("Unable to stop service. Attempting to kill process: " + ex.Message);

				try
				{
					using ManagementObjectSearcher Searcher = new(
						"SELECT * FROM Win32_Service WHERE Name = '" + ServiceName + "'");
					bool Found = false;

					foreach (object Item in Searcher.Get())
					{
						if (Item is not ManagementObject ServiceObject)
							continue;

						string s = ServiceObject["ProcessId"]?.ToString() ?? string.Empty;
						if (string.IsNullOrEmpty(s) || !int.TryParse(s, out int PID))
							continue;

						Process Process = Process.GetProcessById(PID);
						if (Process is null)
							continue;

						Found = true;

						Log.Informational("Attempting to kill process.",
							new KeyValuePair<string, object>("PID", PID));

						Process.Kill(false);
						Task.Delay(1000).Wait();

						Log.Notice("Process killed.");
					}

					if (!Found)
						Log.Informational("No service process found. Continuing uninstallation assuming service is not running.");
				}
				catch (Exception ex2)
				{
					Log.Exception(ex2);
					return true;
				}
			}

			return false;
		}

		internal static bool Uninstall(string InstanceName)
		{
			string AppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string Programs = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			string InstallationFolder = Path.Combine(Programs, AppName + InstanceName);
			string ProgramDataFolder = Path.Combine(AppData, string.IsNullOrEmpty(InstanceName) ? "IoT Gateway" : "IoT Gateway " + InstanceName);
			bool Error = false;

			if (StopService(InstanceName))
				return true;

			Log.Informational("Unregistering Windows Service...");
			try
			{
				string FileName = Path.Combine(InstallationFolder, "Waher.IoTGateway.Svc.exe");

				if (File.Exists(FileName))
				{
					StringBuilder Arguments = new();

					Arguments.Append("-uninstall -instance \"");
					Arguments.Append(InstanceName);
					Arguments.Append('"');

					ProcessStartInfo StartInfo = new()
					{
						FileName = FileName,
						WorkingDirectory = InstallationFolder,
						Arguments = Arguments.ToString(),
						Verb = "runas", // Starts with administrator rights.
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden,
						UseShellExecute = true
					};

					LogProcessStart("Unregistering Windows Service in process with administrative privileges.", StartInfo);

					Process P = Process.Start(StartInfo)
						?? throw new Exception("Unable to start service unregistration process.");

					P.WaitForExit();

					int ExitCode = P.ExitCode;

					if (ExitCode != 0)
					{
						Log.Error("Unable to unregister service.",
							new KeyValuePair<string, object>("ExitCode", ExitCode));
						return true;
					}
					else
						Log.Notice("Windows Service unregistered...");
				}
				else
				{
					Log.Warning("Executable file not found. Unable to unregister service. Assuming service has been unregistered.",
						new KeyValuePair<string, object>("File Name", FileName));
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return true;
			}

			Log.Informational("Removing installed files for instance '" + InstanceName + "'.");

			try
			{
				if (Directory.Exists(ProgramDataFolder))
				{
					Log.Notice("Deleting folder : " + ProgramDataFolder);
					Directory.Delete(ProgramDataFolder, true);
					Log.Notice("Folder deleted: " + ProgramDataFolder);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Error = true;
			}

			try
			{
				if (Directory.Exists(InstallationFolder))
				{
					Log.Notice("Deleting folder : " + InstallationFolder);
					Directory.Delete(InstallationFolder, true);
					Log.Notice("Folder deleted: " + InstallationFolder);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Error = true;
			}

			return Error;
		}

		private static void SetAccessRights(string Folder, ref bool Errors)
		{
			try
			{
				Log.Informational("Setting access rights to folder.",
					new KeyValuePair<string, object>("Folder", Folder));

				WindowsIdentity CurrentUser = WindowsIdentity.GetCurrent();
				DirectoryInfo DirInfo = new(Folder);
				DirectorySecurity DirSecurity = DirInfo.GetAccessControl();

				DirSecurity.AddAccessRule(new FileSystemAccessRule(CurrentUser.Name,
					FileSystemRights.FullControl,
					InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
					PropagationFlags.None,
					AccessControlType.Allow));

				SecurityIdentifier LocalService = new("S-1-5-19");
				DirSecurity.AddAccessRule(new FileSystemAccessRule(LocalService,
					FileSystemRights.FullControl,
					InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
					PropagationFlags.None,
					AccessControlType.Allow));

				DirInfo.SetAccessControl(DirSecurity);
			}
			catch (Exception ex)
			{
				Errors = true;
				Log.Exception(ex);
			}
		}

		private static bool InstallPackage(string PackageName, string ServerApplication,
			string ProgramDataFolder, bool ContentOnly, bool ServerExists)
		{
			Log.Informational("Installing " + PackageName + ".",
				new KeyValuePair<string, object>("Server", ServerApplication),
				new KeyValuePair<string, object>("ProgramDataFolder", ProgramDataFolder),
				new KeyValuePair<string, object>("ContentOnly", ContentOnly),
				new KeyValuePair<string, object>("ServerExists", ServerExists));

			using Stream f = GetManifestResourceStream(PackageName);

			try
			{
				Utility.Install.Program.InstallPackage(PackageName, f, string.Empty, ServerApplication, ProgramDataFolder, ContentOnly, ServerExists);
				return true;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return false;
			}
		}

		private static Stream GetManifestResourceStream(string EmbeddedFileName)
		{
			Type AppType = typeof(App);
			string ResourceName = AppType.Namespace + "." + EmbeddedFileName;
			Assembly Assembly = AppType.Assembly;
			return Assembly.GetManifestResourceStream(ResourceName)
				?? throw new Exception("Package not found: " + EmbeddedFileName);
		}
	}
}
