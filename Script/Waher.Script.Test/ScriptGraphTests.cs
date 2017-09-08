using System;
using System.Collections.Generic;
using SkiaSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Graphs;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptGraphTests
	{
		private void Test(string Script)
		{
			Variables v = new Variables();

			Expression Exp = new Expression(Script);
			Graph Result = Exp.Evaluate(v) as Graph;

			if (Result == null)
				Assert.Fail("Expected graph.");

			GraphSettings Settings = new GraphSettings();
			SKImage Bmp = Result.CreateBitmap(Settings);
		}

		[TestMethod]
		public void Test_01_Plot2dCurve()
		{
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y)");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y,'Blue')");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y,'Blue',5)");
		}

		[TestMethod]
		public void Test_02_Plot2dLine()
		{
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dline(x,y)");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dline(x,y,'Blue')");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dline(x,y,'Blue',5)");
		}

		[TestMethod]
		public void Test_03_DateTimeAxis()
		{
			this.Test("x:=0..59;t:= Now.AddSeconds(x);y:= sin(x);plot2dcurve(t,y)");
			this.Test("x:=0..59;t:= Now.AddMinutes(x);y:= sin(x);plot2dcurve(t,y)");
			this.Test("x:=0..59;t:= Now.AddHours(x);y:= sin(x);plot2dcurve(t,y)");
			this.Test("x:=0..59;t:= Now.AddDays(x);y:= sin(x);plot2dcurve(t,y)");
			this.Test("x:=0..59;t:= Now.AddDays(x*7);y:= sin(x);plot2dcurve(t,y)");
			this.Test("x:=0..59;t:= Now.AddMonths(x);y:= sin(x);plot2dcurve(t,y)");
			this.Test("x:=0..59;t:= Now.AddYears(x);y:= sin(x);plot2dcurve(t,y)");
		}

		[TestMethod]
		public void Test_04_PhysicalQuantities()
		{
			this.Test("x:=-10..10|0.1;t:=DateTime(2016,3,11).AddHours(x);y:=sin(x) C;plot2dcurve(t,y)");
		}
	}
}