using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using Waher.Content.Images;
using Waher.Script;

namespace Waher.Layout.Layout2D.Test
{
	[TestClass]
	public class RenderingTests : ParsingTests
	{
		protected override async Task Test(string FileName, params KeyValuePair<string, object>[] ContentAttachments)
		{
			Layout2DDocument Doc = await Layout2DDocument.FromFile(Path.Combine("Xml", FileName + ".xml"), ContentAttachments);
			RenderSettings Settings = await Doc.GetRenderSettings(new Variables());

			KeyValuePair<SKImage, Map[]> Result = await Doc.Render(Settings);
			using SKImage Image = Result.Key;
			using SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100);

			if (!Directory.Exists("Output"))
				Directory.CreateDirectory("Output");

			File.WriteAllBytes(Path.Combine("Output", FileName + "." + ImageCodec.FileExtensionPng), Data.ToArray());
		}
	}
}
