using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Waher.Content.Markdown.Test
{
	[TestFixture]
	public class MarkdownTests
	{
		private void DoTest(string MarkdownFileName, string HtmlFileName)
		{
			string Markdown = File.ReadAllText("Markdown/" + MarkdownFileName);
			string ExpectedHtml = File.ReadAllText("HTML/" + HtmlFileName);
			MarkdownDocument Doc = new MarkdownDocument(Markdown);
			string GeneratedHtml = Doc.GenerateHTML();

			Assert.AreEqual(ExpectedHtml, GeneratedHtml, "Generated HTML does not match expected HTML.");
		}

		[Test]
		public void Test_01_Paragraphs()
		{
			this.DoTest("Test_01_Paragraphs.md", "Test_01_Paragraphs.html");
		}

		[Test]
		public void Test_02_Links()
		{
			this.DoTest("Test_02_Links.md", "Test_02_Links.html");
		}

		[Test]
		public void Test_03_TextFormatting()
		{
			this.DoTest("Test_03_TextFormatting.md", "Test_03_TextFormatting.html");
		}

		[Test]
		public void Test_04_Multimedia()
		{
			this.DoTest("Test_04_Multimedia.md", "Test_04_Multimedia.html");
		}
	}
}
