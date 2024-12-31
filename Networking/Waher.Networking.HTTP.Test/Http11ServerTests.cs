using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http11ServerTests : HttpServerTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Runtime.Inventory.Types.Initialize(
				typeof(HttpServerTests).Assembly,
				typeof(Script.Expression).Assembly,
				typeof(Content.Images.ImageCodec).Assembly,
				typeof(CommonTypes).Assembly);
		}

		[ClassInitialize]
		public new static void ClassInitialize(TestContext _)
		{
			HttpServerTests.ClassInitialize(_);
		}

		[ClassCleanup]
		public new static async Task ClassCleanup()
		{
			await HttpServerTests.ClassCleanup();
		}

		public override Version ProtocolVersion => HttpVersion.Version11;
	}
}
