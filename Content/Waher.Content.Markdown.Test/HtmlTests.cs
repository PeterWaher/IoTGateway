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
	public class HtmlTests
	{
		private void DoTest(string MarkdownFileName, string HtmlFileName)
		{
			string Markdown = File.ReadAllText("Markdown/" + MarkdownFileName);
			string ExpectedHtml = File.ReadAllText("HTML/" + HtmlFileName);
			MarkdownSettings Settings = new MarkdownSettings(
				new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%", File.Exists, File.ReadAllBytes),
				true, new Variables())
			{
				HttpxProxy = "/HttpxProxy/%URL%"
			};

			MarkdownDocument Doc = new MarkdownDocument(Markdown, Settings);
			string GeneratedHtml = Doc.GenerateHTML();

			Console.Out.WriteLine(GeneratedHtml);
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine();

			AssertEqual(ExpectedHtml, GeneratedHtml, "Generated HTML does not match expected HTML.");
		}

		public static void AssertEqual(string s1, string s2, string Message)
		{
			int i, c = s1.Length;
			int d = s2.Length;

			if (d < c)
				c = d;

			for (i = 0; i < c; i++)
			{
				if (s1[i] != s2[i])
				{
					throw new Exception(Message + "\r\n\r\nMismatch at position " + i.ToString() +
						"\r\n\r\nGenerated: " + s2.Substring(i, Math.Min(100, s2.Length - i)) +
						"\r\n\r\nExpected: " + s1.Substring(i, Math.Min(100, s1.Length - i)));
				}
			}

			if (s1.Length != s2.Length)
				throw new Exception(Message + "\r\n\r\nUnexpected end: " + s2.Substring(c));
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
	}
}
