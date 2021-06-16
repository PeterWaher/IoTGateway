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

                StringBuilder sb = new StringBuilder();

                sb.Append("-install -displayname \"");
                sb.Append(DisplayName);
                sb.Append("\" -description \"");
                sb.Append(Description);
                sb.Append("\" -start AutoStart -immediate");

                ProcessStartInfo ProcessInformation = new ProcessStartInfo()
                {
                    FileName = InstallDir + "Waher.IotGateway.Svc.exe",
                    Arguments = sb.ToString(),
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = InstallDir,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process P = new Process())
                {
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

                using (Process P = new Process())
                {
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
                        {
                            Session.Log("Service uninstalled.");
                            return ActionResult.Success;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Session.Log("Unable to uninstall service. Error reported: " + ex.Message);
            }

            return UninstallService2(Session);
        }

        public static ActionResult UninstallService2(Session Session)
        {
            Session.Log("Uninstalling service (method 2).");
            try
            {
                string InstallDir = Session["INSTALLDIR"];

                if (!InstallDir.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
                    InstallDir += Path.DirectorySeparatorChar;

                Session.Log("Working folder: " + InstallDir);

                ProcessStartInfo ProcessInformation = new ProcessStartInfo()
                {
                    FileName = "sc.exe",
                    Arguments = "delete \"IoT Gateway Service\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = InstallDir,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process P = new Process())
                {
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

                using (Process P = new Process())
                {
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

                using (Process P = new Process())
                {
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
                        {
                            DateTime Started = DateTime.Now;
                            bool Stopped;

                            Session.Log("Service stop request successful. Checking process has stopped");

                            do
                            {
                                using (Semaphore RunningServer = new Semaphore(1, 1, "Waher.IoTGateway.Running"))
                                {
                                    Stopped = RunningServer.WaitOne(1000);

                                    if (Stopped)
                                        RunningServer.Release();
                                }
                            }
                            while (!Stopped && (DateTime.Now - Started).TotalSeconds < 30);

                            if (Stopped)
                                Session.Log("Service stopped.");
                            else
                                throw new Exception("Service stop procedure seems to take time. Cancelling wait.");
                        }
                    }
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
                Thread.Sleep(5000);     // Give process some time.

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

                Process.Start(StartPage);
                Session.Log("Browser started.");
            }
            catch (Exception ex)
            {
                Session.Log("Unable to start browser. Error reported: " + ex.Message);
            }

            return ActionResult.Success;
        }

        public static ActionResult WaitAllModulesStarted(Session Session)
        {
            Session.Log("Waiting for all modules to start.");
            try
            {
                DateTime Start = DateTime.Now;
                bool Running;

                Session.Log("Waiting for service to start.");

                Thread.Sleep(5000);

                do
                {
                    using (Semaphore RunningServer = new Semaphore(1, 1, "Waher.IoTGateway.Running"))
                    {
                        Running = !RunningServer.WaitOne(1000);

                        if (!Running)
                            RunningServer.Release();
                    }

                    if (!Running)
                        Thread.Sleep(1000);
                }
                while (!Running && (DateTime.Now - Start).TotalSeconds < 30);

                if (!Running)
                {
                    Session.Log("Could not detect a start of service. Cancelling wait and continuing.");
                    return ActionResult.Success;
                }

                Session.Log("Waiting for service startup procedure to complete.");

                using (Semaphore StartingServer = new Semaphore(1, 1, "Waher.IoTGateway.Starting"))
                {
                    if (StartingServer.WaitOne(120000))
                    {
                        StartingServer.Release();
                        Session.Log("All modules started.");
                    }
                    else
                        Session.Log("Modules takes too long to start. Cancelling wait and continuing.");
                }
            }
            catch (Exception ex)
            {
                Session.Log("Unable to wait for all modules to start. The following error was reported: " + ex.Message);
            }

            return ActionResult.Success;
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

        [CustomAction]
        public static ActionResult StopServiceAndUninstallManifest(Session Session)
        {
            ActionResult Result = StopService(Session);
            if (Result != ActionResult.Success)
                return Result;

            return UninstallManifest(Session);
        }

        [CustomAction]
        public static ActionResult UninstallManifest(Session Session)
        {
            string ManifestFile = Path.Combine(Session["INSTALLDIR"], Session["ManifestFile"]);
            string ServerApplication = Path.Combine(Session["INSTALLDIR"], "Waher.IoTGateway.Svc.dll");
            string ProgramDataFolder = Session["APPDATADIR"];

            Session.Log("Uninstalling module: " + ManifestFile);
            Session.Log("Server application: " + ServerApplication);
            Session.Log("Program data folder: " + ProgramDataFolder);

            try
            {
                Uninstall(Session, ManifestFile, ServerApplication, ProgramDataFolder, true);
            }
            catch (Exception ex)
            {
                Session.Log(ex.Message);
            }

            return ActionResult.Success;
        }

        #region From Waher.Utility.Install

        private static AssemblyName GetAssemblyName(string ServerApplication)
        {
            if (ServerApplication.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
                ServerApplication = ServerApplication.Substring(0, ServerApplication.Length - 4) + ".dll";

            return AssemblyName.GetAssemblyName(ServerApplication);
        }

        private static void Install(Session Session, string ManifestFile, string ServerApplication, string ProgramDataFolder)
        {
            // Same code as for custom action InstallManifest in Waher.IoTGateway.Installers, except:
            // * Content files can already be installed in the corresponding application data folder.

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
            AssemblyName ServerName = GetAssemblyName(ServerApplication);
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

            if (!(JSON.Parse(s) is Dictionary<string, object> Deps))
                throw new Exception("Invalid deps.json file. Unable to install.");

            Session.Log("Loading manifest file.");

            XmlDocument Manifest = new XmlDocument();
            Manifest.Load(ManifestFile);

            XmlElement Module = Manifest["Module"];
            string SourceFolder = Path.GetDirectoryName(ManifestFile);
            string AppFolder = Path.GetDirectoryName(ServerApplication);
            string DestManifestFileName = Path.Combine(AppFolder, Path.GetFileName(ManifestFile));

            CopyFileIfNewer(Session, ManifestFile, DestManifestFileName, null, false);

            Session.Log("Source folder: " + SourceFolder);
            Session.Log("App folder: " + AppFolder);

            foreach (XmlNode N in Module.ChildNodes)
            {
                if (N is XmlElement E && E.LocalName == "Assembly")
                {
                    KeyValuePair<string, string> FileNames = GetFileName(E, SourceFolder);
                    string FileName = FileNames.Key;
                    string SourceFileName = FileNames.Value;

                    if (CopyFileIfNewer(Session, SourceFileName, Path.Combine(AppFolder, FileName), null, true))
                    {
                        if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string PdbFileName = FileName.Substring(0, FileName.Length - 4) + ".pdb";
                            if (File.Exists(PdbFileName))
                                CopyFileIfNewer(Session, Path.Combine(SourceFolder, PdbFileName), Path.Combine(AppFolder, PdbFileName), null, true);
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

            CopyContent(Session, SourceFolder, AppFolder, ProgramDataFolder, Module);

            Session.Log("Encoding JSON");
            s = JSON.Encode(Deps, true);

            Session.Log("Writing " + DepsJsonFileName);
            File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);
        }

        private static bool CopyFileIfNewer(Session Session, string From, string To, string To2, bool OnlyIfNewer)
        {
            if (!File.Exists(From))
                throw new FileNotFoundException("File not found: " + From);

            bool Copy1 = From != To;

            if (Copy1 && OnlyIfNewer && File.Exists(To))
            {
                DateTime ToTP = File.GetLastWriteTimeUtc(To);
                DateTime FromTP = File.GetLastWriteTimeUtc(From);

                if (ToTP >= FromTP)
                {
                    Session.Log("Skipping file. Destination folder contains newer version: " + From);
                    Copy1 = false;
                }
            }

            if (Copy1)
            {
                Session.Log("Copying " + From + " to " + To);
                File.Copy(From, To, true);
            }

            if (!string.IsNullOrEmpty(To2))
            {
                bool Copy2 = From != To2;

                if (Copy2 && OnlyIfNewer && File.Exists(To2))
                {
                    DateTime ToTP = File.GetLastWriteTimeUtc(To2);
                    DateTime FromTP = File.GetLastWriteTimeUtc(From);

                    if (ToTP >= FromTP)
                    {
                        Session.Log("Skipping file. Destination folder contains newer version: " + From);
                        Copy2 = false;
                    }
                }

                if (Copy2)
                {
                    Session.Log("Copying " + From + " to " + To2);
                    File.Copy(From, To2, true);
                }
            }

            return true;
        }

        private enum CopyOptions
        {
            IfNewer = 2,
            Always = 3
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
                            KeyValuePair<string, string> FileNames;

                            try
                            {
                                FileNames = GetFileName(E, SourceFolder);
                            }
                            catch (FileNotFoundException)
                            {
                                // Already installed.
                                break;
                            }

                            string FileName = FileNames.Key;
                            string SourceFileName = FileNames.Value;
                            CopyOptions CopyOptions = (CopyOptions)XML.Attribute(E, "copy", CopyOptions.IfNewer);

                            Session.Log("Content file: " + FileName);

                            if (!string.IsNullOrEmpty(DataFolder) && !Directory.Exists(DataFolder))
                            {
                                Session.Log("Creating folder " + DataFolder + ".");
                                Directory.CreateDirectory(DataFolder);
                            }

                            if (!string.IsNullOrEmpty(AppFolder) && !Directory.Exists(AppFolder))
                            {
                                Session.Log("Creating folder " + AppFolder + ".");
                                Directory.CreateDirectory(AppFolder);
                            }

                            try
                            {
                                CopyFileIfNewer(Session, SourceFileName,
                                    Path.Combine(DataFolder, FileName),
                                    Path.Combine(AppFolder, FileName),
                                    CopyOptions == CopyOptions.IfNewer);
                            }
                            catch (FileNotFoundException)
                            {
                                // Already installed by installer.
                            }
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

        private static void Uninstall(Session Session, string ManifestFile, string ServerApplication, string ProgramDataFolder, bool Remove)
        {
            // Same code as for custom action UninstallManifest in Waher.IoTGateway.Installers, except:
            // * Content files can already be installed in the corresponding application data folder.
            // * Assembly files can be removed by uninstaller.

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
            AssemblyName ServerName = GetAssemblyName(ServerApplication);
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

            if (!(JSON.Parse(s) is Dictionary<string, object> Deps))
                throw new Exception("Invalid deps.json file. Unable to install.");

            Session.Log("Loading manifest file.");

            XmlDocument Manifest = new XmlDocument();
            Manifest.Load(ManifestFile);

            XmlElement Module = Manifest["Module"];
            string AppFolder = Path.GetDirectoryName(ServerApplication);

            Session.Log("App folder: " + AppFolder);

            foreach (XmlNode N in Module.ChildNodes)
            {
                if (N is XmlElement E && E.LocalName == "Assembly")
                {
                    KeyValuePair<string, string> FileNames = GetFileName(E, AppFolder);
                    string FileName = FileNames.Key;
                    string AppFileName = FileNames.Value;

                    Assembly A = Assembly.LoadFrom(AppFileName);
                    AssemblyName AN = A.GetName();
                    string Key = AN.Name + "/" + AN.Version.ToString();

                    if (Deps != null && Deps.TryGetValue("targets", out object Obj) && Obj is Dictionary<string, object> Targets)
                    {
                        Targets.Remove(Key);

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
                                        Dependencies.Remove(AN.Name);
                                        break;
                                    }
                                }
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
                    }

                    if (Remove)
                    {
                        RemoveFile(Session, AppFileName);
                        if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string PdbFileName = FileName.Substring(0, FileName.Length - 4) + ".pdb";
                            RemoveFile(Session, PdbFileName);
                        }
                    }
                }
            }

            Session.Log("Encoding JSON");
            s = JSON.Encode(Deps, true);

            Session.Log("Writing " + DepsJsonFileName);
            File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);

            if (Path.GetDirectoryName(ManifestFile) == AppFolder)
                RemoveFile(Session, ManifestFile);
        }

        private static bool RemoveFile(Session Session, string FileName)
        {
            if (!File.Exists(FileName))
                return false;

            Session.Log("Deleting " + FileName);
            try
            {
                File.Delete(FileName);
            }
            catch (Exception ex)
            {
                Session.Log("Unable to delete file. Error reported: " + ex.Message);
            }

            return true;
        }

        private static KeyValuePair<string, string> GetFileName(XmlElement E, string ReferenceFolder)
        {
            string FileName = XML.Attribute(E, "fileName");
            string AbsFileName = Path.Combine(ReferenceFolder, FileName);
            if (File.Exists(AbsFileName))
                return new KeyValuePair<string, string>(FileName, AbsFileName);

            string AltFolder = XML.Attribute(E, "altFolder");
            if (string.IsNullOrEmpty(AltFolder))
                throw new FileNotFoundException("File not found: " + AbsFileName);

            AbsFileName = Path.Combine(AltFolder, FileName);
            if (File.Exists(AbsFileName))
                return new KeyValuePair<string, string>(FileName, AbsFileName);

            throw new FileNotFoundException("File not found: " + AbsFileName);
        }

        #endregion

    }
}
