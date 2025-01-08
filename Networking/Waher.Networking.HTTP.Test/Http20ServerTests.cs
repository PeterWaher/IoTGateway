using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Test
{
	/// <summary>
	/// Performs HTTP/2 tests.
	/// 
	/// For a comprehensive set of tests for HTTP/2, use the h2spec tool:
	/// https://github.com/summerwind/h2spec
	/// 
	/// Other testing tools are also available:
	/// https://github.com/httpwg/wiki/wiki/HTTP-Testing-Resources
	/// </summary>
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
