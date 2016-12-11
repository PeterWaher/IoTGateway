using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Persistence.Files.Searching;

namespace Waher.Persistence.Files.Test
{
	[TestFixture]
	public class IncTests
	{
		[Test]
		public void Test_01_Boolean()
		{
			object Value = false;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, true);
		}

		[Test]
		public void Test_02_Byte()
		{
			object Value = (byte)10;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, (byte)11);
		}

		[Test]
		public void Test_03_Int16()
		{
			object Value = (short)10;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, (short)11);
		}

		[Test]
		public void Test_04_Int32()
		{
			object Value = (int)10;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, (int)11);
		}

		[Test]
		public void Test_05_Int64()
		{
			object Value = (long)10;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, (long)11);
		}

		[Test]
		public void Test_06_SByte()
		{
			object Value = (sbyte)10;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, (sbyte)11);
		}

		[Test]
		public void Test_07_UInt16()
		{
			object Value = (ushort)10;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, (ushort)11);
		}

		[Test]
		public void Test_08_UInt32()
		{
			object Value = (uint)10;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, (uint)11);
		}

		[Test]
		public void Test_09_UInt64()
		{
			object Value = (ulong)10;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, (ulong)11);
		}

		[Test]
		public void Test_10_Decimal()
		{
			decimal Org = (decimal)10;
			object Value = Org;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreNotEqual(Value, Org);
			decimal Diff = (decimal)Value - Org;
			Assert.AreEqual(Org + Diff / 2, Org);
		}

		[Test]
		public void Test_11_Double()
		{
			double Org = (double)10;
			object Value = Org;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreNotEqual(Value, Org);
			double Diff = (double)Value - Org;
			Assert.AreEqual(Org + Diff / 2, Org);
		}

		[Test]
		public void Test_12_Single()
		{
			float Org = (float)10;
			object Value = Org;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreNotEqual(Value, Org);
			float Diff = (float)Value - Org;
			Assert.AreEqual(Org + Diff / 2, Org);
		}

		[Test]
		public void Test_13_DateTime()
		{
			DateTime Org = DateTime.Now;
			object Value = Org;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreNotEqual(Value, Org);
		}

		[Test]
		public void Test_14_TimeSpan()
		{
			TimeSpan Org = DateTime.Now.TimeOfDay;
			object Value = Org;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreNotEqual(Value, Org);
		}

		[Test]
		public void Test_15_Char()
		{
			object Value = 'A';
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, 'B');
		}

		[Test]
		public void Test_16_String()
		{
			object Value = "Hello";
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreEqual(Value, "Hello\x00");
		}

		[Test]
		public void Test_17_Guid()
		{
			Guid Guid = System.Guid.NewGuid();
			object Value = Guid;
			Assert.IsTrue(Comparison.Increment(ref Value));
			Assert.AreNotEqual(Value, Guid);
		}
	}
}
