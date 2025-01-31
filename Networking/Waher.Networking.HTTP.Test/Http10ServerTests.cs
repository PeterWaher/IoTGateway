using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http10ServerTests : HttpServerTests
	{
		[TestCleanup]
		public Task TestCleanup()
		{
			return this.Cleanup();
		}

		public override Version ProtocolVersion => HttpVersion.Version10;
	}
}
