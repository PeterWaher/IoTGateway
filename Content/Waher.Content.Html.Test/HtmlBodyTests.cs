using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Waher.Runtime.IO;

namespace Waher.Content.Html.Test
{
	[TestClass]
	public class HtmlBodyTests
	{
		[TestMethod]
		public async Task Test_01_Simple_Body()
		{
			string Html = await Files.ReadAllTextAsync("Data\\Simple.html");
			string Body = HtmlDocument.GetBody(Html);
			string ExpectedBody = await Files.ReadAllTextAsync("Data\\SimpleBody.txt");
		
			Assert.AreEqual(ExpectedBody, Body);
		}

		[TestMethod]
		public async Task Test_02_SimpleWithAttributes_Body()
		{
			string Html = await Files.ReadAllTextAsync("Data\\SimpleWithAttributes.html");
			string Body = HtmlDocument.GetBody(Html);
			string ExpectedBody = await Files.ReadAllTextAsync("Data\\SimpleBody.txt");

			Assert.AreEqual(ExpectedBody, Body);
		}

		[TestMethod]
		public async Task Test_03_Simple_Head()
		{
			string Html = await Files.ReadAllTextAsync("Data\\Simple.html");
			string Head = HtmlDocument.GetHead(Html);
			string ExpectedHead = await Files.ReadAllTextAsync("Data\\SimpleHead.txt");

			Assert.AreEqual(ExpectedHead, Head);
		}

		[TestMethod]
		public async Task Test_04_SimpleWithAttributes_Head()
		{
			string Html = await Files.ReadAllTextAsync("Data\\SimpleWithAttributes.html");
			string Head = HtmlDocument.GetHead(Html);
			string ExpectedHead = await Files.ReadAllTextAsync("Data\\SimpleHead.txt");

			Assert.AreEqual(ExpectedHead, Head);
		}
	}
}
