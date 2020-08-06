using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Test
{
	[TestClass]
	public class RenderingTests : ParsingTests
	{
		protected override void Test(string FileName)
		{
			Layout2DDocument Doc = Layout2DDocument.FromFile("Xml\\" + FileName);
			using (SKImage Image = Doc.Render(800, 600, 1, 0, 0, out Map[] _))
			{
				using (SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100))
				{
					File.WriteAllBytes(Path.Combine("Output", FileName), Data.ToArray());
				}
			}
		}
	}
}
