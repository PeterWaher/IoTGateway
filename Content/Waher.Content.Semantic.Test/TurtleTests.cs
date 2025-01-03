using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;

namespace Waher.Content.Semantic.Test
{
	[TestClass]
	public class TurtleTests : SemanticTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(InternetContent).Assembly,
				typeof(TurtleDocument).Assembly,
				typeof(TurtleTests).Assembly);
		}

		[DataTestMethod]
		[DataRow("example1.ttl")]
		[DataRow("example2.ttl")]
		[DataRow("example2_Short.ttl")]
		[DataRow("example3.ttl")]
		[DataRow("example3.1.ttl")]
		[DataRow("test.nt")]
		[DataRow("test-31.ttl")]
		public async Task Test_01_Examples(string FileName)
		{
			await PerformTest(FileName);
		}

		[DataTestMethod]
		[DataRow("test-00.ttl", "test-00.out", "http://www.w3.org/2001/sw/DataAccess/df1/tests/")]
		[DataRow("test-01.ttl", "test-01.out", "http://www.w3.org/2011/sw/DataAccess/df1/tests/")]
		[DataRow("test-02.ttl", "test-02.out", "http://www.w3.org/2021/sw/DataAccess/df1/tests/")]
		[DataRow("test-03.ttl", "test-03.out", "http://www.w3.org/2031/sw/DataAccess/df1/tests/")]
		[DataRow("test-04.ttl", "test-04.out", "http://www.w3.org/2041/sw/DataAccess/df1/tests/")]
		[DataRow("test-05.ttl", "test-05.out", "http://www.w3.org/2051/sw/DataAccess/df1/tests/")]
		[DataRow("test-06.ttl", "test-06.out", "http://www.w3.org/2061/sw/DataAccess/df1/tests/")]
		[DataRow("test-07.ttl", "test-07.out", "http://www.w3.org/2071/sw/DataAccess/df1/tests/")]
		[DataRow("test-08.ttl", "test-08.out", "http://www.w3.org/2081/sw/DataAccess/df1/tests/")]
		[DataRow("test-09.ttl", "test-09.out", "http://www.w3.org/2091/sw/DataAccess/df1/tests/")]
		[DataRow("test-10.ttl", "test-10.out", "http://www.w3.org/2101/sw/DataAccess/df1/tests/")]
		[DataRow("test-11.ttl", "test-11.out", "http://www.w3.org/2111/sw/DataAccess/df1/tests/")]
		[DataRow("test-12.ttl", "test-12.out", "http://www.w3.org/2121/sw/DataAccess/df1/tests/")]
		[DataRow("test-13.ttl", "test-13.out", "http://www.w3.org/2131/sw/DataAccess/df1/tests/")]
		[DataRow("test-14.ttl", "test-14.out", "http://www.w3.org/2141/sw/DataAccess/df1/tests/")]
		[DataRow("test-15.ttl", "test-15.out", "http://www.w3.org/2151/sw/DataAccess/df1/tests/")]
		[DataRow("test-16.ttl", "test-16.out", "http://www.w3.org/2161/sw/DataAccess/df1/tests/")]
		[DataRow("test-17.ttl", "test-17.out", "http://www.w3.org/2171/sw/DataAccess/df1/tests/")]
		[DataRow("test-18.ttl", "test-18.out", "http://www.w3.org/2181/sw/DataAccess/df1/tests/")]
		[DataRow("test-19.ttl", "test-19.out", "http://www.w3.org/2191/sw/DataAccess/df1/tests/")]
		[DataRow("test-20.ttl", "test-20.out", "http://www.w3.org/2201/sw/DataAccess/df1/tests/")]
		[DataRow("test-21.ttl", "test-21.out", "http://www.w3.org/2211/sw/DataAccess/df1/tests/")]
		[DataRow("test-22.ttl", "test-22.out", "http://www.w3.org/2221/sw/DataAccess/df1/tests/")]
		[DataRow("test-23.ttl", "test-23.out", "http://www.w3.org/2231/sw/DataAccess/df1/tests/")]
		[DataRow("test-24.ttl", "test-24.out", "http://www.w3.org/2241/sw/DataAccess/df1/tests/")]
		[DataRow("test-25.ttl", "test-25.out", "http://www.w3.org/2251/sw/DataAccess/df1/tests/")]
		[DataRow("test-26.ttl", "test-26.out", "http://www.w3.org/2261/sw/DataAccess/df1/tests/")]
		[DataRow("test-27.ttl", "test-27.out", "http://www.w3.org/2271/sw/DataAccess/df1/tests/")]
		[DataRow("test-28.ttl", "test-28.out", "http://www.w3.org/2281/sw/DataAccess/df1/tests/")]
		[DataRow("test-29.ttl", "test-29.out", "http://www.w3.org/2291/sw/DataAccess/df1/tests/")]
		[DataRow("test-30.ttl", "test-30.out", "http://www.w3.org/2001/sw/DataAccess/df1/tests/")]
		[DataRow("test-32.ttl", "test-32.out", "http://www.w3.org/2001/sw/DataAccess/df1/tests/")]
		public async Task Test_02_PassTests(string FileName, string Expected, string BaseUri)
		{
			await PerformTest(FileName, Expected, BaseUri);
		}

		[DataTestMethod]
		[ExpectedException(typeof(ParsingException))]
		[DataRow("bad-00.ttl", "http://www.w3.org/2001/sw/DataAccess/df1/bads/")]
		[DataRow("bad-01.ttl", "http://www.w3.org/2011/sw/DataAccess/df1/bads/")]
		[DataRow("bad-02.ttl", "http://www.w3.org/2021/sw/DataAccess/df1/bads/")]
		[DataRow("bad-03.ttl", "http://www.w3.org/2031/sw/DataAccess/df1/bads/")]
		[DataRow("bad-04.ttl", "http://www.w3.org/2041/sw/DataAccess/df1/bads/")]
		[DataRow("bad-05.ttl", "http://www.w3.org/2051/sw/DataAccess/df1/bads/")]
		[DataRow("bad-06.ttl", "http://www.w3.org/2061/sw/DataAccess/df1/bads/")]
		[DataRow("bad-07.ttl", "http://www.w3.org/2071/sw/DataAccess/df1/bads/")]
		[DataRow("bad-08.ttl", "http://www.w3.org/2081/sw/DataAccess/df1/bads/")]
		[DataRow("bad-09.ttl", "http://www.w3.org/2091/sw/DataAccess/df1/bads/")]
		[DataRow("bad-10.ttl", "http://www.w3.org/2101/sw/DataAccess/df1/bads/")]
		[DataRow("bad-11.ttl", "http://www.w3.org/2111/sw/DataAccess/df1/bads/")]
		[DataRow("bad-12.ttl", "http://www.w3.org/2121/sw/DataAccess/df1/bads/")]
		[DataRow("bad-13.ttl", "http://www.w3.org/2131/sw/DataAccess/df1/bads/")]
		[DataRow("bad-14.ttl", "http://www.w3.org/2141/sw/DataAccess/df1/bads/")]
		public async Task Test_03_Bad(string FileName, string BaseUri)
		{
			await PerformTest(FileName, BaseUri);
		}

		[DataTestMethod]
		[DataRow("manifest.ttl", "http://www.w3.org/2301/sw/DataAccess/df1/tests/")]
		[DataRow("manifest-bad.ttl", "http://www.w3.org/2301/sw/DataAccess/df1/tests/")]
		public async Task Test_04_Manifest(string FileName, string BaseUri)
		{
			await PerformTest(FileName, BaseUri);
		}

		[DataTestMethod]
		[DataRow("rdfq-results.ttl", "rdfq-results.out", "http://www.w3.org/2001/sw/DataAccess/df1/tests/")]
		[DataRow("rdf-schema.ttl", "rdf-schema.out", "http://www.w3.org/2301/sw/DataAccess/df1/tests/")]
		[DataRow("rdfs-namespace.ttl", "rdfs-namespace.out", "http://www.w3.org/2301/sw/DataAccess/df1/tests/")]
		public async Task Test_05_Rdf(string FileName, string Expected, string BaseUri)
		{
			await PerformTest(FileName, Expected, BaseUri);
		}

		private static async Task PerformTest(string FileName)
		{
			TurtleDocument Parsed = await LoadTurtleDocument(FileName, null);
			await Print(Parsed);

			Console.Out.WriteLine();
			Console.Out.WriteLine();
			foreach (ISemanticTriple T in Parsed)
			{
				Console.Out.Write(T.Subject.ToString());
				Console.Out.Write('\t');
				Console.Out.Write(T.Predicate.ToString());
				Console.Out.Write('\t');
				Console.Out.WriteLine(T.Object.ToString());
			}
		}

		private static async Task PerformTest(string FileName, string BaseUri)
		{
			TurtleDocument Parsed = await LoadTurtleDocument(FileName, new Uri(BaseUri + FileName));
			await Print(Parsed);
		}

		internal static async Task Print(TurtleDocument Parsed)
		{
			ContentResponse P = await InternetContent.EncodeAsync(Parsed, Encoding.UTF8);
			P.AssertOk();
			Assert.AreEqual("text/turtle; charset=utf-8", P.ContentType);

			byte[] Data = P.Encoded;
			string s = Encoding.UTF8.GetString(Data);

			Console.Out.WriteLine(s);
		}

		private static async Task<TurtleDocument> LoadTurtleDocument(string FileName, Uri? BaseUri)
		{
			byte[] Data = Resources.LoadResource(typeof(TurtleTests).Namespace + ".Data.Turtle." + FileName);
			object Decoded = await InternetContent.DecodeAsync("text/turtle", Data, BaseUri);
			if (Decoded is not TurtleDocument Parsed)
				throw new Exception("Unable to decode Turtle document.");

			return Parsed;
		}

		private static async Task PerformTest(string FileName, string ExpectedFileName, string BaseUri)
		{
			TurtleDocument Parsed = await LoadTurtleDocument(FileName, new Uri(BaseUri + FileName));
			TurtleDocument ParsedExpected = await LoadTurtleDocument(ExpectedFileName, new Uri(BaseUri + ExpectedFileName));

			await Print(Parsed);

			CompareTriples(Parsed, ParsedExpected);
		}

	}
}