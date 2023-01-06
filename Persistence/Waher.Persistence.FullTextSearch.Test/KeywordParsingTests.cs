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
			this.Parse("Kilroy was here.", false,
				new PlainKeyword("kilroy"),
				new PlainKeyword("was"),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_02_Required()
		{
			this.Parse("+Kilroy was here.", false,
				new RequiredKeyword(new PlainKeyword("kilroy")),
				new PlainKeyword("was"),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_03_Prohibited()
		{
			this.Parse("+Kilroy was -not here.", false,
				new RequiredKeyword(new PlainKeyword("kilroy")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_04_Wildcard()
		{
			this.Parse("+*roy was -not here.", false,
				new RequiredKeyword(new WildcardKeyword("*roy", "*")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_05_Wildcard2()
		{
			this.Parse("+Kil* was -not here.", false,
				new RequiredKeyword(new WildcardKeyword("kil*", "*")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_06_Wildcard3()
		{
			this.Parse("+K*y was -not here.", false,
				new RequiredKeyword(new WildcardKeyword("k*y", "*")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_07_RegularExpression()
		{
			this.Parse("+/(Kil|Fitz)roy/ was -not here.", false,
				new RequiredKeyword(new RegexKeyword("(kil|fitz)roy")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_08_RegularExpression2()
		{
			this.Parse("+/Kil(roy|ling)/ was -not here.", false,
				new RequiredKeyword(new RegexKeyword("kil(roy|ling)")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_09_RegularExpression3()
		{
			this.Parse("+/K.+y/ was -not here.", false,
				new RequiredKeyword(new RegexKeyword("k.+y")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_10_Plain_AsPrefixes()
		{
			this.Parse("Kilroy was here.", true,
				new WildcardKeyword("kilroy*", "*"),
				new WildcardKeyword("was*", "*"),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_11_Required_AsPrefixes()
		{
			this.Parse("+Kilroy was here.", true,
				new RequiredKeyword(new WildcardKeyword("kilroy*", "*")),
				new WildcardKeyword("was*", "*"),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_12_Prohibited_AsPrefixes()
		{
			this.Parse("+Kilroy was -not here.", true,
				new RequiredKeyword(new WildcardKeyword("kilroy*", "*")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_13_Wildcard_AsPrefixes()
		{
			this.Parse("+*roy was -not here.", true,
				new RequiredKeyword(new WildcardKeyword("*roy", "*")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_14_Wildcard2_AsPrefixes()
		{
			this.Parse("+Kil* was -not here.", true,
				new RequiredKeyword(new WildcardKeyword("kil*", "*")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_15_Wildcard3_AsPrefixes()
		{
			this.Parse("+K*y was -not here.", true,
				new RequiredKeyword(new WildcardKeyword("k*y", "*")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_16_RegularExpression_AsPrefixes()
		{
			this.Parse("+/(Kil|Fitz)roy/ was -not here.", true,
				new RequiredKeyword(new RegexKeyword("(kil|fitz)roy")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_17_RegularExpression2_AsPrefixes()
		{
			this.Parse("+/Kil(roy|ling)/ was -not here.", true,
				new RequiredKeyword(new RegexKeyword("kil(roy|ling)")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_18_RegularExpression3_AsPrefixes()
		{
			this.Parse("+/K.+y/ was -not here.", true,
				new RequiredKeyword(new RegexKeyword("k.+y")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		private void Parse(string Query, bool TreatKeywordsAsPrefixes, params Keyword[] Keywords)
		{
			Keyword[] Parsed = Search.ParseKeywords(Query, TreatKeywordsAsPrefixes);
			int i, c;

			Assert.AreEqual(c = Keywords.Length, Parsed.Length);

			for (i = 0; i < c; i++)
				Assert.IsTrue(Keywords[i].Equals(Parsed[i]));
		}

	}
}