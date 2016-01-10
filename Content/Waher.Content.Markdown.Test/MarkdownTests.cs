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

			Console.Out.WriteLine(GeneratedHtml);
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine();

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

		[Test]
		public void Test_05_HTML()
		{
			this.DoTest("Test_05_HTML.md", "Test_05_HTML.html");
		}

		[Test]
		public void Test_06_CodeBlocks()
		{
			this.DoTest("Test_06_CodeBlocks.md", "Test_06_CodeBlocks.html");
		}

		[Test]
		public void Test_07_BlockQuotes()
		{
			this.DoTest("Test_07_BlockQuotes.md", "Test_07_BlockQuotes.html");
		}

		[Test]
		public void Test_08_Headers()
		{
			this.DoTest("Test_08_Headers.md", "Test_08_Headers.html");
		}

		[Test]
		public void Test_09_UnorderedLists()
		{
			this.DoTest("Test_09_UnorderedLists.md", "Test_09_UnorderedLists.html");
		}

		[Test]
		public void Test_10_LazyOrderedLists()
		{
			this.DoTest("Test_10_LazyOrderedLists.md", "Test_10_LazyOrderedLists.html");
		}

		[Test]
		public void Test_11_OrderedLists()
		{
			this.DoTest("Test_11_OrderedLists.md", "Test_11_OrderedLists.html");
		}
		
		[Test]
		public void Test_12_Typography()
		{
			this.DoTest("Test_12_Typography.md", "Test_12_Typography.html");
		}
		
		[Test]
		public void Test_13_Tables()
		{
			this.DoTest("Test_13_Tables.md", "Test_13_Tables.html");
		}
		
		[Test]
		public void Test_14_HorizontalRules()
		{
			this.DoTest("Test_14_HorizontalRules.md", "Test_14_HorizontalRules.html");
		}

		[Test]
		public void Test_15_DefinitionLists()
		{
			this.DoTest("Test_15_DefinitionLists.md", "Test_15_DefinitionLists.html");
		}
	}
}
