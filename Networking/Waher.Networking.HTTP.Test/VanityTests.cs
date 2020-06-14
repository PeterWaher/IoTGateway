using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Authentication;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class VanityTests
	{
		private static HttpServer server;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			server = new HttpServer();

			server.RegisterVanityResource("/Test/(?'Op'view|edit)/(?'Type'video|photo)/(?'Id'\\d+)","/Test/Display.md?op={Op}&t={Type}&id={Id}");
			server.RegisterVanityResource("/Test/(?'Op'print|export)/(?'Type'video|photo)/(?'Id'\\d+)","/Test/Output.md?op={Op}&t={Type}&id={Id}");
			server.RegisterVanityResource("/Test2/(?'Op'view|edit)/(?'Type'video|photo)/(?'Id'\\d+)", "/Test2/Display.md?op={Op}&t={Type}&id={Id}");
			server.RegisterVanityResource("/Test/other/(?'Type'video|photo)/(?'Id'\\d+)","/Test/Other.md?t={Type}&id={Id}");
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			server?.Dispose();
			server = null;
		}

		private void Test(string Url, string Result)
		{
			server.CheckVanityResource(ref Url);
			Assert.AreEqual(Result, Url);
		}

		[TestMethod]
		public void Test_01_NoMatch()
		{
			this.Test("/Other", "/Other");
		}

		[TestMethod]
		public void Test_02_Incipient()
		{
			this.Test("/Test", "/Test");
		}

		[TestMethod]
		public void Test_03_Match_1()
		{
			this.Test("/Test/view/photo/123", "/Test/Display.md?op=view&t=photo&id=123");
		}

		[TestMethod]
		public void Test_04_Match_2()
		{
			this.Test("/Test/edit/video/456", "/Test/Display.md?op=edit&t=video&id=456");
		}

		[TestMethod]
		public void Test_05_Match_3()
		{
			this.Test("/Test/print/video/789", "/Test/Output.md?op=print&t=video&id=789");
		}

		[TestMethod]
		public void Test_06_Match_4()
		{
			this.Test("/Test2/edit/photo/234", "/Test2/Display.md?op=edit&t=photo&id=234");
		}

		[TestMethod]
		public void Test_07_Match_5()
		{
			this.Test("/Test/other/photo/345", "/Test/Other.md?t=photo&id=345");
		}

	}
}
