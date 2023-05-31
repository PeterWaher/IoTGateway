using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Runtime.Inventory;

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
		public void Example1()
		{
			this.PerformTest("example1.ttl");
		}

		[TestMethod]
		public void Example2()
		{
			this.PerformTest("example2.ttl");
		}

		[TestMethod]
		public void Example2Short()
		{
			this.PerformTest("example2_Short.ttl");
		}

		[TestMethod]
		public void Example3()
		{
			this.PerformTest("example3.ttl");
		}

		[TestMethod]
		public void TestNt()
		{
			this.PerformTest("test.nt");
		}

		[TestMethod]
		public void Test_00()
		{
			this.PerformTest("test-00.ttl", "test-00.out", "http://www.w3.org/2001/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_01()
		{
			this.PerformTest("test-01.ttl", "test-01.out", "http://www.w3.org/2011/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_02()
		{
			this.PerformTest("test-02.ttl", "test-02.out", "http://www.w3.org/2021/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_03()
		{
			this.PerformTest("test-03.ttl", "test-03.out", "http://www.w3.org/2031/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_04()
		{
			this.PerformTest("test-04.ttl", "test-04.out", "http://www.w3.org/2041/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_05()
		{
			this.PerformTest("test-05.ttl", "test-05.out", "http://www.w3.org/2051/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_06()
		{
			this.PerformTest("test-06.ttl", "test-06.out", "http://www.w3.org/2061/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_07()
		{
			this.PerformTest("test-07.ttl", "test-07.out", "http://www.w3.org/2071/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_08()
		{
			this.PerformTest("test-08.ttl", "test-08.out", "http://www.w3.org/2081/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_09()
		{
			this.PerformTest("test-09.ttl", "test-09.out", "http://www.w3.org/2091/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_10()
		{
			this.PerformTest("test-10.ttl", "test-10.out", "http://www.w3.org/2101/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_11()
		{
			this.PerformTest("test-11.ttl", "test-11.out", "http://www.w3.org/2111/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_12()
		{
			this.PerformTest("test-12.ttl", "test-12.out", "http://www.w3.org/2121/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_13()
		{
			this.PerformTest("test-13.ttl", "test-13.out", "http://www.w3.org/2131/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_14()
		{
			this.PerformTest("test-14.ttl", "test-14.out", "http://www.w3.org/2141/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_15()
		{
			this.PerformTest("test-15.ttl", "test-15.out", "http://www.w3.org/2151/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_16()
		{
			this.PerformTest("test-16.ttl", "test-16.out", "http://www.w3.org/2161/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_17()
		{
			this.PerformTest("test-17.ttl", "test-17.out", "http://www.w3.org/2171/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_18()
		{
			this.PerformTest("test-18.ttl", "test-18.out", "http://www.w3.org/2181/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_19()
		{
			this.PerformTest("test-19.ttl", "test-19.out", "http://www.w3.org/2191/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_20()
		{
			this.PerformTest("test-20.ttl", "test-20.out", "http://www.w3.org/2201/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_21()
		{
			this.PerformTest("test-21.ttl", "test-21.out", "http://www.w3.org/2211/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_22()
		{
			this.PerformTest("test-22.ttl", "test-22.out", "http://www.w3.org/2221/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_23()
		{
			this.PerformTest("test-23.ttl", "test-23.out", "http://www.w3.org/2231/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_24()
		{
			this.PerformTest("test-24.ttl", "test-24.out", "http://www.w3.org/2241/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_25()
		{
			this.PerformTest("test-25.ttl", "test-25.out", "http://www.w3.org/2251/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_26()
		{
			this.PerformTest("test-26.ttl", "test-26.out", "http://www.w3.org/2261/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_27()
		{
			this.PerformTest("test-27.ttl", "test-27.out", "http://www.w3.org/2271/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_28()
		{
			this.PerformTest("test-28.ttl", "test-28.out", "http://www.w3.org/2281/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_29()
		{
			this.PerformTest("test-29.ttl", "test-29.out", "http://www.w3.org/2291/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void Test_30()
		{
			this.PerformTest("test-30.ttl", "test-30.out", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_00()
		{
			this.PerformTest("bad-00.ttl", "http://www.w3.org/2001/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_01()
		{
			this.PerformTest("bad-01.ttl", "http://www.w3.org/2011/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_02()
		{
			this.PerformTest("bad-02.ttl", "http://www.w3.org/2021/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_03()
		{
			this.PerformTest("bad-03.ttl", "http://www.w3.org/2031/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_04()
		{
			this.PerformTest("bad-04.ttl", "http://www.w3.org/2041/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_05()
		{
			this.PerformTest("bad-05.ttl", "http://www.w3.org/2051/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_06()
		{
			this.PerformTest("bad-06.ttl", "http://www.w3.org/2061/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_07()
		{
			this.PerformTest("bad-07.ttl", "http://www.w3.org/2071/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_08()
		{
			this.PerformTest("bad-08.ttl", "http://www.w3.org/2081/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_09()
		{
			this.PerformTest("bad-09.ttl", "http://www.w3.org/2091/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_10()
		{
			this.PerformTest("bad-10.ttl", "http://www.w3.org/2101/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_11()
		{
			this.PerformTest("bad-11.ttl", "http://www.w3.org/2111/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_12()
		{
			this.PerformTest("bad-12.ttl", "http://www.w3.org/2121/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_13()
		{
			this.PerformTest("bad-13.ttl", "http://www.w3.org/2131/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		[ExpectedException(typeof(ParsingException))]
		public void Bad_14()
		{
			this.PerformTest("bad-14.ttl", "http://www.w3.org/2141/sw/DataAccess/df1/bads/");
		}

		[TestMethod]
		public void Manifest()
		{
			this.PerformTest("manifest.ttl", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void ManifestBad()
		{
			this.PerformTest("manifest-bad.ttl", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void RdfqResults()
		{
			this.PerformTest("rdfq-results.ttl", "rdfq-results.out", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void RdfSchema()
		{
			this.PerformTest("rdf-schema.ttl", "rdf-schema.out", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		[TestMethod]
		public void RdfsNamespace()
		{
			this.PerformTest("rdfs-namespace.ttl", "rdfs-namespace.out", "http://www.w3.org/2301/sw/DataAccess/df1/tests/");
		}

		private void PerformTest(string FileName)
		{
			string Text = Resources.LoadResourceAsText(typeof(TurtleTests).Namespace + ".Data.Turtle.Input." + FileName);
			TurtleDocument Parsed = new(Text);

			foreach (ISemanticTriple Triple in Parsed)
			{
				Console.Out.Write(Triple.Subject);
				Console.Out.Write("\t");
				Console.Out.Write(Triple.Predicate);
				Console.Out.Write("\t");
				Console.Out.Write(Triple.Object);
				Console.Out.WriteLine();
			}
		}

		private void PerformTest(string FileName, string BaseUri)
		{
			string Text = Resources.LoadResourceAsText(typeof(TurtleTests).Namespace + ".Data.Turtle.Input." + FileName);
			TurtleDocument Parsed = new(Text, new Uri(BaseUri));

			foreach (ISemanticTriple Triple in Parsed)
			{
				Console.Out.Write(Triple.Subject);
				Console.Out.Write("\t");
				Console.Out.Write(Triple.Predicate);
				Console.Out.Write("\t");
				Console.Out.Write(Triple.Object);
				Console.Out.WriteLine();
			}
		}

		private void PerformTest(string FileName, string ExpectedFileName, string BaseUri)
		{
			string Text = Resources.LoadResourceAsText(typeof(TurtleTests).Namespace + ".Data.Turtle.Input." + FileName);
			string Expected = Resources.LoadResourceAsText(typeof(TurtleTests).Namespace + ".Data.Turtle.Input." + ExpectedFileName);
			TurtleDocument Parsed = new(Text, new Uri(BaseUri));
			TurtleDocument ParsedExpected = new(Expected, new Uri(BaseUri));

			foreach (ISemanticTriple Triple in Parsed)
			{
				Console.Out.Write(Triple.Subject);
				Console.Out.Write("\t");
				Console.Out.Write(Triple.Predicate);
				Console.Out.Write("\t");
				Console.Out.Write(Triple.Object);
				Console.Out.WriteLine();
			}
		}
	}
}