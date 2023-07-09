using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Waher.Content.Semantic.Test
{
	[TestClass]
	public class JsonLdTests : SemanticTests
	{
		// Tests taken from: https://www.w3.org/TR/json-ld/

		[DataTestMethod]
		[DataRow("example003.jsonld", "example003.ttl", null)]
		[DataRow("example005.jsonld", "example005.ttl", null)]
		[DataRow("example007.jsonld", "example007.ttl", null)]
		[DataRow("example011.jsonld", "example011.ttl", null)]
		[DataRow("example012.jsonld", "example012.ttl", null)]
		[DataRow("example013.jsonld", "example013.ttl", null)]
		[DataRow("example014.jsonld", "example014.ttl", null)]
		[DataRow("example015.jsonld", "example015.ttl", null)]
		[DataRow("example016.jsonld", "example016.ttl", null)]
		public async Task ParsingTests(string FileName, string Expected, string BaseUri)
		{
			await PerformTest(FileName, Expected, BaseUri);
		}

		private static async Task PerformTest(string FileName, string ExpectedFileName, string BaseUri)
		{
			JsonLdDocument Parsed = await LoadJsonLdDocument(FileName, BaseUri is null ? null : new Uri(BaseUri + FileName));
			TurtleDocument ParsedExpected = await LoadTurtleDocument(ExpectedFileName, BaseUri is null ? null : new Uri(BaseUri + ExpectedFileName));

			await Print(Parsed);
			Console.Out.WriteLine();
			await TurtleTests.Print(ParsedExpected);

			CompareTriples(Parsed, ParsedExpected);
		}

		private static async Task Print(JsonLdDocument Parsed)
		{
			KeyValuePair<byte[], string> P = await InternetContent.EncodeAsync(Parsed, Encoding.UTF8);
			Assert.AreEqual("application/ld+json; charset=utf-8", P.Value);

			byte[] Data = P.Key;
			string s = Encoding.UTF8.GetString(Data);

			Console.Out.WriteLine(s);
			Console.Out.WriteLine();

			foreach (ISemanticTriple Triple in Parsed)
				Console.WriteLine(Triple.ToString());
		}

		private static async Task<JsonLdDocument> LoadJsonLdDocument(string FileName, Uri? BaseUri)
		{
			BaseUri ??= new Uri("https://www.w3.org/TR/json-ld/" + FileName);

			byte[] Data = Resources.LoadResource(typeof(JsonLdTests).Namespace + ".Data.JsonLd." + GetResourceName(FileName));
			object Decoded = await InternetContent.DecodeAsync("application/ld+json", Data, BaseUri);
			if (Decoded is JsonLdDocument Parsed)
				return Parsed;
			else
				throw new Exception("Unable to decode JSON-LD document.");
		}

		private static string GetResourceName(string FileName)
		{
			return FileName.
				Replace('-', '_').
				Replace('/', '.').
				Replace("test_", "test-").
				Replace("error_", "error-").
				Replace("warn_", "warn-");
		}

		private static async Task<TurtleDocument> LoadTurtleDocument(string FileName, Uri? BaseUri)
		{
			BaseUri ??= new Uri("https://www.w3.org/TR/json-ld/" + FileName);

			byte[] Data = Resources.LoadResource(typeof(TurtleTests).Namespace + ".Data.JsonLd." + GetResourceName(FileName));
			object Decoded = await InternetContent.DecodeAsync("text/turtle", Data, BaseUri);
			if (Decoded is not TurtleDocument Parsed)
				throw new Exception("Unable to decode Turtle document.");

			return Parsed;
		}

	}
}