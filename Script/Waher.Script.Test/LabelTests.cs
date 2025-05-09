using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Graphs;

namespace Waher.Script.Test
{
	[TestClass]
	public class LabelTests
	{
		[DataTestMethod]
		// Basic cases
		[DataRow("123", "123")] // No decimal point
		[DataRow("123.0", "123.0")] // Fewer than 5 trailing zeros
		[DataRow("123.00", "123.00")] // Fewer than 5 trailing zeros
		[DataRow("123.000", "123.000")] // Fewer than 5 trailing zeros
		[DataRow("123.0000", "123.0000")] // Fewer than 5 trailing zeros
		[DataRow("123.00000", "123")] // Exactly 5 trailing zeros
		[DataRow("123.000000", "123")] // More than 5 trailing zeros
		[DataRow("123.450000", "123.450000")] // Exactly 4 trailing zeros after significant digits
		[DataRow("123.4500000", "123.45")] // Exactly 5 trailing zeros after significant digits
		[DataRow("123.45000000", "123.45")] // More than 5 trailing zeros after significant digits

		// Rounding cases
		[DataRow("123.9", "123.9")] // Fewer than 5 trailing nines
		[DataRow("123.99", "123.99")] // Fewer than 5 trailing nines
		[DataRow("123.999", "123.999")] // Fewer than 5 trailing nines
		[DataRow("123.9999", "123.9999")] // Fewer than 5 trailing nines
		[DataRow("123.99999", "124")] // Exactly 5 trailing nines
		[DataRow("123.999999", "124")] // More than 5 trailing nines
		[DataRow("123.49999", "123.49999")] // Exactly 4 trailing nines in decimal section
		[DataRow("123.499999", "123.5")] // Exactly 5 trailing nines in decimal section
		[DataRow("123.4999999", "123.5")] // More than 5 trailing nines in decimal section

		// Edge cases
		[DataRow("123.00001", "123.00001")] // No trimming for small non-zero decimals
		[DataRow("123.45001", "123.45001")] // No trimming for small non-zero decimals
		[DataRow("0.00000", "0")] // Zero with exactly 5 trailing zeros
		[DataRow("0.000000", "0")] // Zero with more than 5 trailing zeros
		[DataRow("0.9999", "0.9999")] // Rounding up from zero with exactly 4 trailing nines
		[DataRow("0.99999", "1")] // Rounding up from zero with exactly 5 trailing nines
		[DataRow("0.999999", "1")] // Rounding up from zero with more than 5 trailing nines
		[DataRow("0.00001", "0.00001")] // Small non-zero value

		// Negative numbers
		[DataRow("-123.0", "-123.0")] // Fewer than 5 trailing zeros
		[DataRow("-123.00", "-123.00")] // Fewer than 5 trailing zeros
		[DataRow("-123.000", "-123.000")] // Fewer than 5 trailing zeros
		[DataRow("-123.0000", "-123.0000")] // Fewer than 5 trailing zeros
		[DataRow("-123.00000", "-123")] // Negative number with exactly 5 trailing zeros
		[DataRow("-123.000000", "-123")] // Negative number with more than 5 trailing zeros
		[DataRow("-123.9999", "-123.9999")] // Negative number rounding up with exactly 4 trailing nines
		[DataRow("-123.99999", "-124")] // Negative number rounding up with exactly 5 trailing nines
		[DataRow("-123.999999", "-124")] // Negative number rounding up with more than 5 trailing nines
		[DataRow("-123.450000", "-123.450000")] // Negative number with exactly 4 trailing zeros
		[DataRow("-123.4500000", "-123.45")] // Negative number with exactly 5 trailing zeros
		[DataRow("-123.45000000", "-123.45")] // Negative number with more than 5 trailing zeros
		[DataRow("-0.9999", "-0.9999")] // Negative rounding up from zero with exactly 4 trailing nines
		[DataRow("-0.99999", "-1")] // Negative rounding up from zero with exactly 5 trailing nines
		[DataRow("-0.999999", "-1")] // Negative rounding up from zero with more than 5 trailing nines
		[DataRow("-0.00001", "-0.00001")] // Small negative non-zero value
		public void Test_01_TrimLabel(string Input, string Expected)
		{
			string Result = Graph.TrimLabel(Input);
			Assert.AreEqual(Expected, Result);
		}

		[TestMethod]
		public void Test_02_TrimLabels2()
		{
			Assert.AreEqual("0", Graph.TrimLabel("0.00000000000000000001"));
			Assert.AreEqual("0.1", Graph.TrimLabel("0.10000000000000000001"));
			Assert.AreEqual("0.01", Graph.TrimLabel("0.01000000000000000001"));
			Assert.AreEqual("0.001", Graph.TrimLabel("0.00100000000000000001"));
			Assert.AreEqual("0.0001", Graph.TrimLabel("0.00010000000000000001"));

			Assert.AreEqual("-0", Graph.TrimLabel("-0.00000000000000000001"));
			Assert.AreEqual("-0.1", Graph.TrimLabel("-0.10000000000000000001"));
			Assert.AreEqual("-0.01", Graph.TrimLabel("-0.01000000000000000001"));
			Assert.AreEqual("-0.001", Graph.TrimLabel("-0.00100000000000000001"));
			Assert.AreEqual("-0.0001", Graph.TrimLabel("-0.00010000000000000001"));

			Assert.AreEqual("40", Graph.TrimLabel("39.99999999999999999999"));
			Assert.AreEqual("10", Graph.TrimLabel("9.99999999999999999999"));
			Assert.AreEqual("1", Graph.TrimLabel("0.99999999999999999999"));
			Assert.AreEqual("0.1", Graph.TrimLabel("0.09999999999999999999"));
			Assert.AreEqual("0.01", Graph.TrimLabel("0.00999999999999999999"));
			Assert.AreEqual("0.001", Graph.TrimLabel("0.000999999999999999999"));
			Assert.AreEqual("0.0001", Graph.TrimLabel("0.000099999999999999999"));

			Assert.AreEqual("-40", Graph.TrimLabel("-39.99999999999999999999"));
			Assert.AreEqual("-10", Graph.TrimLabel("-9.99999999999999999999"));
			Assert.AreEqual("-1", Graph.TrimLabel("-0.99999999999999999999"));
			Assert.AreEqual("-0.1", Graph.TrimLabel("-0.09999999999999999999"));
			Assert.AreEqual("-0.01", Graph.TrimLabel("-0.00999999999999999999"));
			Assert.AreEqual("-0.001", Graph.TrimLabel("-0.000999999999999999999"));
			Assert.AreEqual("-0.0001", Graph.TrimLabel("-0.000099999999999999999"));
		}
	}
}
