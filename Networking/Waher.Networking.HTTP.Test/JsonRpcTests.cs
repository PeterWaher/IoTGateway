using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class JsonRpcTests : IUserSource
	{
		private HttpServer server;
		private ConsoleEventSink sink = null;
		private XmlFileSniffer xmlSniffer = null;
		private JsonRpcWebService jsonRpcWebService;
		private JsonRpcClient client;

		/// <summary>
		/// Test context
		/// </summary>
		public TestContext TestContext { get; set; }

		[TestInitialize]
		public void TestInitialize()
		{
			string SnifferFileName = this.TestContext.TestName;
			if (string.IsNullOrEmpty(SnifferFileName))
				SnifferFileName = "JsonRpc";

			SnifferFileName = "Sniffers" + Path.DirectorySeparatorChar + SnifferFileName + ".xml";

			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);

			File.Delete(SnifferFileName);
			this.xmlSniffer = new XmlFileSniffer(SnifferFileName,
				@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
				int.MaxValue, BinaryPresentationMethod.ByteCount);

			this.server = new HttpServer(8081);

			this.jsonRpcWebService = new JsonRpcWebService("/endpoint", true);
			this.server.Register(this.jsonRpcWebService);

			this.client = new JsonRpcClient("http://localhost:8081/endpoint",
				new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine),
				this.xmlSniffer);
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.jsonRpcWebService is not null)
			{
				this.server.Unregister(this.jsonRpcWebService);
				this.jsonRpcWebService = null;
			}

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

		[TestMethod]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2)]
		public async Task Test_01_RequestDictionaryParameters(JsonRpcHttpMethod Method, JsonRpcVersion Version)
		{
			object Result = await this.client.Request(Method, Version, "sum",
				new Dictionary<string, object>()
				{ 
					{ "a", 3 },
					{ "b", 4 }
				});

			Assert.AreEqual(7, Result);
		}

		[TestMethod]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2)]
		public async Task Test_02_RequestArrayParameters(JsonRpcHttpMethod Method, JsonRpcVersion Version)
		{
			object Result = await this.client.Request(Method, Version, "sum",
				new object[] { 3, 4 });

			Assert.AreEqual(7, Result);
		}
	}
}
