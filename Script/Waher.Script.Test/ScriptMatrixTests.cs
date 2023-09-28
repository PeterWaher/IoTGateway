using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;
using System.Threading.Tasks;

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
	}
}