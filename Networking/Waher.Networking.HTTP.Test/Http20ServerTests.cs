using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http20ServerTests : HttpServerTests
	{
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

		public override Version ProtocolVersion => HttpVersion.Version20;

		/* To Test
		 * 
		 * Huffman encoded strings
		 * padding in requests
		 * Long headers (multiple HEADERS & CONTINUATION frames)
		 * data-less responses
		 * long headers in data responses (multiple HEADER & CONTINUATION frames)
		 * push promise
		 * stream priorities
		 * stream errors
		 * connection errors
		 * ping/pong
		 * internal flow control parameters OK at end of test.
		 * maximum concurrent streams.
		 */
	}
}
