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
			Layout2DDocument Doc = Layout2DDocument.FromFile("Xml\\" + FileName + ".xml");
			RenderSettings Settings = new RenderSettings();

			using (SKImage Image = Doc.Render(Settings, out Map[] _))
			{
				using (SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100))
				{
					if (!Directory.Exists("Output"))
						Directory.CreateDirectory("Output");

					File.WriteAllBytes(Path.Combine("Output", FileName + ".png"), Data.ToArray());
				}
			}
		}
	}
}
