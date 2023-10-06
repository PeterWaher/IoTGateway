using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Emoji.Emoji1;
using Waher.Runtime.Text;
using Waher.Script;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Waher.Script.Operators.Membership;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownHtmlTests
	{
		private async Task DoTest(string MarkdownFileName, string HtmlFileName)
		{
			string Markdown = await Resources.ReadAllTextAsync("Markdown/Syntax/" + MarkdownFileName);
			string ExpectedHtml = await Resources.ReadAllTextAsync("HTML/" + HtmlFileName);
			Emoji1LocalFiles Emoji1LocalFiles = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%", Path.Combine("Graphics", "Emoji1.zip"), "Graphics");

			MarkdownSettings Settings = new MarkdownSettings(Emoji1LocalFiles, true, new Variables())
			{
				HttpxProxy = "/HttpxProxy/%URL%"
			};

			Assert.IsTrue(Emoji1LocalFiles.WaitUntilInitialized(60000));

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
			string GeneratedHtml = await Doc.GenerateHTML();

			Console.Out.WriteLine(GeneratedHtml);
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine();

			AssertEqual(ExpectedHtml, GeneratedHtml, "Generated HTML does not match expected HTML.");
		}

		public static void AssertEqual(string Expected, string Generated, string Message)
		{
			Generated = RemoveGuids(Generated);
			Expected = RemoveGuids(Expected);

			if (Generated != Expected)
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine(Message);
				sb.AppendLine();
				sb.AppendLine("Changes required:");
				sb.AppendLine();

				EditScript<string> Script = Difference.AnalyzeRows(Expected, Generated);

				foreach (Step<string> Op in Script.Steps)
				{
					foreach (string Row in Op.Symbols)
					{
						switch (Op.Operation)
						{
							case EditOperation.Delete:
								sb.Append("- ");
								sb.AppendLine(Row);
								break;

							case EditOperation.Insert:
								sb.Append("+ ");
								sb.AppendLine(Row);
								break;
						}
					}
				}
			
				throw new Exception(sb.ToString());
			}
		}

		private static string RemoveGuids(string s)
		{
			string s2;
			int i = 0;

			foreach (Match M in guids.Matches(s))
			{
				s2 = "GUID" + (++i).ToString();
				s2 += new string('_', 36 - s2.Length);

				s = s.Remove(M.Index, 36).Insert(M.Index, s2);
			}

			return s;
		}

		private readonly static Regex guids = new Regex(@"[0-9a-fA-F]{8}(-[0-9a-fA-F]{4}){4}[0-9a-fA-F]{8}", RegexOptions.Multiline | RegexOptions.Compiled);

		[TestMethod]
		public async Task Test_01_Paragraphs()
		{
			await this.DoTest("Test_01_Paragraphs.md", "Test_01_Paragraphs.html");
		}

		[TestMethod]
		public async Task Test_02_Links()
		{
			await this.DoTest("Test_02_Links.md", "Test_02_Links.html");
		}

		[TestMethod]
		public async Task Test_03_TextFormatting()
		{
			await this.DoTest("Test_03_TextFormatting.md", "Test_03_TextFormatting.html");
		}

		[TestMethod]
		public async Task Test_04_Multimedia()
		{
			await this.DoTest("Test_04_Multimedia.md", "Test_04_Multimedia.html");
		}

		[TestMethod]
		public async Task Test_05_HTML()
		{
			await this.DoTest("Test_05_HTML.md", "Test_05_HTML.html");
		}

		[TestMethod]
		public async Task Test_06_CodeBlocks()
		{
			await this.DoTest("Test_06_CodeBlocks.md", "Test_06_CodeBlocks.html");
		}

		[TestMethod]
		public async Task Test_07_BlockQuotes()
		{
			await this.DoTest("Test_07_BlockQuotes.md", "Test_07_BlockQuotes.html");
		}

		[TestMethod]
		public async Task Test_08_Headers()
		{
			await this.DoTest("Test_08_Headers.md", "Test_08_Headers.html");
		}

		[TestMethod]
		public async Task Test_09_UnorderedLists()
		{
			await this.DoTest("Test_09_UnorderedLists.md", "Test_09_UnorderedLists.html");
		}

		[TestMethod]
		public async Task Test_10_LazyOrderedLists()
		{
			await this.DoTest("Test_10_LazyOrderedLists.md", "Test_10_LazyOrderedLists.html");
		}

		[TestMethod]
		public async Task Test_11_OrderedLists()
		{
			await this.DoTest("Test_11_OrderedLists.md", "Test_11_OrderedLists.html");
		}

		[TestMethod]
		public async Task Test_12_Typography()
		{
			await this.DoTest("Test_12_Typography.md", "Test_12_Typography.html");
		}

		[TestMethod]
		public async Task Test_13_Tables()
		{
			await this.DoTest("Test_13_Tables.md", "Test_13_Tables.html");
		}

		[TestMethod]
		public async Task Test_14_HorizontalRules()
		{
			await this.DoTest("Test_14_HorizontalRules.md", "Test_14_HorizontalRules.html");
		}

		[TestMethod]
		public async Task Test_15_DefinitionLists()
		{
			await this.DoTest("Test_15_DefinitionLists.md", "Test_15_DefinitionLists.html");
		}

		[TestMethod]
		public async Task Test_16_MetaData()
		{
			await this.DoTest("Test_16_MetaData.md", "Test_16_MetaData.html");
		}

		[TestMethod]
		public async Task Test_17_Footnotes()
		{
			await this.DoTest("Test_17_Footnotes.md", "Test_17_Footnotes.html");
		}

		[TestMethod]
		public async Task Test_18_Emojis()
		{
			await this.DoTest("Test_18_Emojis.md", "Test_18_Emojis.html");
		}

		[TestMethod]
		public async Task Test_19_Sections()
		{
			await this.DoTest("Test_19_Sections.md", "Test_19_Sections.html");
		}

		[TestMethod]
		public async Task Test_20_Script()
		{
			await this.DoTest("Test_20_Script.md", "Test_20_Script.html");
		}

		[TestMethod]
		public async Task Test_21_Httpx()
		{
			await this.DoTest("Test_21_Httpx.md", "Test_21_Httpx.html");
		}

		[TestMethod]
		public async Task Test_22_TaskLists()
		{
			await this.DoTest("Test_22_TaskLists.md", "Test_22_TaskLists.html");
		}

		[TestMethod]
		public async Task Test_23_Superscript()
		{
			await this.DoTest("Test_23_Superscript.md", "Test_23_Superscript.html");
		}

		[TestMethod]
		public async Task Test_24_Subscript()
		{
			await this.DoTest("Test_24_Subscript.md", "Test_24_Subscript.html");
		}

		[TestMethod]
		public async Task Test_25_HashTags()
		{
			await this.DoTest("Test_25_HashTags.md", "Test_25_HashTags.html");
		}

		[TestMethod]
		public async Task Test_26_Comments()
		{
			await this.DoTest("Test_26_Comments.md", "Test_26_Comments.html");
		}

		[TestMethod]
		public async Task Test_27_Contract()
		{
			await this.DoTest("Test_27_Contract.md", "Test_27_Contract.html");
		}

		[TestMethod]
		public async Task Test_28_Nesting()
		{
			await this.DoTest("Test_28_Nesting.md", "Test_28_Nesting.html");
		}

		[TestMethod]
		public async Task Test_29_Justification()
		{
			await this.DoTest("Test_29_Justification.md", "Test_29_Justification.html");
		}

		[TestMethod]
		public async Task Test_30_Incomplete()
		{
			await this.DoTest("Test_30_Incomplete.md", "Test_30_Incomplete.html");
		}

		[TestMethod]
		public async Task Test_31_Justification2()
		{
			await this.DoTest("Test_31_Justification2.md", "Test_31_Justification2.html");
		}

		[TestMethod]
		public async Task Test_32_TablesAndNotes()
		{
			await this.DoTest("Test_32_TablesAndNotes.md", "Test_32_TablesAndNotes.html");
		}
	}
}
