using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using Waher.Content.Images;
using Waher.Runtime.Text;
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
			string PrevDimXml = null;
			string PrevPosXml = null;
			int DimIndex = 1;
			int PosIndex = 1;

			EmptyFolder(Path.Combine("Output", "Dimensions", FileName));
			EmptyFolder(Path.Combine("Output", "Positions", FileName));

			Doc.OnMeasuringDimensions += (sender, e) =>
			{
				string Xml = Doc.ExportState();
				NewXml(Path.Combine("Output", "Dimensions", FileName, DimIndex++.ToString() + ".xml"), Xml, ref PrevDimXml);
			};

			Doc.OnMeasuringPositions += (sender, e) =>
			{
				string Xml = Doc.ExportState();
				NewXml(Path.Combine("Output", "Positions", FileName, PosIndex++.ToString() + ".xml"), Xml, ref PrevPosXml);
			};

			KeyValuePair<SKImage, Map[]> Result = await Doc.Render(Settings);
			using SKImage Image = Result.Key;
			using SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100);

			if (!Directory.Exists("Output"))
				Directory.CreateDirectory("Output");

			File.WriteAllBytes(Path.Combine("Output", FileName + "." + ImageCodec.FileExtensionPng), Data.ToArray());
		}

		private static void NewXml(string Path, string Xml, ref string PrevXml)
		{
			File.WriteAllText(Path, Xml);

			if (!string.IsNullOrEmpty(PrevXml))
			{
				EditScript<string> Changes = Difference.AnalyzeRows(PrevXml, Xml);
				StringBuilder Diff = new();

				foreach (Step<string> Segment in Changes)
				{
					foreach (string Row in Segment.Symbols)
					{
						switch (Segment.Operation)
						{
							case EditOperation.Keep:
							default:
								break;

							case EditOperation.Insert:
								Diff.Append('+');
								break;

							case EditOperation.Delete:
								Diff.Append('-');
								break;
						}

						Diff.Append('\t');
						Diff.AppendLine(Row);
					}
				}

				File.WriteAllText(Path + ".diff", Diff.ToString());
			}

			PrevXml = Xml;
		}

		private static void EmptyFolder(string Path)
		{
			if (Directory.Exists(Path))
			{
				foreach (string FileName in Directory.GetFiles(Path, "*.*"))
					File.Delete(FileName);
			}
			else
				Directory.CreateDirectory(Path);
		}

	}
}
