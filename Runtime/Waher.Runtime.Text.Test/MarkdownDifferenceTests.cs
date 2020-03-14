using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownDifferenceTests
	{
		private void Test(string FileName)
		{
			MarkdownSettings Settings = new MarkdownSettings();

			string OldMarkdown = File.ReadAllText("Markdown/Diff/Old/" + FileName);
			string NewMarkdown = File.ReadAllText("Markdown/Diff/New/" + FileName);
			string DiffMarkdown = File.ReadAllText("Markdown/Diff/Diff/" + FileName);

			string Result = MarkdownDocument.Compare(OldMarkdown, NewMarkdown, Settings, true);

			Assert.AreEqual(DiffMarkdown, Result);
		}

		[TestMethod]
		public void Test_01_SimpleText()
		{
			this.Test("Test_01_Simple.md");
		}

		[TestMethod]
		public void Test_02_Paragraphs()
		{
			this.Test("Test_02_Paragraphs.md");
		}
	}
}
