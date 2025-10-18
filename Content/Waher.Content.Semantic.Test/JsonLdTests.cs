using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Waher.Runtime.IO;

namespace Waher.Content.Semantic.Test
{
	[TestClass]
	public class JsonLdTests : SemanticTests
	{
		// Tests taken from: https://www.w3.org/TR/json-ld/

		[DataTestMethod]
		[DataRow("example003.jsonld", "example003.ttl", null)]
		[DataRow("example004.jsonld", null, null)]
		[DataRow("example005.jsonld", "example005.ttl", null)]
		//[DataRow("example006.jsonld", null, null)]
		[DataRow("example007.jsonld", "example007.ttl", null)]
		[DataRow("example008.jsonld", null, null)]
		[DataRow("example008.jsonld", null, null)]
		[DataRow("example010.jsonld", null, null)]
		[DataRow("example011.jsonld", "example011.ttl", null)]
		[DataRow("example012.jsonld", "example012.ttl", null)]
		[DataRow("example013.jsonld", "example013.ttl", null)]
		[DataRow("example014.jsonld", "example014.ttl", null)]
		[DataRow("example015.jsonld", "example015.ttl", null)]
		[DataRow("example016.jsonld", "example016.ttl", null)]
		[DataRow("example017.jsonld", "example017.ttl", null)]
		[DataRow("example018.jsonld", "example018.ttl", null)]
		[DataRow("example019.jsonld", "example019.ttl", null)]
		[DataRow("example020.jsonld", "example020.ttl", null)]
		[DataRow("example021.jsonld", "example021.ttl", null)]
		[DataRow("example022.jsonld", "example022.ttl", null)]
		[DataRow("example023.jsonld", null, null)]
		[DataRow("example024.jsonld", "example024.ttl", null)]
		[DataRow("example025.jsonld", "example025.ttl", null)]
		[DataRow("example026.jsonld", "example026.ttl", null)]
		[DataRow("example027.jsonld", "example027.ttl", null)]
		[DataRow("example028.jsonld", "example028.ttl", null)]
		[DataRow("example029.jsonld", "example029.ttl", null)]
		[DataRow("example030.jsonld", "example030.ttl", null)]
		[DataRow("example031.jsonld", "example031.ttl", null)]
		[DataRow("example032.jsonld", "example032.ttl", null)]
		[DataRow("example033.jsonld", null, null)]
		[DataRow("example034.jsonld", null, null)]
		[DataRow("example035.jsonld", "example035.ttl", null)]
		[DataRow("example036.jsonld", null, null)]
		[DataRow("example037.jsonld", "example037.ttl", null)]
		[DataRow("example038.jsonld", "example038.ttl", null)]
		[DataRow("example039.jsonld", null, null)]
		[DataRow("example040.jsonld", null, null)]
		[DataRow("example041.jsonld", null, null)]
		[DataRow("example042.jsonld", null, null)]
		[DataRow("example043.jsonld", null, null)]
		[DataRow("example044.jsonld", null, null)]
		[DataRow("example045.jsonld", "example045.ttl", null)]
		[DataRow("example046.jsonld", "example046.ttl", null)]
		[DataRow("example047.jsonld", null, null)]
		[DataRow("example048.jsonld", null, null)]
		[DataRow("example049.jsonld", "example049.ttl", null)]
		[DataRow("example050.jsonld", null, null)]
		[DataRow("example051.jsonld", null, null)]
		[DataRow("example052.jsonld", null, null)]
		[DataRow("example053.jsonld", null, null)]
		[DataRow("example054.jsonld", null, null)]
		[DataRow("example055.jsonld", null, null)]
		[DataRow("example056.jsonld", "example056.ttl", null)]
		[DataRow("example057.jsonld", "example057.ttl", null)]
		[DataRow("example058.jsonld", "example058.ttl", null)]
		[DataRow("example059.jsonld", "example059.ttl", null)]
		[DataRow("example060.jsonld", "example060.ttl", null)]
		[DataRow("example061.jsonld", "example062.ttl", null)]
		[DataRow("example063.jsonld", "example063.ttl", null)]
		[DataRow("example064.jsonld", "example064.ttl", null)]
		[DataRow("example065.jsonld", "example065.ttl", null)]
		[DataRow("example066.jsonld", "example066.ttl", null)]
		[DataRow("example067.jsonld", "example067.ttl", null)]
		[DataRow("example068.jsonld", "example068.ttl", null)]
		[DataRow("example069.jsonld", null, null)]
		[DataRow("example070.jsonld", null, null)]
		[DataRow("example071.jsonld", null, null)]
		[DataRow("example072.jsonld", null, null)]
		[DataRow("example073.jsonld", null, null)]
		[DataRow("example074.jsonld", "example074.ttl", null)]
		[DataRow("example075.jsonld", null, null)]
		[DataRow("example076.jsonld", null, null)]
		[DataRow("example077.jsonld", null, null)]
		[DataRow("example078.jsonld", "example078.ttl", null)]
		[DataRow("example079.jsonld", "example079.ttl", null)]
		[DataRow("example080.jsonld", "example080.ttl", null)]
		[DataRow("example081.jsonld", "example081.ttl", null)]
		[DataRow("example082.jsonld", "example082.ttl", null)]
		[DataRow("example083.jsonld", null, null)]
		[DataRow("example084.jsonld", "example084.ttl", null)]
		[DataRow("example085.jsonld", "example085.ttl", null)]
		[DataRow("example086.jsonld", "example086.ttl", null)]
		[DataRow("example087.jsonld", null, null)]
		[DataRow("example088.jsonld", "example088.ttl", null)]
		[DataRow("example089.jsonld", "example089.ttl", null)]
		[DataRow("example090.jsonld", null, null)]
		[DataRow("example091.jsonld", null, null)]
		[DataRow("example092.jsonld", "example092.ttl", null)]
		[DataRow("example093.jsonld", "example093.ttl", null)]
		[DataRow("example094.jsonld", "example094.ttl", null)]
		[DataRow("example095.jsonld", "example095.ttl", null)]
		[DataRow("example096.jsonld", "example096.ttl", null)]
		[DataRow("example097.jsonld", "example097.ttl", null)]
		[DataRow("example098.jsonld", "example098.ttl", null)]
		[DataRow("example099.jsonld", "example099.ttl", null)]
		[DataRow("example100.jsonld", "example100.ttl", null)]
		[DataRow("example101.jsonld", "example101.ttl", null)]
		[DataRow("example102.jsonld", "example102.ttl", null)]
		[DataRow("example103.jsonld", "example103.ttl", null)]
		[DataRow("example104.jsonld", "example104.ttl", null)]
		[DataRow("example105.jsonld", "example105.ttl", null)]
		[DataRow("example106.jsonld", "example106.ttl", null)]
		[DataRow("example107.jsonld", "example107.ttl", null)]
		[DataRow("example108.jsonld", "example108.ttl", null)]
		[DataRow("example109.jsonld", null, null)]
		[DataRow("example110.jsonld", "example110.ttl", null)]
		[DataRow("example111.jsonld", "example111.ttl", null)]
		[DataRow("example112.jsonld", "example112.ttl", null)]
		[DataRow("example113.jsonld", "example113.ttl", null)]
		[DataRow("example114.jsonld", "example114.ttl", null)]
		[DataRow("example115.jsonld", "example115.trig", null)]
		[DataRow("example116.jsonld", "example116.trig", null)]
		[DataRow("example117.jsonld", "example117.trig", null)]
		[DataRow("example118.jsonld", "example118.trig", null)]
		[DataRow("example119.jsonld", "example119.trig", null)]
		[DataRow("example120.jsonld", "example120.trig", null)]
		[DataRow("example121.jsonld", "example121.trig", null)]
		[DataRow("example122.jsonld", "example122.trig", null)]
		[DataRow("example123.jsonld", null, null)]
		[DataRow("example124.jsonld", "example124.ttl", null)]
		[DataRow("example125.jsonld", null, null)]
		[DataRow("example126.jsonld", null, null)]
		[DataRow("example127.jsonld", null, null)]
		[DataRow("example137.jsonld", null, null)]
		[DataRow("example138.jsonld", null, null)]
		[DataRow("example139.jsonld", null, null)]
		[DataRow("example140.jsonld", null, null)]
		[DataRow("example141.jsonld", null, null)]
		[DataRow("example149.jsonld", null, null)]
		[DataRow("example150.jsonld", "example150.trig", null)]
		[DataRow("example151.jsonld", "example153.ttl", null)]
		[DataRow("example152.jsonld", "example153.ttl", null)]
		[DataRow("example155.jsonld", "example154.ttl", null)]
		[DataRow("example157.jsonld", "example156.ttl", null)]
		[DataRow("example158.jsonld", "example159.ttl", null)]
		[DataRow("example161.jsonld", "example160.ttl", null)]
		[DataRow("example163.jsonld", "example162.rdfa", null)]
		[DataRow("example165.jsonld", "example164.html", null)]
		public async Task ParsingTests(string FileName, string Expected, string BaseUri)
		{
			await PerformTest(FileName, Expected, BaseUri);
		}

		private static async Task PerformTest(string FileName, string ExpectedFileName, string BaseUri)
		{
			JsonLdDocument Parsed = await LoadJsonLdDocument(FileName, BaseUri is null ? null : new Uri(BaseUri + FileName));
			TurtleDocument? ParsedExpected = ExpectedFileName is null ? null : await LoadTurtleDocument(ExpectedFileName, BaseUri is null ? null : new Uri(BaseUri + ExpectedFileName));

			await Print(Parsed);
			Console.Out.WriteLine();

			await RdfTests.ExportAsImage(Parsed, "jsonld", FileName);

			if (ParsedExpected is not null)
			{
				await TurtleTests.Print(ParsedExpected);
				CompareTriples(Parsed, ParsedExpected);
			}
		}

		private static async Task Print(JsonLdDocument Parsed)
		{
			ContentResponse P = await InternetContent.EncodeAsync(Parsed, Encoding.UTF8);
			P.AssertOk();
			Assert.AreEqual("application/ld+json; charset=utf-8", P.ContentType);

			byte[] Data = P.Encoded;
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
			ContentResponse Decoded = await InternetContent.DecodeAsync("application/ld+json", Data, BaseUri);
			Decoded.AssertOk();

			if (Decoded.Decoded is JsonLdDocument Parsed)
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
			ContentResponse Decoded = await InternetContent.DecodeAsync("text/turtle", Data, BaseUri);
			Decoded.AssertOk();

			if (Decoded.Decoded is not TurtleDocument Parsed)
				throw new Exception("Unable to decode Turtle document.");

			return Parsed;
		}

	}
}