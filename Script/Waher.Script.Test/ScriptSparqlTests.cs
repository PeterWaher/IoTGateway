using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Runtime.Console;
using Waher.Runtime.IO;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Persistence.SPARQL;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptSparqlTests
	{
		private static string LoadTextResource(string FileName)
		{
			return Files.LoadResourceAsText(typeof(ScriptSparqlTests).Namespace + ".Sparql." + FileName);
		}

		private static TurtleDocument LoadTurtleResource(string FileName)
		{
			string Text = LoadTextResource("Query." + FileName);
			return new TurtleDocument(Text);
		}

		private static async Task<(string, SparqlResultSet)> LoadSparqlResultSet(string FileName)
		{
			string Extension = Path.GetExtension(FileName);
			string ContentType = InternetContent.GetContentType(Extension);

			Assert.IsFalse(string.IsNullOrEmpty(ContentType));

			byte[] Bin = Resources.LoadResource(typeof(ScriptSparqlTests).Namespace + ".Sparql.Query." + FileName);
			ContentResponse Decoded = await InternetContent.DecodeAsync(ContentType, Bin, null);
			Decoded.AssertOk();

			SparqlResultSet Result = Decoded.Decoded as SparqlResultSet;
			Assert.IsNotNull(Result);

			return (Strings.GetString(Bin, Encoding.UTF8), Result);
		}

		private static Expression LoadSparqlExpression(string FileName)
		{
			string s = LoadTextResource("Expression." + FileName);
			s = "SELECT ?x WHERE { VALUES ?y {1} BIND(" + s + " AS ?x) }";
			return new Expression(s);
		}

		private static Expression LoadResultExpression(string FileName)
		{
			string s = LoadTextResource("Expression." + FileName);
			return new Expression(s);
		}

		[DataTestMethod]
		[DataRow("Test_001.ttl", "Test_001.rq", null, "Test_001.srx")]
		[DataRow("Test_002.ttl", "Test_002.rq", null, "Test_002.srx")]
		[DataRow("Test_003.ttl", "Test_003.rq", "data.n3", "Test_003.srx")]
		[DataRow("Test_004.ttl", "Test_004.rq", "data.n3", "Test_004.srx")]
		[DataRow("Test_005.ttl", "Test_005.rq", "data.n3", "Test_005.srx")]
		[DataRow("Test_006.ttl", "Test_006.rq", null, "Test_006.srx")]
		[DataRow("Test_007.ttl", "Test_007.rq", null, "Test_007.srx")]
		[DataRow("Test_008.ttl", "Test_008.rq", null, "Test_008.srx")]
		[DataRow("Test_009.ttl", "Test_009.rq", null, "Test_009.srx")]
		[DataRow("Test_010.ttl", "Test_010.rq", null, "Test_010.srx")]
		[DataRow("Test_011.ttl", "Test_011.rq", null, "Test_011.srx")]
		[DataRow("Test_012.ttl", "Test_012.rq", null, "Test_012.srx")]
		[DataRow("Test_013.ttl", "Test_013.rq", null, "Test_013b.ttl")]
		[DataRow("Test_014.ttl", "Test_014.rq", null, "Test_014.srx")]
		[DataRow("Test_015.ttl", "Test_015.rq", null, "Test_015.srx")]
		[DataRow("Test_016.ttl", "Test_016.rq", null, "Test_016.srx")]
		[DataRow("Test_017.ttl", "Test_017.rq", null, "Test_017.srx")]
		[DataRow("Test_018.ttl", "Test_018.rq", null, "Test_018.srx")]
		[DataRow("Test_019.ttl", "Test_019.rq", null, "Test_019.srx")]
		[DataRow("Test_020.ttl", "Test_020.rq", null, "Test_020.srx")]
		[DataRow("Test_021.ttl", "Test_021.rq", null, "Test_021.srx")]
		[DataRow("Test_022.ttl", "Test_022.rq", null, "Test_022.srx")]
		[DataRow("Test_023.ttl", "Test_023.rq", null, "Test_023.srx")]
		[DataRow("Test_024.ttl", "Test_024.rq", null, "Test_024.srx")]
		[DataRow("Test_025.ttl", "Test_025.rq", null, "Test_025.srx")]
		[DataRow("Test_026.ttl", "Test_026.rq", null, "Test_026.srx")]
		[DataRow("Test_027.ttl", "Test_027.rq", null, "Test_027.srx")]
		[DataRow("Test_028.ttl", "Test_028.rq", null, "Test_028.srx")]
		[DataRow("Test_029.ttl", "Test_029.rq", null, "Test_029.srx")]
		[DataRow("Test_030.ttl", "Test_030.rq", null, "Test_030.srx")]
		[DataRow("Test_031.ttl", "Test_031.rq", null, "Test_031.srx")]
		[DataRow("Test_032.ttl", "Test_032.rq", null, "Test_032.srx")]
		[DataRow("Test_032.ttl", "Test_032b.rq", null, "Test_032.srx")]
		[DataRow("Test_033.ttl", "Test_033.rq", null, "Test_033.srx")]
		[DataRow("Test_034.ttl", "Test_034.rq", null, "Test_034.srx")]
		[DataRow("Test_034.ttl", "Test_034b.rq", null, "Test_034.srx")]
		[DataRow("Test_035.ttl", "Test_035.rq", null, "Test_035.srx")]
		[DataRow("Test_036.ttl", "Test_036.rq", null, "Test_036.srx")]
		[DataRow("Test_037.ttl", "Test_037.rq", null, "Test_037.srx")]
		[DataRow("Test_038.ttl", "Test_038.rq", null, "Test_038.srx")]
		[DataRow("Test_039.ttl", "Test_039.rq", "http://example.org/foaf/aliceFoaf", "Test_039.srx")]
		[DataRow("Test_040.ttl|Test_040a.ttl|Test_040b.ttl", "Test_040.rq", "http://example.org/dft.ttl|http://example.org/bob|http://example.org/alice", "Test_040.srx")]
		[DataRow("Test_041a.ttl|Test_041b.ttl", "Test_041.rq", "http://example.org/foaf/aliceFoaf|http://example.org/foaf/bobFoaf", "Test_041.srx")]
		[DataRow("Test_042a.ttl|Test_042b.ttl", "Test_042.rq", "http://example.org/foaf/aliceFoaf|http://example.org/foaf/bobFoaf", "Test_042.srx")]
		[DataRow("Test_043a.ttl|Test_043b.ttl", "Test_043.rq", "http://example.org/foaf/aliceFoaf|http://example.org/foaf/bobFoaf", "Test_043.srx")]
		[DataRow("Test_044.ttl|Test_044a.ttl|Test_044b.ttl", "Test_044.rq", "|tag:example.org,2005-06-06:graph1|tag:example.org,2005-06-06:graph2", "Test_044.srx")]
		[DataRow("Test_045.ttl", "Test_045.rq", null, "Test_045.srx")]
		[DataRow("Test_046.ttl", "Test_046.rq", null, "Test_046.srx")]
		[DataRow("Test_047.ttl", "Test_047.rq", null, "Test_047.srx")]
		[DataRow("Test_048.ttl", "Test_048.rq", null, "Test_048.srx")]
		[DataRow("Test_049.ttl", "Test_049.rq", null, "Test_049.srj")]
		[DataRow("Test_050.ttl", "Test_050.rq", null, "Test_050.srx")]
		[DataRow("Test_051.ttl", "Test_051.rq", null, "Test_051.srj")]
		[DataRow("Test_052.ttl", "Test_052.rq", null, "Test_052.srj")]
		[DataRow("Test_053.ttl", "Test_053.rq", null, "Test_053r.ttl")]
		[DataRow("Test_054.ttl", "Test_054.rq", null, "Test_054r.ttl")]
		[DataRow("Test_055.ttl", "Test_055.rq", null, "Test_055r.ttl")]
		[DataRow("Test_056.ttl", "Test_056.rq", null, "Test_056r.ttl")]
		[DataRow("Test_057.ttl", "Test_057.rq", null, "Test_057.srj")]
		[DataRow("Test_058.ttl", "Test_058.rq", null, "Test_058.srx")]
		[DataRow("Test_059.ttl", "Test_059.rq", null, "Test_059.srj")]
		[DataRow("Test_060.ttl", "Test_060.rq", null, "Test_060.srj")]
		[DataRow("Test_061.ttl", "Test_061.rq", null, "Test_061.srj")]
		[DataRow("Test_062.ttl", "Test_062.rq", null, "Test_062.srj")]
		[DataRow("Test_063.ttl", "Test_063.rq", null, "Test_063.srj")]
		[DataRow("Test_064.ttl", "Test_064.rq", null, "Test_064.srj")]
		public async Task Query_Test(string DataSetFileName, string QueryFileName,
			string SourceName, string ResultName)
		{
			List<TurtleDocument> Docs = [];
			Variables v = [];
			string[] SourceFileNames = DataSetFileName.Split('|');
			string[] SourceUris = SourceName?.Split('|');
			int i, c;

			Assert.AreEqual(c = SourceFileNames.Length, SourceUris?.Length ?? 1);

			for (i = 0; i < c; i++)
			{
				TurtleDocument Doc = LoadTurtleResource(SourceFileNames[i]);
				Docs.Add(Doc);

				if (SourceUris is null || string.IsNullOrEmpty(SourceUris[i]))
					v[" Default Graph "] = Doc;
				else
					v[" " + SourceUris[i] + " "] = Doc;
			}

			await Test(v, QueryFileName, ResultName, SourceUris, [.. Docs]);
		}

		private static async Task Test(Variables v, string QueryFileName, string ResultName,
			string[] SourceUris, TurtleDocument[] Docs)
		{
			string Query = LoadTextResource("Query." + QueryFileName);
			Expression Exp = new(Query);

			if (SourceUris is not null && Exp.Root is SparqlQuery SparqlQuery && SparqlQuery.NamedGraphNames.Length == 0)
				SparqlQuery.RegisterNamedGraph(SourceUris);

			object Result = await Exp.EvaluateAsync(v);
			Assert.IsNotNull(Result);

			if (Result is SparqlResultSet ResultSet)
			{
				IMatrix M = ResultSet.ToMatrix();
				Assert.IsNotNull(M);

				ConsoleOut.WriteLine(Expression.ToString(M));
				ConsoleOut.WriteLine();
				ConsoleOut.WriteLine(Query);

				foreach (TurtleDocument Doc in Docs)
				{
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine(Doc.Text);
				}

				if (!string.IsNullOrEmpty(ResultName))
				{
					(string ExpectedDoc, SparqlResultSet Expected) = await LoadSparqlResultSet(ResultName);

					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine(ExpectedDoc);

					Assert.IsFalse(Expected.BooleanResult.HasValue ^ ResultSet.BooleanResult.HasValue);
					if (Expected.BooleanResult.HasValue)
						Assert.AreEqual(Expected.BooleanResult.Value, ResultSet.BooleanResult.Value);

					int i, c;// = Expected.Variables?.Length ?? 0;
							 //Assert.AreEqual(c, ResultSet.Variables?.Length ?? 0, "Variable count not as expected.");
							 //
							 //for (i = 0; i < c; i++)
							 //	Assert.AreEqual(Expected.Variables[i], ResultSet.Variables[i]);

					c = Expected.Records?.Length ?? 0;
					Assert.AreEqual(c, ResultSet.Records?.Length ?? 0, "Record count not as expected.");

					Dictionary<string, string> BlankNodeDictionary = [];

					for (i = 0; i < c; i++)
					{
						ISparqlResultRecord ExpectedRecord = Expected.Records[i];
						ISparqlResultRecord Record = ResultSet.Records[i];

						foreach (string VariableName in Expected.Variables)
						{
							ISemanticElement e1 = ExpectedRecord[VariableName];
							ISemanticElement e2 = Record[VariableName];

							Assert.IsFalse((e1 is null) ^ (e2 is null));

							if (e1 is null)
								continue;

							AssertEqual(e1, e2, BlankNodeDictionary);
						}
					}
				}
			}
			else if (Result is InMemorySemanticModel Model)
			{
				IMatrix M = Model.ToMatrix();
				Assert.IsNotNull(M);

				ConsoleOut.WriteLine(Expression.ToString(M));

				if (!string.IsNullOrEmpty(ResultName))
				{
					TurtleDocument Expected = LoadTurtleResource(ResultName);

					Dictionary<string, string> BlankNodeDictionary = [];
					IEnumerator<ISemanticTriple> e1 = Expected.GetEnumerator();
					IEnumerator<ISemanticTriple> e2 = Model.GetEnumerator();
					bool b1 = e1.MoveNext();
					bool b2 = e2.MoveNext();

					while (b1 && b2)
					{
						AssertEqual(e1.Current.Subject, e2.Current.Subject, BlankNodeDictionary);
						AssertEqual(e1.Current.Predicate, e2.Current.Predicate, BlankNodeDictionary);
						AssertEqual(e1.Current.Object, e2.Current.Object, BlankNodeDictionary);

						b1 = e1.MoveNext();
						b2 = e2.MoveNext();
					}

					Assert.IsFalse(b1);
					Assert.IsFalse(b2);
				}
			}
		}

		private static void AssertEqual(ISemanticElement e1, ISemanticElement e2,
			Dictionary<string, string> BlankNodeDictionary)
		{
			if (e1 is BlankNode bn1 && e2 is BlankNode bn2)
			{
				if (BlankNodeDictionary.TryGetValue(bn1.NodeId, out string s))
					Assert.AreEqual(s, bn2.NodeId);
				else
					BlankNodeDictionary[bn1.NodeId] = bn2.NodeId;
			}
			else
				Assert.AreEqual(e1, e2);
		}

		[DataTestMethod]
		[DataRow("Test_001.expression", "Test_001.result")]
		[DataRow("Test_002.expression", "Test_002.result")]
		[DataRow("Test_003.expression", "Test_003.result")]
		[DataRow("Test_004.expression", "Test_004.result")]
		[DataRow("Test_005.expression", "Test_005.result")]
		[DataRow("Test_006.expression", "Test_006.result")]
		[DataRow("Test_007.expression", "Test_007.result")]
		[DataRow("Test_008.expression", "Test_008.result")]
		[DataRow("Test_009.expression", "Test_009.result")]
		[DataRow("Test_010.expression", "Test_010.result")]
		[DataRow("Test_011.expression", "Test_011.result")]
		[DataRow("Test_012.expression", "Test_012.result")]
		[DataRow("Test_013.expression", "Test_013.result")]
		[DataRow("Test_014.expression", "Test_014.result")]
		[DataRow("Test_015.expression", "Test_015.result")]
		[DataRow("Test_016.expression", "Test_016.result")]
		[DataRow("Test_017.expression", "Test_017.result")]
		[DataRow("Test_018.expression", "Test_018.result")]
		public async Task Expression_Test(string SparqlExpressionFileName, string ResultExpression)
		{
			Expression Exp1 = LoadSparqlExpression(SparqlExpressionFileName);
			Expression Exp2 = LoadResultExpression(ResultExpression);
			object Result1;
			object Result2;

			try
			{
				Result1 = await Exp1.EvaluateAsync(new Variables
				{
					[" Default Graph "] = new InMemorySemanticCube()
				});

				if (Result1 is SparqlResultSet SparqlResult)
				{
					foreach (ISparqlResultRecord Record in SparqlResult.Records)
					{
						Result1 = Record["x"];
						break;
					}
				}

				if (Result1 is IElement E)
					Result1 = E.AssociatedObjectValue;
			}
			catch (Exception ex)
			{
				Result1 = ex;
			}

			try
			{
				Result2 = await Exp2.EvaluateAsync([]);
			}
			catch (Exception ex)
			{
				Result2 = ex;
			}

			if (Result1 is null)
				Assert.IsTrue(Result2 is Exception);
			else
				Assert.IsTrue(Result1.Equals(Result2));
		}

		// TODO: Property paths.
		// TODO: DESCRIBE.
	}
}
