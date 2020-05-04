using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Xml;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptSqlTests
	{
		private static FilesProvider filesProvider = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(typeof(Expression).Assembly,
				typeof(Graphs.Graph).Assembly,
				typeof(XmlParser).Assembly,
				typeof(System.Text.RegularExpressions.Regex).Assembly,
				typeof(Persistence.SQL.Select).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly);

			filesProvider = new FilesProvider("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000);
			Database.Register(filesProvider);

			await Database.Clear("Orders");

			await Database.Insert(new Data.Order()
			{
				OrderID = 1,
				CustomerID = 2,
				OrderDate = new DateTime(2020, 04, 30)
			});

			await Database.Insert(new Data.Order()
			{
				OrderID = 2,
				CustomerID = 3,
				OrderDate = new DateTime(2020, 05, 1)
			});

			await Database.Insert(new Data.Order()
			{
				OrderID = 3,
				CustomerID = 4,
				OrderDate = new DateTime(2020, 05, 2)
			});

			await Database.Clear("Customers");

			await Database.Insert(new Data.Customer()
			{
				CustomerID = 1,
				CustomerName = "P1",
				ContactName = "CP1",
				Country = "C1"
			});

			await Database.Insert(new Data.Customer()
			{
				CustomerID = 2,
				CustomerName = "P2",
				ContactName = "CP2",
				Country = "C2"
			});

			await Database.Insert(new Data.Customer()
			{
				CustomerID = 3,
				CustomerName = "P3",
				ContactName = "CP3",
				Country = "C2"
			});
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			filesProvider?.Dispose();
			filesProvider = null;
		}

		private void Test(string Script, object[][] ExpectedOutput)
		{
			Variables v = new Variables();
			Expression Exp = new Expression(Script);
			object Obj = Exp.Evaluate(v);
			ObjectMatrix M = Obj as ObjectMatrix;
			int NrRows, RowIndex;
			int NrColumns, ColumnIndex;

			Assert.IsNotNull(M, "Object matrix expected.");
			Assert.AreEqual(NrRows = ExpectedOutput.Length, M.Rows, "Number of rows in response incorrect.");

			for (RowIndex = 0; RowIndex < NrRows; RowIndex++)
			{
				object[] ExpectedRow = ExpectedOutput[RowIndex];
				ObjectVector Row = M.GetRow(RowIndex) as ObjectVector;

				Assert.IsNotNull(Row, "Object row vector expected.");
				Assert.AreEqual(NrColumns = ExpectedRow.Length, Row.Dimension, "Number of columns in response incorrect.");

				for (ColumnIndex = 0; ColumnIndex < NrColumns; ColumnIndex++)
					Assert.AreEqual(ExpectedRow[ColumnIndex], Row.GetElement(ColumnIndex).AssociatedObjectValue);
			}
		}

		[TestMethod]
		public void SQL_Test_01_Orders()
		{
			this.Test("Select OrderID, CustomerID, OrderDate from Orders",
				new object[][]
				{
					new object[] { 1d, 2d, new DateTime(2020, 4, 30) },
					new object[] { 2d, 3d, new DateTime(2020, 5, 1) },
					new object[] { 3d, 4d, new DateTime(2020, 5, 2) }
				});
		}

		[TestMethod]
		public void SQL_Test_02_Customers()
		{
			this.Test("Select CustomerID, CustomerName, ContactName, Country from Customers",
				new object[][]
				{
					new object[] { 1d, "P1", "CP1", "C1" },
					new object[] { 2d, "P2", "CP2", "C2" },
					new object[] { 3d, "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SQL_Test_03_INNER_JOIN()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders inner join Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

	}
}