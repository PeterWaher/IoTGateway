using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Keywords;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class KeywordParsingTests
	{
		[TestMethod]
		public void Test_01_Plain()
		{
			this.Parse("Kilroy was here.",
				new PlainKeyword("kilroy"),
				new PlainKeyword("was"),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_02_Required()
		{
			this.Parse("+Kilroy was here.",
				new RequiredKeyword(new PlainKeyword("kilroy")),
				new PlainKeyword("was"),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_03_Prohibited()
		{
			this.Parse("+Kilroy was -not here.",
				new RequiredKeyword(new PlainKeyword("kilroy")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_04_Wildcard()
		{
			this.Parse("+*roy was -not here.",
				new RequiredKeyword(new WildcardKeyword("*roy", "*")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_05_Wildcard2()
		{
			this.Parse("+Kil* was -not here.",
				new RequiredKeyword(new WildcardKeyword("kil*", "*")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_06_Wildcard3()
		{
			this.Parse("+K*y was -not here.",
				new RequiredKeyword(new WildcardKeyword("k*y", "*")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_07_RegularExpression()
		{
			this.Parse("+/(Kil|Fitz)roy/ was -not here.",
				new RequiredKeyword(new RegexKeyword("(kil|fitz)roy")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_08_RegularExpression2()
		{
			this.Parse("+/Kil(roy|ling)/ was -not here.",
				new RequiredKeyword(new RegexKeyword("kil(roy|ling)")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_09_RegularExpression3()
		{
			this.Parse("+/K.+y/ was -not here.",
				new RequiredKeyword(new RegexKeyword("k.+y")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		private void Parse(string Query, params Keyword[] Keywords)
		{
			Keyword[] Parsed = Search.ParseKeywords(Query);
			int i, c;

			Assert.AreEqual(c = Keywords.Length, Parsed.Length);

			for (i = 0; i < c; i++)
				Assert.IsTrue(Keywords[i].Equals(Parsed[i]));
		}

	}
}