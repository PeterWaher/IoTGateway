using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	public abstract class HttpServerTestsBase : IUserSource
	{
		protected static HttpServer server;
		private static ConsoleEventSink sink = null;
		private static XmlFileSniffer xmlSniffer = null;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			ClassInitialize(true);
		}

		public static void ClassInitialize(bool UseConsoleSniffer)
		{
			sink = new ConsoleEventSink();
			Log.Register(sink);

			if (xmlSniffer is null)
			{
				File.Delete("HTTP.xml");
				xmlSniffer = new XmlFileSniffer("HTTP.xml",
					@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
					int.MaxValue, BinaryPresentationMethod.ByteCount);
			}

			X509Certificate2 Certificate = Resources.LoadCertificate("Waher.Networking.HTTP.Test.Data.certificate.pfx", "testexamplecom");  // Certificate from http://www.cert-depot.com/
			server = new HttpServer(8081, 8088, Certificate, xmlSniffer);

			if (UseConsoleSniffer)
				server.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine));

			server.SetHttp2ConnectionSettings(2500000, 16384, 100, 8192, true, false, false);

			ServicePointManager.ServerCertificateValidationCallback = delegate (object obj, X509Certificate X509certificate, X509Chain chain, SslPolicyErrors errors)
			{
				return true;
			};
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			server?.Dispose();
			server = null;

			if (xmlSniffer is not null)
			{
				await xmlSniffer.DisposeAsync();
				xmlSniffer = null;
			}

			if (sink is not null)
			{
				Log.Unregister(sink);
				await sink.DisposeAsync();
				sink = null;
			}
		}

		public Task<IUser> TryGetUser(string UserName)
		{
			if (UserName == "User")
				return Task.FromResult<IUser>(new User());
			else
				return Task.FromResult<IUser>(null);
		}
	}
}
