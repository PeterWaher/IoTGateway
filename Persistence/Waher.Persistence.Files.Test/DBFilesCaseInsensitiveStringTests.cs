using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.Files.Searching;

#if !LW
namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public class DBFilesCaseInsensitiveStringTests
	{
		[TestMethod]
		public void DBFiles_CIString_Test_01_Equality()
		{
			CaseInsensitiveString S = "Hello";

			Assert.IsTrue(S == "hello");
			Assert.IsTrue(S == "Hello");
			Assert.IsTrue(S == "HELLO");
			Assert.AreEqual(S, "hello");
			Assert.AreEqual(S, "Hello");
			Assert.AreEqual(S, "HELLO");
		}

		[TestMethod]
		public void DBFiles_CIString_Test_02_NonEquality()
		{
			CaseInsensitiveString S = "Hello";

			Assert.IsFalse(S != "hello");
			Assert.IsFalse(S != "Hello");
			Assert.IsFalse(S != "HELLO");
		}

		[TestMethod]
		public void DBFiles_CIString_Test_03_Less()
		{
			CaseInsensitiveString S = "ABC";

			Assert.IsFalse(S < "a");
			Assert.IsTrue(S < "b");
		}

		[TestMethod]
		public void DBFiles_CIString_Test_04_Greater()
		{
			CaseInsensitiveString S = "ABC";

			Assert.IsTrue(S > "a");
			Assert.IsFalse(S > "b");
		}

		[TestMethod]
		public void DBFiles_CIString_Test_05_LessOrEqual()
		{
			CaseInsensitiveString S = "ABC";

			Assert.IsFalse(S <= "a");
			Assert.IsTrue(S <= "b");
			Assert.IsTrue(S <= "abc");
			Assert.IsTrue(S <= "Abc");
			Assert.IsTrue(S <= "ABC");
		}

		[TestMethod]
		public void DBFiles_CIString_Test_06_GreaterOrEqual()
		{
			CaseInsensitiveString S = "ABC";

			Assert.IsTrue(S >= "a");
			Assert.IsFalse(S >= "b");
			Assert.IsTrue(S >= "abc");
			Assert.IsTrue(S >= "Abc");
			Assert.IsTrue(S >= "ABC");
		}

		[TestMethod]
		public void DBFiles_CIString_Test_07_IndexOfAny()
		{
			CaseInsensitiveString S = "Hello World!";

			Assert.AreEqual(1, S.IndexOfAny(new CaseInsensitiveString[] { "LO", "EL" }));
			Assert.AreEqual(1, S.IndexOfAny(new CaseInsensitiveString[] { "EL", "LO" }));
		}

		[TestMethod]
		public void DBFiles_CIString_Test_08_Replace()
		{
			CaseInsensitiveString S = "Hello World!";

			Assert.IsTrue("HeABCABCo WorABCd!" == S.Replace("L", "ABC"));
		}

		[TestMethod]
		public void DBFiles_CIString_Test_09_Split()
		{
			CaseInsensitiveString S = "Hello World!";

			CaseInsensitiveString[] Expected = new CaseInsensitiveString[] { "HE", " W", "R", "D!" };
			CaseInsensitiveString[] Actual = S.Split(new CaseInsensitiveString[] { "L", "O" }, StringSplitOptions.RemoveEmptyEntries);
			int i, c;

			Assert.AreEqual(c = Expected.Length, Actual.Length);

			for (i = 0; i < c; i++)
				Assert.AreEqual(Expected[i], Actual[i]);
		}
	}
}
