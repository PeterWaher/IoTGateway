using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.HTTP.Vanity;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class VanityTests
	{
		private static HttpServer server;
		private static VanityResources resources;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			server = new HttpServer();

			server.RegisterVanityResource("/Test/(?'Op'view|edit)/(?'Type'video|photo)/(?'Id'\\d+)", "/Test/Display.md?op={Op}&t={Type}&id={Id}");
			server.RegisterVanityResource("/Test/(?'Op'print|export)/(?'Type'video|photo)/(?'Id'\\d+)", "/Test/Output.md?op={Op}&t={Type}&id={Id}");
			server.RegisterVanityResource("/Test2/(?'Op'view|edit)/(?'Type'video|photo)/(?'Id'\\d+)", "/Test2/Display.md?op={Op}&t={Type}&id={Id}");
			server.RegisterVanityResource("/Test/other/(?'Type'video|photo)/(?'Id'\\d+)", "/Test/Other.md?t={Type}&id={Id}");

			// Tests taken from MicrosoftInterop repository.

			resources = new VanityResources();

			resources.RegisterVanityResource(@"H(EAD(ING)?)?\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"RUBRIK\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"RUBRIEK\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"ÜBERSCHRIFT\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"OVERSKRIFT\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"TITRE\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"TITOLO\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"TÍTULO\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"ЗАГОЛОВОК\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"OTSIKKO\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"NAGŁÓWEK\s*(?'N'\d+)", "H{N}");
			resources.RegisterVanityResource(@"NADPIS\s*(?'N'\d+)", "H{N}");

			resources.RegisterVanityResource(@"NORMAL", "NORMAL");
			resources.RegisterVanityResource(@"SUBTITLE", "NORMAL");
			resources.RegisterVanityResource(@"BODY TEXT(\s+\d+)", "NORMAL");

			resources.RegisterVanityResource(@"TITLE", "TITLE");
			resources.RegisterVanityResource(@"BOOK\s+TITLE", "TITLE");

			resources.RegisterVanityResource(@"LIST PARAGRAPH", "UL");
			resources.RegisterVanityResource(@"LISTSTYCKE", "UL");

			resources.RegisterVanityResource(@"QUOTE", "QUOTE");
			resources.RegisterVanityResource(@"INTENSE\s+QUOTE", "QUOTE");
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

		[DataTestMethod]
		[DataRow("RUBRIK1", "H1")]
		[DataRow("RUBRIK2", "H2")]
		[DataRow("LISTSTYCKE", "UL")]
		public void Test_08_WordStyles(string Resource, string Expected)
		{
			Assert.IsTrue(resources.CheckVanityResource(ref Resource));
			Assert.AreEqual(Expected, Resource);
		}
	}
}
