using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Waher.Content.Semantic.Model;
using Waher.Runtime.Inventory;
using Waher.Script.Functions.Runtime;

namespace Waher.Content.Semantic.Test
{
	[TestClass]
	public class TurtleTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(TurtleDocument).Assembly,
				typeof(TurtleTests).Assembly);
		}

		[TestMethod]
		public async Task Example1()
		{
			await PerformTest("example1.ttl");
		}

		[TestMethod]
		public async Task Example2()
		{
			await PerformTest("example2.ttl");
		}

		[TestMethod]
		public async Task Example2Short()
		{
			await PerformTest("example2_Short.ttl");
		}

		[TestMethod]
		public async Task Example3()
		{
			await PerformTest("example3.ttl");
		}

		[TestMethod]
		public async Task TestNt()
		{
			await PerformTest("test.nt");
		}

		[TestMethod]
		public async Task Test_00()
		{
			await PerformTest("test-00.ttl", "test-00.out", "http://www.w3.org/2001/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_01()
		{
			await PerformTest("test-01.ttl", "test-01.out", "http://www.w3.org/2011/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_02()
		{
			await PerformTest("test-02.ttl", "test-02.out", "http://www.w3.org/2021/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_03()
		{
			await PerformTest("test-03.ttl", "test-03.out", "http://www.w3.org/2031/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_04()
		{
			await PerformTest("test-04.ttl", "test-04.out", "http://www.w3.org/2041/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_05()
		{
			await PerformTest("test-05.ttl", "test-05.out", "http://www.w3.org/2051/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_06()
		{
			await PerformTest("test-06.ttl", "test-06.out", "http://www.w3.org/2061/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_07()
		{
			await PerformTest("test-07.ttl", "test-07.out", "http://www.w3.org/2071/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_08()
		{
			await PerformTest("test-08.ttl", "test-08.out", "http://www.w3.org/2081/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_09()
		{
			await PerformTest("test-09.ttl", "test-09.out", "http://www.w3.org/2091/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_10()
		{
			await PerformTest("test-10.ttl", "test-10.out", "http://www.w3.org/2101/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_11()
		{
			await PerformTest("test-11.ttl", "test-11.out", "http://www.w3.org/2111/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_12()
		{
			await PerformTest("test-12.ttl", "test-12.out", "http://www.w3.org/2121/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_13()
		{
			await PerformTest("test-13.ttl", "test-13.out", "http://www.w3.org/2131/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_14()
		{
			await PerformTest("test-14.ttl", "test-14.out", "http://www.w3.org/2141/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_15()
		{
			await PerformTest("test-15.ttl", "test-15.out", "http://www.w3.org/2151/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_16()
		{
			await PerformTest("test-16.ttl", "test-16.out", "http://www.w3.org/2161/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_17()
		{
			await PerformTest("test-17.ttl", "test-17.out", "http://www.w3.org/2171/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_18()
		{
			await PerformTest("test-18.ttl", "test-18.out", "http://www.w3.org/2181/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_19()
		{
			await PerformTest("test-19.ttl", "test-19.out", "http://www.w3.org/2191/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_20()
		{
			await PerformTest("test-20.ttl", "test-20.out", "http://www.w3.org/2201/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_21()
		{
			await PerformTest("test-21.ttl", "test-21.out", "http://www.w3.org/2211/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_22()
		{
			await PerformTest("test-22.ttl", "test-22.out", "http://www.w3.org/2221/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_23()
		{
			await PerformTest("test-23.ttl", "test-23.out", "http://www.w3.org/2231/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_24()
		{
			await PerformTest("test-24.ttl", "test-24.out", "http://www.w3.org/2241/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_25()
		{
			await PerformTest("test-25.ttl", "test-25.out", "http://www.w3.org/2251/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_26()
		{
			await PerformTest("test-26.ttl", "test-26.out", "http://www.w3.org/2261/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_27()
		{
			await PerformTest("test-27.ttl", "test-27.out", "http://www.w3.org/2271/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_28()
		{
			await PerformTest("test-28.ttl", "test-28.out", "http://www.w3.org/2281/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_29()
		{
			await PerformTest("test-29.ttl", "test-29.out", "http://www.w3.org/2291/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task Test_30()
		{
			await PerformTest("test-30.ttl", "test-30.out", "http://www.w3.org/2001/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_00()
		{
			await PerformTest("bad-00.ttl", "http://www.w3.org/2001/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_01()
		{
			await PerformTest("bad-01.ttl", "http://www.w3.org/2011/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_02()
		{
			await PerformTest("bad-02.ttl", "http://www.w3.org/2021/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_03()
		{
			await PerformTest("bad-03.ttl", "http://www.w3.org/2031/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_04()
		{
			await PerformTest("bad-04.ttl", "http://www.w3.org/2041/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_05()
		{
			await PerformTest("bad-05.ttl", "http://www.w3.org/2051/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_06()
		{
			await PerformTest("bad-06.ttl", "http://www.w3.org/2061/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_07()
		{
			await PerformTest("bad-07.ttl", "http://www.w3.org/2071/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_08()
		{
			await PerformTest("bad-08.ttl", "http://www.w3.org/2081/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_09()
		{
			await PerformTest("bad-09.ttl", "http://www.w3.org/2091/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_10()
		{
			await PerformTest("bad-10.ttl", "http://www.w3.org/2101/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_11()
		{
			await PerformTest("bad-11.ttl", "http://www.w3.org/2111/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_12()
		{
			await PerformTest("bad-12.ttl", "http://www.w3.org/2121/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_13()
		{
			await PerformTest("bad-13.ttl", "http://www.w3.org/2131/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public async Task Bad_14()
		{
			await PerformTest("bad-14.ttl", "http://www.w3.org/2141/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		public async Task Manifest()
		{
			await PerformTest("manifest.ttl", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task ManifestBad()
		{
			await PerformTest("manifest-bad.ttl", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task RdfqResults()
		{
			await PerformTest("rdfq-results.ttl", "rdfq-results.out", "http://www.w3.org/2001/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task RdfSchema()
		{
			await PerformTest("rdf-schema.ttl", "rdf-schema.out", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public async Task RdfsNamespace()
		{
			await PerformTest("rdfs-namespace.ttl", "rdfs-namespace.out", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		private static async Task PerformTest(string FileName)
		{
			TurtleDocument Parsed = await LoadTurtleDocument(FileName, null);
			await Print(Parsed);
		}

		private static async Task PerformTest(string FileName, string BaseUri)
		{
			TurtleDocument Parsed = await LoadTurtleDocument(FileName, new Uri(BaseUri + FileName));
			await Print(Parsed);
		}

		private static async Task Print(TurtleDocument Parsed)
		{
			KeyValuePair<byte[], string> P = await InternetContent.EncodeAsync(Parsed, Encoding.UTF8);
			Assert.AreEqual("text/turtle; charset=utf-8", P.Value);

			byte[] Data = P.Key;
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

		private static void CompareTriples(ISemanticModel Result, ISemanticModel Expected)
		{
			Dictionary<string, string> NodeIdMap = new();
			List<ISemanticTriple> Triples = new();
			Triples.AddRange(Result);
			List<ISemanticTriple> NotFound = new();

			int i, c = Triples.Count;

			foreach (ISemanticTriple T in Expected)
			{
				bool Match = false;

				for (i = 0; i < c; i++)
				{
					ISemanticTriple T2 = Triples[i];

					if (Matches(Triples[i], T, NodeIdMap, false))
					{
						Assert.IsTrue(Matches(Triples[i], T, NodeIdMap, true), "Blank node inconsistency.");

						Match = true;
						Triples.RemoveAt(i);
						c--;
						break;
					}
				}

				if (!Match)
					NotFound.Add(T);
			}

			if (NotFound.Count > 0 || Triples.Count > 0)
			{
				StringBuilder sb = new();

				sb.AppendLine("Unexpected result.");

				if (NotFound.Count > 0)
				{
					sb.AppendLine();
					sb.AppendLine("Expected Triples not found in result: ");
					sb.AppendLine("=======================================");

					foreach (ISemanticTriple T in NotFound)
					{
						sb.Append(T.Subject.ToString());
						sb.Append('\t');
						sb.Append(T.Predicate.ToString());
						sb.Append('\t');
						sb.AppendLine(T.Object.ToString());
					}
				}

				if (Triples.Count > 0)
				{
					sb.AppendLine();
					sb.AppendLine("Generated Triples not expected: ");
					sb.AppendLine("==================================");

					foreach (ISemanticTriple T in Triples)
					{
						sb.Append(T.Subject.ToString());
						sb.Append('\t');
						sb.Append(T.Predicate.ToString());
						sb.Append('\t');
						sb.AppendLine(T.Object.ToString());
					}
				}

				Assert.Fail(sb.ToString());
			}
		}

		private static bool Matches(ISemanticTriple T1, ISemanticTriple T2, Dictionary<string, string> NodeIdMap, bool AddToMap)
		{
			return Matches(T1.Subject, T2.Subject, NodeIdMap, AddToMap) &&
				Matches(T1.Predicate, T2.Predicate, NodeIdMap, AddToMap) &&
				Matches(T1.Object, T2.Object, NodeIdMap, AddToMap);
		}

		private static bool Matches(ISemanticElement E1, ISemanticElement E2, Dictionary<string, string> NodeIdMap, bool AddToMap)
		{
			if (E1 is BlankNode B1 && E2 is BlankNode B2)
			{
				if (NodeIdMap.TryGetValue(B1.NodeId, out string? NodeId))
					return B2.NodeId == NodeId;

				if (AddToMap)
					NodeIdMap[B1.NodeId] = B2.NodeId;

				return true;
			}
			else
				return E1.ToString() == E2.ToString();
		}



	}
}