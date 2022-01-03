using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownDifferenceTests
	{
		private async Task Test(string FileName)
		{
			MarkdownSettings Settings = new MarkdownSettings();

			string OldMarkdown = await Resources.ReadAllTextAsync("Markdown/Diff/Old/" + FileName);
			string NewMarkdown = await Resources.ReadAllTextAsync("Markdown/Diff/New/" + FileName);
			string DiffMarkdown = await Resources.ReadAllTextAsync("Markdown/Diff/Diff/" + FileName);

			string Result = await MarkdownDocument.Compare(OldMarkdown, NewMarkdown, Settings, true);

			Assert.AreEqual(DiffMarkdown, Result);
		}

		[TestMethod]
		public async Task Test_01_SimpleText()
		{
			await this.Test("Test_01_Simple.md");
		}

		[TestMethod]
		public async Task Test_02_Paragraphs()
		{
			await this.Test("Test_02_Paragraphs.md");
		}

		[TestMethod]
		[Ignore("Being implemented")]
		public async Task Test_03_Bullets()
		{
			await this.Test("Test_03_Bullets.md");
		}
	}
}
