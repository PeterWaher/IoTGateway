using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Graphs;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptDifferentiationTests
	{
		private async Task Test(string Script)
		{
			Variables v = new Variables();

			Expression Exp = new Expression("DrawTangent(x0[],f,min,max):=" +
				"(" +
				"x:=min..max|((max-min)/100);" +
				"plot2dcurve(x,f(x),\"Blue\")+" +
				"sum([foreach xi in x0 do plot2dcurve(x,f'(xi)*(x-xi)+f(xi),\"Red\")])+" +
				"scatter2d(x0,f(x0),\"Red\",5);" +
				");" + Script);
			Graph Result = await Exp.EvaluateAsync(v) as Graph;

			if (Result is null)
				Assert.Fail("Expected graph.");

			GraphSettings Settings = new GraphSettings();
			Result.CreatePixels(Settings);
		}

		[TestMethod]
		public async Task Differentiation_Test_01_Sin()
		{
			await this.Test("DrawTangent([-5,-2,2,5],sin,-10,10)");
		}

		[TestMethod]
		public async Task Differentiation_Test_02_Cos()
		{
			await this.Test("DrawTangent([-5,-2,2,5],cos,-10,10)");
		}

		[TestMethod]
		public async Task Differentiation_Test_03_Tan()
		{
			await this.Test("DrawTangent([-0.5,0.5],tan,-1,1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_04_Sec()
		{
			await this.Test("DrawTangent([-0.5,0.5],sec,-1,1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_05_Csc()
		{
			await this.Test("DrawTangent([1],csc,0.5,2)+DrawTangent([-1],csc,-2,-0.5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_06_Cot()
		{
			await this.Test("DrawTangent([1],cot,0.5,2)+DrawTangent([-1],cot,-2,-0.5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_07_SinH()
		{
			await this.Test("DrawTangent([-2,2],sinh,-3,3)");
		}

		[TestMethod]
		public async Task Differentiation_Test_08_CosH()
		{
			await this.Test("DrawTangent([-2,2],cosh,-3,3)");
		}

		[TestMethod]
		public async Task Differentiation_Test_09_TanH()
		{
			await this.Test("DrawTangent([-0.5,0.5],tanh,-1,1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_10_SecH()
		{
			await this.Test("DrawTangent([-0.5,0.5],sech,-1,1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_11_CscH()
		{
			await this.Test("DrawTangent([1],csch,0.5,2)+DrawTangent([-1],csch,-2,-0.5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_12_CotH()
		{
			await this.Test("DrawTangent([1],coth,0.5,2)+DrawTangent([-1],coth,-2,-0.5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_13_ArcSin()
		{
			await this.Test("DrawTangent([-0.5,0.5],arcsin,-1,1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_14_ArcCos()
		{
			await this.Test("DrawTangent([-0.5,0.5],arccos,-1,1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_15_ArcTan()
		{
			await this.Test("DrawTangent([-5,5],arctan,-10,10)");
		}

		[TestMethod]
		public async Task Differentiation_Test_16_ArcSec()
		{
			await this.Test("DrawTangent([2,5],arcsec,1,10)");
			await this.Test("DrawTangent([-2,-5],arcsec,-10,-1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_17_ArcCsc()
		{
			await this.Test("DrawTangent([2,5],arccsc,1,10)");
			await this.Test("DrawTangent([-2,-5],arccsc,-10,-1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_18_ArcCot()
		{
			await this.Test("DrawTangent([2,5],arccot,1,10)");
			await this.Test("DrawTangent([-2,-5],arccot,-10,-1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_19_ArcSinH()
		{
			await this.Test("DrawTangent([-0.5,0.5],arcsinh,-1,1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_20_ArcCosH()
		{
			await this.Test("DrawTangent([4,8],arccosh,2,10)");
		}

		[TestMethod]
		public async Task Differentiation_Test_21_ArcTanH()
		{
			await this.Test("DrawTangent([-0.5,0.5],arctanh,-0.9,0.9)");
		}

		[TestMethod]
		public async Task Differentiation_Test_22_ArcSecH()
		{
			await this.Test("DrawTangent([0.25,0.75],arcsech,0.1,0.9)");
		}

		[TestMethod]
		public async Task Differentiation_Test_23_ArcCscH()
		{
			await this.Test("DrawTangent([2,5],arccsch,1,10)");
			await this.Test("DrawTangent([-2,-5],arccsch,-10,-1)");
		}

		[TestMethod]
		public async Task Differentiation_Test_24_ArcCotH()
		{
			await this.Test("DrawTangent([2],arccoth,1.1,3)");
		}

		[TestMethod]
		public async Task Differentiation_Test_25_Ln()
		{
			await this.Test("DrawTangent([2,4],ln,1,5);");
		}

		[TestMethod]
		public async Task Differentiation_Test_26_Lg()
		{
			await this.Test("DrawTangent([2,4],lg,1,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_27_Log2()
		{
			await this.Test("DrawTangent([2,4],log2,1,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_28_Exp()
		{
			await this.Test("DrawTangent([-2,2],exp,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_29_Power()
		{
			await this.Test("DrawTangent([-2,2],x->x^1,-5,5)");
			await this.Test("DrawTangent([-2,2],x->x^2,-5,5)");
			await this.Test("DrawTangent([-2,2],x->x^3,-5,5)");
			await this.Test("DrawTangent([-2,2],x->2^x,-5,5)");
			await this.Test("DrawTangent([1,4],x->x^x,0.1,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_30_Addition()
		{
			await this.Test("DrawTangent([-2,2],x->x^2+x^3,-5,5)");
			await this.Test("DrawTangent([-2,2],x->x^2.+x^3,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_31_Subtraction()
		{
			await this.Test("DrawTangent([-2,2],x->x^2-x^3,-5,5)");
			await this.Test("DrawTangent([-2,2],x->x^2.-x^3,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_32_Multiplication()
		{
			await this.Test("DrawTangent([-2,2],x->x^2*x^3,-5,5)");
			await this.Test("DrawTangent([-2,2],x->x^2.*x^3,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_33_Division()
		{
			await this.Test("DrawTangent([-2,2],x->x^2/(1+x^4),-5,5)");
			await this.Test("DrawTangent([-2,2],x->x^2./(1+x^4),-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_34_Cube()
		{
			await this.Test("DrawTangent([-2,2],x->x³,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_35_Square()
		{
			await this.Test("DrawTangent([-2,2],x->x²,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_36_DegreesToRadians()
		{
			await this.Test("DrawTangent([-2,2],x->x°,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_37_Percent()
		{
			await this.Test("DrawTangent([-2,2],x->x%,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_38_Permil()
		{
			await this.Test("DrawTangent([-2,2],x->x‰,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_39_Perdiezmil()
		{
			await this.Test("DrawTangent([-2,2],x->x‱,-5,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_40_Inverse()
		{
			await this.Test("DrawTangent([2,4],ln',1,5)");
		}

		[TestMethod]
		public async Task Differentiation_Test_41_Negation()
		{
			await this.Test("DrawTangent([-2,2],x->-x^2,-5,5)");
		}
	}
}