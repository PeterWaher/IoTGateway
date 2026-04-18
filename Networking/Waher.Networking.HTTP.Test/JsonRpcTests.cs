using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.JsonRpc.Exceptions;
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

			this.jsonRpcWebService = new JsonRpcWebService("/endpoint", true, Add, Sub);
			this.server.Register(this.jsonRpcWebService);

			this.client = new JsonRpcClient("http://localhost:8081/endpoint",
				new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine),
				this.xmlSniffer);
		}

		private static int Add(int a, int b)
		{
			return a + b;
		}

		private static int Sub(int a, int b)
		{
			return a - b;
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
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", 3, 4, 7)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", 3, 4, 7)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", 3, 4, 7)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", 3, 4, 7)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", 4, 3, 7)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", 4, 3, 7)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", 4, 3, 7)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", 4, 3, 7)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Sub", 3, 4, -1)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Sub", 3, 4, -1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Sub", 3, 4, -1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Sub", 3, 4, -1)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Sub", 4, 3, 1)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Sub", 4, 3, 1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Sub", 4, 3, 1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Sub", 4, 3, 1)]
		public async Task Test_01_RequestDictionaryParameters(JsonRpcHttpMethod Method,
			JsonRpcVersion Version, string MethodName, int A, int B, int ExpectedResult)
		{
			object Result = await this.client.Request(Method, Version, MethodName,
				new Dictionary<string, object>()
				{
					{ "a", A },
					{ "b", B }
				});

			Assert.AreEqual(ExpectedResult, Result);
		}

		[TestMethod]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", 3, 4, 7)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", 3, 4, 7)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", 3, 4, 7)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", 3, 4, 7)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", 4, 3, 7)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", 4, 3, 7)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", 4, 3, 7)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", 4, 3, 7)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Sub", 3, 4, -1)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Sub", 3, 4, -1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Sub", 3, 4, -1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Sub", 3, 4, -1)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Sub", 4, 3, 1)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Sub", 4, 3, 1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Sub", 4, 3, 1)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Sub", 4, 3, 1)]
		public async Task Test_02_RequestArrayParameters(JsonRpcHttpMethod Method,
			JsonRpcVersion Version, string MethodName, int A, int B, int ExpectedResult)
		{
			object Result = await this.client.Request(Method, Version, MethodName,
				new object[] { A, B });

			Assert.AreEqual(ExpectedResult, Result);
		}

		[TestMethod]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", 3, 4)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", 3, 4)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", 3, 4)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", 3, 4)]
		public async Task Test_03_NotifyDictionaryParameters(JsonRpcHttpMethod Method,
			JsonRpcVersion Version, string MethodName, int A, int B)
		{
			await this.client.Notify(Method, Version, MethodName,
				new Dictionary<string, object>()
				{
					{ "a", A },
					{ "b", B }
				});
		}

		[TestMethod]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", 3, 4)]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", 3, 4)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", 3, 4)]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", 3, 4)]
		public async Task Test_04_NotifyArrayParameters(JsonRpcHttpMethod Method,
			JsonRpcVersion Version, string MethodName, int A, int B)
		{
			await this.client.Notify(Method, Version, MethodName,
				new object[] { A, B });
		}

		[TestMethod]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		public async Task Test_05_RequestDictionaryParameters_Error(JsonRpcHttpMethod Method,
			JsonRpcVersion Version, string MethodName, object A, object B,
			Type ExpectedException)
		{
			Exception Exception = null;

			try
			{
				await this.client.Request(Method, Version, MethodName,
					new Dictionary<string, object>()
					{
						{ "a", A },
						{ "b", B }
					});
			}
			catch (Exception ex)
			{
				Exception = ex;
			}

			Assert.IsNotNull(Exception, "Exception expected.");
			Assert.AreEqual(ExpectedException, Exception.GetType());
		}

		[TestMethod]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		public async Task Test_06_RequestArrayParameters_Error(JsonRpcHttpMethod Method,
			JsonRpcVersion Version, string MethodName, object A, object B,
			Type ExpectedException)
		{
			Exception Exception = null;

			try
			{
				await this.client.Request(Method, Version, MethodName,
					new object[] { A, B });
			}
			catch (Exception ex)
			{
				Exception = ex;
			}

			Assert.IsNotNull(Exception, "Exception expected.");
			Assert.AreEqual(ExpectedException, Exception.GetType());
		}

		[TestMethod]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		public async Task Test_07_NotifyDictionaryParameters_Error(JsonRpcHttpMethod Method,
			JsonRpcVersion Version, string MethodName, object A, object B,
			Type ExpectedException)
		{
			Exception Exception = null;

			try
			{
				await this.client.Notify(Method, Version, MethodName,
					new Dictionary<string, object>()
					{
						{ "a", A },
						{ "b", B }
					});
			}
			catch (Exception ex)
			{
				Exception = ex;
			}

			Assert.IsNotNull(Exception, "Exception expected.");
			Assert.AreEqual(ExpectedException, Exception.GetType());
		}

		[TestMethod]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "X", 3, 4, typeof(JsonRpcMethodNotFoundError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV1, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.GET, JsonRpcVersion.JsonRpcV2, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV1, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		[DataRow(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, "Add", "a", "b", typeof(JsonRpcInvalidParametersError))]
		public async Task Test_08_NotifyArrayParameters_Error(JsonRpcHttpMethod Method,
			JsonRpcVersion Version, string MethodName, object A, object B,
			Type ExpectedException)
		{
			Exception Exception = null;

			try
			{
				await this.client.Notify(Method, Version, MethodName,
					new object[] { A, B });
			}
			catch (Exception ex)
			{
				Exception = ex;
			}

			Assert.IsNotNull(Exception, "Exception expected.");
			Assert.AreEqual(ExpectedException, Exception.GetType());
		}

		[TestMethod]
		public async Task Test_09_BatchRequests()
		{
			JsonRpcResult[] Results = await this.client.BatchProcess(
				new JsonRpcRequest("Add", 3, 4),
				new JsonRpcRequest("Sub", 3, 4));

			Assert.AreEqual(2, Results.Length);
			
			Assert.IsTrue(Results[0].HasResult);
			Assert.AreEqual(7, Results[0].Result);
			
			Assert.IsTrue(Results[1].HasResult);
			Assert.AreEqual(-1, Results[1].Result);
		}

		[TestMethod]
		[ExpectedException(typeof(JsonRpcInvalidRequestError))]
		public async Task Test_10_EmptyBatch()
		{
			await this.client.BatchProcess();
		}

		[TestMethod]
		public async Task Test_11_BatchNotifications()
		{
			JsonRpcResult[] Results = await this.client.BatchProcess(
				new JsonRpcNotification("Add", 3, 4),
				new JsonRpcNotification("Sub", 3, 4));

			Assert.AreEqual(0, Results.Length);
		}

		[TestMethod]
		public async Task Test_12_BatchMixed()
		{
			JsonRpcResult[] Results = await this.client.BatchProcess(
				new JsonRpcNotification("Add", 3, 4),
				new JsonRpcRequest("Sub", 3, 4));

			Assert.AreEqual(1, Results.Length);

			Assert.IsTrue(Results[0].HasResult);
			Assert.AreEqual(-1, Results[0].Result);
		}

	}
}
