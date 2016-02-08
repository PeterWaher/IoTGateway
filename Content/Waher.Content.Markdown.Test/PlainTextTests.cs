using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Content.Emoji.Emoji1;

namespace Waher.Content.Markdown.Test
{
	[TestFixture]
	public class PlainTextTests
	{
		private void DoTest(string MarkdownFileName, string PlainTextFileName)
		{
			string Markdown = File.ReadAllText("Markdown/" + MarkdownFileName);
			string ExpectedText = File.ReadAllText("PlainText/" + PlainTextFileName);
			//MarkdownDocument Doc = new MarkdownDocument(Markdown, new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24));
			MarkdownDocument Doc = new MarkdownDocument(Markdown, new MarkdownSettings(
				new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%"), true));
			string GeneratedText = Doc.GeneratePlainText();

			Console.Out.WriteLine(GeneratedText);
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine();

			Assert.AreEqual(ExpectedText, GeneratedText, "Generated Plain Text does not match expected Plain Text.");
		}

		[Test]
		public void Test_01_Paragraphs()
		{
			this.DoTest("Test_01_Paragraphs.md", "Test_01_Paragraphs.txt");
		}

		[Test]
		public void Test_02_Links()
		{
			this.DoTest("Test_02_Links.md", "Test_02_Links.txt");
		}

		[Test]
		public void Test_03_TextFormatting()
		{
			this.DoTest("Test_03_TextFormatting.md", "Test_03_TextFormatting.txt");
		}

		[Test]
		public void Test_04_Multimedia()
		{
			this.DoTest("Test_04_Multimedia.md", "Test_04_Multimedia.txt");
		}

		[Test]
		public void Test_05_HTML()
		{
			this.DoTest("Test_05_HTML.md", "Test_05_HTML.txt");
		}

		[Test]
		public void Test_06_CodeBlocks()
		{
			this.DoTest("Test_06_CodeBlocks.md", "Test_06_CodeBlocks.txt");
		}

		[Test]
		public void Test_07_BlockQuotes()
		{
			this.DoTest("Test_07_BlockQuotes.md", "Test_07_BlockQuotes.txt");
		}

		[Test]
		public void Test_08_Headers()
		{
			this.DoTest("Test_08_Headers.md", "Test_08_Headers.txt");
		}

		[Test]
		public void Test_09_UnorderedLists()
		{
			this.DoTest("Test_09_UnorderedLists.md", "Test_09_UnorderedLists.txt");
		}

		[Test]
		public void Test_10_LazyOrderedLists()
		{
			this.DoTest("Test_10_LazyOrderedLists.md", "Test_10_LazyOrderedLists.txt");
		}

		[Test]
		public void Test_11_OrderedLists()
		{
			this.DoTest("Test_11_OrderedLists.md", "Test_11_OrderedLists.txt");
		}
		
		[Test]
		public void Test_12_Typography()
		{
			this.DoTest("Test_12_Typography.md", "Test_12_Typography.txt");
		}
		
		[Test]
		public void Test_13_Tables()
		{
			this.DoTest("Test_13_Tables.md", "Test_13_Tables.txt");
		}
		
		[Test]
		public void Test_14_HorizontalRules()
		{
			this.DoTest("Test_14_HorizontalRules.md", "Test_14_HorizontalRules.txt");
		}

		[Test]
		public void Test_15_DefinitionLists()
		{
			this.DoTest("Test_15_DefinitionLists.md", "Test_15_DefinitionLists.txt");
		}

		[Test]
		public void Test_16_MetaData()
		{
			this.DoTest("Test_16_MetaData.md", "Test_16_MetaData.txt");
		}

		[Test]
		public void Test_17_Footnotes()
		{
			this.DoTest("Test_17_Footnotes.md", "Test_17_Footnotes.txt");
		}

		[Test]
		public void Test_18_Emojis()
		{
			this.DoTest("Test_18_Emojis.md", "Test_18_Emojis.txt");
		}

		[Test]
		public void Test_19_Sections()
		{
			this.DoTest("Test_19_Sections.md", "Test_19_Sections.txt");
		}
	}
}
