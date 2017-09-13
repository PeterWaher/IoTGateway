using System;
using Waher.Events;
using Waher.IoTGateway.Svc.ServiceManagement;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;

namespace Waher.IoTGateway.Svc
{
	/// <summary>
	/// IoT Gateway Windows Service Application.
	/// 
	/// Command line switches:
	/// 
	/// -install             Installs service in operating system
	/// -uninstall           Uninstalls service from operating system.
	/// -displayname Name    Sets the display name of the service. Default is "IoT Gateway Service".
	/// -description Desc    Sets the textual description of the service. Default is "Windows Service hosting the Waher IoT Gateway.".
	/// -start Mode          Sets the default starting mode of the service. Default is Disabled. Available options are StartOnBoot, StartOnSystemStart, AutoStart, StartOnDemand and Disabled
	/// -immediate           If the service should be started immediately.
	/// </summary>
	public class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				string ServiceName = "IoT Gateway Service";
				string DisplayName = ServiceName;
				string Description = "Windows Service hosting the Waher IoT Gateway.";
				string Arg;
				ServiceStartType StartType = ServiceStartType.Disabled;
				bool Install = false;
				bool Uninstall = false;
				bool Immediate = false;
				bool Error = false;
				int i, c = args.Length;

				for (i = 0; i < c; i++)
				{
					Arg = args[i];

					switch (Arg.ToLower())
					{
						case "-install":
							Install = true;
							break;

						case "-uninstall":
							Uninstall = true;
							break;

						case "-immediate":
							Immediate = true;
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

							if (!Enum.TryParse<ServiceStartType>(args[i], out StartType))
							{
								Error = true;
								break;
							}
							break;

						default:
							Error = true;
							break;
					}
				}

				if (Error)
				{
					Console.Out.WriteLine("IoT Gateway Windows Service Application.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-install             Installs service in operating system");
					Console.Out.WriteLine("-uninstall           Uninstalls service from operating system.");
					Console.Out.WriteLine("-displayname Name    Sets the display name of the service. Default is \"IoT ");
					Console.Out.WriteLine("                     Gateway Service\".");
					Console.Out.WriteLine("-description Desc    Sets the textual description of the service. Default is ");
					Console.Out.WriteLine("                     \"Windows Service hosting the Waher IoT Gateway.\".");
					Console.Out.WriteLine("-start Mode          Sets the default starting mode of the service. Default is ");
					Console.Out.WriteLine("                     Disabled. Available options are StartOnBoot, ");
					Console.Out.WriteLine("                     StartOnSystemStart, AutoStart, StartOnDemand and Disabled.");

					return -1;
				}

				if (Install && Uninstall)
				{
					Console.Out.Write("Conflicting arguments.");
					return -1;
				}

				ServiceHost host = new ServiceHost(ServiceName);

				if (Install)
				{
					switch (i = host.Install(DisplayName, Description, StartType, Immediate))
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
				else if (Uninstall)
				{
					if (host.Uninstall())
						Console.Out.WriteLine("Service successfully uninstalled.");
					else
						Console.Out.WriteLine("Service not found. Uninstall not required.");
				}
				else
					host.Run();

				return 0;
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
				return -1;
			}
		}
	}
}
