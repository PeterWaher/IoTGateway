using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Layout.Layout2D.Test
{
	[TestClass]
	public class ParsingTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(ParsingTests).Assembly,
				typeof(InternetContent).Assembly,
				typeof(Log).Assembly,
				typeof(Expression).Assembly,
				typeof(Layout2DDocument).Assembly);
		}

		protected virtual void Test(string FileName)
		{
			Layout2DDocument.FromFile("Xml\\" + FileName + ".xml");
		}

		[TestMethod]
		public void Test_01_Lengths()
		{
			this.Test("Test_01_Lengths");
		}

		[TestMethod]
		public void Test_02_Arrow()
		{
			this.Test("Test_02_Arrow");
		}

		[TestMethod]
		public void Test_03_Circle()
		{
			this.Test("Test_03_Circle");
		}

		[TestMethod]
		public void Test_04_CircleArc()
		{
			this.Test("Test_04_CircleArc");
		}

		[TestMethod]
		public void Test_05_Dot()
		{
			this.Test("Test_05_Dot");
		}

		[TestMethod]
		public void Test_06_Ellipse()
		{
			this.Test("Test_06_Ellipse");
		}

		[TestMethod]
		public void Test_07_EllipseArc()
		{
			this.Test("Test_07_EllipseArc");
		}

		[TestMethod]
		public void Test_08_Line()
		{
			this.Test("Test_08_Line");
		}

		[TestMethod]
		public void Test_09_Loop()
		{
			this.Test("Test_09_Loop");
		}

		[TestMethod]
		public void Test_10_Polygon()
		{
			this.Test("Test_10_Polygon");
		}

		[TestMethod]
		public void Test_11_PolyLine()
		{
			this.Test("Test_11_PolyLine");
		}

		[TestMethod]
		public void Test_12_Rectangle()
		{
			this.Test("Test_12_Rectangle");
		}

		[TestMethod]
		public void Test_13_RoundedRectangle()
		{
			this.Test("Test_13_RoundedRectangle");
		}

		[TestMethod]
		public void Test_14_Spline()
		{
			this.Test("Test_14_Spline");
		}
		
	}
}
