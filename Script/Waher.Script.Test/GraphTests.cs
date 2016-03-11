using System;
using System.Drawing;
using System.Collections.Generic;
using NUnit.Framework;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Graphs;

namespace Waher.Script.Test
{
	[TestFixture]
	public class GraphTests
	{
		private void Test(string Script)
		{
			Variables v = new Variables();

			Expression Exp = new Expression(Script);
			Graph Result = Exp.Evaluate(v) as Graph;
			Assert.NotNull(Result, "Expected graph.");
			GraphSettings Settings = new GraphSettings();
			Bitmap Bmp = Result.CreateBitmap(Settings);
		}

		[Test]
		public void Test_01_Plot2dCurve()
		{
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y)");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y,'Blue')");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y,'Blue',5)");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dcurve(x,y,'Blue',5,1)");
		}

		[Test]
		public void Test_02_Plot2dLine()
		{
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dline(x,y)");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dline(x,y,'Blue')");
			this.Test("x:=-10..10|0.1;y:=sin(x);plot2dline(x,y,'Blue',5)");
		}

		[Test]
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

		[Test]
		public void Test_04_PhysicalQuantities()
		{
			this.Test("x:=-10..10|0.1;t:=DateTime(2016,3,11).AddHours(x);y:=sin(x) C;plot2dcurve(t,y)");
		}
	}
}