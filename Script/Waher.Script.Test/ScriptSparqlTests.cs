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
		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
		}

		private static string LoadTextResource(string FileName)
		{
			return Resources.LoadResourceAsText(typeof(ScriptSparqlTests).Namespace + ".Sparql." + FileName);
		}

		private static TurtleDocument LoadTurtleResource(string FileName)
		{
			string Text = LoadTextResource(FileName);
			return new TurtleDocument(Text);
		}

		#region SELECT

		[DataTestMethod]
		[DataRow("Test_01.ttl", "Test_01.rq")]
		public async Task Test_01_SELECT(string DataSetFileName, string QueryFileName)
		{
			TurtleDocument Doc = LoadTurtleResource(DataSetFileName);
			string Query = LoadTextResource(QueryFileName);
			Expression Exp = new Expression(Query);
			Variables v = new Variables
			{
				[" Default Graph "] = Doc
			};

			object Result = await Exp.EvaluateAsync(v);
			Assert.IsNotNull(Result);

			ObjectMatrix M = Result as ObjectMatrix;
			Assert.IsNotNull(M);
			Assert.IsNotNull(M.ColumnNames);

			Console.Out.WriteLine(Expression.ToString(M));
		}

		#endregion

	}
}
 