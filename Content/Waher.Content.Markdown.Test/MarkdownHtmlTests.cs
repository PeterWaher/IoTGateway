using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Emoji.Emoji1;
using Waher.Script;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownHtmlTests
	{
		private void DoTest(string MarkdownFileName, string HtmlFileName)
		{
			string Markdown = File.ReadAllText("Markdown/Syntax/" + MarkdownFileName);
			string ExpectedHtml = File.ReadAllText("HTML/" + HtmlFileName);
			Emoji1LocalFiles Emoji1LocalFiles = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%", Path.Combine("Graphics", "Emoji1.zip"), "Graphics");

			MarkdownSettings Settings = new MarkdownSettings(Emoji1LocalFiles, true, new Variables())
			{
				HttpxProxy = "/HttpxProxy/%URL%"
			};

			Assert.IsTrue(Emoji1LocalFiles.WaitUntilInitialized(60000));

			MarkdownDocument Doc = new MarkdownDocument(Markdown, Settings);
			string GeneratedHtml = Doc.GenerateHTML();

			Console.Out.WriteLine(GeneratedHtml);
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine();

			AssertEqual(ExpectedHtml, GeneratedHtml, "Generated HTML does not match expected HTML.");
		}

		public static void AssertEqual(string Expected, string Generated, string Message)
		{
			int i, c = Expected.Length;
			int d = Generated.Length;

			if (d < c)
				c = d;

			for (i = 0; i < c; i++)
			{
				if (Expected[i] != Generated[i])
				{
					throw new Exception(Message + "\r\n\r\nMismatch at position " + i.ToString() +
						"\r\n\r\nGenerated: " + Generated.Substring(i, Math.Min(100, Generated.Length - i)) +
						"\r\n\r\nExpected: " + Expected.Substring(i, Math.Min(100, Expected.Length - i)));
				}
			}

			if (Expected.Length != Generated.Length)
				throw new Exception(Message + "\r\n\r\nUnexpected end: " + Generated.Substring(c));
		}

		[TestMethod]
		public void Test_01_Paragraphs()
		{
			this.DoTest("Test_01_Paragraphs.md", "Test_01_Paragraphs.html");
		}

		[TestMethod]
		public void Test_02_Links()
		{
			this.DoTest("Test_02_Links.md", "Test_02_Links.html");
		}

		[TestMethod]
		public void Test_03_TextFormatting()
		{
			this.DoTest("Test_03_TextFormatting.md", "Test_03_TextFormatting.html");
		}

		[TestMethod]
		public void Test_04_Multimedia()
		{
			this.DoTest("Test_04_Multimedia.md", "Test_04_Multimedia.html");
		}

		[TestMethod]
		public void Test_05_HTML()
		{
			this.DoTest("Test_05_HTML.md", "Test_05_HTML.html");
		}

		[TestMethod]
		public void Test_06_CodeBlocks()
		{
			this.DoTest("Test_06_CodeBlocks.md", "Test_06_CodeBlocks.html");
		}

		[TestMethod]
		public void Test_07_BlockQuotes()
		{
			this.DoTest("Test_07_BlockQuotes.md", "Test_07_BlockQuotes.html");
		}

		[TestMethod]
		public void Test_08_Headers()
		{
			this.DoTest("Test_08_Headers.md", "Test_08_Headers.html");
		}

		[TestMethod]
		public void Test_09_UnorderedLists()
		{
			this.DoTest("Test_09_UnorderedLists.md", "Test_09_UnorderedLists.html");
		}

		[TestMethod]
		public void Test_10_LazyOrderedLists()
		{
			this.DoTest("Test_10_LazyOrderedLists.md", "Test_10_LazyOrderedLists.html");
		}

		[TestMethod]
		public void Test_11_OrderedLists()
		{
			this.DoTest("Test_11_OrderedLists.md", "Test_11_OrderedLists.html");
		}

		[TestMethod]
		public void Test_12_Typography()
		{
			this.DoTest("Test_12_Typography.md", "Test_12_Typography.html");
		}

		[TestMethod]
		public void Test_13_Tables()
		{
			this.DoTest("Test_13_Tables.md", "Test_13_Tables.html");
		}

		[TestMethod]
		public void Test_14_HorizontalRules()
		{
			this.DoTest("Test_14_HorizontalRules.md", "Test_14_HorizontalRules.html");
		}

		[TestMethod]
		public void Test_15_DefinitionLists()
		{
			this.DoTest("Test_15_DefinitionLists.md", "Test_15_DefinitionLists.html");
		}

		[TestMethod]
		public void Test_16_MetaData()
		{
			this.DoTest("Test_16_MetaData.md", "Test_16_MetaData.html");
		}

		[TestMethod]
		public void Test_17_Footnotes()
		{
			this.DoTest("Test_17_Footnotes.md", "Test_17_Footnotes.html");
		}

		[TestMethod]
		public void Test_18_Emojis()
		{
			this.DoTest("Test_18_Emojis.md", "Test_18_Emojis.html");
		}

		[TestMethod]
		public void Test_19_Sections()
		{
			this.DoTest("Test_19_Sections.md", "Test_19_Sections.html");
		}

		[TestMethod]
		public void Test_20_Script()
		{
			this.DoTest("Test_20_Script.md", "Test_20_Script.html");
		}

		[TestMethod]
		public void Test_21_Httpx()
		{
			this.DoTest("Test_21_Httpx.md", "Test_21_Httpx.html");
		}

		[TestMethod]
		public void Test_22_TaskLists()
		{
			this.DoTest("Test_22_TaskLists.md", "Test_22_TaskLists.html");
		}

		[TestMethod]
		public void Test_23_Superscript()
		{
			this.DoTest("Test_23_Superscript.md", "Test_23_Superscript.html");
		}

		[TestMethod]
		public void Test_24_Subscript()
		{
			this.DoTest("Test_24_Subscript.md", "Test_24_Subscript.html");
		}
	}
}
