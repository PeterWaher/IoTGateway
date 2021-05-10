using System;
using System.Numerics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.Sets;
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
			Assert.AreEqual(new Unit(Prefix.None, new KeyValuePair<string, int>("m", 2)), Unit);

			Unit = Unit.Parse("m²");
			Assert.AreEqual(new Unit(Prefix.None, new KeyValuePair<string, int>("m", 2)), Unit);
		}

		[TestMethod]
		public void Test_05_m3()
		{
			Unit Unit = Unit.Parse("m^3");
			Assert.AreEqual(new Unit(Prefix.None, new KeyValuePair<string, int>("m", 3)), Unit);

			Unit = Unit.Parse("m³");
			Assert.AreEqual(new Unit(Prefix.None, new KeyValuePair<string, int>("m", 3)), Unit);
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
				new KeyValuePair<string, int>("m", 1),
				new KeyValuePair<string, int>("s", -1)), Unit);

			Unit = Unit.Parse("m/s");
			Assert.AreEqual(new Unit(Prefix.None,
				new KeyValuePair<string, int>("m", 1),
				new KeyValuePair<string, int>("s", -1)), Unit);
		}

		[TestMethod]
		public void Test_08_m2ps()
		{
			Unit Unit = Unit.Parse("m^2/s");
			Assert.AreEqual(new Unit(Prefix.None,
				new KeyValuePair<string, int>("m", 2),
				new KeyValuePair<string, int>("s", -1)), Unit);

			Unit = Unit.Parse("m²/s");
			Assert.AreEqual(new Unit(Prefix.None,
				new KeyValuePair<string, int>("m", 2),
				new KeyValuePair<string, int>("s", -1)), Unit);
		}

		[TestMethod]
		public void Test_09_mps2()
		{
			Unit Unit = Unit.Parse("m/s^2");
			Assert.AreEqual(new Unit(Prefix.None,
				new KeyValuePair<string, int>("m", 2),
				new KeyValuePair<string, int>("s", -2)), Unit);

			Unit = Unit.Parse("m/s²");
			Assert.AreEqual(new Unit(Prefix.None,
				new KeyValuePair<string, int>("m", 2),
				new KeyValuePair<string, int>("s", -2)), Unit);
		}

		[TestMethod]
		public void Test_10_Expression()
		{
			Unit Unit = Unit.Parse("kg⋅m²/(A⋅s³)");
			Assert.AreEqual(new Unit(Prefix.Kilo,
				new KeyValuePair<string, int>("g", 1),
				new KeyValuePair<string, int>("m", 2),
				new KeyValuePair<string, int>("A", -1),
				new KeyValuePair<string, int>("s", -3)), Unit);
		}

		[TestMethod]
		public void Test_11_kWh()
		{
			Unit Unit = Unit.Parse("kWh");
			Assert.AreEqual(new Unit(Prefix.Kilo,
				new KeyValuePair<string, int>("W", 1),
				new KeyValuePair<string, int>("h", 1)), Unit);
		}

		[TestMethod]
		public void Test_12_mph()
		{
			Unit Unit = Unit.Parse("mph");
			Assert.AreEqual(new Unit(Prefix.None,
				new KeyValuePair<string, int>("SM", 1),
				new KeyValuePair<string, int>("h", -1)), Unit);
		}

		[TestMethod]
		public void Test_13_fps()
		{
			Unit Unit = Unit.Parse("fps");
			Assert.AreEqual(new Unit(Prefix.None,
				new KeyValuePair<string, int>("ft", 1),
				new KeyValuePair<string, int>("s", -1)), Unit);
		}

		[TestMethod]
		public void Test_14_ft_in()
		{
			Unit Unit = Unit.Parse("ft");
			Assert.AreEqual(new Unit(Prefix.None,
				new KeyValuePair<string, int>("ft", 1)), Unit);

			Unit = Unit.Parse("inch^2");
			Assert.AreEqual(new Unit(Prefix.None,
				new KeyValuePair<string, int>("inch", 2)), Unit);
		}

		[TestMethod]
		public void Test_15_da()
		{
			Unit Unit = Unit.Parse("dal");
			Assert.AreEqual(new Unit(Prefix.Deka,
				new KeyValuePair<string, int>("l", 1)), Unit);
		}
	}
}