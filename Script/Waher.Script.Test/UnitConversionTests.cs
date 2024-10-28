using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Units;

namespace Waher.Script.Test
{
	[TestClass]
	public class UnitConversionTests
	{
		[TestMethod]
		public void Test_01_Current()
		{
			Unit Ampere = Unit.Parse("A");
			Unit MilliAmpere = Unit.Parse("mA");
			Unit KiloAmpere = Unit.Parse("kA");

			Assert.IsTrue(Unit.TryConvert(2, Ampere, MilliAmpere, out double x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Ampere, KiloAmpere, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, MilliAmpere, KiloAmpere, out x));
			Assert.AreEqual(0.000002, x);

			Assert.IsTrue(Unit.TryConvert(2, MilliAmpere, Ampere, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloAmpere, Ampere, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloAmpere, MilliAmpere, out x));
			Assert.AreEqual(2000000, x);
		}

		[TestMethod]
		public void Test_02_Length()
		{
			Unit Meter = Unit.Parse("m");
			Unit MilliMeter = Unit.Parse("mm");
			Unit KiloMeter = Unit.Parse("km");

			Assert.IsTrue(Unit.TryConvert(2, Meter, MilliMeter, out double x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Meter, KiloMeter, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, MilliMeter, KiloMeter, out x));
			Assert.AreEqual(0.000002, x);

			Assert.IsTrue(Unit.TryConvert(2, MilliMeter, Meter, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloMeter, Meter, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloMeter, MilliMeter, out x));
			Assert.AreEqual(2000000, x);
		}

		[TestMethod]
		public void Test_03_Mass()
		{
			Unit KiloGram = Unit.Parse("kg");
			Unit Gram = Unit.Parse("g");
			Unit Tonne = Unit.Parse("t");

			Assert.IsTrue(Unit.TryConvert(2, KiloGram, Gram, out double x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloGram, Tonne, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, Gram, Tonne, out x));
			Assert.AreEqual(0.000002, x);

			Assert.IsTrue(Unit.TryConvert(2, Gram, KiloGram, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, Tonne, KiloGram, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Tonne, Gram, out x));
			Assert.AreEqual(2000000, x);
		}

		[TestMethod]
		public void Test_04_Temperature()
		{
			Unit Celcius = Unit.Parse("°C");
			Unit Farenheit = Unit.Parse("°F");
			Unit Kelvin = Unit.Parse("K");

			Assert.IsTrue(Unit.TryConvert(2, Celcius, Farenheit, out double x));
			Assert.AreEqual(35.6, x);

			Assert.IsTrue(Unit.TryConvert(2, Celcius, Kelvin, out x));
			Assert.AreEqual(275.15, x);
		}

		[TestMethod]
		public void Test_05_Time()
		{
			Unit Hour = Unit.Parse("h");
			Unit Minute = Unit.Parse("min");
			Unit Second = Unit.Parse("s");

			Assert.IsTrue(Unit.TryConvert(2, Hour, Minute, out double x));
			Assert.AreEqual(120, x);

			Assert.IsTrue(Unit.TryConvert(2, Hour, Second, out x));
			Assert.AreEqual(7200, x);
		}

		[TestMethod]
		public void Test_06_Energy()
		{
			Unit Joule = Unit.Parse("J");
			Unit KiloWattHour = Unit.Parse("kWh");
			Unit GigaJoule = Unit.Parse("GJ");
			Unit MegaBtu = Unit.Parse("MBTU");
			Unit GramMetersSquaredPerSecondsSquared = Unit.Parse("g*m^2/s^2");

			Assert.IsTrue(Unit.TryConvert(2, Joule, KiloWattHour, out double x));
			Assert.AreEqual(0.002 / 3600, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloWattHour, Joule, out x));
			Assert.AreEqual(2000 * 3600, x);

			Assert.IsTrue(Unit.TryConvert(2, GigaJoule, MegaBtu, out x));
			Assert.AreEqual(2 / 1.055055853, x);

			Assert.IsTrue(Unit.TryConvert(2, MegaBtu, GigaJoule, out x));
			Assert.AreEqual(2 * 1.055055853, x);

			Assert.IsTrue(Unit.TryConvert(2, Joule, GramMetersSquaredPerSecondsSquared, out x));
			Assert.AreEqual(2000, x);
		}

		[TestMethod]
		public void Test_07_Speed()
		{
			Unit StatuteMilesPerHour = Unit.Parse("SM/h");
			Unit MilesPerHours = Unit.Parse("mph");
			Unit MetersPerSecond = Unit.Parse("m/s");
			Unit KiloMetersPerHour = Unit.Parse("km/h");
			Unit KiloMetersPerHour2 = Unit.Parse("kph");
			Unit Knots = Unit.Parse("kt");

			Assert.IsTrue(Unit.TryConvert(2, StatuteMilesPerHour, MilesPerHours, out double x));
			Assert.AreEqual(2, x);

			Assert.IsTrue(Unit.TryConvert(2, MilesPerHours, StatuteMilesPerHour, out x));
			Assert.AreEqual(2, x);

			Assert.IsTrue(Unit.TryConvert(2, MetersPerSecond, KiloMetersPerHour, out x));
			Assert.AreEqual(7.2, x);

			Assert.IsTrue(Unit.TryConvert(2, MetersPerSecond, KiloMetersPerHour2, out x));
			Assert.AreEqual(7.2, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloMetersPerHour, MetersPerSecond, out x));
			Assert.AreEqual(2 / 3.6, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloMetersPerHour2, MetersPerSecond, out x));
			Assert.AreEqual(2 / 3.6, x);

			Assert.IsTrue(Unit.TryConvert(2, Knots, MetersPerSecond, out x));
			Assert.AreEqual(2 * 0.514444, x);

			Assert.IsTrue(Unit.TryConvert(2, MetersPerSecond, Knots, out x));
			Assert.AreEqual(2 / 0.514444, x);

			Assert.IsTrue(Unit.TryConvert(2, Knots, KiloMetersPerHour, out x));
			Assert.AreEqual(3.6 * 2 * 0.514444, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloMetersPerHour, Knots, out x));
			Assert.AreEqual(1.079914539882972, x);
		}

		[TestMethod]
		public void Test_08_Capacitance()
		{
			Unit Farad = Unit.Parse("F");
			Unit MilliFarad = Unit.Parse("mF");
			Unit SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared = Unit.Parse("s^4*A²/(g*m²)");

			Assert.IsTrue(Unit.TryConvert(2, Farad, MilliFarad, out double x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Farad, SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, MilliFarad, SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared, out x));
			Assert.AreEqual(0.000002, x);

			Assert.IsTrue(Unit.TryConvert(2, MilliFarad, Farad, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared, Farad, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared, MilliFarad, out x));
			Assert.AreEqual(2000000, x);
		}

		[TestMethod]
		public void Test_09_Charge()
		{
			Unit MilliCoulomb = Unit.Parse("mC");
			Unit SecondAmpere = Unit.Parse("s*A");

			Assert.IsTrue(Unit.TryConvert(2, MilliCoulomb, SecondAmpere, out double x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, SecondAmpere, MilliCoulomb, out x));
			Assert.AreEqual(2000, x);
		}

		[TestMethod]
		public void Test_10_Force()
		{
			Unit Newton = Unit.Parse("N");
			Unit KiloNewton = Unit.Parse("kN");
			Unit GramMeterPerSecondSquared = Unit.Parse("g*m/s^2");

			Assert.IsTrue(Unit.TryConvert(2, Newton, KiloNewton, out double x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloNewton, Newton, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Newton, GramMeterPerSecondSquared, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, GramMeterPerSecondSquared, Newton, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloNewton, GramMeterPerSecondSquared, out x));
			Assert.AreEqual(2000000, x);

			Assert.IsTrue(Unit.TryConvert(2, GramMeterPerSecondSquared, KiloNewton, out x));
			Assert.AreEqual(0.000002, x);
		}

		[TestMethod]
		public void Test_11_Frequency()
		{
			Unit Herz = Unit.Parse("Hz");
			Unit CyclesPerSecond = Unit.Parse("cps");
			Unit RevolutionsPerMinute = Unit.Parse("rpm");

			Assert.IsTrue(Unit.TryConvert(2, Herz, CyclesPerSecond, out double x));
			Assert.AreEqual(2, x);

			Assert.IsTrue(Unit.TryConvert(2, Herz, RevolutionsPerMinute, out x));
			Assert.AreEqual(120, x);

			Assert.IsTrue(Unit.TryConvert(2, CyclesPerSecond, Herz, out x));
			Assert.AreEqual(2, x);

			Assert.IsTrue(Unit.TryConvert(2, RevolutionsPerMinute, Herz, out x));
			Assert.AreEqual(2.0 / 60, x);

			Assert.IsTrue(Unit.TryConvert(2, CyclesPerSecond, RevolutionsPerMinute, out x));
			Assert.AreEqual(120, x);

			Assert.IsTrue(Unit.TryConvert(2, RevolutionsPerMinute, CyclesPerSecond, out x));
			Assert.AreEqual(2.0 / 60, x);
		}

		[TestMethod]
		public void Test_12_Power()
		{
			Unit Watt = Unit.Parse("W");
			Unit KiloWatt = Unit.Parse("kW");
			Unit GramMetersSquaredPerSecondsCube = Unit.Parse("g*m^2/s^3");

			Assert.IsTrue(Unit.TryConvert(2, Watt, KiloWatt, out double x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloWatt, Watt, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Watt, GramMetersSquaredPerSecondsCube, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, GramMetersSquaredPerSecondsCube, Watt, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloWatt, GramMetersSquaredPerSecondsCube, out x));
			Assert.AreEqual(2000000, x);

			Assert.IsTrue(Unit.TryConvert(2, GramMetersSquaredPerSecondsCube, KiloWatt, out x));
			Assert.AreEqual(0.000002, x);
		}

		[TestMethod]
		public void Test_13_Pressure()
		{
			Unit Pascal = Unit.Parse("Pa");
			Unit KiloPascal = Unit.Parse("kPa");
			Unit Bar = Unit.Parse("bar");
			Unit Millibar = Unit.Parse("mbar");
			Unit Psi = Unit.Parse("psi");
			Unit Atm = Unit.Parse("atm");
			Unit GramsPerMetersAndSecondsSquared = Unit.Parse("g/(m*s^2)");

			Assert.IsTrue(Unit.TryConvert(2, Pascal, KiloPascal, out double x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloPascal, Pascal, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Bar, Millibar, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Millibar, Bar, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, Pascal, Millibar, out x));
			Assert.AreEqual(0.02, x);

			Assert.IsTrue(Unit.TryConvert(2, Millibar, Pascal, out x));
			Assert.AreEqual(200, x);

			Assert.IsTrue(Unit.TryConvert(2, Atm, Millibar, out x));
			Assert.AreEqual(2 * 1013.25, x);

			Assert.IsTrue(Unit.TryConvert(2, Millibar, Atm, out x));
			Assert.AreEqual(0.0019738465334320256, x);

			Assert.IsTrue(Unit.TryConvert(2, Psi, Pascal, out x));
			Assert.AreEqual(13789.514000000001, x);

			Assert.IsTrue(Unit.TryConvert(2, Atm, Psi, out x));
			Assert.AreEqual(29.39189880078442, x);
		}

		[TestMethod]
		public void Test_14_Resistance()
		{
			Unit Ohm = Unit.Parse("Ω");
			Unit KiloOhm = Unit.Parse("kOhm");
			Unit MeterSquaredGramsPerSecondCubeAndAmpereSquared = Unit.Parse("m^2*g/(s^3*A^2)");

			Assert.IsTrue(Unit.TryConvert(2, Ohm, KiloOhm, out double x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloOhm, Ohm, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Ohm, MeterSquaredGramsPerSecondCubeAndAmpereSquared, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, MeterSquaredGramsPerSecondCubeAndAmpereSquared, Ohm, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloOhm, MeterSquaredGramsPerSecondCubeAndAmpereSquared, out x));
			Assert.AreEqual(2000000, x);

			Assert.IsTrue(Unit.TryConvert(2, MeterSquaredGramsPerSecondCubeAndAmpereSquared, KiloOhm, out x));
			Assert.AreEqual(0.000002, x);
		}

		[TestMethod]
		public void Test_15_Voltage()
		{
			Unit Volt = Unit.Parse("V");
			Unit KiloVolt = Unit.Parse("kV");
			Unit MeterSquaredGramsPerSecondCubeAndAmpere = Unit.Parse("m^2*g/(s^3*A)");

			Assert.IsTrue(Unit.TryConvert(2, Volt, KiloVolt, out double x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloVolt, Volt, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Volt, MeterSquaredGramsPerSecondCubeAndAmpere, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, MeterSquaredGramsPerSecondCubeAndAmpere, Volt, out x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, KiloVolt, MeterSquaredGramsPerSecondCubeAndAmpere, out x));
			Assert.AreEqual(2000000, x);

			Assert.IsTrue(Unit.TryConvert(2, MeterSquaredGramsPerSecondCubeAndAmpere, KiloVolt, out x));
			Assert.AreEqual(0.000002, x);
		}

		[TestMethod]
		public void Test_16_Volume()
		{
			Unit Liter = Unit.Parse("l");
			Unit DeciLiter = Unit.Parse("dl");
			Unit CubicMeter = Unit.Parse("m^3");

			Assert.IsTrue(Unit.TryConvert(2, Liter, CubicMeter, out double x));
			Assert.AreEqual(0.002, x);

			Assert.IsTrue(Unit.TryConvert(2, CubicMeter, Liter, out x));
			Assert.AreEqual(2000, x);

			Assert.IsTrue(Unit.TryConvert(2, Liter, DeciLiter, out x));
			Assert.AreEqual(20, x);

			Assert.IsTrue(Unit.TryConvert(2, DeciLiter, Liter, out x));
			Assert.AreEqual(0.2, x);
		}

	}
}