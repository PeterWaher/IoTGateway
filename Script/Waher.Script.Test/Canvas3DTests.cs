using System;
using System.IO;
using System.Numerics;
using SkiaSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Graphs3D;

namespace Waher.Script.Test
{
	[TestClass]
	public class Canvas3DTests
	{
		private void Save(Canvas3D Canvas, string FileName)
		{
			if (!Directory.Exists("Canvas3D"))
				Directory.CreateDirectory("Canvas3D");

			using (SKImage Image = Canvas.GetBitmap())
			{
				using (SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100))
				{
					File.WriteAllBytes(Path.Combine("Canvas3D", FileName), Data.ToArray());
				}
			}
		}

		[TestMethod]
		public void Canvas3D_Test_01_Plot()
		{
			Canvas3D Canvas = new Canvas3D(640, 480, 1, SKColors.White);
			int t;

			for (t = 0; t < 1000000; t++)
			{
				double x = t * Math.Sin(t / 10000.0) / 5000.0;
				double y = t * Math.Cos(t / 20000.0) / 5000.0;
				double z = t / 10000.0;
				Vector4 P = new Vector4((float)x, (float)y, (float)z, 1);
				Canvas.Plot(P, SKColors.Red);
			}

			this.Save(Canvas, "01.png");
		}

		[TestMethod]
		public void Canvas3D_Test_02_Line()
		{
			Canvas3D Canvas = new Canvas3D(640, 480, 1, SKColors.White);
			this.DrawCurve(Canvas);
			this.Save(Canvas, "02.png");
		}

		private void DrawCurve(Canvas3D Canvas)
		{
			int t;

			for (t = 0; t < 10000; t++)
			{
				double x = t * Math.Sin(t / 100.0) / 50.0;
				double y = t * Math.Cos(t / 200.0) / 50.0;
				double z = t / 100.0;
				Vector4 P = new Vector4((float)x, (float)y, (float)z, 1);

				if (t == 0)
					Canvas.MoveTo(P);
				else
					Canvas.LineTo(P, SKColors.Red);
			}
		}

		[TestMethod]
		public void Canvas3D_Test_03_Oversampling()
		{
			Canvas3D Canvas = new Canvas3D(640, 480, 3, SKColors.White);
			this.DrawCurve(Canvas);
			this.Save(Canvas, "03.png");
		}

		[TestMethod]
		public void Canvas3D_Test_04_Perspective()
		{
			Canvas3D Canvas = new Canvas3D(640, 480, 3, SKColors.White);
			Canvas.ProjectZ(100);
			this.DrawCube(Canvas);
			this.Save(Canvas, "04.png");
		}

		private void DrawCube(Canvas3D Canvas)
		{
			Vector4 P0 = new Vector4(-200, -200, 100, 1);
			Vector4 P1 = new Vector4(-200, -200, 200, 1);
			Vector4 P2 = new Vector4(200, -200, 200, 1);
			Vector4 P3 = new Vector4(200, -200, 100, 1);
			Vector4 P4 = new Vector4(-200, 200, 100, 1);
			Vector4 P5 = new Vector4(-200, 200, 200, 1);
			Vector4 P6 = new Vector4(200, 200, 200, 1);
			Vector4 P7 = new Vector4(200, 200, 100, 1);

			Canvas.PolyLine(new Vector4[] { P0, P1, P2, P3, P0 }, SKColors.Red);
			Canvas.PolyLine(new Vector4[] { P4, P5, P6, P7, P4 }, SKColors.Red);
			Canvas.Line(P0, P4, SKColors.Red);
			Canvas.Line(P1, P5, SKColors.Red);
			Canvas.Line(P2, P6, SKColors.Red);
			Canvas.Line(P3, P7, SKColors.Red);
		}

		// TODO: Clip
	}
}