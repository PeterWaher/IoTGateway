using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Runtime.Text.Test
{
	[TestClass]
	public class DifferenceTests
	{
		[TestMethod]
		public void Difference_Test_01_Insert()
		{
			this.TestStrings("HelloWorld", "Hello World", "Hello__ __World");
		}

		[TestMethod]
		public void Difference_Test_02_Delete()
		{
			this.TestStrings("Hello World", "Hello", "Hello~~ World~~");
		}

		[TestMethod]
		public void Difference_Test_03_Replace()
		{
			this.TestStrings("aaaa", "bbbb", "__bbbb__~~aaaa~~");
		}

		[TestMethod]
		public void Difference_Test_04_NoChange()
		{
			this.TestStrings("aaaa", "aaaa", "aaaa");
		}

		[TestMethod]
		public void Difference_Test_05_New()
		{
			this.TestStrings(string.Empty, "aaaa", "__aaaa__");
		}

		[TestMethod]
		public void Difference_Test_06_DeleteAll()
		{
			this.TestStrings("aaaa", string.Empty, "~~aaaa~~");
		}

		[TestMethod]
		public void Difference_Test_07_Empty()
		{
			this.TestStrings(string.Empty, string.Empty, string.Empty);
		}

		[TestMethod]
		public void Difference_Test_08_Diff2_1()
		{
			this.TestStrings("ABCABBA", "CBABA", "~~AB~~C__B__AB~~B~~A");
		}

		[TestMethod]
		public void Difference_Test_09_Rows()
		{
			// Ref: https://en.wikipedia.org/wiki/Diff
			string Org = File.ReadAllText("Data\\Test_09_Org.txt");
			string New = File.ReadAllText("Data\\Test_09_New.txt");
			string Diff = File.ReadAllText("Data\\Test_09_Diff.txt");
			this.TestRows(Org, New, Diff);
		}

		private void TestStrings(string s1, string s2, string Expected)
		{
			EditScript<char> Script = Difference.AnalyzeStrings(s1, s2);
			StringBuilder sb = new StringBuilder();

			foreach (Step<char> Step in Script.Steps)
			{
				switch (Step.Operation)
				{
					case EditOperation.Keep:
						this.Append<char>(sb, Step.Symbols, string.Empty, string.Empty);
						break;

					case EditOperation.Insert:
						sb.Append("__");
						this.Append<char>(sb, Step.Symbols, string.Empty, string.Empty);
						sb.Append("__");
						break;

					case EditOperation.Delete:
						sb.Append("~~");
						this.Append<char>(sb, Step.Symbols, string.Empty, string.Empty);
						sb.Append("~~");
						break;
				}
			}

			string Result = sb.ToString();
			Assert.AreEqual<string>(Expected, Result);
		}

		private void TestRows(string s1, string s2, string Expected)
		{
			EditScript<string> Script = Difference.AnalyzeRows(s1, s2);
			StringBuilder sb = new StringBuilder();

			foreach (Step<string> Step in Script.Steps)
			{
				switch (Step.Operation)
				{
					case EditOperation.Keep:
						this.Append<string>(sb, Step.Symbols, "  ", "\r\n");
						break;

					case EditOperation.Insert:
						this.Append<string>(sb, Step.Symbols, "+ ", "\r\n");
						break;

					case EditOperation.Delete:
						this.Append<string>(sb, Step.Symbols, "- ", "\r\n");
						break;
				}
			}

			string Result = sb.ToString();
			Assert.AreEqual<string>(Expected, Result);
		}

		private void Append<T>(StringBuilder sb, T[] Symbols, string Pre, string Post)
		{
			foreach (T Symbol in Symbols)
			{
				sb.Append(Pre);
				sb.Append(Symbol.ToString());
				sb.Append(Post);
			}
		}

	}
}
