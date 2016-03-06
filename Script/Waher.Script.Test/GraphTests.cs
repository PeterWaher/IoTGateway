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
	}
}