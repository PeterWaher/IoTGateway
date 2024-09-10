using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Xml;
using Waher.Runtime.Console;
using Waher.Script.Xml;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptXmlTests
	{
		private async Task Test(string Script, string ExpectedOutput)
		{
			Variables v = new();
			Expression Exp = new(Script);
			object Result = await Exp.EvaluateAsync(v);

			if (Result is XmlDocument Xml)
				Assert.AreEqual(ExpectedOutput ?? Script, Xml.OuterXml);
			else if (Result is string s)
				Assert.AreEqual(ExpectedOutput ?? Script, s);
			else
				Assert.Fail("XmlDocument or string expected.");
	
			ScriptParsingTests.AssertParentNodesAndSubsexpressions(Exp);

			ConsoleOut.WriteLine();
			Exp.ToXml(ConsoleOut.Writer);
			ConsoleOut.WriteLine();
		}

		[TestMethod]
		public async Task XML_Test_01_Text()
		{
			await this.Test("<a>Hello</a>", null);
		}

		[TestMethod]
		public async Task XML_Test_02_Expression()
		{
			await this.Test("<a><b>1</b><b><[1+2]></b></a>", "<a><b>1</b><b>3</b></a>");
		}

		[TestMethod]
		public async Task XML_Test_03_Attributes()
		{
			await this.Test("<a><b value=\"1\"/><b value=\"2\"/></a>", "<a><b value=\"1\" /><b value=\"2\" /></a>");
		}

		[TestMethod]
		public async Task XML_Test_04_Attributes_Expression()
		{
			await this.Test("x:=7;<a><b value=1/><b value=(2+x)/></a>", "<a><b value=\"1\" /><b value=\"9\" /></a>");
		}

		[TestMethod]
		public async Task XML_Test_05_CDATA()
		{
			await this.Test("<a><![CDATA[Hello World.]]></a>", null);
		}

		[TestMethod]
		public async Task XML_Test_06_Comment()
		{
			await this.Test("<a><!--Hello World.--></a>", null);
		}

		[TestMethod]
		public async Task XML_Test_07_Mixed_1()
		{
			await this.Test("<a>2+3=<[2+3]>.</a>", "<a>2+3=5.</a>");
		}

		[TestMethod]
		public async Task XML_Test_08_Mixed_2()
		{
			await this.Test("<a>2+3=<(2+3)>.</a>", "<a>2+3=5.</a>");
		}

		[TestMethod]
		public async Task XML_Test_09_XML_Declaration()
		{
			await this.Test("<?xml version=\"1.0\" encoding=\"UTF-8\"?><a>Hello</a>", null);
		}

		[TestMethod]
		public async Task XML_Test_10_ProcessingInstruction()
		{
			await this.Test("<?xml version=\"1.0\" encoding=\"UTF-8\"?><?xml-stylesheet type=\"text/xsl\" href=\"style.xsl\"?><a>Hello</a>", null);
		}

		[TestMethod]
		public async Task XML_Test_11_DefaultNamespace()
		{
			await this.Test("x:=<a xmlns=\"test\"><b /></a>;x.DocumentElement.NamespaceURI", "test");
		}

	}
}