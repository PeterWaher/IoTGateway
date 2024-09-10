using JSTest;
using JSTest.ScriptLibraries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Content.Emoji.Emoji1;
using Waher.Content.Markdown.JavaScript;
using Waher.Runtime.Console;
using Waher.Script;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownJavaScriptTests
	{
		private static Task DoTest(string MarkdownFileName, string JavaScriptFileName)
		{
			return DoTest(MarkdownFileName, JavaScriptFileName, true);
		}

		private static Task DoTest(string MarkdownFileName, string JavaScriptFileName, bool ExecuteJavaScript)
		{
			return DoTest(MarkdownFileName, JavaScriptFileName, ExecuteJavaScript, "<body>", "</body>");
		}

		private static async Task DoTest(string MarkdownFileName, string JavaScriptFileName, bool ExecuteJavaScript,
			string HtmlStartTag, string HtmlEndTag)
		{
			string Markdown = await Resources.ReadAllTextAsync("Markdown/Syntax/" + MarkdownFileName);
			string ExpectedJavaScript = await Resources.ReadAllTextAsync("JavaScript/" + JavaScriptFileName);
			string ExpectedHtml = await Resources.ReadAllTextAsync("HTML/" + Path.ChangeExtension(JavaScriptFileName, ".html"));
			Emoji1LocalFiles Emoji1LocalFiles = new(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%", Path.Combine("Graphics", "Emoji1.zip"), "Graphics");

			MarkdownSettings Settings = new(Emoji1LocalFiles, true, new Variables())
			{
				HttpxProxy = "/HttpxProxy/%URL%"
			};

			Assert.IsTrue(Emoji1LocalFiles.WaitUntilInitialized(60000));

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
			string GeneratedJavaScript = await Doc.GenerateJavaScript();

			ConsoleOut.WriteLine(GeneratedJavaScript);
			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			ExpectedJavaScript = ExpectedJavaScript.Replace("\\r\\n", "\\n").Replace("\\r", "\\n");
			GeneratedJavaScript = GeneratedJavaScript.Replace("\\r\\n", "\\n").Replace("\\r", "\\n");

			MarkdownHtmlTests.AssertEqual(ExpectedJavaScript, GeneratedJavaScript, "Generated JavaScript does not match expected JavaScript.");

			if (ExecuteJavaScript)
			{
				// Ref: https://github.com/cbaxter/JSTest.NET/wiki

				TestScript Parsed = new();
				Parsed.AppendBlock(new JsAssertLibrary());

				string JsFileName = Path.ChangeExtension(Path.GetTempFileName(), ".js");
				await Resources.WriteAllTextAsync(JsFileName, GeneratedJavaScript, System.Text.Encoding.UTF8);
				Parsed.AppendFile(JsFileName);

				string JsonEncoded = Parsed.RunTest("return CreateHTML();");

				ConsoleOut.WriteLine("JSON Returned after executing JavaScript:");
				ConsoleOut.WriteLine();
				ConsoleOut.WriteLine(JsonEncoded);
				ConsoleOut.WriteLine();

				object Decoded = JSON.Parse(JsonEncoded);
				if (Decoded is not string GeneratedHtml)
					throw new Exception("Expected a string result returned from the JavaScript");

				ConsoleOut.WriteLine("Generated HTML by executing JavaScript:");
				ConsoleOut.WriteLine();
				ConsoleOut.WriteLine(GeneratedHtml);

				int i = ExpectedHtml.IndexOf(HtmlStartTag);
				if (i > 0)
					ExpectedHtml = ExpectedHtml[(i + HtmlStartTag.Length)..].TrimStart();

				i = ExpectedHtml.IndexOf(HtmlEndTag);
				if (i > 0)
					ExpectedHtml = ExpectedHtml[..i];

				MarkdownHtmlTests.AssertEqual(ExpectedHtml, GeneratedHtml, "Generated HTML does not match expected HTML.");
			}
		}

		[TestMethod]
		public async Task Test_01_Paragraphs()
		{
			await DoTest("Test_01_Paragraphs.md", "Test_01_Paragraphs.js");
		}

		[TestMethod]
		public async Task Test_02_Links()
		{
			await DoTest("Test_02_Links.md", "Test_02_Links.js");
		}

		[TestMethod]
		public async Task Test_03_TextFormatting()
		{
			await DoTest("Test_03_TextFormatting.md", "Test_03_TextFormatting.js");
		}

		[TestMethod]
		public async Task Test_04_Multimedia()
		{
			await DoTest("Test_04_Multimedia.md", "Test_04_Multimedia.js");
		}

		[TestMethod]
		public async Task Test_05_HTML()
		{
			await DoTest("Test_05_HTML.md", "Test_05_HTML.js");
		}

		[TestMethod]
		public async Task Test_06_CodeBlocks()
		{
			await DoTest("Test_06_CodeBlocks.md", "Test_06_CodeBlocks.js");
		}

		[TestMethod]
		public async Task Test_07_BlockQuotes()
		{
			await DoTest("Test_07_BlockQuotes.md", "Test_07_BlockQuotes.js");
		}

		[TestMethod]
		public async Task Test_08_Headers()
		{
			await DoTest("Test_08_Headers.md", "Test_08_Headers.js");
		}

		[TestMethod]
		public async Task Test_09_UnorderedLists()
		{
			await DoTest("Test_09_UnorderedLists.md", "Test_09_UnorderedLists.js");
		}

		[TestMethod]
		public async Task Test_10_LazyOrderedLists()
		{
			await DoTest("Test_10_LazyOrderedLists.md", "Test_10_LazyOrderedLists.js");
		}

		[TestMethod]
		public async Task Test_11_OrderedLists()
		{
			await DoTest("Test_11_OrderedLists.md", "Test_11_OrderedLists.js");
		}

		[TestMethod]
		public async Task Test_12_Typography()
		{
			await DoTest("Test_12_Typography.md", "Test_12_Typography.js");
		}

		[TestMethod]
		public async Task Test_13_Tables()
		{
			await DoTest("Test_13_Tables.md", "Test_13_Tables.js");
		}

		[TestMethod]
		public async Task Test_14_HorizontalRules()
		{
			await DoTest("Test_14_HorizontalRules.md", "Test_14_HorizontalRules.js");
		}

		[TestMethod]
		public async Task Test_15_DefinitionLists()
		{
			await DoTest("Test_15_DefinitionLists.md", "Test_15_DefinitionLists.js");
		}

		[TestMethod]
		public async Task Test_16_MetaData()
		{
			await DoTest("Test_16_MetaData.md", "Test_16_MetaData.js");
		}

		[TestMethod]
		public async Task Test_17_Footnotes()
		{
			await DoTest("Test_17_Footnotes.md", "Test_17_Footnotes.js");
		}

		[TestMethod]
		public async Task Test_18_Emojis()
		{
			await DoTest("Test_18_Emojis.md", "Test_18_Emojis.js");
		}

		[TestMethod]
		public async Task Test_19_Sections()
		{
			await DoTest("Test_19_Sections.md", "Test_19_Sections.js");
		}

		[TestMethod]
		public async Task Test_20_Script()
		{
			await DoTest("Test_20_Script.md", "Test_20_Script.js", false);
		}

		[TestMethod]
		public async Task Test_21_Httpx()
		{
			await DoTest("Test_21_Httpx.md", "Test_21_Httpx.js");
		}

		[TestMethod]
		public async Task Test_22_TaskLists()
		{
			await DoTest("Test_22_TaskLists.md", "Test_22_TaskLists.js");
		}

		[TestMethod]
		public async Task Test_23_Superscript()
		{
			await DoTest("Test_23_Superscript.md", "Test_23_Superscript.js");
		}

		[TestMethod]
		public async Task Test_24_Subscript()
		{
			await DoTest("Test_24_Subscript.md", "Test_24_Subscript.js");
		}

		[TestMethod]
		public async Task Test_25_HashTags()
		{
			await DoTest("Test_25_HashTags.md", "Test_25_HashTags.js");
		}

		[TestMethod]
		public async Task Test_26_Comments()
		{
			await DoTest("Test_26_Comments.md", "Test_26_Comments.js");
		}

		[TestMethod]
		public async Task Test_27_Contract()
		{
			await DoTest("Test_27_Contract.md", "Test_27_Contract.js");
		}

		[TestMethod]
		public async Task Test_28_Nesting()
		{
			await DoTest("Test_28_Nesting.md", "Test_28_Nesting.js");
		}

		[TestMethod]
		public async Task Test_29_Justification()
		{
			await DoTest("Test_29_Justification.md", "Test_29_Justification.js");
		}

		[TestMethod]
		public async Task Test_30_Incomplete()
		{
			await DoTest("Test_30_Incomplete.md", "Test_30_Incomplete.js");
		}

		[TestMethod]
		public async Task Test_31_Justification2()
		{
			await DoTest("Test_31_Justification2.md", "Test_31_Justification2.js");
		}

		[TestMethod]
		public async Task Test_32_TablesAndNotes()
		{
			await DoTest("Test_32_TablesAndNotes.md", "Test_32_TablesAndNotes.js");
		}

		[TestMethod]
		public async Task Test_33_SingleNoHeaderTable()
		{
			await DoTest("Test_33_SingleNoHeaderTable.md", "Test_33_SingleNoHeaderTable.js", true, "<tbody>", "</tbody>");
		}
	}
}
