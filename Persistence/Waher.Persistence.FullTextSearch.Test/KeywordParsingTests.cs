using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Waher.Events;
using Waher.Events.Console;
using Waher.Persistence.Files;
using Waher.Persistence.FullTextSearch.Keywords;
using Waher.Persistence.FullTextSearch.Test.Classes;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class KeywordParsingTests
	{
		private static FilesProvider? filesProvider = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(FullTextSearchModule).Assembly,
				typeof(TestClass).Assembly);

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);

			Log.Register(new ConsoleEventSink());

			await Types.StartAllModules(10000);
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await Log.TerminateAsync();
			await Types.StopAllModules();

			if (filesProvider is not null)
			{
				await filesProvider.DisposeAsync();
				filesProvider = null;
			}
		}

		[TestMethod]
		public void Test_01_Plain()
		{
			Parse("Kilroy was here.", false,
				new PlainKeyword("kilroy"),
				new PlainKeyword("was"),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_02_Required()
		{
			Parse("+Kilroy was here.", false,
				new RequiredKeyword(new PlainKeyword("kilroy")),
				new PlainKeyword("was"),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_03_Prohibited()
		{
			Parse("+Kilroy was -not here.", false,
				new RequiredKeyword(new PlainKeyword("kilroy")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_04_Wildcard()
		{
			Parse("+*roy was -not here.", false,
				new RequiredKeyword(new WildcardKeyword("*roy", "*")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_05_Wildcard2()
		{
			Parse("+Kil* was -not here.", false,
				new RequiredKeyword(new WildcardKeyword("kil*", "*")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_06_Wildcard3()
		{
			Parse("+K*y was -not here.", false,
				new RequiredKeyword(new WildcardKeyword("k*y", "*")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_07_RegularExpression()
		{
			Parse("+/(Kil|Fitz)roy/ was -not here.", false,
				new RequiredKeyword(new RegexKeyword("(kil|fitz)roy")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_08_RegularExpression2()
		{
			Parse("+/Kil(roy|ling)/ was -not here.", false,
				new RequiredKeyword(new RegexKeyword("kil(roy|ling)")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_09_RegularExpression3()
		{
			Parse("+/K.+y/ was -not here.", false,
				new RequiredKeyword(new RegexKeyword("k.+y")),
				new PlainKeyword("was"),
				new ProhibitedKeyword(new PlainKeyword("not")),
				new PlainKeyword("here"));
		}

		[TestMethod]
		public void Test_10_Plain_AsPrefixes()
		{
			Parse("Kilroy was here.", true,
				new WildcardKeyword("kilroy*", "*"),
				new WildcardKeyword("was*", "*"),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_11_Required_AsPrefixes()
		{
			Parse("+Kilroy was here.", true,
				new RequiredKeyword(new WildcardKeyword("kilroy*", "*")),
				new WildcardKeyword("was*", "*"),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_12_Prohibited_AsPrefixes()
		{
			Parse("+Kilroy was -not here.", true,
				new RequiredKeyword(new WildcardKeyword("kilroy*", "*")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_13_Wildcard_AsPrefixes()
		{
			Parse("+*roy was -not here.", true,
				new RequiredKeyword(new WildcardKeyword("*roy", "*")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_14_Wildcard2_AsPrefixes()
		{
			Parse("+Kil* was -not here.", true,
				new RequiredKeyword(new WildcardKeyword("kil*", "*")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_15_Wildcard3_AsPrefixes()
		{
			Parse("+K*y was -not here.", true,
				new RequiredKeyword(new WildcardKeyword("k*y", "*")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_16_RegularExpression_AsPrefixes()
		{
			Parse("+/(Kil|Fitz)roy/ was -not here.", true,
				new RequiredKeyword(new RegexKeyword("(kil|fitz)roy")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_17_RegularExpression2_AsPrefixes()
		{
			Parse("+/Kil(roy|ling)/ was -not here.", true,
				new RequiredKeyword(new RegexKeyword("kil(roy|ling)")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_18_RegularExpression3_AsPrefixes()
		{
			Parse("+/K.+y/ was -not here.", true,
				new RequiredKeyword(new RegexKeyword("k.+y")),
				new WildcardKeyword("was*", "*"),
				new ProhibitedKeyword(new WildcardKeyword("not*", "*")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_19_Sequence_Quotes()
		{
			Parse("\"Kilroy was here\"", true,
				new SequenceOfKeywords(
					new PlainKeyword("kilroy"),
					new PlainKeyword("was"),
					new PlainKeyword("here")));
		}

		[TestMethod]
		public void Test_20_Sequence_Apostrophes()
		{
			Parse("'Kilroy was here'", true,
				new SequenceOfKeywords(
					new PlainKeyword("kilroy"),
					new PlainKeyword("was"),
					new PlainKeyword("here")));
		}

		[TestMethod]
		public void Test_21_Sequence_Nesting1()
		{
			Parse("+'Kilroy was here'", true,
				new RequiredKeyword(
					new SequenceOfKeywords(
						new PlainKeyword("kilroy"),
						new PlainKeyword("was"),
						new PlainKeyword("here"))));
		}

		[TestMethod]
		public void Test_22_Sequence_Nesting2()
		{
			Parse("'Kilroy was' here", true,
				new SequenceOfKeywords(
					new PlainKeyword("kilroy"),
					new PlainKeyword("was")),
				new WildcardKeyword("here*", "*"));
		}

		[TestMethod]
		public void Test_23_Sequence_Nesting3()
		{
			Parse("Kilroy 'was here'", true,
				new WildcardKeyword("kilroy*", "*"),
				new SequenceOfKeywords(
					new PlainKeyword("was"),
					new PlainKeyword("here")));
		}

		private static void Parse(string Query, bool TreatKeywordsAsPrefixes, params Keyword[] Keywords)
		{
			Keyword[] Parsed = Search.ParseKeywords(Query, TreatKeywordsAsPrefixes);
			int i, c;

			Assert.AreEqual(c = Keywords.Length, Parsed.Length);

			for (i = 0; i < c; i++)
				Assert.IsTrue(Keywords[i].Equals(Parsed[i]));
		}

	}
}