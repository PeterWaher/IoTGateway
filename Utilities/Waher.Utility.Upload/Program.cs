using System;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;

namespace Waher.Utility.Upload
{
	class Program
	{
		/// <summary>
		/// Uploads a package (optionally signed) to a broker.
		/// 
		/// Command line switches:
		/// 
		/// -p PACKAGE_FILENAME    Package file name.
		/// -s SIGNATURE_FILENAME  Optional package signature file name.
		/// -h HOST                Host name of broker to upload to.
		/// -n PORT                Port number of broker to upload to, 
		///                        if not using the default port number.
		/// -?                     Help.
		/// </summary>
		static int Main(string[] args)
		{
			string PackageFileName = null;
			string SignatureFileName = null;
			string HostName = null;
			string s;
			int Port = 0;
			int i = 0;
			int c = args.Length;
			bool Help = false;

			try
			{
				Types.Initialize(
					typeof(Program).Assembly);

				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-p":
							if (i >= c)
								throw new Exception("Missing package file name.");

							if (string.IsNullOrEmpty(PackageFileName))
								PackageFileName = args[i++];
							else
								throw new Exception("Only one package file name allowed.");
							break;

						case "-s":
							if (i >= c)
								throw new Exception("Missing signature file name.");

							if (string.IsNullOrEmpty(SignatureFileName))
								SignatureFileName = args[i++];
							else
								throw new Exception("Only one signature file name allowed.");
							break;

						case "-h":
							if (i >= c)
								throw new Exception("Missing host name.");

							if (string.IsNullOrEmpty(HostName))
								HostName = args[i++];
							else
								throw new Exception("Only one host name allowed.");
							break;

						case "-n":
							if (i >= c)
								throw new Exception("Missing port number.");

							if (!int.TryParse(args[i++], out int j) || j <= 0 || j > 65535)
								throw new Exception("Invalid port number.");

							if (Port == 0)
								Port = j;
							else
								throw new Exception("Only one port number allowed.");
							break;

						case "-?":
							Help = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (Help || c == 0)
				{
					ConsoleOut.WriteLine("Uploads a package (optionally signed) to a broker.");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("Command line switches:");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("-p PACKAGE_FILENAME    Package file name.");
					ConsoleOut.WriteLine("-s SIGNATURE_FILENAME  Optional package signature file name.");
					ConsoleOut.WriteLine("-h HOST                Host name of broker to upload to.");
					ConsoleOut.WriteLine("-n PORT                Port number of broker to upload to, ");
					ConsoleOut.WriteLine("                       if not using the default port number.");
					ConsoleOut.WriteLine("-?                     Help.");
					return 0;
				}

				return 0;
			}
			catch (Exception ex)
			{
				ConsoleOut.WriteLine(ex.Message);
				return -1;
			}
			finally
			{
				ConsoleOut.Flush(true);
			}
		}
	}
}
