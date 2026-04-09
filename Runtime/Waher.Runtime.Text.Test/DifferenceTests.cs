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
			TestStrings("HelloWorld", "Hello World", "Hello__ __World");
		}

		[TestMethod]
		public void Difference_Test_02_Delete()
		{
			TestStrings("Hello World", "Hello", "Hello~~ World~~");
		}

		[TestMethod]
		public void Difference_Test_03_Replace()
		{
			TestStrings("aaaa", "bbbb", "__bbbb__~~aaaa~~");
		}

		[TestMethod]
		public void Difference_Test_04_NoChange()
		{
			TestStrings("aaaa", "aaaa", "aaaa");
		}

		[TestMethod]
		public void Difference_Test_05_New()
		{
			TestStrings(string.Empty, "aaaa", "__aaaa__");
		}

		[TestMethod]
		public void Difference_Test_06_DeleteAll()
		{
			TestStrings("aaaa", string.Empty, "~~aaaa~~");
		}

		[TestMethod]
		public void Difference_Test_07_Empty()
		{
			TestStrings(string.Empty, string.Empty, string.Empty);
		}

		[TestMethod]
		public void Difference_Test_08_Diff2_1()
		{
			TestStrings("ABCABBA", "CBABA", "~~AB~~C__B__A~~B~~BA");
		}

		[TestMethod]
		public void Difference_Test_09_Rows()
		{
			// Ref: https://en.wikipedia.org/wiki/Diff
			string Org = File.ReadAllText("Data\\Test_09_Org.txt");
			string New = File.ReadAllText("Data\\Test_09_New.txt");
			string Diff = File.ReadAllText("Data\\Test_09_Diff.txt");
			TestRows(Org, New, Diff, true);
		}

		[TestMethod]
		public void Difference_Test_10_Rows_Large()
		{
			string Org = File.ReadAllText("Data\\Test_10_Org.txt");
			string New = File.ReadAllText("Data\\Test_10_New.txt");
			string Diff = File.ReadAllText("Data\\Test_10_Diff.txt");
			TestRows(Org, New, Diff, false);
		}

		private static void TestStrings(string s1, string s2, string Expected)
		{
			EditScript<char> Script = Difference.AnalyzeStrings(s1, s2);
			StringBuilder sb = new();

			foreach (Step<char> Step in Script.Steps)
			{
				switch (Step.Operation)
				{
					case EditOperation.Keep:
						Append(sb, Step.Symbols, string.Empty, string.Empty);
						break;

					case EditOperation.Insert:
						sb.Append("__");
						Append(sb, Step.Symbols, string.Empty, string.Empty);
						sb.Append("__");
						break;

					case EditOperation.Delete:
						sb.Append("~~");
						Append(sb, Step.Symbols, string.Empty, string.Empty);
						sb.Append("~~");
						break;
				}
			}

			string Result = sb.ToString();
			Assert.AreEqual(Expected, Result);
		}

		private static void TestRows(string s1, string s2, string Expected, bool ShowKeep)
		{
			EditScript<string> Script = Difference.AnalyzeRows(s1, s2);
			StringBuilder sb = new();

			foreach (Step<string> Step in Script.Steps)
			{
				switch (Step.Operation)
				{
					case EditOperation.Keep:
						if (ShowKeep)
							Append(sb, Step.Symbols, "  ", "\r\n");
						break;

					case EditOperation.Insert:
						Append(sb, Step.Symbols, "+ ", "\r\n");
						break;

					case EditOperation.Delete:
						Append(sb, Step.Symbols, "- ", "\r\n");
						break;
				}
			}

			string Result = sb.ToString();
			Assert.AreEqual(Expected, Result);
		}

		private static void Append<T>(StringBuilder sb, T[] Symbols, string Pre, string Post)
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
