using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Waher.Persistence.FullTextSearch.Keywords;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class TokenizationTests
	{
		[TestMethod]
		public async Task Test_01_String()
		{
			AreEqual(await Search.Tokenize("Kilroy was here."),
				new TokenCount("kilroy", 1),
				new TokenCount("was", 2),
				new TokenCount("here", 3));
		}

		[TestMethod]
		public async Task Test_02_CaseInsensitiveString()
		{
			AreEqual(await Search.Tokenize(new CaseInsensitiveString("Kilroy was here.")),
				new TokenCount("kilroy", 1),
				new TokenCount("was", 2),
				new TokenCount("here", 3));
		}

		[TestMethod]
		public async Task Test_03_StringArray()
		{
			AreEqual(await Search.Tokenize((object)(new string[] { "Kilroy", "was", "here" })),
				new TokenCount("kilroy", 1),
				new TokenCount("was", 3),
				new TokenCount("here", 5));
		}

		[TestMethod]
		public async Task Test_04_CaseInsensitiveArray()
		{
			AreEqual(await Search.Tokenize((object)(new CaseInsensitiveString[]
				{
					new CaseInsensitiveString("Kilroy"),
					new CaseInsensitiveString("was"),
					new CaseInsensitiveString("here"),
				})),
				new TokenCount("kilroy", 1),
				new TokenCount("was", 3),
				new TokenCount("here", 5));
		}

		[TestMethod]
		public async Task Test_05_XmlDocument()
		{
			AreEqual(await Search.Tokenize(GetXmlDocument()),
				new TokenCount("kilroy", 1),
				new TokenCount("was", 2),
				new TokenCount("here", 3));
		}

		[TestMethod]
		public async Task Test_06_XmlElement()
		{
			AreEqual(await Search.Tokenize(GetXmlDocument().DocumentElement),
				new TokenCount("kilroy", 1),
				new TokenCount("was", 2),
				new TokenCount("here", 3));
		}

		private static XmlDocument GetXmlDocument()
		{
			XmlDocument Doc = new();
			Doc.LoadXml("<a><b c='Kilroy'>was here</b></a>");
			return Doc;
		}

		private static void AreEqual(TokenCount[] Tokenized, params TokenCount[] Expected)
		{
			Assert.IsNotNull(Tokenized);

			int i, c = Expected.Length;

			Assert.AreEqual(c, Tokenized.Length);

			for (i = 0; i < c; i++)
				Assert.IsTrue(Expected[i].Equals(Tokenized[i]));
		}

		// TODO: Multiple locations

	}
}