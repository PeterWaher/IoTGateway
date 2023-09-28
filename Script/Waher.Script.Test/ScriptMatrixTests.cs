using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using Waher.Script.Objects;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptMatrixTests
	{
		[TestMethod]
		public async Task DoubleMatrix_Test_01_Multiply()
		{
			await ScriptEvaluationTests.Test("[[1,2,3],[2,3,4]]*[[3,4],[4,5],[5,6]]",
				new double[,]
				{
					{ 26, 32 },
					{ 38, 47 }
				});
		}

		[TestMethod]
		public async Task DoubleMatrix_Test_02_Inverse()
		{
			await ScriptEvaluationTests.Test("inv([[1,2],[3,4]])",
				new double[,]
				{
					{ -2, 1 },
					{ 1.5, -0.5 }
				});
		}

		[TestMethod]
		public async Task DoubleMatrix_Test_03_Reduce()
		{
			await ScriptEvaluationTests.Test("reduce([[1,2,3,4],[2,3,4,5],[3,4,5,6]])",
				new double[,]
				{
					{ 1, 4d/3, 5d/3, 2 },
					{ 0, 1, 2, 3 },
					{ 0, 0, 1, 2d/3 }
				});
		}

		[TestMethod]
		public async Task DoubleMatrix_Test_04_Eliminate()
		{
			await ScriptEvaluationTests.Test("eliminate([[1,2,3,4],[2,3,4,5],[3,4,5,6]])",
				new double[,]
				{
					{ 1, 0, 0, -4d/3 },
					{ 0, 1, 0, 5d/3 },
					{ 0, 0, 1, 2d/3 }
				});
		}

		[TestMethod]
		public async Task DoubleMatrix_Test_05_Diagonal()
		{
			await ScriptEvaluationTests.Test("diagonal([[1,2,3,4],[2,3,4,5],[3,4,5,6]])",
				new double[] { 1, 3, 5 });
			await ScriptEvaluationTests.Test("diag([[1,2,3,4],[2,3,4,5],[3,4,5,6]])",
				new double[] { 1, 3, 5 });
		}

		[TestMethod]
		public async Task DoubleMatrix_Test_06_Diagonal()
		{
			await ScriptEvaluationTests.Test("trace([[1,2,3,4],[2,3,4,5],[3,4,5,6]])", 9);
			await ScriptEvaluationTests.Test("tr([[1,2,3,4],[2,3,4,5],[3,4,5,6]])", 9);
		}

		[TestMethod]
		public async Task DoubleMatrix_Test_07_Determinant()
		{
			await ScriptEvaluationTests.Test("determinant([[3,8],[4,6]])", -14);
			await ScriptEvaluationTests.Test("det([[1,2],[3,4]])", -2);
			await ScriptEvaluationTests.Test("determinant([[4,6],[3,8]])", 14);
			await ScriptEvaluationTests.Test("det([[6,1,1],[4,-2,5],[2,8,7]])", -306);
			await ScriptEvaluationTests.Test("det([[4,3,4,2],[8,7,5,3],[4,3,8,5],[4,3,4,3]])", 16);
		}

		[TestMethod]
		public async Task ComplexMatrix_Test_01_Multiply()
		{
			await ScriptEvaluationTests.Test("[[i,2,3],[2,3,4]]*[[3*i,4],[4,5],[5,6]]",
				new Complex[,]
				{
					{ new Complex(20, 0), new Complex(28, 4) },
					{ new Complex(32, 6), new Complex(47, 0) }
				});

			await ScriptEvaluationTests.Test("[[1,2*i,3],[2,3,4]]*[[3,4],[4,5],[5*i,6]]",
				new Complex[,]
				{
					{ new Complex(3, 23), new Complex(22, 10) },
					{ new Complex(18, 20), new Complex(47, 0) }
				});
		}

		[TestMethod]
		public async Task ObjectMatrix_Test_01_Multiply()
		{
			await ScriptEvaluationTests.Test("[[1,2,3],[2,3,4*i]]*[[3,4],[4,5],[5,6]]",
				new Complex[,]
				{
					{ new Complex(26, 0), new Complex(32, 0) },
					{ new Complex(18, 20), new Complex(23, 24) }
				});
		}

		[TestMethod]
		public async Task ObjectMatrix_Test_02_Inverse()
		{
			await ScriptEvaluationTests.Test("inv([[#1,#2],[#3,#4]])",
				new object[,]
				{
					{ new BigInteger(-2), new BigInteger(1) },
					{ new RationalNumber(3, 2), new RationalNumber(-1, 2) }
				});
		}

	}
}