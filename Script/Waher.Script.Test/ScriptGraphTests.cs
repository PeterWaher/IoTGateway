using System;
using System.IO;
using SkiaSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Graphs;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptGraphTests
	{
		private void Test(string Script, string FileName)
		{
			Variables v = new Variables();

			Expression Exp = new Expression(Script);
			Graph Result = Exp.Evaluate(v) as Graph;

			if (Result is null)
				Assert.Fail("Expected graph.");

			GraphSettings Settings = new GraphSettings();
			SKImage Bmp = Result.CreateBitmap(Settings);
			
			this.Save(Bmp, FileName);
		}

		private void Save(SKImage Image, string FileName)
		{
			if (!Directory.Exists("Graphs"))
				Directory.CreateDirectory("Graphs");

			using (SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100))
			{
				File.WriteAllBytes(Path.Combine("Graphs", FileName), Data.ToArray());
			}
		}

		[TestMethod]
		public void Graph2D_Test_01_Plot2dCurve()
		{
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y)", "2D_01_1.png");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y,'Blue')", "2D_01_2.png");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y,'Blue',5)", "2D_01_3.png");
		}

		[TestMethod]
		public void Graph2D_Test_02_Plot2dLine()
		{
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dline(x,y)", "2D_02_1.png");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dline(x,y,'Blue')", "2D_02_2.png");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dline(x,y,'Blue',5)", "2D_02_3.png");
		}

		[TestMethod]
		public void Graph2D_Test_03_DateTimeAxis()
		{
			this.Test("x:=0..59;t:= Now.AddSeconds(x);y:= sin(x);plot2dcurve(t,y)", "2D_03_1.png");
			this.Test("x:=0..59;t:= Now.AddMinutes(x);y:= sin(x);plot2dcurve(t,y)", "2D_03_2.png");
			this.Test("x:=0..59;t:= Now.AddHours(x);y:= sin(x);plot2dcurve(t,y)", "2D_03_3.png");
			this.Test("x:=0..59;t:= Now.AddDays(x);y:= sin(x);plot2dcurve(t,y)", "2D_03_4.png");
			this.Test("x:=0..59;t:= Now.AddDays(x*7);y:= sin(x);plot2dcurve(t,y)", "2D_03_5.png");
			this.Test("x:=0..59;t:= Now.AddMonths(x);y:= sin(x);plot2dcurve(t,y)", "2D_03_6.png");
			this.Test("x:=0..59;t:= Now.AddYears(x);y:= sin(x);plot2dcurve(t,y)", "2D_03_7.png");
		}

		[TestMethod]
		public void Graph2D_Test_04_PhysicalQuantities()
		{
			this.Test("x:=-10..10|0.1;t:=DateTime(2016,3,11).AddHours(x);y:=sin(x) C;plot2dcurve(t,y)", "2D_04.png");
		}

		[TestMethod]
		public void Graph3D_Test_01_LineMesh()
		{
			this.Test("x:=Columns(-10..10|0.5);z:=Rows(-10..10|0.5);r:=sqrt(x.^2+z.^2);y:=sin(r*2).*exp(-r/3);linemesh3d(x,y,z)", "3D_01_1.png");
			this.Test("x:=Columns(-10..10|0.5);z:=Rows(-10..10|0.5);r:=sqrt(x.^2+z.^2);y:=sin(r*2).*exp(-r/3);linemesh3d(x,y,z,'Blue')", "3D_01_2.png");
			this.Test("theta:=Columns((0..360|5)°);phi:=Rows((0..360|5)°);R0:=5;R1:=20;x:=(R1+R0*cos(theta)).*cos(phi);y:=(R1+R0*cos(theta)).*sin(phi);z:=R0*sin(theta);samescale(linemesh3d(x,y,z))", "3D_01_3.png");
		}

		[TestMethod]
		public void Graph3D_Test_02_PolygonMesh()
		{
			this.Test("x:=Columns(-10..10|0.5);z:=Rows(-10..10|0.5);r:=sqrt(x.^2+z.^2);y:=sin(r*2).*exp(-r/3);polygonmesh3d(x,y,z)", "3D_02_1.png");
			this.Test("x:=Columns(-10..10|0.5);z:=Rows(-10..10|0.5);r:=sqrt(x.^2+z.^2);y:=sin(r*2).*exp(-r/3);polygonmesh3d(x,y,z,'Blue')", "3D_02_2.png");
			this.Test("theta:=Columns((0..360|5)°);phi:=Rows((0..360|5)°);R0:=5;R1:=20;x:=(R1+R0*cos(theta)).*cos(phi);y:=(R1+R0*cos(theta)).*sin(phi);z:=R0*sin(theta);samescale(polygonmesh3d(x,y,z))", "3D_02_3.png");
		}

		[TestMethod]
		public void Graph3D_Test_03_Surface()
		{
			this.Test("x:=Columns(-10..10|0.5);z:=Rows(-10..10|0.5);r:=sqrt(x.^2+z.^2);y:=sin(r*2).*exp(-r/3);surface3d(x,y,z)", "3D_03_1.png");
			this.Test("x:=Columns(-10..10|0.5);z:=Rows(-10..10|0.5);r:=sqrt(x.^2+z.^2);y:=sin(r*2).*exp(-r/3);surface3d(x,y,z,'Blue')", "3D_03_2.png");
			this.Test("theta:=Columns((0..360|5)°);phi:=Rows((0..360|5)°);R0:=5;R1:=20;x:=(R1+R0*cos(theta)).*cos(phi);y:=(R1+R0*cos(theta)).*sin(phi);z:=R0*sin(theta);samescale(surface3d(x,y,z))", "3D_03_3.png");
		}
	}
}