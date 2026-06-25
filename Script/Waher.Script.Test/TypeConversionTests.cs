using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;
using Waher.Persistence;
using Waher.Script.Objects;
using Waher.Script.Units;

namespace Waher.Script.Test
{
	public enum TestEnum
	{
		One = 1,
		Two = 2,
		Three = 3
	}

	[TestClass]
	public class TypeConversionTests
	{
		private const decimal TestDecimal = 1.234M;
		private static readonly BigInteger TestBigInteger = new(123);
		private static readonly Measurement TestMeasurement = new(1.234, new Unit("m"), 0.01);
		private static readonly Measurement TestMeasurementNoError = new(1.234, new Unit("m"), 0);
		private static readonly PhysicalQuantity TestQuantity = new(1.234, new Unit("m"));
		private static readonly ComplexNumber TestComplexNumber = new(1.234);
		private static readonly CaseInsensitiveString TestCaseInsensitiveString = new("Hello World.");

		[TestMethod]
		[DataRow("TestBigInteger", 123.0)]
		[DataRow(true, 1.0)]
		[DataRow(false, 0.0)]
		[DataRow("TestDecimal", 1.234)]
		[DataRow(TestEnum.Two, 2.0)]
		[DataRow((sbyte)123, 123.0)]
		[DataRow((short)123, 123.0)]
		[DataRow(123, 123.0)]
		[DataRow((long)123, 123.0)]
		[DataRow((byte)123, 123.0)]
		[DataRow((ushort)123, 123.0)]
		[DataRow((uint)123, 123.0)]
		[DataRow((ulong)123, 123.0)]
		[DataRow("TestMeasurement", 1.234)]
		[DataRow("TestQuantity", 1.234)]
		[DataRow("1.234", 1.234)]
		[DataRow("1,234", 1.234)]
		public void Test_01_ToDouble(object From, object To)
		{
			From = CheckValue(From);
			Assert.IsTrue(Expression.TryConvert(From, To.GetType(), true, out object Result));
			Assert.AreEqual(To, Result);
		}

		[TestMethod]
		[DataRow(123.0, "TestBigInteger")]
		[DataRow(1.234, "TestComplexNumber")]
		[DataRow(1.234, "TestDecimal")]
		[DataRow(123.0, (sbyte)123)]
		[DataRow(123.0, (short)123)]
		[DataRow(123.0, 123)]
		[DataRow(123.0, (long)123)]
		[DataRow(123.0, (byte)123)]
		[DataRow(123.0, (ushort)123)]
		[DataRow(123.0, (uint)123)]
		[DataRow(123.0, (ulong)123)]
		[DataRow(1.234, "1.234")]
		public void Test_02_FromDouble(object From, object To)
		{
			To = CheckValue(To);
			Assert.IsTrue(Expression.TryConvert(From, To.GetType(), true, out object Result));
			Assert.AreEqual(To, Result);
		}

		[TestMethod]
		[DataRow("TestQuantity", "TestMeasurementNoError")]
		[DataRow("TestMeasurement", "TestQuantity")]
		[DataRow("TestCaseInsensitiveString", "Hello World.")]
		[DataRow("Hello World.", "TestCaseInsensitiveString")]
		public void Test_03_ObjectTypes(object From, object To)
		{
			From = CheckValue(From);
			To = CheckValue(To);
			Assert.IsTrue(Expression.TryConvert(From, To.GetType(), true, out object Result));
			Assert.AreEqual(To, Result);
		}

		private static object CheckValue(object From)
		{
			if (From is not string s)
				return From;

			return s switch
			{
				nameof(TestBigInteger) => TestBigInteger,
				nameof(TestComplexNumber) => TestComplexNumber,
				nameof(TestDecimal) => TestDecimal,
				nameof(TestMeasurement) => TestMeasurement,
				nameof(TestMeasurementNoError) => TestMeasurementNoError,
				nameof(TestQuantity) => TestQuantity,
				nameof(TestCaseInsensitiveString) => TestCaseInsensitiveString,
				_ => From,
			};
		}
	}
}