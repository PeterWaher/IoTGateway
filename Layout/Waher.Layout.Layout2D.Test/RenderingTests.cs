using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using Waher.Script;

namespace Waher.Layout.Layout2D.Test
{
	[TestClass]
	public class RenderingTests : ParsingTests
	{
		protected override void Test(string FileName, params KeyValuePair<string, object>[] ContentAttachments)
		{
			Layout2DDocument Doc = Layout2DDocument.FromFile("Xml\\" + FileName + ".xml", ContentAttachments);
			RenderSettings Settings = Doc.GetRenderSettings(new Variables());

			using SKImage Image = Doc.Render(Settings, out Map[] _);
			using SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100);

			if (!Directory.Exists("Output"))
				Directory.CreateDirectory("Output");

			File.WriteAllBytes(Path.Combine("Output", FileName + ".png"), Data.ToArray());
		}
	}
}
