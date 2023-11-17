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
using System.Threading.Tasks;

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

		protected virtual async Task Test(string FileName, params KeyValuePair<string, object>[] ContentAttachments)
		{
			await Layout2DDocument.FromFile("Xml\\" + FileName + ".xml", ContentAttachments);
		}

		[TestMethod]
		public async Task Test_01_Lengths()
		{
			await this.Test("Test_01_Lengths");
		}

		[TestMethod]
		public async Task Test_02_Circle()
		{
			await this.Test("Test_02_Circle");
		}

		[TestMethod]
		public async Task Test_03_CircleArc()
		{
			await this.Test("Test_03_CircleArc");
		}

		[TestMethod]
		public async Task Test_04_Dot()
		{
			await this.Test("Test_04_Dot");
		}

		[TestMethod]
		public async Task Test_05_Ellipse()
		{
			await this.Test("Test_05_Ellipse");
		}

		[TestMethod]
		public async Task Test_06_EllipseArc()
		{
			await this.Test("Test_06_EllipseArc");
		}

		[TestMethod]
		public async Task Test_07_Line()
		{
			await this.Test("Test_07_Line");
		}

		[TestMethod]
		public async Task Test_08_Loop()
		{
			await this.Test("Test_08_Loop");
		}

		[TestMethod]
		public async Task Test_09_Polygon()
		{
			await this.Test("Test_09_Polygon");
		}

		[TestMethod]
		public async Task Test_10_PolyLine()
		{
			await this.Test("Test_10_PolyLine");
		}

		[TestMethod]
		public async Task Test_11_Rectangle()
		{
			await this.Test("Test_11_Rectangle");
		}

		[TestMethod]
		public async Task Test_12_RoundedRectangle()
		{
			await this.Test("Test_12_RoundedRectangle");
		}

		[TestMethod]
		public async Task Test_13_Spline()
		{
			await this.Test("Test_13_Spline");
		}

		[TestMethod]
		public async Task Test_14_Path()
		{
			await this.Test("Test_14_Path");
		}

		[TestMethod]
		public async Task Test_15_Arrow()
		{
			await this.Test("Test_15_Arrow");
		}

		[TestMethod]
		public async Task Test_16_Rotate()
		{
			await this.Test("Test_16_Rotate");
		}

		[TestMethod]
		public async Task Test_17_Rotate_Pivot()
		{
			await this.Test("Test_17_Rotate_Pivot");
		}

		[TestMethod]
		public async Task Test_18_Translate()
		{
			await this.Test("Test_18_Translate");
		}

		[TestMethod]
		public async Task Test_19_Scale()
		{
			await this.Test("Test_19_Scale");
		}

		[TestMethod]
		public async Task Test_20_Scale_Pivot()
		{
			await this.Test("Test_20_Scale_Pivot");
		}

		[TestMethod]
		public async Task Test_21_SkewX()
		{
			await this.Test("Test_21_SkewX");
		}

		[TestMethod]
		public async Task Test_22_SkewX_Pivot()
		{
			await this.Test("Test_22_SkewX_Pivot");
		}

		[TestMethod]
		public async Task Test_23_SkewY()
		{
			await this.Test("Test_23_SkewY");
		}

		[TestMethod]
		public async Task Test_24_SkewY_Pivot()
		{
			await this.Test("Test_24_SkewY_Pivot");
		}

		[TestMethod]
		public async Task Test_25_Horizontal()
		{
			await this.Test("Test_25_Horizontal");
		}

		[TestMethod]
		public async Task Test_26_Vertical()
		{
			await this.Test("Test_26_Vertical");
		}

		[TestMethod]
		public async Task Test_27_Overlays()
		{
			await this.Test("Test_27_Overlays");
		}

		[TestMethod]
		public async Task Test_28_Flexible_LRTD()
		{
			await this.Test("Test_28_Flexible_LRTD");
		}

		[TestMethod]
		public async Task Test_29_Flexible_RLTD()
		{
			await this.Test("Test_29_Flexible_RLTD");
		}

		[TestMethod]
		public async Task Test_30_Flexible_LRDT()
		{
			await this.Test("Test_30_Flexible_LRDT");
		}

		[TestMethod]
		public async Task Test_31_Flexible_RLDT()
		{
			await this.Test("Test_31_Flexible_RLDT");
		}

		[TestMethod]
		public async Task Test_32_Flexible_TDLR()
		{
			await this.Test("Test_32_Flexible_TDLR");
		}

		[TestMethod]
		public async Task Test_33_Flexible_TDRL()
		{
			await this.Test("Test_33_Flexible_TDRL");
		}

		[TestMethod]
		public async Task Test_34_Flexible_DTLR()
		{
			await this.Test("Test_34_Flexible_DTLR");
		}

		[TestMethod]
		public async Task Test_35_Flexible_DTRL()
		{
			await this.Test("Test_35_Flexible_DTRL");
		}

		[TestMethod]
		public async Task Test_36_Grid()
		{
			await this.Test("Test_36_Grid");
		}

		[TestMethod]
		public async Task Test_37_Images()
		{
			SKBitmap Bitmap = SKBitmap.Decode("Images\\Icon_64x64_File.png");
			SKImage Image = SKImage.FromBitmap(Bitmap);

			await this.Test("Test_37_Images", new KeyValuePair<string, object>("img0001", Image));
		}

		[TestMethod]
		public async Task Test_38_Labels()
		{
			await this.Test("Test_38_Labels");
		}

		[TestMethod]
		public async Task Test_39_Stack()
		{
			await this.Test("Test_39_Stack");
		}

		[TestMethod]
		public async Task Test_40_HorizontalBars()
		{
			await this.Test("Test_40_HorizontalBars");
		}

		[TestMethod]
		public async Task Test_41_FlexibleLegend()
		{
			await this.Test("Test_41_FlexibleLegend");
		}

		[TestMethod]
		public async Task Test_42_Pie()
		{
			await this.Test("Test_42_Pie");
		}

		[TestMethod]
		public async Task Test_43_HorizontalBars()
		{
			await this.Test("Test_43_HorizontalBars");
		}

		[TestMethod]
		public async Task Test_44_Paragraph_TL()
		{
			await this.Test("Test_44_Paragraph_TL");
		}

		[TestMethod]
		public async Task Test_45_Paragraph_TC()
		{
			await this.Test("Test_45_Paragraph_TC");
		}

		[TestMethod]
		public async Task Test_46_Paragraph_TR()
		{
			await this.Test("Test_46_Paragraph_TR");
		}

		[TestMethod]
		public async Task Test_47_Paragraph_BL()
		{
			await this.Test("Test_47_Paragraph_BL");
		}

		[TestMethod]
		public async Task Test_48_Paragraph_BC()
		{
			await this.Test("Test_48_Paragraph_BC");
		}

		[TestMethod]
		public async Task Test_49_Paragraph_BR()
		{
			await this.Test("Test_49_Paragraph_BR");
		}

		[TestMethod]
		public async Task Test_50_Paragraph_CL()
		{
			await this.Test("Test_50_Paragraph_CL");
		}

		[TestMethod]
		public async Task Test_51_Paragraph_CC()
		{
			await this.Test("Test_51_Paragraph_CC");
		}

		[TestMethod]
		public async Task Test_52_Paragraph_CR()
		{
			await this.Test("Test_52_Paragraph_CR");
		}

		[TestMethod]
		public async Task Test_53_Paragraph_BLL()
		{
			await this.Test("Test_53_Paragraph_BLL");
		}

		[TestMethod]
		public async Task Test_54_Paragraph_BLC()
		{
			await this.Test("Test_54_Paragraph_BLC");
		}

		[TestMethod]
		public async Task Test_55_Paragraph_BLR()
		{
			await this.Test("Test_55_Paragraph_BLR");
		}
	}
}
