using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Emoji.Emoji1;
using Waher.Script;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownDifferenceTests
	{
		private void Test(string FileName)
		{
			Emoji1LocalFiles Emoji1LocalFiles = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%", Path.Combine("Graphics", "Emoji1.zip"), "Graphics");
			MarkdownSettings Settings = new MarkdownSettings(Emoji1LocalFiles, true, new Variables())
			{
				HttpxProxy = "/HttpxProxy/%URL%"
			};

			Assert.IsTrue(Emoji1LocalFiles.WaitUntilInitialized(60000));

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
	}
}
