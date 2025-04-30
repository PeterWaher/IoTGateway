using System.Security.Cryptography.X509Certificates;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Web;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Brotli;
using Waher.Networking.HTTP.ContentEncodings;
using Waher.Networking.Sniffers;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Runtime.Profiling;

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
	/// -http PORT          Port number for HTTP
	/// -http2 PORT         Port number for HTTPS
	/// -cert FILENAME      Certificate file name.
	/// -pwd PASSWORD       Certificate password, if any.
	/// -no7540prio         No RFC 7540 priorities, in accordance with RFC 9218
	/// -deflate            Permit deflate encoding
	/// -gzip               Permit gzip encoding
	/// -br                 Permit br encoding
	/// 
	/// Example:
	/// Waher.Networking.HTTP.TestServer.exe -http 8081 -https 8088 -cert CERTIFICATE_FILENAME -deflate
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
	/// 
	/// h2spec.exe -h 127.0.0.1 -p 8081 -P /Hello.md
	/// h2spec.exe -h 127.0.0.1 -p 8081 -P /Hello.md --strict
	/// 
	/// h2spec.exe http2 -h 127.0.0.1 -p 8081 -P /Hello.md
	/// h2spec.exe http2 -h 127.0.0.1 -p 8081 -P /Hello.md --strict
	/// 
	/// h2spec.exe hpack -h 127.0.0.1 -p 8081 -P /Hello.md
	/// h2spec.exe hpack -h 127.0.0.1 -p 8081 -P /Hello.md --strict
	/// 
	/// h2spec.exe generic -h 127.0.0.1 -p 8081 -P /Hello.md
	/// h2spec.exe generic -h 127.0.0.1 -p 8081 -P /Hello.md --strict
	/// </example>
	/// <param name="args">Command-line arguments.</param>
	private static void Main(string[] args)
	{
		X509Certificate2? Certificate = null;
		HttpServer? WebServer = null;
		ConsoleOutSniffer? Sniffer = null;
		string s;
		string? CertFileName = null;
		string? CertPassword = null;
		int HttpPort = 0;
		int HttpsPort = 0;
		int i = 0;
		int c = args.Length;
		bool Help = false;
		bool Terminating = false;
		bool No7540prio = false;

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

						if (!string.IsNullOrEmpty(CertFileName))
							throw new Exception("Certificate file name already defined.");

						CertFileName = args[i++];
						break;

					case "-pwd":
						if (i >= c)
							throw new Exception("Missing Certificate password.");

						if (!string.IsNullOrEmpty(CertPassword))
							throw new Exception("Certificate password already defined.");

						CertPassword = args[i++];
						break;

					case "-no7540prio":
						No7540prio = true;
						break;

					case "-deflate":
						DeflateContentEncoding DeflateContentEncoding = new();
						DeflateContentEncoding.ConfigureSupport(true, true);
						break;

					case "-gzip":
						GZipContentEncoding GZipContentEncoding = new();
						GZipContentEncoding.ConfigureSupport(true, true);
						break;

					case "-br":
						BrotliContentEncoding BrotliContentEncoding = new();
						BrotliContentEncoding.ConfigureSupport(true, true);
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
				ConsoleOut.WriteLine("-cert FILENAME       Certificate file name.");
				ConsoleOut.WriteLine("-pwd PASSWORD        Certificate password, if any.");
				ConsoleOut.WriteLine("-no7540prio          No RFC 7540 priorities, in accordance with RFC 9218");
				ConsoleOut.WriteLine("-deflate             Permit deflate encoding");
				ConsoleOut.WriteLine("-gzip                Permit gzip encoding");
				ConsoleOut.WriteLine("-br                  Permit br encoding");
				ConsoleOut.WriteLine("-?                   Help.");
				return;
			}

			if (!string.IsNullOrEmpty(CertFileName))
			{
				if (string.IsNullOrEmpty(CertPassword))
					Certificate = new X509Certificate2(CertFileName);
				else
					Certificate = new X509Certificate2(CertFileName, CertPassword);
			}

			if (HttpPort == 0)
				throw new Exception("HTTP Port not defined.");

			if (Certificate is not null && HttpsPort == 0)
				throw new Exception("HTTPS Port not defined.");

			Log.Informational("Starting application.");

			Types.Initialize(
				typeof(HttpServer).Assembly,
				typeof(BrotliContentEncoding).Assembly,
				typeof(Log).Assembly,
				typeof(InternetContent).Assembly,
				typeof(MarkdownDocument).Assembly,
				typeof(MarkdownToHtmlConverter).Assembly);

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

			int ProfilerIndex = 0;

			WebServer.SetHttp2ConnectionSettings(true, 2500000, 16384, 100, 8192, false, No7540prio, true, true);
			WebServer.ConnectionProfiled += (Sender, e) =>
			{
				if (!Directory.Exists("Profiler"))
					Directory.CreateDirectory("Profiler");

				string Uml = e.Profiler.ExportPlantUml(TimeUnit.Seconds);
				string FileName = Path.Combine("Profiler", (++ProfilerIndex).ToString() + ".uml");
				return Files.WriteAllTextAsync(FileName, Uml);
			};

			static Task Hello(HttpRequest Request, HttpResponse Response)
			{
				Response.ContentType = "text/plain";
				return Response.Write("Hello World.");
			};

			static async Task HelloMarkdown(HttpRequest Request, HttpResponse Response)
			{
				string Markdown = "Title: Hello World\r\nCSS: Hello.css\r\n\r\nHello **World**.";
				MarkdownSettings Settings = new()
				{
					Progress = Request.Http2Stream
				};

				MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
				string Html = await Doc.GenerateHTML();

				Response.ContentType = "text/html";
				await Response.Write(Html);
			}
			;

			static Task HelloStyles(HttpRequest Request, HttpResponse Response)
			{
				Response.ContentType = "text/css";
				return Response.Write("body\r\n{\r\nbackground-color:yellow\r\n}");
			}
			;

			WebServer.Register("/Hello", Hello, Hello);
			WebServer.Register("/Hello.md", HelloMarkdown, HelloMarkdown);
			WebServer.Register("/Hello.css", HelloStyles);

			Log.Informational("Press CTRL+C to quit.");

			Console.CancelKeyPress += (_, e) =>
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

			WebServer?.DisposeAsync().Wait();
			Sniffer?.DisposeAsync().Wait();

			Log.TerminateAsync().Wait();
		}
	}
}