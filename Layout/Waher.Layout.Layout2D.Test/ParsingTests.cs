using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SkiaSharp;
using Waher.Content;
using Waher.Content.Images;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Graphs;

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
				typeof(ImageCodec).Assembly,
				typeof(XML).Assembly,
				typeof(Log).Assembly,
				typeof(Expression).Assembly,
				typeof(Graph).Assembly,
				typeof(Layout2DDocument).Assembly);
		}

		protected virtual void Test(string FileName, params KeyValuePair<string, object>[] ContentAttachments)
		{
			Layout2DDocument.FromFile("Xml\\" + FileName + ".xml", ContentAttachments);
		}

		[TestMethod]
		public void Test_01_Lengths()
		{
			this.Test("Test_01_Lengths");
		}

		[TestMethod]
		public void Test_02_Circle()
		{
			this.Test("Test_02_Circle");
		}

		[TestMethod]
		public void Test_03_CircleArc()
		{
			this.Test("Test_03_CircleArc");
		}

		[TestMethod]
		public void Test_04_Dot()
		{
			this.Test("Test_04_Dot");
		}

		[TestMethod]
		public void Test_05_Ellipse()
		{
			this.Test("Test_05_Ellipse");
		}

		[TestMethod]
		public void Test_06_EllipseArc()
		{
			this.Test("Test_06_EllipseArc");
		}

		[TestMethod]
		public void Test_07_Line()
		{
			this.Test("Test_07_Line");
		}

		[TestMethod]
		public void Test_08_Loop()
		{
			this.Test("Test_08_Loop");
		}

		[TestMethod]
		public void Test_09_Polygon()
		{
			this.Test("Test_09_Polygon");
		}

		[TestMethod]
		public void Test_10_PolyLine()
		{
			this.Test("Test_10_PolyLine");
		}

		[TestMethod]
		public void Test_11_Rectangle()
		{
			this.Test("Test_11_Rectangle");
		}

		[TestMethod]
		public void Test_12_RoundedRectangle()
		{
			this.Test("Test_12_RoundedRectangle");
		}

		[TestMethod]
		public void Test_13_Spline()
		{
			this.Test("Test_13_Spline");
		}

		[TestMethod]
		public void Test_14_Path()
		{
			this.Test("Test_14_Path");
		}

		[TestMethod]
		public void Test_15_Arrow()
		{
			this.Test("Test_15_Arrow");
		}

		[TestMethod]
		public void Test_16_Rotate()
		{
			this.Test("Test_16_Rotate");
		}

		[TestMethod]
		public void Test_17_Rotate_Pivot()
		{
			this.Test("Test_17_Rotate_Pivot");
		}

		[TestMethod]
		public void Test_18_Translate()
		{
			this.Test("Test_18_Translate");
		}

		[TestMethod]
		public void Test_19_Scale()
		{
			this.Test("Test_19_Scale");
		}

		[TestMethod]
		public void Test_20_Scale_Pivot()
		{
			this.Test("Test_20_Scale_Pivot");
		}

		[TestMethod]
		public void Test_21_SkewX()
		{
			this.Test("Test_21_SkewX");
		}

		[TestMethod]
		public void Test_22_SkewX_Pivot()
		{
			this.Test("Test_22_SkewX_Pivot");
		}

		[TestMethod]
		public void Test_23_SkewY()
		{
			this.Test("Test_23_SkewY");
		}

		[TestMethod]
		public void Test_24_SkewY_Pivot()
		{
			this.Test("Test_24_SkewY_Pivot");
		}

		[TestMethod]
		public void Test_25_Horizontal()
		{
			this.Test("Test_25_Horizontal");
		}

		[TestMethod]
		public void Test_26_Vertical()
		{
			this.Test("Test_26_Vertical");
		}

		[TestMethod]
		public void Test_27_Overlays()
		{
			this.Test("Test_27_Overlays");
		}

		[TestMethod]
		public void Test_28_Flexible_LRTD()
		{
			this.Test("Test_28_Flexible_LRTD");
		}

		[TestMethod]
		public void Test_29_Flexible_RLTD()
		{
			this.Test("Test_29_Flexible_RLTD");
		}

		[TestMethod]
		public void Test_30_Flexible_LRDT()
		{
			this.Test("Test_30_Flexible_LRDT");
		}

		[TestMethod]
		public void Test_31_Flexible_RLDT()
		{
			this.Test("Test_31_Flexible_RLDT");
		}

		[TestMethod]
		public void Test_32_Flexible_TDLR()
		{
			this.Test("Test_32_Flexible_TDLR");
		}

		[TestMethod]
		public void Test_33_Flexible_TDRL()
		{
			this.Test("Test_33_Flexible_TDRL");
		}

		[TestMethod]
		public void Test_34_Flexible_DTLR()
		{
			this.Test("Test_34_Flexible_DTLR");
		}

		[TestMethod]
		public void Test_35_Flexible_DTRL()
		{
			this.Test("Test_35_Flexible_DTRL");
		}

		[TestMethod]
		public void Test_36_Grid()
		{
			this.Test("Test_36_Grid");
		}

		[TestMethod]
		public void Test_37_Images()
		{
			SKBitmap Bitmap = SKBitmap.Decode("Images\\Icon_64x64_File.png");
			SKImage Image = SKImage.FromBitmap(Bitmap);

			this.Test("Test_37_Images", new KeyValuePair<string, object>("img0001", Image));
		}

		[TestMethod]
		public void Test_38_Labels()
		{
			this.Test("Test_38_Labels");
		}

	}
}
