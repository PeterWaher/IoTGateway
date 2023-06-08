using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptSparqlTests
	{
		private static string LoadTextResource(string FileName)
		{
			return Resources.LoadResourceAsText(typeof(ScriptSparqlTests).Namespace + ".Sparql." + FileName);
		}

		private static TurtleDocument LoadTurtleResource(string FileName)
		{
			string Text = LoadTextResource(FileName);
			return new TurtleDocument(Text);
		}

		private static async Task<SparqlResultSet> LoadSparqlResultSet(string FileName)
		{
			byte[] Bin = Resources.LoadResource(typeof(ScriptSparqlTests).Namespace + ".Sparql." + FileName);
			object Decoded = await InternetContent.DecodeAsync("application/sparql-results+xml", Bin, null);
			Assert.IsNotNull(Decoded);

			SparqlResultSet Result = Decoded as SparqlResultSet;
			Assert.IsNotNull(Result);

			return Result;
		}

		[DataTestMethod]
		[DataRow("Test_01.ttl", "Test_01.rq", null, null)]
		[DataRow("Test_02.ttl", "Test_02.rq", null, null)]
		[DataRow("Test_03.ttl", "Test_03.rq", "data.n3", "Test_03.srx")]
		[DataRow("Test_04.ttl", "Test_04.rq", "data.n3", "Test_04.srx")]
		[DataRow("Test_05.ttl", "Test_05.rq", "data.n3", "Test_05.srx")]
		[DataRow("Test_06.ttl", "Test_06.rq", null, null)]
		[DataRow("Test_07.ttl", "Test_07.rq", null, null)]
		[DataRow("Test_08.ttl", "Test_08.rq", null, null)]
		[DataRow("Test_09.ttl", "Test_09.rq", null, null)]
		[DataRow("Test_10.ttl", "Test_10.rq", null, null)]
		[DataRow("Test_11.ttl", "Test_11.rq", null, null)]
		[DataRow("Test_12.ttl", "Test_12.rq", null, null)]
		[DataRow("Test_13.ttl", "Test_13.rq", null, "Test_13b.ttl")]
		public async Task SPARQL_Tests(string DataSetFileName, string QueryFileName,
			string SourceName, string ResultName)
		{
			TurtleDocument Doc = LoadTurtleResource(DataSetFileName);
			string Query = LoadTextResource(QueryFileName);
			Expression Exp = new Expression(Query);
			Variables v = new Variables();

			if (string.IsNullOrEmpty(SourceName))
				v[" Default Graph "] = Doc;
			else
				v[" " + SourceName + " "] = Doc;

			object Result = await Exp.EvaluateAsync(v);
			Assert.IsNotNull(Result);

			if (Result is SparqlResultSet ResultSet)
			{
				IMatrix M = ResultSet.ToMatrix();
				Assert.IsNotNull(M);

				Console.Out.WriteLine(Expression.ToString(M));

				if (!string.IsNullOrEmpty(ResultName))
				{
					SparqlResultSet Expected = await LoadSparqlResultSet(ResultName);

					Assert.IsFalse(Expected.BooleanResult.HasValue ^ ResultSet.BooleanResult.HasValue);
					if (Expected.BooleanResult.HasValue)
						Assert.AreEqual(Expected.BooleanResult.Value, ResultSet.BooleanResult.Value);

					int i, c = Expected.Variables?.Length ?? 0;
					Assert.AreEqual(c, ResultSet.Variables?.Length ?? 0, "Variable count not as expected.");

					for (i = 0; i < c; i++)
						Assert.AreEqual(Expected.Variables[i], ResultSet.Variables[i]);

					c = Expected.Records?.Length ?? 0;
					Assert.AreEqual(c, ResultSet.Records?.Length ?? 0, "Record count not as expected.");

					Dictionary<string, string> BlankNodeDictionary = new Dictionary<string, string>();

					for (i = 0; i < c; i++)
					{
						SparqlResultRecord ExpectedRecord = Expected.Records[i];
						SparqlResultRecord Record = ResultSet.Records[i];

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

				Console.Out.WriteLine(Expression.ToString(M));

				if (!string.IsNullOrEmpty(ResultName))
				{
					TurtleDocument Expected = LoadTurtleResource(ResultName);

					Dictionary<string, string> BlankNodeDictionary = new Dictionary<string, string>();
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

	}
}
