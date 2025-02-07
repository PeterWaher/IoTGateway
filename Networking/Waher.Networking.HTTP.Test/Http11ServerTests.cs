using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.Brotli;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http11ServerTests : HttpServerTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Runtime.Inventory.Types.Initialize(
				typeof(HttpServer).Assembly,
				typeof(BrotliContentEncoding).Assembly,
				typeof(HttpServerTests).Assembly,
				typeof(Script.Expression).Assembly,
				typeof(Content.Images.ImageCodec).Assembly,
				typeof(CommonTypes).Assembly);
		}

		[TestCleanup]
		public Task TestCleanup()
		{
			return this.Cleanup();
		}

		public override Version ProtocolVersion => HttpVersion.Version11;
	}
}
