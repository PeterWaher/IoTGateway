using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.Brotli;
using Waher.Networking.HTTP.ContentEncodings;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.Sniffers;
using Waher.Runtime.IO;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	public abstract class HttpServerTestsBase : IUserSource
	{
		protected HttpServer server;
		private ConsoleEventSink sink = null;
		private XmlFileSniffer xmlSniffer = null;

		protected void Setup(bool UseConsoleSniffer, string SnifferFileName, bool NoRfc7540Priorities,
			bool SupportDeflate, bool SupportGZip, bool SupportBrotli)
		{
			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);

			if (!Directory.Exists("Sniffers"))
				Directory.CreateDirectory("Sniffers");

			SnifferFileName = Path.Combine("Sniffers", SnifferFileName);

			File.Delete(SnifferFileName);
			this.xmlSniffer = new XmlFileSniffer(SnifferFileName,
				@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
				int.MaxValue, BinaryPresentationMethod.ByteCount);

			X509Certificate2 Certificate = Certificates.LoadCertificate("Waher.Networking.HTTP.Test.Data.certificate.pfx", "testexamplecom");  // Certificate from http://www.cert-depot.com/
			this.server = new HttpServer(8081, 8088, Certificate, this.xmlSniffer);

			if (UseConsoleSniffer)
				this.server.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine));

			this.server.SetHttp2ConnectionSettings(true, 2500000, 16384, 100, 8192, false, NoRfc7540Priorities, true, false);

			this.server.ConnectionProfiled += async (sender, e) =>
			{
				await Files.WriteAllTextAsync(Path.ChangeExtension(SnifferFileName, ".uml"),
					e.ExportPlantUml(Runtime.Profiling.TimeUnit.MilliSeconds));
			};

			DeflateContentEncoding DeflateContentEncoding = new();
			DeflateContentEncoding.ConfigureSupport(SupportDeflate, SupportDeflate);

			GZipContentEncoding GZipContentEncoding = new();
			GZipContentEncoding.ConfigureSupport(SupportGZip, SupportGZip);

			BrotliContentEncoding BrotliContentEncoding = new();
			BrotliContentEncoding.ConfigureSupport(SupportBrotli, SupportBrotli);

			HttpFieldAcceptEncoding.ContentEncodingsReconfigured();

			ServicePointManager.ServerCertificateValidationCallback = delegate (object obj, X509Certificate X509certificate, X509Chain chain, SslPolicyErrors errors)
			{
				return true;
			};
		}

		protected async Task Cleanup()
		{
			if (this.server is not null)
			{
				await this.server.DisposeAsync();
				this.server = null;
			}

			if (this.xmlSniffer is not null)
			{
				await this.xmlSniffer.DisposeAsync();
				this.xmlSniffer = null;
			}

			if (this.sink is not null)
			{
				Log.Unregister(this.sink);
				await this.sink.DisposeAsync();
				this.sink = null;
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
