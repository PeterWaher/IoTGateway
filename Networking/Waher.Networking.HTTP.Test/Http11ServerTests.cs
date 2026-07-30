using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Networking.HTTP.Brotli;
using Waher.Networking.HTTP.Mcp;
using Waher.Networking.HTTP.OAuth.Clients;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Security.JWS;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http11ServerTests : HttpServerTests
	{
		private static FilesProvider filesProvider = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(HttpServer).Assembly,
				typeof(BrotliContentEncoding).Assembly,
				typeof(HttpServerTests).Assembly,
				typeof(Script.Expression).Assembly,
				typeof(Content.Images.ImageCodec).Assembly,
				typeof(InternetContent).Assembly,
				typeof(HtmlDocument).Assembly,
				typeof(IJwsAlgorithm).Assembly,
				typeof(JwtToken).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(OAuthClientInformation).Assembly,
				typeof(HttpMcpServerResource).Assembly);

			filesProvider = await FilesProvider.CreateAsync("DB", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, false);
			Database.Register(filesProvider);

			await Types.StartAllModules(10000);
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await Types.StopAllModules();

			if (filesProvider is not null)
			{
				await filesProvider.DisposeAsync();
				filesProvider = null;
			}
		}

		[TestCleanup]
		public Task TestCleanup()
		{
			return this.Cleanup();
		}

		public override Version ProtocolVersion => HttpVersion.Version11;
	}
}
