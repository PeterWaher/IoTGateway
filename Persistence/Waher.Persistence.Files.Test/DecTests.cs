using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.Files.Searching;

#if !LW
namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public class DecTests
	{
		[TestMethod]
		public void Test_01_Boolean()
		{
			object Value = true;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, false);
		}

		[TestMethod]
		public void Test_02_Byte()
		{
			object Value = (byte)10;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, (byte)9);
		}

		[TestMethod]
		public void Test_03_Int16()
		{
			object Value = (short)10;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, (short)9);
		}

		[TestMethod]
		public void Test_04_Int32()
		{
			object Value = (int)10;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, (int)9);
		}

		[TestMethod]
		public void Test_05_Int64()
		{
			object Value = (long)10;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, (long)9);
		}

		[TestMethod]
		public void Test_06_SByte()
		{
			object Value = (sbyte)10;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, (sbyte)9);
		}

		[TestMethod]
		public void Test_07_UInt16()
		{
			object Value = (ushort)10;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, (ushort)9);
		}

		[TestMethod]
		public void Test_08_UInt32()
		{
			object Value = (uint)10;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, (uint)9);
		}

		[TestMethod]
		public void Test_09_UInt64()
		{
			object Value = (ulong)10;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, (ulong)9);
		}

		[TestMethod]
		public void Test_10_Decimal()
		{
			decimal Org = (decimal)10;
			object Value = Org;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreNotEqual(Value, Org);
			decimal Diff = (decimal)Value - Org;
			Assert.AreEqual(Org + Diff / 2, Org);
		}

		[TestMethod]
		public void Test_11_Double()
		{
			double Org = (double)10;
			object Value = Org;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreNotEqual(Value, Org);
			double Diff = (double)Value - Org;
			Assert.AreEqual(Org + Diff / 2, Org);
		}

		[TestMethod]
		public void Test_12_Single()
		{
			float Org = (float)10;
			object Value = Org;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreNotEqual(Value, Org);
			float Diff = (float)Value - Org;
			Assert.AreEqual(Org + Diff / 2, Org);
		}

		[TestMethod]
		public void Test_13_DateTime()
		{
			DateTime Org = DateTime.Now;
			object Value = Org;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreNotEqual(Value, Org);
		}

		[TestMethod]
		public void Test_14_TimeSpan()
		{
			TimeSpan Org = DateTime.Now.TimeOfDay;
			object Value = Org;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreNotEqual(Value, Org);
		}

		[TestMethod]
		public void Test_15_Char()
		{
			object Value = 'A';
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, '@');
		}

		[TestMethod]
		public void Test_16_String()
		{
			object Value = "Hello";
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, "Helln\uffff");
		}

		[TestMethod]
		public void Test_17_Guid()
		{
			Guid Guid = System.Guid.NewGuid();
			object Value = Guid;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreNotEqual(Value, Guid);
		}
		[TestMethod]
		public void Test_18_Boolean_Underflow()
		{
			object Value = false;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, -1);
		}

		[TestMethod]
		public void Test_19_Byte_Underflow()
		{
			object Value = byte.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, -1);
		}

		[TestMethod]
		public void Test_20_Int16_Underflow()
		{
			object Value = short.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, -32769);
		}

		[TestMethod]
		public void Test_21_Int32_Underflow()
		{
			object Value = int.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, (long)int.MinValue - 1);
		}

		[TestMethod]
		public void Test_22_Int64_Underflow()
		{
			object Value = long.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			double d = long.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref d));
			Assert.AreEqual(Value, d);
		}

		[TestMethod]
		public void Test_23_SByte_Underflow()
		{
			object Value = sbyte.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, sbyte.MinValue - 1);
		}

		[TestMethod]
		public void Test_24_UInt16_Underflow()
		{
			object Value = ushort.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, -1);
		}

		[TestMethod]
		public void Test_25_UInt32_Underflow()
		{
			object Value = uint.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, -1);
		}

		[TestMethod]
		public void Test_26_UInt64_Underflow()
		{
			object Value = ulong.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, -1);
		}

		[TestMethod]
		public void Test_27_Char_Underflow()
		{
			object Value = char.MinValue;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreEqual(Value, -1);
		}

		[TestMethod]
		public void Test_28_Decimal_Epsilon()
		{
			decimal Org = (decimal)0;
			object Value = Org;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreNotEqual(Value, Org);
			decimal Diff = (decimal)Value - Org;
			Assert.AreEqual(Org + Diff / 2, Org);
		}

		[TestMethod]
		public void Test_29_Double_Epsilon()
		{
			double Org = (double)0;
			object Value = Org;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreNotEqual(Value, Org);
			double Diff = (double)Value - Org;
			Assert.AreEqual(Org + Diff / 2, Org);
		}

		[TestMethod]
		public void Test_30_Single_Epsilon()
		{
			float Org = (float)0;
			object Value = Org;
			Assert.IsTrue(Comparison.Decrement(ref Value));
			Assert.AreNotEqual(Value, Org);
			float Diff = (float)Value - Org;
			Assert.AreEqual(Org + Diff / 2, Org);
		}
	}
}
