using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Operators.Arithmetics;
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

			Assert.IsTrue(Unit.TryConvert(2, Ampere, 0, MilliAmpere, out double x, out byte NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Ampere, 0, KiloAmpere, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MilliAmpere, 0, KiloAmpere, out x, out NrDec));
			Assert.AreEqual(0.000002, x);
			Assert.AreEqual(6, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MilliAmpere, 0, Ampere, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloAmpere, 0, Ampere, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloAmpere, 0, MilliAmpere, out x, out NrDec));
			Assert.AreEqual(2000000, x);
			Assert.AreEqual(0, NrDec);
		}

		[TestMethod]
		public void Test_02_Length()
		{
			Unit Meter = Unit.Parse("m");
			Unit MilliMeter = Unit.Parse("mm");
			Unit KiloMeter = Unit.Parse("km");

			Assert.IsTrue(Unit.TryConvert(2, Meter, 0, MilliMeter, out double x, out byte NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Meter, 0, KiloMeter, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MilliMeter, 0, KiloMeter, out x, out NrDec));
			Assert.AreEqual(0.000002, x);
			Assert.AreEqual(6, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MilliMeter, 0, Meter, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloMeter, 0, Meter, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloMeter, 0, MilliMeter, out x, out NrDec));
			Assert.AreEqual(2000000, x);
			Assert.AreEqual(0, NrDec);
		}

		[TestMethod]
		public void Test_03_Mass()
		{
			Unit KiloGram = Unit.Parse("kg");
			Unit Gram = Unit.Parse("g");
			Unit Tonne = Unit.Parse("t");

			Assert.IsTrue(Unit.TryConvert(2, KiloGram, 0, Gram, out double x, out byte NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloGram, 0, Tonne, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Gram, 0, Tonne, out x, out NrDec));
			Assert.AreEqual(0.000002, x);
			Assert.AreEqual(6, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Gram, 0, KiloGram, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Tonne, 0, KiloGram, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Tonne, 0, Gram, out x, out NrDec));
			Assert.AreEqual(2000000, x);
			Assert.AreEqual(0, NrDec);
		}

		[TestMethod]
		public void Test_04_Temperature()
		{
			Unit Celcius = Unit.Parse("°C");
			Unit Farenheit = Unit.Parse("°F");
			Unit Kelvin = Unit.Parse("K");

			Assert.IsTrue(Unit.TryConvert(2, Celcius, 1, Farenheit, out double x, out byte NrDec));
			Assert.AreEqual(35.6, x);
			Assert.AreEqual(1, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Celcius, 1, Kelvin, out x, out NrDec));
			Assert.AreEqual(275.15, x);
			Assert.AreEqual(1, NrDec);
		}

		[TestMethod]
		public void Test_05_Time()
		{
			Unit Hour = Unit.Parse("h");
			Unit Minute = Unit.Parse("min");
			Unit Second = Unit.Parse("s");

			Assert.IsTrue(Unit.TryConvert(2, Hour, 0, Minute, out double x, out byte NrDec));
			Assert.AreEqual(120, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Hour, 0, Second, out x, out NrDec));
			Assert.AreEqual(7200, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(12, Minute, 0, Hour, out x, out NrDec));
			Assert.AreEqual(0.2, x);
			Assert.AreEqual(2, NrDec);
		}

		[TestMethod]
		public void Test_06_Energy()
		{
			Unit Joule = Unit.Parse("J");
			Unit KiloWattHour = Unit.Parse("kWh");
			Unit GigaJoule = Unit.Parse("GJ");
			Unit MegaBtu = Unit.Parse("MBTU");
			Unit GramMetersSquaredPerSecondsSquared = Unit.Parse("g*m^2/s^2");

			Assert.IsTrue(Unit.TryConvert(2, Joule, 0, KiloWattHour, out double x, out byte NrDec));
			Assert.AreEqual(0.002 / 3600, x);
			Assert.AreEqual(7, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloWattHour, 0, Joule, out x, out NrDec));
			Assert.AreEqual(2000 * 3600, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, GigaJoule, 0, MegaBtu, out x, out NrDec));
			Assert.AreEqual(2 / 1.055055853, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MegaBtu, 0, GigaJoule, out x, out NrDec));
			Assert.AreEqual(2 * 1.055055853, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Joule, 0, GramMetersSquaredPerSecondsSquared, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);
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

			Assert.IsTrue(Unit.TryConvert(2, StatuteMilesPerHour, 0, MilesPerHours, out double x, out byte NrDec));
			Assert.AreEqual(2, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MilesPerHours, 0, StatuteMilesPerHour, out x, out NrDec));
			Assert.AreEqual(2, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MetersPerSecond, 0, KiloMetersPerHour, out x, out NrDec));
			Assert.AreEqual(7.2, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MetersPerSecond, 0, KiloMetersPerHour2, out x, out NrDec));
			Assert.AreEqual(7.2, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloMetersPerHour, 0, MetersPerSecond, out x, out NrDec));
			Assert.AreEqual(2 / 3.6, x);
			Assert.AreEqual(1, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloMetersPerHour2, 0, MetersPerSecond, out x, out NrDec));
			Assert.AreEqual(2 / 3.6, x);
			Assert.AreEqual(1, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Knots, 0, MetersPerSecond, out x, out NrDec));
			Assert.AreEqual(2 * 0.514444, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MetersPerSecond, 0, Knots, out x, out NrDec));
			Assert.AreEqual(2 / 0.514444, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Knots, 0, KiloMetersPerHour, out x, out NrDec));
			Assert.AreEqual(3.6 * 2 * 0.514444, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloMetersPerHour, 0, Knots, out x, out NrDec));
			Assert.AreEqual(1.079914539882972, x);
			Assert.AreEqual(0, NrDec);
		}

		[TestMethod]
		public void Test_08_Capacitance()
		{
			Unit Farad = Unit.Parse("F");
			Unit MilliFarad = Unit.Parse("mF");
			Unit SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared = Unit.Parse("s^4*A²/(g*m²)");

			Assert.IsTrue(Unit.TryConvert(2, Farad, 0, MilliFarad, out double x, out byte NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Farad, 0, SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MilliFarad, 0, SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared, out x, out NrDec));
			Assert.AreEqual(0.000002, x);
			Assert.AreEqual(6, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MilliFarad, 0, Farad, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared, 0, Farad, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, SecondsToThePowerOfFourAmperesSquaredPerGramsAndMeterSquared, 0, MilliFarad, out x, out NrDec));
			Assert.AreEqual(2000000, x);
			Assert.AreEqual(0, NrDec);
		}

		[TestMethod]
		public void Test_09_Charge()
		{
			Unit MilliCoulomb = Unit.Parse("mC");
			Unit SecondAmpere = Unit.Parse("s*A");

			Assert.IsTrue(Unit.TryConvert(2, MilliCoulomb, 0, SecondAmpere, out double x, out byte NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, SecondAmpere, 0, MilliCoulomb, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);
		}

		[TestMethod]
		public void Test_10_Force()
		{
			Unit Newton = Unit.Parse("N");
			Unit KiloNewton = Unit.Parse("kN");
			Unit GramMeterPerSecondSquared = Unit.Parse("g*m/s^2");

			Assert.IsTrue(Unit.TryConvert(2, Newton, 0, KiloNewton, out double x, out byte NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloNewton, 0, Newton, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Newton, 0, GramMeterPerSecondSquared, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, GramMeterPerSecondSquared, 0, Newton, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloNewton, 0, GramMeterPerSecondSquared, out x, out NrDec));
			Assert.AreEqual(2000000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, GramMeterPerSecondSquared, 0, KiloNewton, out x, out NrDec));
			Assert.AreEqual(0.000002, x);
			Assert.AreEqual(6, NrDec);
		}

		[TestMethod]
		public void Test_11_Frequency()
		{
			Unit Herz = Unit.Parse("Hz");
			Unit CyclesPerSecond = Unit.Parse("cps");
			Unit RevolutionsPerMinute = Unit.Parse("rpm");

			Assert.IsTrue(Unit.TryConvert(2, Herz, 0, CyclesPerSecond, out double x, out byte NrDec));
			Assert.AreEqual(2, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Herz, 0, RevolutionsPerMinute, out x, out NrDec));
			Assert.AreEqual(120, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, CyclesPerSecond, 0, Herz, out x, out NrDec));
			Assert.AreEqual(2, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, RevolutionsPerMinute, 0, Herz, out x, out NrDec));
			Assert.AreEqual(2.0 / 60, x);
			Assert.AreEqual(2, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, CyclesPerSecond, 0, RevolutionsPerMinute, out x, out NrDec));
			Assert.AreEqual(120, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, RevolutionsPerMinute, 0, CyclesPerSecond, out x, out NrDec));
			Assert.AreEqual(2.0 / 60, x);
			Assert.AreEqual(2, NrDec);
		}

		[TestMethod]
		public void Test_12_Power()
		{
			Unit Watt = Unit.Parse("W");
			Unit KiloWatt = Unit.Parse("kW");
			Unit GramMetersSquaredPerSecondsCube = Unit.Parse("g*m^2/s^3");

			Assert.IsTrue(Unit.TryConvert(2, Watt, 0, KiloWatt, out double x, out byte NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloWatt, 0, Watt, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Watt, 0, GramMetersSquaredPerSecondsCube, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, GramMetersSquaredPerSecondsCube, 0, Watt, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloWatt, 0, GramMetersSquaredPerSecondsCube, out x, out NrDec));
			Assert.AreEqual(2000000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, GramMetersSquaredPerSecondsCube, 0, KiloWatt, out x, out NrDec));
			Assert.AreEqual(0.000002, x);
			Assert.AreEqual(6, NrDec);
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

			Assert.IsTrue(Unit.TryConvert(2, Pascal, 0, KiloPascal, out double x, out byte NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloPascal, 0, Pascal, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Bar, 0, Millibar, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Millibar, 0, Bar, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Pascal, 0, Millibar, out x, out NrDec));
			Assert.AreEqual(0.02, x);
			Assert.AreEqual(2, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Millibar, 0, Pascal, out x, out NrDec));
			Assert.AreEqual(200, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Atm, 0, Millibar, out x, out NrDec));
			Assert.AreEqual(2 * 1013.25, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Millibar, 0, Atm, out x, out NrDec));
			Assert.AreEqual(0.0019738465334320256, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Psi, 0, Pascal, out x, out NrDec));
			Assert.AreEqual(13789.514000000001, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Atm, 0, Psi, out x, out NrDec));
			Assert.AreEqual(29.39189880078442, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Pascal, 0, GramsPerMetersAndSecondsSquared, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);
		}

		[TestMethod]
		public void Test_14_Resistance()
		{
			Unit Ohm = Unit.Parse("Ω");
			Unit KiloOhm = Unit.Parse("kOhm");
			Unit MeterSquaredGramsPerSecondCubeAndAmpereSquared = Unit.Parse("m^2*g/(s^3*A^2)");

			Assert.IsTrue(Unit.TryConvert(2, Ohm, 0, KiloOhm, out double x, out byte NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloOhm, 0, Ohm, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Ohm, 0, MeterSquaredGramsPerSecondCubeAndAmpereSquared, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MeterSquaredGramsPerSecondCubeAndAmpereSquared, 0, Ohm, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloOhm, 0, MeterSquaredGramsPerSecondCubeAndAmpereSquared, out x, out NrDec));
			Assert.AreEqual(2000000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MeterSquaredGramsPerSecondCubeAndAmpereSquared, 0, KiloOhm, out x, out NrDec));
			Assert.AreEqual(0.000002, x);
			Assert.AreEqual(6, NrDec);
		}

		[TestMethod]
		public void Test_15_Voltage()
		{
			Unit Volt = Unit.Parse("V");
			Unit KiloVolt = Unit.Parse("kV");
			Unit MeterSquaredGramsPerSecondCubeAndAmpere = Unit.Parse("m^2*g/(s^3*A)");

			Assert.IsTrue(Unit.TryConvert(2, Volt, 0, KiloVolt, out double x, out byte NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloVolt, 0, Volt, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Volt, 0, MeterSquaredGramsPerSecondCubeAndAmpere, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MeterSquaredGramsPerSecondCubeAndAmpere, 0, Volt, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, KiloVolt, 0, MeterSquaredGramsPerSecondCubeAndAmpere, out x, out NrDec));
			Assert.AreEqual(2000000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, MeterSquaredGramsPerSecondCubeAndAmpere, 0, KiloVolt, out x, out NrDec));
			Assert.AreEqual(0.000002, x);
			Assert.AreEqual(6, NrDec);
		}

		[TestMethod]
		public void Test_16_Volume()
		{
			Unit Liter = Unit.Parse("l");
			Unit DeciLiter = Unit.Parse("dl");
			Unit CubicMeter = Unit.Parse("m^3");

			Assert.IsTrue(Unit.TryConvert(2, Liter, 0, CubicMeter, out double x, out byte NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, CubicMeter, 0, Liter, out x, out NrDec));
			Assert.AreEqual(2000, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Liter, 0, DeciLiter, out x, out NrDec));
			Assert.AreEqual(20, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, DeciLiter, 0, Liter, out x, out NrDec));
			Assert.AreEqual(0.2, x);
			Assert.AreEqual(1, NrDec);
		}

		[TestMethod]
		public void Test_17_Dimensionless()
		{
			Unit One = Unit.Parse("1");
			Unit Pieces = Unit.Parse("pcs");
			Unit Dozen1 = Unit.Parse("dz");
			Unit Dozen2 = Unit.Parse("dozen");
			Unit Gross1 = Unit.Parse("gr");
			Unit Gross2 = Unit.Parse("gross");
			Unit Radians = Unit.Parse("rad");
			Unit Degrees1 = Unit.Parse("deg");
			Unit Degrees2 = Unit.Parse("°");
			Unit Percent = Unit.Parse("%");
			Unit Permille1 = Unit.Parse("‰");
			Unit Permille2 = Unit.Parse("%0");
			Unit Perdixmille1 = Unit.Parse("‱");
			Unit Perdixmille2 = Unit.Parse("%00");

			Assert.IsTrue(Unit.TryConvert(2, Pieces, 0, One, out double x, out byte NrDec));
			Assert.AreEqual(2, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Dozen1, 0, One, out x, out NrDec));
			Assert.AreEqual(24, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Dozen2, 0, One, out x, out NrDec));
			Assert.AreEqual(24, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Gross1, 0, One, out x, out NrDec));
			Assert.AreEqual(288, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Gross2, 0, One, out x, out NrDec));
			Assert.AreEqual(288, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Radians, 0, One, out x, out NrDec));
			Assert.AreEqual(2, x);
			Assert.AreEqual(0, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Degrees1, 0, One, out x, out NrDec));
			Assert.AreEqual(2 * DegToRad.Scale, x);
			Assert.AreEqual(2, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Degrees2, 0, One, out x, out NrDec));
			Assert.AreEqual(2 * DegToRad.Scale, x);
			Assert.AreEqual(2, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Percent, 0, One, out x, out NrDec));
			Assert.AreEqual(0.02, x);
			Assert.AreEqual(2, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Permille1, 0, One, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Permille2, 0, One, out x, out NrDec));
			Assert.AreEqual(0.002, x);
			Assert.AreEqual(3, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Perdixmille1, 0, One, out x, out NrDec));
			Assert.AreEqual(0.0002, x);
			Assert.AreEqual(4, NrDec);

			Assert.IsTrue(Unit.TryConvert(2, Perdixmille2, 0, One, out x, out NrDec));
			Assert.AreEqual(0.0002, x);
			Assert.AreEqual(4, NrDec);
		}

	}
}