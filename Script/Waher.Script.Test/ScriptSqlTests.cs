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
				typeof(Graphs3D.Graph3D).Assembly,
				typeof(XmlParser).Assembly,
				typeof(System.Text.RegularExpressions.Regex).Assembly,
				typeof(Persistence.SQL.Select).Assembly,
				typeof(ScriptSqlTests).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly);

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000);
			Database.Register(filesProvider);

			await Database.Clear("Orders");
			await Database.Clear("WebUsers");

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
			Console.Out.WriteLine(Expression.ToString(Obj));
			
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
		public void SELECT_Test_01_Orders()
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
		public void SELECT_Test_02_Orders_Typed()
		{
			this.Test("Select OrderID, CustomerID, OrderDate from Waher.Script.Test.Data.Order",
				new object[][]
				{
					new object[] { 1d, 2d, new DateTime(2020, 4, 30) },
					new object[] { 2d, 3d, new DateTime(2020, 5, 1) },
					new object[] { 3d, 4d, new DateTime(2020, 5, 2) }
				});
		}

		[TestMethod]
		public void SELECT_Test_03_Customers()
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
		public void SELECT_Test_04_Customers_Typed()
		{
			this.Test("Select CustomerID, CustomerName, ContactName, Country from Waher.Script.Test.Data.Customer as Customers",
				new object[][]
				{
					new object[] { 1d, "P1", "CP1", "C1" },
					new object[] { 2d, "P2", "CP2", "C2" },
					new object[] { 3d, "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_05_INNER_JOIN()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders inner join Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_06_INNER_JOIN_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders inner join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_07_LEFT_OUTER_JOIN()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders left outer join Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null }
				});
		}

		[TestMethod]
		public void SELECT_Test_08_LEFT_OUTER_JOIN_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders left join Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null }
				});
		}

		[TestMethod]
		public void SELECT_Test_09_LEFT_OUTER_JOIN_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders left outer join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null }
				});
		}

		[TestMethod]
		public void SELECT_Test_10_LEFT_OUTER_JOIN_Typed_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders left join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null }
				});
		}

		[TestMethod]
		public void SELECT_Test_11_RIGHT_OUTER_JOIN()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders right outer join Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { null, null, "P1", "CP1", "C1" },
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_12_RIGHT_OUTER_JOIN_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders right join Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { null, null, "P1", "CP1", "C1" },
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_13_RIGHT_OUTER_JOIN_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders right outer join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { null, null, "P1", "CP1", "C1" },
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_14_RIGHT_OUTER_JOIN_Typed_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders right join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { null, null, "P1", "CP1", "C1" },
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_15_FULL_OUTER_JOIN()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders full outer join Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null },
					new object[] { null, null, "P1", "CP1", "C1" }
				});
		}

		[TestMethod]
		public void SELECT_Test_16_FULL_OUTER_JOIN_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders full join Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null },
					new object[] { null, null, "P1", "CP1", "C1" }
				});
		}

		[TestMethod]
		public void SELECT_Test_17_FULL_OUTER_JOIN_3()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders outer join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null },
					new object[] { null, null, "P1", "CP1", "C1" }
				});
		}

		[TestMethod]
		public void SELECT_Test_18_FULL_OUTER_JOIN_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders full outer join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null },
					new object[] { null, null, "P1", "CP1", "C1" }
				});
		}

		[TestMethod]
		public void SELECT_Test_19_FULL_OUTER_JOIN_Typed_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders full join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null },
					new object[] { null, null, "P1", "CP1", "C1" }
				});
		}

		[TestMethod]
		public void SELECT_Test_20_FULL_OUTER_JOIN_Typed_3()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders outer join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 02), null, null, null },
					new object[] { null, null, "P1", "CP1", "C1" }
				});
		}

		[TestMethod]
		public void SELECT_Test_21_CROSS_JOIN()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders, Customers where Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_22_CROSS_JOIN_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders, Customers",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P1", "CP1", "C1" },
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 1d, new DateTime(2020, 4, 30), "P3", "CP3", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P1", "CP1", "C1" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 2), "P1", "CP1", "C1" },
					new object[] { 3d, new DateTime(2020, 5, 2), "P2", "CP2", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 2), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_23_CROSS_JOIN_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders, Waher.Script.Test.Data.Customer as Customers where Orders.CustomerID=Customers.CustomerID",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_24_CROSS_JOIN_Typed_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders, Waher.Script.Test.Data.Customer as Customers",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P1", "CP1", "C1" },
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" },
					new object[] { 1d, new DateTime(2020, 4, 30), "P3", "CP3", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P1", "CP1", "C1" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P3", "CP3", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 2), "P1", "CP1", "C1" },
					new object[] { 3d, new DateTime(2020, 5, 2), "P2", "CP2", "C2" },
					new object[] { 3d, new DateTime(2020, 5, 2), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_25_Orders_WHERE()
		{
			this.Test("Select OrderID, CustomerID, OrderDate from Orders where OrderID=2",
				new object[][]
				{
					new object[] { 2d, 3d, new DateTime(2020, 5, 1) }
				});
		}

		[TestMethod]
		public void SELECT_Test_26_Orders_WHERE_Typed()
		{
			this.Test("Select OrderID, CustomerID, OrderDate from Waher.Script.Test.Data.Order as Orders where OrderID=2",
				new object[][]
				{
					new object[] { 2d, 3d, new DateTime(2020, 5, 1) }
				});
		}

		[TestMethod]
		public void SELECT_Test_27_Customers_WHERE()
		{
			this.Test("Select CustomerID, CustomerName, ContactName, Country from Customers where CustomerID=2",
				new object[][]
				{
					new object[] { 2d, "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_28_Customers_WHERE_Typed()
		{
			this.Test("Select CustomerID, CustomerName, ContactName, Country from Waher.Script.Test.Data.Customer as Customers where CustomerID=2",
				new object[][]
				{
					new object[] { 2d, "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_29_INNER_JOIN_WHERE()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders inner join Customers on Orders.CustomerID=Customers.CustomerID where OrderID=2",
				new object[][]
				{
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_30_INNER_JOIN_WHERE_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders inner join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID where OrderID=2",
				new object[][]
				{
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_31_LEFT_OUTER_JOIN_WHERE()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders left outer join Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_32_LEFT_OUTER_JOIN_WHERE_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders left join Customers on Orders.CustomerID=Customers.CustomerID where Orders.CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_33_LEFT_OUTER_JOIN_WHERE_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders left outer join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_34_LEFT_OUTER_JOIN_WHERE_Typed_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders left join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID where Orders.CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_35_RIGHT_OUTER_JOIN_WHERE()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders right outer join Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_36_RIGHT_OUTER_JOIN_WHERE_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders right join Customers on Orders.CustomerID=Customers.CustomerID where Customers.CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_37_RIGHT_OUTER_JOIN_WHERE_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders right outer join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_38_RIGHT_OUTER_JOIN_WHERE_Typed_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders right join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID where Customers.CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_39_FULL_OUTER_JOIN_WHERE()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders full outer join Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_40_FULL_OUTER_JOIN_WHERE_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders full join Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_41_FULL_OUTER_JOIN_3_WHERE()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders outer join Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_42_FULL_OUTER_JOIN_WHERE_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders full outer join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_43_FULL_OUTER_JOIN_WHERE_Typed_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders full join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_44_FULL_OUTER_JOIN_3_WHERE_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders outer join Waher.Script.Test.Data.Customer as Customers on Orders.CustomerID=Customers.CustomerID where CustomerID=2",
				new object[][]
				{
					new object[] { 1d, new DateTime(2020, 4, 30), "P2", "CP2", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_45_CROSS_JOIN_WHERE()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders, Customers where Orders.CustomerID=Customers.CustomerID and OrderID=2",
				new object[][]
				{
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_46_CROSS_JOIN_WHERE_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Orders, Customers where Orders.OrderID=2",
				new object[][]
				{
					new object[] { 2d, new DateTime(2020, 5, 1), "P1", "CP1", "C1" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_47_CROSS_JOIN_WHERE_Typed()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders, Waher.Script.Test.Data.Customer as Customers where Orders.CustomerID=Customers.CustomerID and OrderID=2",
				new object[][]
				{
					new object[] { 2d, new DateTime(2020, 5, 01), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_48_CROSS_JOIN_WHERE_Typed_2()
		{
			this.Test("Select Orders.OrderID, Orders.OrderDate, Customers.CustomerName, Customers.ContactName, Customers.Country from Waher.Script.Test.Data.Order as Orders, Waher.Script.Test.Data.Customer as Customers where Orders.OrderID=2",
				new object[][]
				{
					new object[] { 2d, new DateTime(2020, 5, 1), "P1", "CP1", "C1" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P2", "CP2", "C2" },
					new object[] { 2d, new DateTime(2020, 5, 1), "P3", "CP3", "C2" }
				});
		}

		[TestMethod]
		public void SELECT_Test_49_SELF_JOIN()
		{
			this.Test("Select o1.OrderID, o1.CustomerID, o1.OrderDate, o2.OrderID, o2.CustomerID, o2.OrderDate from Orders o1 inner join Orders o2 on o1.OrderID=o2.CustomerID",
				new object[][]
				{
					new object[] { 2d, 3d, new DateTime(2020, 5, 1), 1d, 2d, new DateTime(2020, 4, 30) },
					new object[] { 3d, 4d, new DateTime(2020, 5, 2), 2d, 3d, new DateTime(2020, 5, 1) }
				});
		}

		[TestMethod]
		public void SELECT_Test_50_SELF_JOIN_2()
		{
			this.Test("Select o1.OrderID, o1.CustomerID, o1.OrderDate, o2.OrderID, o2.CustomerID, o2.OrderDate from Orders o1, Orders o2 where o1.OrderID=o2.CustomerID",
				new object[][]
				{
					new object[] { 2d, 3d, new DateTime(2020, 5, 1), 1d, 2d, new DateTime(2020, 4, 30) },
					new object[] { 3d, 4d, new DateTime(2020, 5, 2), 2d, 3d, new DateTime(2020, 5, 1) }
				});
		}

		[TestMethod]
		public void SELECT_Test_51_SELF_JOIN_Typed()
		{
			this.Test("Select o1.OrderID, o1.CustomerID, o1.OrderDate, o2.OrderID, o2.CustomerID, o2.OrderDate from Waher.Script.Test.Data.Order o1 inner join Waher.Script.Test.Data.Order o2 on o1.OrderID=o2.CustomerID",
				new object[][]
				{
					new object[] { 2d, 3d, new DateTime(2020, 5, 1), 1d, 2d, new DateTime(2020, 4, 30) },
					new object[] { 3d, 4d, new DateTime(2020, 5, 2), 2d, 3d, new DateTime(2020, 5, 1) }
				});
		}

		[TestMethod]
		public void SELECT_Test_52_SELF_JOIN_Typed_2()
		{
			this.Test("Select o1.OrderID, o1.CustomerID, o1.OrderDate, o2.OrderID, o2.CustomerID, o2.OrderDate from Waher.Script.Test.Data.Order o1, Waher.Script.Test.Data.Order o2 where o1.OrderID=o2.CustomerID",
				new object[][]
				{
					new object[] { 2d, 3d, new DateTime(2020, 5, 1), 1d, 2d, new DateTime(2020, 4, 30) },
					new object[] { 3d, 4d, new DateTime(2020, 5, 2), 2d, 3d, new DateTime(2020, 5, 1) }
				});
		}

		[TestMethod]
		public void SELECT_Test_53_JOIN_3_SOURCES()
		{
			this.Test("Select o1.OrderID, o1.CustomerID, o1.OrderDate, o2.OrderID, o2.CustomerID, o2.OrderDate, o3.OrderID, o3.CustomerID, o3.OrderDate from Orders o1 inner join Orders o2 on o1.OrderID=o2.CustomerID inner join Orders o3 on o2.OrderID=o3.CustomerID",
				new object[][]
				{
					new object[] { 3d, 4d, new DateTime(2020, 5, 2), 2d, 3d, new DateTime(2020, 5, 1), 1d, 2d, new DateTime(2020, 4, 30) }
				});
		}

		[TestMethod]
		public void SELECT_Test_54_JOIN_3_SOURCES_2()
		{
			this.Test("Select o1.OrderID, o1.CustomerID, o1.OrderDate, o2.OrderID, o2.CustomerID, o2.OrderDate, o3.OrderID, o3.CustomerID, o3.OrderDate from Orders o1, Orders o2, Orders o3 where o1.OrderID=o2.CustomerID and o2.OrderID=o3.CustomerID",
				new object[][]
				{
					new object[] { 3d, 4d, new DateTime(2020, 5, 2), 2d, 3d, new DateTime(2020, 5, 1), 1d, 2d, new DateTime(2020, 4, 30) }
				});
		}

		[TestMethod]
		public void SELECT_Test_55_JOIN_3_SOURCES_Typed()
		{
			this.Test("Select o1.OrderID, o1.CustomerID, o1.OrderDate, o2.OrderID, o2.CustomerID, o2.OrderDate, o3.OrderID, o3.CustomerID, o3.OrderDate from Waher.Script.Test.Data.Order o1 inner join Waher.Script.Test.Data.Order o2 on o1.OrderID=o2.CustomerID inner join Waher.Script.Test.Data.Order o3 on o2.OrderID=o3.CustomerID",
				new object[][]
				{
					new object[] { 3d, 4d, new DateTime(2020, 5, 2), 2d, 3d, new DateTime(2020, 5, 1), 1d, 2d, new DateTime(2020, 4, 30) }
				});
		}

		[TestMethod]
		public void SELECT_Test_56_JOIN_3_SOURCES_Typed_2()
		{
			this.Test("Select o1.OrderID, o1.CustomerID, o1.OrderDate, o2.OrderID, o2.CustomerID, o2.OrderDate, o3.OrderID, o3.CustomerID, o3.OrderDate from Waher.Script.Test.Data.Order o1, Waher.Script.Test.Data.Order o2, Waher.Script.Test.Data.Order o3 where o1.OrderID=o2.CustomerID and o2.OrderID=o3.CustomerID",
				new object[][]
				{
					new object[] { 3d, 4d, new DateTime(2020, 5, 2), 2d, 3d, new DateTime(2020, 5, 1), 1d, 2d, new DateTime(2020, 4, 30) }
				});
		}

		[TestMethod]
		public void SELECT_Test_57_Custom_Filters()
		{
			this.Test("Select OrderID, CustomerID, OrderDate from Orders where (OrderID & 1)=1",
				new object[][]
				{
					new object[] { 1d, 2d, new DateTime(2020, 4, 30) },
					new object[] { 3d, 4d, new DateTime(2020, 5, 2) }
				});
		}

		[TestMethod]
		public void SELECT_Test_58_Custom_Filters_Typed()
		{
			this.Test("Select OrderID, CustomerID, OrderDate from Waher.Script.Test.Data.Order where (OrderID & 1)=1",
				new object[][]
				{
					new object[] { 1d, 2d, new DateTime(2020, 4, 30) },
					new object[] { 3d, 4d, new DateTime(2020, 5, 2) }
				});
		}

		[TestMethod]
		public void INSERT_Test_01_INSERT_VALUES()
		{
			this.Test(
				"insert into WebUsers (UserName, Password) values (\"User01\", \"Pwd01\");"+
				"select UserName, Password from WebUsers where UserName=\"User01\" order by UserName",
				new object[][]
				{
					new object[] { "User01", "Pwd01" }
				});
		}

		[TestMethod]
		public void INSERT_Test_02_INSERT_SELECT_Columns()
		{
			this.Test(
				"ToStr(i):=i<10?\"0\"+i:i;" +
				"insert into WebUsers select UserName, Password from [foreach i in 2..10 do {UserName:\"User\"+ToStr(i),Password:\"Pwd\"+ToStr(i)}];" +
				"select UserName, Password from WebUsers where UserName>=\"User02\" and UserName<=\"User10\" order by UserName",
				new object[][]
				{
					new object[] { "User02", "Pwd02" },
					new object[] { "User03", "Pwd03" },
					new object[] { "User04", "Pwd04" },
					new object[] { "User05", "Pwd05" },
					new object[] { "User06", "Pwd06" },
					new object[] { "User07", "Pwd07" },
					new object[] { "User08", "Pwd08" },
					new object[] { "User09", "Pwd09" },
					new object[] { "User10", "Pwd10" }
				});
		}

		[TestMethod]
		public void INSERT_Test_03_INSERT_SELECT_Objects()
		{
			this.Test(
				"insert into WebUsers select * from [foreach i in 11..20 do {UserName:\"User\"+i,Password:\"Pwd\"+i}];" +
				"select UserName, Password from WebUsers where UserName>=\"User11\" and UserName<=\"User20\" order by UserName",
				new object[][]
				{
					new object[] { "User11", "Pwd11" },
					new object[] { "User12", "Pwd12" },
					new object[] { "User13", "Pwd13" },
					new object[] { "User14", "Pwd14" },
					new object[] { "User15", "Pwd15" },
					new object[] { "User16", "Pwd16" },
					new object[] { "User17", "Pwd17" },
					new object[] { "User18", "Pwd18" },
					new object[] { "User19", "Pwd19" },
					new object[] { "User20", "Pwd20" }
				});
		}

		[TestMethod]
		public void INSERT_Test_04_INSERT_OBJECT()
		{
			this.Test(
				"insert into WebUsers object {UserName:\"User21\",Password:\"Pwd21\"};" +
				"select UserName, Password from WebUsers where UserName=\"User21\" order by UserName",
				new object[][]
				{
					new object[] { "User21", "Pwd21" }
				});
		}

		[TestMethod]
		public void INSERT_Test_05_INSERT_OBJECTS_1()
		{
			this.Test(
				"insert into WebUsers objects "+
				"{UserName:\"User22\",Password:\"Pwd22\"}," +
				"{UserName:\"User23\",Password:\"Pwd23\"}," +
				"{UserName:\"User24\",Password:\"Pwd24\"};" +
				"select UserName, Password from WebUsers where UserName>=\"User22\" and UserName<=\"User24\" order by UserName",
				new object[][]
				{
					new object[] { "User22", "Pwd22" },
					new object[] { "User23", "Pwd23" },
					new object[] { "User24", "Pwd24" }
				});
		}

		[TestMethod]
		public void INSERT_Test_06_INSERT_OBJECTS_2()
		{
			this.Test(
				"insert into WebUsers objects [foreach i in 25..27 do {UserName:\"User\"+i,Password:\"Pwd\"+i}];" +
				"select UserName, Password from WebUsers where UserName>=\"User25\" and UserName<=\"User27\" order by UserName",
				new object[][]
				{
					new object[] { "User25", "Pwd25" },
					new object[] { "User26", "Pwd26" },
					new object[] { "User27", "Pwd27" }
				});
		}

		[TestMethod]
		public void INSERT_Test_07_INSERT_OBJECTS_3()
		{
			this.Test(
				"insert into WebUsers objects {foreach i in 28..30 do {UserName:\"User\"+i,Password:\"Pwd\"+i}};" +
				"select UserName, Password from WebUsers where UserName>=\"User28\" and UserName<=\"User30\" order by UserName",
				new object[][]
				{
					new object[] { "User28", "Pwd28" },
					new object[] { "User29", "Pwd29" },
					new object[] { "User30", "Pwd30" }
				});
		}

		/* TODO:
		 *	SELECT
		 *		UNION	
		 *		GROUP BY
		 *		HAVING
		 *		ORDER BY
		 *		TOP
		 *		DISTINCT
		 *		OFFSET
		 */

	}
}
 