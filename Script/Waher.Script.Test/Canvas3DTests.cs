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
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			int t;

			for (t = 0; t < 1000000; t++)
			{
				double x = t * Math.Sin(t / 10000.0) / 2500.0;
				double y = t * Math.Cos(t / 20000.0) / 2500.0;
				double z = t / 5000.0;
				Vector4 P = new Vector4((float)x, (float)y, (float)z, 1);
				Canvas.Plot(P, SKColors.Red);
			}

			this.Save(Canvas, "01.png");
		}

		[TestMethod]
		public void Canvas3D_Test_02_Line()
		{
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			this.DrawCurve(Canvas);
			this.Save(Canvas, "02.png");
		}

		private void DrawCurve(Canvas3D Canvas)
		{
			int t;

			for (t = 0; t < 10000; t++)
			{
				double x = t * Math.Sin(t / 100.0) / 25.0;
				double y = t * Math.Cos(t / 200.0) / 25.0;
				double z = t / 50.0;
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
			Canvas3D Canvas = new Canvas3D(1200, 800, 3, SKColors.White);
			this.DrawCurve(Canvas);
			this.Save(Canvas, "03.png");
		}

		[TestMethod]
		public void Canvas3D_Test_04_Perspective()
		{
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			Canvas.Perspective(200, 2000);
			this.DrawWireframeCube(Canvas);
			this.Save(Canvas, "04.png");
		}

		private void DrawWireframeCube(Canvas3D Canvas)
		{
			Vector4 P0 = new Vector4(-500, -500, 1000, 1);
			Vector4 P1 = new Vector4(-500, -500, 2000, 1);
			Vector4 P2 = new Vector4(500, -500, 2000, 1);
			Vector4 P3 = new Vector4(500, -500, 1000, 1);
			Vector4 P4 = new Vector4(-500, 500, 1000, 1);
			Vector4 P5 = new Vector4(-500, 500, 2000, 1);
			Vector4 P6 = new Vector4(500, 500, 2000, 1);
			Vector4 P7 = new Vector4(500, 500, 1000, 1);

			Canvas.PolyLine(new Vector4[] { P0, P1, P2, P3, P0 }, SKColors.Red);
			Canvas.PolyLine(new Vector4[] { P4, P5, P6, P7, P4 }, SKColors.Red);
			Canvas.Line(P0, P4, SKColors.Red);
			Canvas.Line(P1, P5, SKColors.Red);
			Canvas.Line(P2, P6, SKColors.Red);
			Canvas.Line(P3, P7, SKColors.Red);
		}

		[TestMethod]
		public void Canvas3D_Test_05_Polygon()
		{
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			Canvas.Perspective(200, 2000);
			this.DrawCube(Canvas);
			this.Save(Canvas, "05.png");
		}

		private void DrawCube(Canvas3D Canvas)
		{
			Vector4 P0 = new Vector4(-500, -500, 1000, 1);
			Vector4 P1 = new Vector4(-500, -500, 2000, 1);
			Vector4 P2 = new Vector4(500, -500, 2000, 1);
			Vector4 P3 = new Vector4(500, -500, 1000, 1);
			Vector4 P4 = new Vector4(-500, 500, 1000, 1);
			Vector4 P5 = new Vector4(-500, 500, 2000, 1);
			Vector4 P6 = new Vector4(500, 500, 2000, 1);
			Vector4 P7 = new Vector4(500, 500, 1000, 1);

			Canvas.Polygon(new Vector4[] { P0, P1, P2, P3 }, new SKColor(255, 0, 0, 128), true);
			Canvas.Polygon(new Vector4[] { P4, P5, P6, P7 }, new SKColor(255, 0, 0, 128), true);
			Canvas.Polygon(new Vector4[] { P1, P2, P6, P5 }, new SKColor(0, 255, 0, 128), true);
			Canvas.Polygon(new Vector4[] { P0, P1, P5, P4 }, new SKColor(0, 0, 255, 128), true);
			Canvas.Polygon(new Vector4[] { P2, P3, P7, P6 }, new SKColor(0, 0, 255, 128), true);
			Canvas.Polygon(new Vector4[] { P0, P3, P7, P4 }, new SKColor(0, 255, 0, 128), true);
		}

		[TestMethod]
		public void Canvas3D_Test_06_ZBuffer()
		{
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			Canvas.Perspective(200, 2000);
			this.DrawPlanes(Canvas);
			this.Save(Canvas, "06.png");
		}

		private void DrawPlanes(Canvas3D Canvas)
		{
			Canvas.Polygon(new Vector4[]
			{
				new Vector4(-500, 100, 1000, 1),
				new Vector4(-500, 100, 2000, 1),
				new Vector4(500, 100, 2000, 1),
				new Vector4(500, 100, 1000, 1)
			}, SKColors.Red, true);

			Canvas.Polygon(new Vector4[]
			{
				new Vector4(100, -500, 1000, 1),
				new Vector4(100, -500, 2000, 1),
				new Vector4(100, 500, 2000, 1),
				new Vector4(100, 500, 1000, 1)
			}, SKColors.Green, true);

			Canvas.Polygon(new Vector4[]
			{
				new Vector4(-500, -500, 1500, 1),
				new Vector4(500, -500, 1500, 1),
				new Vector4(500, 500, 1500, 1),
				new Vector4(-500, 500, 1500, 1),
			}, new SKColor(0, 0, 255, 64), true);
		}

		[TestMethod]
		public void Canvas3D_Test_07_Text()
		{
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			Canvas.Perspective(200, 2000);
			this.DrawPlanes(Canvas);
			Canvas.Text("Hello World!", new Vector4(-400, 50, 1150, 1), "Tahoma", 150, SKColors.BlueViolet);

			this.Save(Canvas, "07.png");
		}

		[TestMethod]
		public void Canvas3D_Test_08_PhongShading_NoOversampling()
		{
			I3DShader Shader = this.GetPhongShader(SKColors.Red);
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			Canvas.Perspective(200, 2000);
			this.DrawThreePlanes(Canvas, Shader);
			this.Save(Canvas, "08.png");
		}

		[TestMethod]
		public void Canvas3D_Test_09_PhongShading_Oversampling_2()
		{
			I3DShader Shader = this.GetPhongShader(SKColors.Red);
			Canvas3D Canvas = new Canvas3D(1200, 800, 2, SKColors.White);
			Canvas.Perspective(200, 2000);
			this.DrawThreePlanes(Canvas, Shader);
			this.Save(Canvas, "09.png");
		}

		[TestMethod]
		public void Canvas3D_Test_10_PhongShading_Oversampling_3()
		{
			I3DShader Shader = this.GetPhongShader(SKColors.Red);
			Canvas3D Canvas = new Canvas3D(1200, 800, 3, SKColors.White);
			Canvas.Perspective(200, 2000);
			this.DrawThreePlanes(Canvas, Shader);
			this.Save(Canvas, "10.png");
		}

		private I3DShader GetPhongShader(SKColor Color)
		{
			return new PhongShader(
				new PhongMaterial(1, 2, 0, 10),
				new PhongIntensity(64, 64, 64, Color.Alpha),
				new PhongLightSource(
					new PhongIntensity(Color.Red, Color.Green, Color.Blue, Color.Alpha),
					new PhongIntensity(255, 255, 255, 255),
					new Vector3(1000, 1000, 0)));
		}

		private void DrawThreePlanes(Canvas3D Canvas, I3DShader Shader)
		{
			this.DrawThreePlanes(Canvas, Shader, -500, -500, 2000);
		}

		private void DrawThreePlanes(Canvas3D Canvas, I3DShader Shader, float x, float y, float z)
		{
			Canvas.Polygon(new Vector4[]
			{
				new Vector4(-500, 500, z, 1),
				new Vector4(500, 500, z, 1),
				new Vector4(500, -500, z, 1),
				new Vector4(-500, -500, z, 1)
			}, Shader, true);

			Canvas.Polygon(new Vector4[]
			{
				new Vector4(x, 500, 1000, 1),
				new Vector4(x, 500, 2000, 1),
				new Vector4(x, -500, 2000, 1),
				new Vector4(x, -500, 1000, 1)
			}, Shader, true);

			Canvas.Polygon(new Vector4[]
			{
				new Vector4(-500, y, 2000, 1),
				new Vector4(500, y, 2000, 1),
				new Vector4(500, y, 1000, 1),
				new Vector4(-500, y, 1000, 1)
			}, Shader, true);
		}

		[TestMethod]
		public void Canvas3D_Test_11_Rotate_X()
		{
			I3DShader Shader = this.GetPhongShader(SKColors.Red);
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			Canvas.Perspective(200, 2000);

			this.DrawThreePlanes(Canvas, Shader);

			Shader = this.GetPhongShader(SKColors.Blue);
			Matrix4x4 Bak = Canvas.Translate(-250, 250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateX(30, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			Canvas.ModelTransformation = Bak;
			Canvas.Translate(250, 250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateX(120, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			Canvas.ModelTransformation = Bak;
			Canvas.Translate(-250, -250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateX(210, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			Canvas.ModelTransformation = Bak;
			Canvas.Translate(250, -250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateX(500, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			this.Save(Canvas, "11.png");
		}

		[TestMethod]
		public void Canvas3D_Test_12_Rotate_Y()
		{
			I3DShader Shader = this.GetPhongShader(SKColors.Red);
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			Canvas.Perspective(200, 2000);

			this.DrawThreePlanes(Canvas, Shader);

			Shader = this.GetPhongShader(SKColors.Blue);
			Matrix4x4 Bak = Canvas.Translate(-250, 250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateY(30, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			Canvas.ModelTransformation = Bak;
			Canvas.Translate(250, 250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateY(120, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			Canvas.ModelTransformation = Bak;
			Canvas.Translate(-250, -250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateY(210, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			Canvas.ModelTransformation = Bak;
			Canvas.Translate(250, -250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateY(500, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			this.Save(Canvas, "12.png");
		}

		[TestMethod]
		public void Canvas3D_Test_13_Rotate_Z()
		{
			I3DShader Shader = this.GetPhongShader(SKColors.Red);
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			Canvas.Perspective(200, 2000);

			this.DrawThreePlanes(Canvas, Shader);

			Shader = this.GetPhongShader(SKColors.Blue);
			Matrix4x4 Bak = Canvas.Translate(-250, 250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateZ(30, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			Canvas.ModelTransformation = Bak;
			Canvas.Translate(250, 250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateZ(120, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			Canvas.ModelTransformation = Bak;
			Canvas.Translate(-250, -250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateZ(210, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			Canvas.ModelTransformation = Bak;
			Canvas.Translate(250, -250, 0);
			Canvas.Scale(0.25f, new Vector3(0, 0, 1500));
			Canvas.RotateZ(500, new Vector3(0, 0, 1500));
			Canvas.Box(-500, -500, 1000, 500, 500, 2000, Shader);

			this.Save(Canvas, "13.png");
		}

		[TestMethod]
		public void Canvas3D_Test_14_Ellipsoid()
		{
			Canvas3D Canvas = new Canvas3D(1200, 800, 1, SKColors.White);
			Canvas.Perspective(200, 2000);
			//Canvas.LookAt(-200, 500, 0, 0, 0, 1500, 0, 1, 0);
			Canvas.RotateX(30, new Vector3(0, 0, 1500));
			Canvas.RotateY(45, new Vector3(0, 0, 1500));
			Canvas.RotateZ(60, new Vector3(0, 0, 1500));

			I3DShader Shader = this.GetPhongShader(SKColors.Orange);
			Canvas.Ellipsoid(0, 0, 1500, 400, 400, 400, 1000, Shader);

			Shader = this.GetPhongShader(new SKColor(0, 0, 255, 64));
			this.DrawThreePlanes(Canvas, Shader, 0, 0, 1500);

			this.Save(Canvas, "14.png");
		}

		// TODO: Clip Z
		// TODO: Test Light / Phong shading with multiple light sources
		// TODO: Specular lighting
		// TODO: Sort transparent polygons
		// TODO: Check if quicker to work with Vector3 directly, than with 3 separate coordinate values.
		// TODO: text-bug
		// TODO: C.LookAt(-200, 500, 0, 0, 0, 1500, 0, -1, 0);

/* Test script:

GraphWidth:=800;
GraphHeight:=600;

[LabelsX,LabelsZ,Y]:=Histogram2D([Normal(0,1,100000),Normal(0,1,100000)],-5,5,50,-5,5,50);
X1:=-5..4|0.2;
X2:=-4..5|0.2;
Z1:=-5..4|0.2;
Z2:=-4..5|0.2;
MinX:=Min(X1);
MaxX:=Max(X2);
MinY:=Min(Y);
MaxY:=Max(Y);
MinZ:=Min(Z1);
MaxZ:=Max(Z2);
NX:=count(X1);
NZ:=count(Z1);

SR:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 255),PhongLightSource("Red",PhongIntensity("White"),[1000, 1000, 0]));
SB:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 64),PhongLightSource(Alpha("Blue",64),PhongIntensity("White"),[1000, 1000, 0]));
C:=Canvas3D(GraphWidth,GraphHeight,3,"LightGray");
C.Perspective(GraphHeight/2, 2000);
C.Translate(0,-GraphHeight/2-MinY,3*GraphWidth-MinZ);
C.RotateX(-15);
C.RotateY(-Angle);
C.Scale(2*GraphWidth/(MaxX-MinX),2*GraphHeight/(MaxY-MinY),2*GraphWidth/(MaxZ-MinZ));

for x:=0 to NX-1 do
(
	for z:=0 to NZ-1 do
		C.Box(X1[x],0,Z1[z],X2[x],Y[x,z],Z2[z],SR);
);
C.Polygon([Vector4(MinX,MinY,0,1),Vector4(MaxX,MinY,0,1),Vector4(MaxX,MaxY,0,1),Vector4(MinX,MaxY,0,1)],SB,true);
C.Polygon([Vector4(MinX,0,MinZ,1),Vector4(MaxX,0,MinZ,1),Vector4(MaxX,0,MaxZ,1),Vector4(MinX,0,MaxZ,1)],SB,true);
C.Polygon([Vector4(0,MinY,MinZ,1),Vector4(0,MinY,MaxZ,1),Vector4(0,MaxY,MaxZ,1),Vector4(0,MaxY,MinZ,1)],SB,true);
C


foreach Angle in 30..390 do
(
	C:=Canvas3D(GraphWidth,GraphHeight,1,"LightGray");
	C.Perspective(200, 2000);
	C.Translate(0,-GraphHeight/2-MinY,3*GraphWidth-MinZ);
	C.RotateX(-15);
	C.RotateY(-Angle);
	C.Scale(2*GraphWidth/(MaxX-MinX),2*GraphHeight/(MaxY-MinY),2*GraphWidth/(MaxZ-MinZ));

	for x:=0 to NX-1 do
	(
		for z:=0 to NZ-1 do
			C.Box(X1[x],0,Z1[z],X2[x],Y[x,z],Z2[z],SR);
	);
	C.Polygon([Vector4(MinX,MinY,0,1),Vector4(MaxX,MinY,0,1),Vector4(MaxX,MaxY,0,1),Vector4(MinX,MaxY,0,1)],SB,true);
	C.Polygon([Vector4(MinX,0,MinZ,1),Vector4(MaxX,0,MinZ,1),Vector4(MaxX,0,MaxZ,1),Vector4(MinX,0,MaxZ,1)],SB,true);
	C.Polygon([Vector4(0,MinY,MinZ,1),Vector4(0,MinY,MaxZ,1),Vector4(0,MaxY,MaxZ,1),Vector4(0,MaxY,MinZ,1)],SB,true);
	preview(C)
)


[LabelsX,LabelsY,Counts]:=Histogram2D([Normal(0,1,100000),Normal(0,1,100000)],-5,5,100,-5,5,100);


SR:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 64),PhongLightSource(Alpha("Red",64),PhongIntensity("White"),[1000, 1000, 0]));
SO:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 255),PhongLightSource("Blue",PhongIntensity("White"),[1000, 1000, 0]));
foreach Angle in 0..360|0.5 do
(
	C:=Canvas3D(480,320,1,"LightGray");
	C.Perspective(200, 2000);
	C.RotateX(Angle,[0,0,1500]);
	C.RotateY(2*Angle,[0,0,1500]);
	C.RotateZ(3*Angle,[0,0,1500]);
	C.Ellipsoid(0, 0, 1500, 400, 400, 400, 5000, SO);
	C.Box(-300,-300,1200,300,300,1800,SR);
	Preview(C)
)


SR:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 64),PhongLightSource(Alpha("Red",64),PhongIntensity("White"),[1000, 1000, 0]));
SO:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 255),PhongLightSource("Blue",PhongIntensity("White"),[1000, 1000, 0]));
foreach Angle in 0..360|0.5 do
(
	C:=Canvas3D(480,320,1,"LightGray");
	C.Perspective(200, 2000);
	C.RotateX(Angle,[0,0,1500]);
	C.RotateY(2*Angle,[0,0,1500]);
	C.RotateZ(3*Angle,[0,0,1500]);
	C.Ellipsoid(0, 0, 1500, 400, 400, 400, 5000, SO);
	C.Polygon([Vector4(-500,0,1000,1),Vector4(500,0,1000,1),Vector4(500,0,2000,1),Vector4(-500,0,2000,1)],SR,true);
	C.Polygon([Vector4(0,-500,1000,1),Vector4(0,500,1000,1),Vector4(0,500,2000,1),Vector4(0,-500,2000,1)],SR,true);
	C.Polygon([Vector4(-500,-500,1500,1),Vector4(500,-500,1500,1),Vector4(500,500,1500,1),Vector4(-500,500,1500,1)],SR,true);
	Preview(C)
)



SR:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 255),PhongLightSource("Red",PhongIntensity("White"),[1000, 1000, 0]));
SG:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 255),PhongLightSource("Green",PhongIntensity("White"),[1000, 1000, 0]));
SB:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 255),PhongLightSource("Blue",PhongIntensity("White"),[1000, 1000, 0]));
SY:=PhongShader(PhongMaterial(1, 2, 0, 10),PhongIntensity(64, 64, 64, 255),PhongLightSource("Yellow",PhongIntensity("White"),[1000, 1000, 0]));
C:=Canvas3D(480,320,1,"LightGray");

foreach Angle in 0..720|0.5 do
(
	C.Clear();
	C.Perspective(200, 2000);

	M:=C.Translate(-200,200,0);
	C.Scale(0.5,[0,0,1300]);
	C.RotateX(Angle,[0,0,1300]);
	C.Box(-300,-300,1000,300,300,1600,SR);

	C.ModelTransformation:=M;
	C.Translate(200,200,0);
	C.Scale(0.5,[0,0,1300]);
	C.RotateY(Angle,[0,0,1300]);
	C.Box(-300,-300,1000,300,300,1600,SG);

	C.ModelTransformation:=M;
	C.Translate(-200,-200,0);
	C.Scale(0.5,[0,0,1300]);
	C.RotateZ(Angle,[0,0,1300]);
	C.Box(-300,-300,1000,300,300,1600,SB);

	C.ModelTransformation:=M;
	C.Translate(200,-200,0);
	C.Scale(0.5,[0,0,1300]);
	C.RotateX(Angle,[0,0,1300]);
	C.RotateY(2*Angle,[0,0,1300]);
	C.RotateZ(3*Angle,[0,0,1300]);
	C.Box(-300,-300,1000,300,300,1600,SY);

	Preview(C)
)
		 */
	}
}