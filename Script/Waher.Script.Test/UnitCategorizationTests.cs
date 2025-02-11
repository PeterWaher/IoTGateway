using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Units;

namespace Waher.Script.Test
{
	[TestClass]
	public class UnitCategorizationTests
	{
		[DataTestMethod]
		[DataRow("A")]
		[DataRow("mA")]
		[DataRow("kA")]
		public void Test_01_Current(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Current", Category.Name);
		}

		[DataTestMethod]
		[DataRow("m")]
		[DataRow("mm")]
		[DataRow("km")]
		public void Test_02_Length(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Length", Category.Name);
		}

		[DataTestMethod]
		[DataRow("kg")]
		[DataRow("g")]
		[DataRow("t")]
		public void Test_03_Mass(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Mass", Category.Name);
		}

		[DataTestMethod]
		[DataRow("°C")]
		[DataRow("°F")]
		[DataRow("K")]
		public void Test_04_Temperature(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Temperature", Category.Name);
		}

		[DataTestMethod]
		[DataRow("h")]
		[DataRow("min")]
		[DataRow("s")]
		public void Test_05_Time(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Time", Category.Name);
		}

		[DataTestMethod]
		[DataRow("J")]
		[DataRow("kWh")]
		[DataRow("GJ")]
		[DataRow("MBTU")]
		[DataRow("g*m^2/s^2")]
		public void Test_06_Energy(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Energy", Category.Name);
		}

		[DataTestMethod]
		[DataRow("SM/h")]
		[DataRow("mph")]
		[DataRow("m/s")]
		[DataRow("km/h")]
		[DataRow("kph")]
		[DataRow("kt")]
		public void Test_07_Speed(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Speed", Category.Name);
		}

		[DataTestMethod]
		[DataRow("F")]
		[DataRow("mF")]
		[DataRow("s^4*A²/(g*m²)")]
		public void Test_08_Capacitance(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Capacitance", Category.Name);
		}

		[DataTestMethod]
		[DataRow("mC")]
		[DataRow("s*A")]
		public void Test_09_Charge(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Charge", Category.Name);
		}

		[DataTestMethod]
		[DataRow("N")]
		[DataRow("kN")]
		[DataRow("g*m/s^2")]
		public void Test_10_Force(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Force", Category.Name);
		}

		[DataTestMethod]
		[DataRow("Hz")]
		[DataRow("cps")]
		[DataRow("rpm")]
		public void Test_11_Frequency(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Frequency", Category.Name);
		}

		[DataTestMethod]
		[DataRow("W")]
		[DataRow("kW")]
		[DataRow("g*m^2/s^3")]
		public void Test_12_Power(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Power", Category.Name);
		}

		[DataTestMethod]
		[DataRow("Pa")]
		[DataRow("kPa")]
		[DataRow("bar")]
		[DataRow("mbar")]
		[DataRow("psi")]
		[DataRow("atm")]
		[DataRow("g/(m*s^2)")]
		public void Test_13_Pressure(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Pressure", Category.Name);
		}

		[DataTestMethod]
		[DataRow("Ω")]
		[DataRow("kOhm")]
		[DataRow("m^2*g/(s^3*A^2)")]
		public void Test_14_Resistance(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Resistance", Category.Name);
		}

		[DataTestMethod]
		[DataRow("V")]
		[DataRow("kV")]
		[DataRow("m^2*g/(s^3*A)")]
		public void Test_15_Voltage(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Voltage", Category.Name);
		}

		[DataTestMethod]
		[DataRow("l")]
		[DataRow("dl")]
		[DataRow("m^3")]
		public void Test_16_Volume(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Volume", Category.Name);
		}

		[DataTestMethod]
		[DataRow("1")]
		[DataRow("pcs")]
		[DataRow("dz")]
		[DataRow("dozen")]
		[DataRow("gr")]
		[DataRow("gross")]
		[DataRow("%")]
		[DataRow("‰")]
		[DataRow("‱")]
		[DataRow("%0")]
		[DataRow("%00")]
		[DataRow("°")]
		[DataRow("rad")]
		[DataRow("deg")]
		public void Test_17_Dimensionless(string UnitString)
		{
			Unit Parsed = Unit.Parse(UnitString);
			Assert.IsTrue(Unit.TryGetCategory(Parsed, out IUnitCategory Category));
			Assert.AreEqual("Dimensionless", Category.Name);
		}
	}
}