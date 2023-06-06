using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects.Matrices;
using Waher.Script.Xml;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptSparqlTests
	{
		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
		}

		private async Task Test(string Script, object[][] ExpectedOutput)
		{
			Variables v = new Variables();
			Expression Exp = new Expression(Script);
			object Obj = await Exp.EvaluateAsync(v);
			Console.Out.WriteLine(Expression.ToString(Obj));
			
			ObjectMatrix M = Obj as ObjectMatrix;
			int NrRows, RowIndex;
			int NrColumns, ColumnIndex;

			Assert.IsNotNull(M, "Object matrix expected.");
			Assert.AreEqual(NrRows = ExpectedOutput.Length, M.Rows, "Number of rows in response incorrect.");

			for (RowIndex = 0; RowIndex < NrRows; RowIndex++)
			{
				object[] ExpectedRow = ExpectedOutput[RowIndex];
				IVector Row = M.GetRow(RowIndex);

				Assert.IsNotNull(Row, "Object row vector expected.");
				Assert.AreEqual(NrColumns = ExpectedRow.Length, Row.Dimension, "Number of columns in response incorrect.");

				for (ColumnIndex = 0; ColumnIndex < NrColumns; ColumnIndex++)
					Assert.AreEqual(ExpectedRow[ColumnIndex], Row.GetElement(ColumnIndex).AssociatedObjectValue);
			}
	
			ScriptParsingTests.AssertParentNodesAndSubsexpressions(Exp);

			Console.Out.WriteLine();
			Exp.ToXml(Console.Out);
			Console.Out.WriteLine();
		}

		#region SELECT

		[TestMethod]
		public async Task SELECT_Test_01_Orders()
		{
			await this.Test("Select OrderID, CustomerID, OrderDate from Orders",
				new object[][]
				{
					new object[] { 1d, 2d, new DateTime(2020, 4, 30) },
					new object[] { 2d, 3d, new DateTime(2020, 5, 1) },
					new object[] { 3d, 4d, new DateTime(2020, 5, 2) }
				});
		}
		
		#endregion

	}
}
 