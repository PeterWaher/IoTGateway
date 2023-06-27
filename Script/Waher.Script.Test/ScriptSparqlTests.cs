using System;
using System.Collections.Generic;
using System.Text;
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

		private static async Task<(string, SparqlResultSet)> LoadSparqlResultSet(string FileName)
		{
			byte[] Bin = Resources.LoadResource(typeof(ScriptSparqlTests).Namespace + ".Sparql." + FileName);
			object Decoded = await InternetContent.DecodeAsync("application/sparql-results+xml", Bin, null);
			Assert.IsNotNull(Decoded);

			SparqlResultSet Result = Decoded as SparqlResultSet;
			Assert.IsNotNull(Result);

			return (CommonTypes.GetString(Bin, Encoding.UTF8), Result);
		}

		[DataTestMethod]
		[DataRow("Test_01.ttl", "Test_01.rq", null, "Test_01.srx")]
		[DataRow("Test_02.ttl", "Test_02.rq", null, "Test_02.srx")]
		[DataRow("Test_03.ttl", "Test_03.rq", "data.n3", "Test_03.srx")]
		[DataRow("Test_04.ttl", "Test_04.rq", "data.n3", "Test_04.srx")]
		[DataRow("Test_05.ttl", "Test_05.rq", "data.n3", "Test_05.srx")]
		[DataRow("Test_06.ttl", "Test_06.rq", null, "Test_06.srx")]
		[DataRow("Test_07.ttl", "Test_07.rq", null, "Test_07.srx")]
		[DataRow("Test_08.ttl", "Test_08.rq", null, "Test_08.srx")]
		[DataRow("Test_09.ttl", "Test_09.rq", null, "Test_09.srx")]
		[DataRow("Test_10.ttl", "Test_10.rq", null, "Test_10.srx")]
		[DataRow("Test_11.ttl", "Test_11.rq", null, "Test_11.srx")]
		[DataRow("Test_12.ttl", "Test_12.rq", null, "Test_12.srx")]
		[DataRow("Test_13.ttl", "Test_13.rq", null, "Test_13b.ttl")]
		[DataRow("Test_14.ttl", "Test_14.rq", null, "Test_14.srx")]
		[DataRow("Test_15.ttl", "Test_15.rq", null, "Test_15.srx")]
		[DataRow("Test_16.ttl", "Test_16.rq", null, "Test_16.srx")]
		[DataRow("Test_17.ttl", "Test_17.rq", null, "Test_17.srx")]
		[DataRow("Test_18.ttl", "Test_18.rq", null, "Test_18.srx")]
		[DataRow("Test_19.ttl", "Test_19.rq", null, "Test_19.srx")]
		[DataRow("Test_20.ttl", "Test_20.rq", null, "Test_20.srx")]
		[DataRow("Test_21.ttl", "Test_21.rq", null, "Test_21.srx")]
		[DataRow("Test_22.ttl", "Test_22.rq", null, "Test_22.srx")]
		[DataRow("Test_23.ttl", "Test_23.rq", null, "Test_23.srx")]
		[DataRow("Test_24.ttl", "Test_24.rq", null, "Test_24.srx")]
		[DataRow("Test_25.ttl", "Test_25.rq", null, "Test_25.srx")]
		[DataRow("Test_26.ttl", "Test_26.rq", null, "Test_26.srx")]
		[DataRow("Test_27.ttl", "Test_27.rq", null, "Test_27.srx")]
		[DataRow("Test_28.ttl", "Test_28.rq", null, "Test_28.srx")]
		[DataRow("Test_29.ttl", "Test_29.rq", null, "Test_29.srx")]
		[DataRow("Test_30.ttl", "Test_30.rq", null, "Test_30.srx")]
		[DataRow("Test_31.ttl", "Test_31.rq", null, "Test_31.srx")]
		[DataRow("Test_32.ttl", "Test_32.rq", null, "Test_32.srx")]
		[DataRow("Test_32.ttl", "Test_32b.rq", null, "Test_32.srx")]
		[DataRow("Test_33.ttl", "Test_33.rq", null, "Test_33.srx")]
		[DataRow("Test_34.ttl", "Test_34.rq", null, "Test_34.srx")]
		[DataRow("Test_34.ttl", "Test_34b.rq", null, "Test_34.srx")]
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
				Console.Out.WriteLine();
				Console.Out.WriteLine(Query);
				Console.Out.WriteLine();
				Console.Out.WriteLine(Doc.Text);

				if (!string.IsNullOrEmpty(ResultName))
				{
					(string ExpectedDoc, SparqlResultSet Expected) = await LoadSparqlResultSet(ResultName);

					Console.Out.WriteLine();
					Console.Out.WriteLine(ExpectedDoc);

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

		// TODO: Property paths.
	}
}
