using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Script.Objects.Matrices;

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

		[DataTestMethod]
		[DataRow("Test_01.ttl", "Test_01.rq", null)]
		[DataRow("Test_02.ttl", "Test_02.rq", null)]
		[DataRow("Test_03.ttl", "Test_03.rq", "data.n3")]
		public async Task SELECT_Tests(string DataSetFileName, string QueryFileName,
			string SourceName)
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

			SparqlResultSet ResultSet = Result as SparqlResultSet;
			Assert.IsNotNull(ResultSet);

			ObjectMatrix M = ResultSet.ToMatrix() as ObjectMatrix;
			Assert.IsNotNull(M);
			Assert.IsNotNull(M.ColumnNames);

			Console.Out.WriteLine(Expression.ToString(M));
		}

	}
}
 