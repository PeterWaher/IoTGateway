using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Graphs;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptDifferentiationTests
	{
		private void Test(string Script)
		{
			Variables v = new Variables();

			Expression Exp = new Expression("DrawTangent(x0[],f,min,max):=" +
				"(" +
				"x:=min..max|((max-min)/100);" +
				"plot2dcurve(x,f(x),\"Blue\")+" +
				"sum([foreach xi in x0 do plot2dcurve(x,f'(xi)*(x-xi)+f(xi),\"Red\")])+" +
				"scatter2d(x0,f(x0),\"Red\",5);" +
				");" + Script);
			Graph Result = Exp.Evaluate(v) as Graph;

			if (Result is null)
				Assert.Fail("Expected graph.");

			GraphSettings Settings = new GraphSettings();
			Result.CreatePixels(Settings);
		}

		[TestMethod]
		public void Differentiation_Test_01_Sin()
		{
			this.Test("DrawTangent([-5,-2,2,5],sin,-10,10)");
		}

		[TestMethod]
		public void Differentiation_Test_02_Cos()
		{
			this.Test("DrawTangent([-5,-2,2,5],cos,-10,10)");
		}

		[TestMethod]
		public void Differentiation_Test_03_Tan()
		{
			this.Test("DrawTangent([-0.5,0.5],tan,-1,1)");
		}

		[TestMethod]
		public void Differentiation_Test_04_Sec()
		{
			this.Test("DrawTangent([-0.5,0.5],sec,-1,1)");
		}

		[TestMethod]
		public void Differentiation_Test_05_Csc()
		{
			this.Test("DrawTangent([1],csc,0.5,2)+DrawTangent([-1],csc,-2,-0.5)");
		}

		[TestMethod]
		public void Differentiation_Test_06_Cot()
		{
			this.Test("DrawTangent([1],cot,0.5,2)+DrawTangent([-1],cot,-2,-0.5)");
		}

		[TestMethod]
		public void Differentiation_Test_07_SinH()
		{
			this.Test("DrawTangent([-2,2],sinh,-3,3)");
		}

		[TestMethod]
		public void Differentiation_Test_08_CosH()
		{
			this.Test("DrawTangent([-2,2],cosh,-3,3)");
		}

		[TestMethod]
		public void Differentiation_Test_09_TanH()
		{
			this.Test("DrawTangent([-0.5,0.5],tanh,-1,1)");
		}

		[TestMethod]
		public void Differentiation_Test_10_SecH()
		{
			this.Test("DrawTangent([-0.5,0.5],sech,-1,1)");
		}

		[TestMethod]
		public void Differentiation_Test_11_CscH()
		{
			this.Test("DrawTangent([1],csch,0.5,2)+DrawTangent([-1],csch,-2,-0.5)");
		}

		[TestMethod]
		public void Differentiation_Test_12_CotH()
		{
			this.Test("DrawTangent([1],coth,0.5,2)+DrawTangent([-1],coth,-2,-0.5)");
		}

		[TestMethod]
		public void Differentiation_Test_13_ArcSin()
		{
			this.Test("DrawTangent([-0.5,0.5],arcsin,-1,1)");
		}

		[TestMethod]
		public void Differentiation_Test_14_ArcCos()
		{
			this.Test("DrawTangent([-0.5,0.5],arccos,-1,1)");
		}

		[TestMethod]
		public void Differentiation_Test_15_ArcTan()
		{
			this.Test("DrawTangent([-5,5],arctan,-10,10)");
		}

		[TestMethod]
		public void Differentiation_Test_16_ArcSec()
		{
			this.Test("DrawTangent([2,5],arcsec,1,10)");
			this.Test("DrawTangent([-2,-5],arcsec,-10,-1)");
		}

		[TestMethod]
		public void Differentiation_Test_17_ArcCsc()
		{
			this.Test("DrawTangent([2,5],arccsc,1,10)");
			this.Test("DrawTangent([-2,-5],arccsc,-10,-1)");
		}

		[TestMethod]
		public void Differentiation_Test_18_ArcCot()
		{
			this.Test("DrawTangent([2,5],arccot,1,10)");
			this.Test("DrawTangent([-2,-5],arccot,-10,-1)");
		}

		[TestMethod]
		public void Differentiation_Test_19_ArcSinH()
		{
			this.Test("DrawTangent([-0.5,0.5],arcsinh,-1,1)");
		}

		[TestMethod]
		public void Differentiation_Test_20_ArcCosH()
		{
			this.Test("DrawTangent([4,8],arccosh,2,10)");
		}

		[TestMethod]
		public void Differentiation_Test_21_ArcTanH()
		{
			this.Test("DrawTangent([-0.5,0.5],arctanh,-0.9,0.9)");
		}

		[TestMethod]
		public void Differentiation_Test_22_ArcSecH()
		{
			this.Test("DrawTangent([2,5],arcsech,1,10)");
			this.Test("DrawTangent([0.25,0.75],arcsech,0.1,0.9)");
		}

		[TestMethod]
		public void Differentiation_Test_23_ArcCscH()
		{
			this.Test("DrawTangent([2,5],arccsch,1,10)");
			this.Test("DrawTangent([-2,-5],arccsch,-10,-1)");
		}

		[TestMethod]
		public void Differentiation_Test_24_ArcCotH()
		{
			this.Test("DrawTangent([2],arccoth,1.1,3)");
		}

		[TestMethod]
		public void Differentiation_Test_25_Ln()
		{
			this.Test("DrawTangent([2,4],ln,1,5);");
		}

		[TestMethod]
		public void Differentiation_Test_26_Lg()
		{
			this.Test("DrawTangent([2,4],lg,1,5)");
		}

		[TestMethod]
		public void Differentiation_Test_27_Log2()
		{
			this.Test("DrawTangent([2,4],log2,1,5)");
		}

		[TestMethod]
		public void Differentiation_Test_28_Exp()
		{
			this.Test("DrawTangent([-2,2],exp,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_29_Power()
		{
			this.Test("DrawTangent([-2,2],x->x^1,-5,5)");
			this.Test("DrawTangent([-2,2],x->x^2,-5,5)");
			this.Test("DrawTangent([-2,2],x->x^3,-5,5)");
			this.Test("DrawTangent([-2,2],x->2^x,-5,5)");
			this.Test("DrawTangent([1,4],x->x^x,0.1,5)");
		}

		[TestMethod]
		public void Differentiation_Test_30_Addition()
		{
			this.Test("DrawTangent([-2,2],x->x^2+x^3,-5,5)");
			this.Test("DrawTangent([-2,2],x->x^2.+x^3,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_31_Subtraction()
		{
			this.Test("DrawTangent([-2,2],x->x^2-x^3,-5,5)");
			this.Test("DrawTangent([-2,2],x->x^2.-x^3,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_32_Multiplication()
		{
			this.Test("DrawTangent([-2,2],x->x^2*x^3,-5,5)");
			this.Test("DrawTangent([-2,2],x->x^2.*x^3,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_33_Division()
		{
			this.Test("DrawTangent([-2,2],x->x^2/(1+x^4),-5,5)");
			this.Test("DrawTangent([-2,2],x->x^2./(1+x^4),-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_34_Cube()
		{
			this.Test("DrawTangent([-2,2],x->x³,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_35_Square()
		{
			this.Test("DrawTangent([-2,2],x->x²,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_36_DegreesToRadians()
		{
			this.Test("DrawTangent([-2,2],x->x°,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_37_Percent()
		{
			this.Test("DrawTangent([-2,2],x->x%,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_38_Permil()
		{
			this.Test("DrawTangent([-2,2],x->x‰,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_39_Perdiezmil()
		{
			this.Test("DrawTangent([-2,2],x->x‱,-5,5)");
		}

		[TestMethod]
		public void Differentiation_Test_40_Inverse()
		{
			this.Test("DrawTangent([2,4],ln',1,5)");
		}

		[TestMethod]
		public void Differentiation_Test_41_Negation()
		{
			this.Test("DrawTangent([-2,2],x->-x^2,-5,5)");
		}
	}
}