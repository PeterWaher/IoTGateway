using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Test
{
	/// <summary>
	/// Performs HTTP/2 tests.
	/// </summary>
	[TestClass]
	public class Http20ServerTests : HttpServerTests
	{
		[TestCleanup]
		public Task TestCleanup()
		{
			return this.Cleanup();
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
