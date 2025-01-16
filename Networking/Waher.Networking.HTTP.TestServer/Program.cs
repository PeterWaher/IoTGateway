using System.Security.Cryptography.X509Certificates;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;

internal class Program
{
	/// <summary>
	/// Test server for use with the the h2spec and similar.
	/// 
	/// References:
	/// https://github.com/summerwind/h2spec
	/// https://github.com/httpwg/wiki/wiki/HTTP-Testing-Resources
	/// </summary>
	/// <example>
	/// To run tests over HTTPS
	/// 
	/// Test-server command-line arguments:
	/// Waher.Networking.HTTP.TestServer.exe -http 8081 -https 8088 -cert CERTIFICATE_FILENAME
	/// 
	/// h2spec command-line arguments:
	/// h2spec.exe http2 -h 127.0.0.1 -p 8088 -P /Hello -t -k --strict
	/// </example>
	/// <example>
	/// To run tests over HTTP
	/// 
	/// Test-server command-line arguments:
	/// Waher.Networking.HTTP.TestServer.exe -http 8081
	/// 
	/// h2spec command-line arguments:
	/// 
	/// h2spec.exe -h 127.0.0.1 -p 8081 -P /Hello
	/// h2spec.exe -h 127.0.0.1 -p 8081 -P /Hello --strict
	/// 
	/// h2spec.exe http2 -h 127.0.0.1 -p 8081 -P /Hello
	/// h2spec.exe http2 -h 127.0.0.1 -p 8081 -P /Hello --strict
	/// 
	/// h2spec.exe hpack -h 127.0.0.1 -p 8081 -P /Hello
	/// h2spec.exe hpack -h 127.0.0.1 -p 8081 -P /Hello --strict
	/// 
	/// h2spec.exe generic -h 127.0.0.1 -p 8081 -P /Hello
	/// h2spec.exe generic -h 127.0.0.1 -p 8081 -P /Hello --strict
	/// </example>
	/// <param name="args">Command-line arguments.</param>
	private static void Main(string[] args)
	{
		X509Certificate2? Certificate = null;
		HttpServer? WebServer = null;
		ConsoleOutSniffer? Sniffer = null;
		string s;
		int HttpPort = 0;
		int HttpsPort = 0;
		int i = 0;
		int c = args.Length;
		bool Help = false;
		bool Terminating = false;

		try
		{
			Log.Register(new ConsoleEventSink(true, true));

			while (i < c)
			{
				s = args[i++].ToLower();

				switch (s)
				{
					case "-http":
						if (i >= c)
							throw new Exception("Missing HTTP Port.");

						if (HttpPort != 0)
							throw new Exception("HTTP Port already defined.");

						if (!int.TryParse(args[i++], out HttpPort) || HttpPort <= 0 || HttpPort > ushort.MaxValue)
							throw new Exception("Invalid HTTP Port number.");
						break;

					case "-https":
						if (i >= c)
							throw new Exception("Missing HTTPS Port.");

						if (HttpsPort != 0)
							throw new Exception("HTTPS Port already defined.");

						if (!int.TryParse(args[i++], out HttpsPort) || HttpsPort <= 0 || HttpsPort > ushort.MaxValue)
							throw new Exception("Invalid HTTPS Port number.");
						break;

					case "-cert":
						if (i >= c)
							throw new Exception("Missing Certificate file name.");

						if (Certificate is not null)
							throw new Exception("Certificate already defined.");

						Certificate = new X509Certificate2(args[i++]);
						break;

					case "-?":
						Help = true;
						break;

					default:
						throw new Exception("Unrecognized switch: " + s);
				}
			}

			if (Help)
			{
				ConsoleOut.WriteLine("-http PORT_NUMBER    The port number to use for unencrypted HTTP connections.");
				ConsoleOut.WriteLine("-https PORT_NUMBER   The port number to use for encrypted HTTPS connections.");
				ConsoleOut.WriteLine("-?                   Help.");
				return;
			}

			if (HttpPort == 0)
				throw new Exception("HTTP Port not defined.");

			if (Certificate is not null && HttpsPort == 0)
				throw new Exception("HTTPS Port not defined.");

			Log.Informational("Starting application.");

			Types.Initialize(
				typeof(HttpServer).Assembly,
				typeof(Log).Assembly,
				typeof(InternetContent).Assembly);

			Sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal, LineEnding.PadWithSpaces);

			if (Certificate is null)
			{
				Log.Informational("Starting unencrypted web-server.");

				WebServer = new HttpServer(HttpPort, Sniffer);
			}
			else
			{
				Log.Informational("Starting encrypted web-server.");

				WebServer = new HttpServer([HttpPort], [HttpsPort], Certificate, Sniffer);
			}

			WebServer.Register("/Hello", 
				(req, resp) => resp.Write("Hello World."),
				(req, resp) => resp.Write("Hello World."));

			Log.Informational("Press CTRL+C to quit.");

			Console.CancelKeyPress += (_, e)=>
			{
				Terminating = true;
				e.Cancel = true;
			};

			while (!Terminating)
				Thread.Sleep(100);
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
		finally
		{
			Log.Informational("Shutting down.");

			WebServer?.Dispose();

			if (Sniffer is not null)
				Sniffer?.DisposeAsync().Wait();

			Log.TerminateAsync().Wait();
		}
	}
}