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
	public class RangeTests
	{
		private RangeInfo range;

		[SetUp]
		public void SetUp()
		{
			this.range = new RangeInfo("Field");
		}

		[Test]
		public void Test_01_Point()
		{
			Assert.IsTrue(this.range.SetPoint(10));
			Assert.IsFalse(this.range.IsRange);
			Assert.AreEqual(this.range.Point, 10);
		}

		[Test]
		public void Test_02_Inconsistent_Point()
		{
			Assert.IsTrue(this.range.SetPoint(10));
			Assert.IsFalse(this.range.SetPoint(20));
		}

		[Test]
		public void Test_03_MinInclusive()
		{
			Assert.IsTrue(this.range.SetMin(10, true));
			Assert.IsTrue(this.range.IsRange);
			Assert.AreEqual(this.range.Min, 10);
			Assert.IsTrue(this.range.MinInclusive);
		}

		[Test]
		public void Test_04_MinExclusive()
		{
			Assert.IsTrue(this.range.SetMin(10, false));
			Assert.IsTrue(this.range.IsRange);
			Assert.AreEqual(this.range.Min, 10);
			Assert.IsFalse(this.range.MinInclusive);
		}

		[Test]
		public void Test_05_Complimentary_MinInclusive()
		{
			Assert.IsTrue(this.range.SetMin(10, false));
			Assert.IsTrue(this.range.SetMin(20, true));
			Assert.IsTrue(this.range.SetMin(15, true));
			Assert.IsTrue(this.range.IsRange);
			Assert.AreEqual(this.range.Min, 20);
			Assert.IsTrue(this.range.MinInclusive);
		}

		[Test]
		public void Test_06_Complimentary_Point_MinInclusive()
		{
			Assert.IsTrue(this.range.SetPoint(10));
			Assert.IsTrue(this.range.SetMin(10, true));
			Assert.IsFalse(this.range.IsRange);
			Assert.AreEqual(this.range.Point, 10);
			Assert.Null(this.range.Min);
		}

		[Test]
		public void Test_07_Complimentary_Point_MinInclusive_2()
		{
			Assert.IsTrue(this.range.SetMin(10, true));
			Assert.IsTrue(this.range.SetPoint(10));
			Assert.IsFalse(this.range.IsRange);
			Assert.AreEqual(this.range.Point, 10);
			Assert.Null(this.range.Min);
		}

		[Test]
		public void Test_08_Inconsistent_Point_MinInclusive()
		{
			Assert.IsTrue(this.range.SetPoint(10));
			Assert.IsFalse(this.range.SetMin(15, true));
		}

		[Test]
		public void Test_09_Inconsistent_Point_MinInclusive_2()
		{
			Assert.IsTrue(this.range.SetMin(15, true));
			Assert.IsFalse(this.range.SetPoint(10));
		}

		[Test]
		public void Test_10_Inconsistent_Point_MinInclusive_3()
		{
			Assert.IsTrue(this.range.SetPoint(10));
			Assert.IsFalse(this.range.SetMin(10, false));
		}

		[Test]
		public void Test_11_Inconsistent_Point_MinInclusive_4()
		{
			Assert.IsTrue(this.range.SetMin(10, false));
			Assert.IsFalse(this.range.SetPoint(10));
		}

		[Test]
		public void Test_12_MaxInclusive()
		{
			Assert.IsTrue(this.range.SetMax(10, true));
			Assert.IsTrue(this.range.IsRange);
			Assert.AreEqual(this.range.Max, 10);
			Assert.IsTrue(this.range.MaxInclusive);
		}

		[Test]
		public void Test_13_MaxExclusive()
		{
			Assert.IsTrue(this.range.SetMax(10, false));
			Assert.IsTrue(this.range.IsRange);
			Assert.AreEqual(this.range.Max, 10);
			Assert.IsFalse(this.range.MaxInclusive);
		}

		[Test]
		public void Test_14_Complimentary_MaxInclusive()
		{
			Assert.IsTrue(this.range.SetMax(20, false));
			Assert.IsTrue(this.range.SetMax(10, true));
			Assert.IsTrue(this.range.SetMax(15, true));
			Assert.IsTrue(this.range.IsRange);
			Assert.AreEqual(this.range.Max, 10);
			Assert.IsTrue(this.range.MaxInclusive);
		}

		[Test]
		public void Test_15_Complimentary_Point_MaxInclusive()
		{
			Assert.IsTrue(this.range.SetPoint(10));
			Assert.IsTrue(this.range.SetMax(10, true));
			Assert.IsFalse(this.range.IsRange);
			Assert.AreEqual(this.range.Point, 10);
			Assert.Null(this.range.Max);
		}

		[Test]
		public void Test_16_Complimentary_Point_MaxInclusive_2()
		{
			Assert.IsTrue(this.range.SetMax(10, true));
			Assert.IsTrue(this.range.SetPoint(10));
			Assert.IsFalse(this.range.IsRange);
			Assert.AreEqual(this.range.Point, 10);
			Assert.Null(this.range.Max);
		}

		[Test]
		public void Test_17_Inconsistent_Point_MaxInclusive()
		{
			Assert.IsTrue(this.range.SetPoint(15));
			Assert.IsFalse(this.range.SetMax(10, true));
		}

		[Test]
		public void Test_18_Inconsistent_Point_MaxInclusive_2()
		{
			Assert.IsTrue(this.range.SetMax(10, true));
			Assert.IsFalse(this.range.SetPoint(15));
		}

		[Test]
		public void Test_19_Inconsistent_Point_MaxInclusive_3()
		{
			Assert.IsTrue(this.range.SetPoint(10));
			Assert.IsFalse(this.range.SetMax(10, false));
		}

		[Test]
		public void Test_20_Inconsistent_Point_MaxInclusive_4()
		{
			Assert.IsTrue(this.range.SetMax(10, false));
			Assert.IsFalse(this.range.SetPoint(10));
		}

		[Test]
		public void Test_21_Complimentary_MinMax()
		{
			Assert.IsTrue(this.range.SetMin(10, true));
			Assert.IsTrue(this.range.SetMax(20, true));
			Assert.IsTrue(this.range.IsRange);
			Assert.AreEqual(this.range.Min, 10);
			Assert.AreEqual(this.range.Max, 20);
			Assert.IsTrue(this.range.MinInclusive);
			Assert.IsTrue(this.range.MaxInclusive);
		}

		[Test]
		public void Test_22_Complimentary_MinMax_2()
		{
			Assert.IsTrue(this.range.SetMin(10, true));
			Assert.IsTrue(this.range.SetMin(12, false));
			Assert.IsTrue(this.range.SetMin(11, true));
			Assert.IsTrue(this.range.SetMax(20, true));
			Assert.IsTrue(this.range.SetMax(18, false));
			Assert.IsTrue(this.range.SetMax(19, true));
			Assert.IsTrue(this.range.IsRange);
			Assert.AreEqual(this.range.Min, 12);
			Assert.AreEqual(this.range.Max, 18);
			Assert.IsFalse(this.range.MinInclusive);
			Assert.IsFalse(this.range.MaxInclusive);
		}

		[Test]
		public void Test_23_Inconsistent_MinMax()
		{
			Assert.IsTrue(this.range.SetMin(20, true));
			Assert.IsFalse(this.range.SetMax(10, true));
		}

		[Test]
		public void Test_24_Inconsistent_MinMax_2()
		{
			Assert.IsTrue(this.range.SetMax(10, true));
			Assert.IsFalse(this.range.SetMin(20, true));
		}

		[Test]
		public void Test_25_Inconsistent_MinMax_3()
		{
			Assert.IsTrue(this.range.SetMax(10, true));
			Assert.IsFalse(this.range.SetMin(10, false));
		}

		[Test]
		public void Test_26_Inconsistent_MinMax_4()
		{
			Assert.IsTrue(this.range.SetMax(10, false));
			Assert.IsFalse(this.range.SetMin(10, true));
		}

		[Test]
		public void Test_27_Inconsistent_MinMax_5()
		{
			Assert.IsTrue(this.range.SetMax(10, false));
			Assert.IsFalse(this.range.SetMin(10, false));
		}

		[Test]
		public void Test_28_MinMax_Point()
		{
			Assert.IsTrue(this.range.SetMax(10, true));
			Assert.IsTrue(this.range.SetMin(10, true));
			Assert.IsFalse(this.range.IsRange);
			Assert.AreEqual(this.range.Point, 10);
			Assert.Null(this.range.Min);
			Assert.Null(this.range.Max);
		}
	}
}
