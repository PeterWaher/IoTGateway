using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Runtime.IO;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownDifferenceTests
	{
		private static async Task Test(string FileName)
		{
			MarkdownSettings Settings = new();

			string OldMarkdown = await Files.ReadAllTextAsync("Markdown/Diff/Old/" + FileName);
			string NewMarkdown = await Files.ReadAllTextAsync("Markdown/Diff/New/" + FileName);
			string DiffMarkdown = await Files.ReadAllTextAsync("Markdown/Diff/Diff/" + FileName);

			string Result = await MarkdownDocument.Compare(OldMarkdown, NewMarkdown, Settings, true);

			Assert.AreEqual(DiffMarkdown, Result);
		}

		[TestMethod]
		public async Task Test_01_SimpleText()
		{
			await Test("Test_01_Simple.md");
		}

		[TestMethod]
		public async Task Test_02_Paragraphs()
		{
			await Test("Test_02_Paragraphs.md");
		}

		[TestMethod]
		[Ignore("Being implemented")]
		public async Task Test_03_Bullets()
		{
			await Test("Test_03_Bullets.md");
		}
	}
}
