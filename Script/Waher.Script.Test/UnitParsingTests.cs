using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Units;

namespace Waher.Script.Test
{
	[TestClass]
	public class UnitParsingTests
	{
		[TestMethod]
		public void Test_01_m()
		{
			Unit Unit = Unit.Parse("m");
			Assert.AreEqual(new Unit(Prefix.None, "m"), Unit);
		}

		[TestMethod]
		public void Test_02_km()
		{
			Unit Unit = Unit.Parse("km");
			Assert.AreEqual(new Unit(Prefix.Kilo, "m"), Unit);
		}

		[TestMethod]
		public void Test_03_mm()
		{
			Unit Unit = Unit.Parse("mm");
			Assert.AreEqual(new Unit(Prefix.Milli, "m"), Unit);
		}

		[TestMethod]
		public void Test_04_m2()
		{
			Unit Unit = Unit.Parse("m^2");
			Assert.AreEqual(new Unit(Prefix.None, new UnitFactor("m", 2)), Unit);

			Unit = Unit.Parse("m²");
			Assert.AreEqual(new Unit(Prefix.None, new UnitFactor("m", 2)), Unit);
		}

		[TestMethod]
		public void Test_05_m3()
		{
			Unit Unit = Unit.Parse("m^3");
			Assert.AreEqual(new Unit(Prefix.None, new UnitFactor("m", 3)), Unit);

			Unit = Unit.Parse("m³");
			Assert.AreEqual(new Unit(Prefix.None, new UnitFactor("m", 3)), Unit);
		}

		[TestMethod]
		public void Test_06_Ws()
		{
			Unit Unit = Unit.Parse("W⋅s");
			Assert.AreEqual(new Unit(Prefix.None, "W", "s"), Unit);

			Unit = Unit.Parse("W*s");
			Assert.AreEqual(new Unit(Prefix.None, "W", "s"), Unit);
		}

		[TestMethod]
		public void Test_07_mps()
		{
			Unit Unit = Unit.Parse("m⋅s^-1");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("m", 1),
				new UnitFactor("s", -1)), Unit);

			Unit = Unit.Parse("m/s");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("m", 1),
				new UnitFactor("s", -1)), Unit);
		}

		[TestMethod]
		public void Test_08_m2ps()
		{
			Unit Unit = Unit.Parse("m^2/s");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("m", 2),
				new UnitFactor("s", -1)), Unit);

			Unit = Unit.Parse("m²/s");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("m", 2),
				new UnitFactor("s", -1)), Unit);
		}

		[TestMethod]
		public void Test_09_mps2()
		{
			Unit Unit = Unit.Parse("m/s^2");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("m", 1),
				new UnitFactor("s", -2)), Unit);

			Unit = Unit.Parse("m/s²");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("m", 1),
				new UnitFactor("s", -2)), Unit);
		}

		[TestMethod]
		public void Test_10_Expression()
		{
			Unit Unit = Unit.Parse("kg⋅m²/(A⋅s³)");
			Assert.AreEqual(new Unit(Prefix.Kilo,
				new UnitFactor("g", 1),
				new UnitFactor("m", 2),
				new UnitFactor("A", -1),
				new UnitFactor("s", -3)), Unit);
		}

		[TestMethod]
		public void Test_11_kWh()
		{
			Unit Unit = Unit.Parse("kWh");
			Assert.AreEqual(new Unit(Prefix.Kilo,
				new UnitFactor("W", 1),
				new UnitFactor("h", 1)), Unit);
		}

		[TestMethod]
		public void Test_12_mph()
		{
			Unit Unit = Unit.Parse("mph");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("SM", 1),
				new UnitFactor("h", -1)), Unit);
		}

		[TestMethod]
		public void Test_13_fps()
		{
			Unit Unit = Unit.Parse("fps");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("ft", 1),
				new UnitFactor("s", -1)), Unit);
		}

		[TestMethod]
		public void Test_14_ft_in()
		{
			Unit Unit = Unit.Parse("ft");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("ft", 1)), Unit);

			Unit = Unit.Parse("inch^2");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("inch", 2)), Unit);
		}

		[TestMethod]
		public void Test_15_da()
		{
			Unit Unit = Unit.Parse("dal");
			Assert.AreEqual(new Unit(Prefix.Deka,
				new UnitFactor("l", 1)), Unit);
		}

		[TestMethod]
		public void Test_16_Percent()
		{
			Unit Unit = Unit.Parse("%");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("%", 1)), Unit);
		}

		[TestMethod]
		public void Test_17_Degree()
		{
			Unit Unit = Unit.Parse("°");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("°", 1)), Unit);

			Unit = Unit.Parse("deg");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("deg", 1)), Unit);
		}

		[TestMethod]
		public void Test_18_PerMille()
		{
			Unit Unit = Unit.Parse("‰");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("‰", 1)), Unit);

			Unit = Unit.Parse("%0");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("%0", 1)), Unit);
		}

		[TestMethod]
		public void Test_19_PerDixMille()
		{
			Unit Unit = Unit.Parse("‱");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("‱", 1)), Unit);

			Unit = Unit.Parse("%00");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("%00", 1)), Unit);
		}

		[TestMethod]
		public void Test_20_Radians()
		{
			Unit Unit = Unit.Parse("rad");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("rad", 1)), Unit);
		}

		[TestMethod]
		public void Test_21_One()
		{
			Unit Unit = Unit.Parse("1");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("1", 1)), Unit);

			Unit = Unit.Parse("pcs");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("pcs", 1)), Unit);
		}

		[TestMethod]
		public void Test_22_Dozen()
		{
			Unit Unit = Unit.Parse("dz");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("dz", 1)), Unit);

			Unit = Unit.Parse("dozen");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("dozen", 1)), Unit);
		}

		[TestMethod]
		public void Test_23_Gross()
		{
			Unit Unit = Unit.Parse("gr");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("gr", 1)), Unit);

			Unit = Unit.Parse("gross");
			Assert.AreEqual(new Unit(Prefix.None,
				new UnitFactor("gross", 1)), Unit);
		}

	}
}