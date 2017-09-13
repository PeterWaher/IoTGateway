using System;
using Waher.Events;
using Waher.IoTGateway.Svc.ServiceManagement;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;

namespace Waher.IoTGateway.Svc
{
	class Program
	{
		private static ServiceHost host;

		static int Main(string[] args)
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

						case "-displayname":
							i++;
							if (i >= c)
							{
								Console.Out.Write("Unexpected end of command line arguments.");
								return -1;
							}

							DisplayName = args[i];
							break;

						case "-description":
							i++;
							if (i >= c)
							{
								Console.Out.Write("Unexpected end of command line arguments.");
								return -1;
							}

							Description = args[i];
							break;

						case "-start":
							i++;
							if (i >= c)
							{
								Console.Out.Write("Unexpected end of command line arguments.");
								return -1;
							}

							if (!Enum.TryParse<ServiceStartType>(args[i], out StartType))
							{
								Console.Out.WriteLine("Supported start types:");
								foreach (string s in Enum.GetNames(typeof(ServiceStartType)))
									Console.Out.WriteLine(s);

								return -1;
							}
							break;

						default:
							Console.Out.Write("Unrecognized command line argument: " + Arg);
							return -1;
					}
				}

				if (Install && Uninstall)
				{
					Console.Out.Write("Conflicting arguments.");
					return -1;
				}

				host = new ServiceHost(ServiceName);

				if (Install)
				{
					switch (i = host.Install(DisplayName, Description, StartType))
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
