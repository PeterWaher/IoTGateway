using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.ExceptionServices;
using Waher.Content.Xml;
using Waher.Runtime.Console;

namespace Waher.Content.Test
{
	[TestClass]
	public class DurationTests
	{
		[DataTestMethod]
		[DataRow("2023-01-01T00:00:00", "2024-01-01T00:00:00", "P1Y")]
		[DataRow("2023-01-01T00:00:00", "2023-02-01T00:00:00", "P1M")]
		[DataRow("2023-01-01T00:00:00", "2023-01-02T00:00:00", "P1D")]
		[DataRow("2023-01-01T00:00:00", "2023-01-01T01:00:00", "PT1H")]
		[DataRow("2023-01-01T00:00:00", "2023-01-01T00:01:00", "PT1M")]
		[DataRow("2023-01-01T00:00:00", "2023-01-01T00:00:01", "PT1S")]
		[DataRow("2023-01-01T00:00:00", "2023-01-01T00:00:00.250", "PT0.25S")]
		public void Test_01_Components(string TP1, string TP2, string Expected)
		{
			Test(TP1, TP2, Expected);
		}

		[DataTestMethod]
		[DataRow("2023-12-01T00:00:00", "2025-01-01T00:00:00", "P1Y1M")]
		[DataRow("2023-01-31T00:00:00", "2023-03-30T00:00:00", "P58D")]
		[DataRow("2023-01-01T20:00:00", "2023-01-03T10:00:00", "P1DT14H")]
		[DataRow("2023-01-01T10:50:00", "2023-01-01T15:45:00", "PT4H55M")]
		[DataRow("2023-01-01T00:30:50", "2023-01-01T00:35:45", "PT4M55S")]
		[DataRow("2023-01-01T00:00:30.500", "2023-01-01T00:00:35.100", "PT4.600S")]
		public void Test_02_Overflow(string TP1, string TP2, string Expected)
		{
			Test(TP1, TP2, Expected);
		}

		[DataTestMethod]
		[DataRow("2024-01-01T00:00:00", "2023-01-01T00:00:00", "-P1Y")]
		[DataRow("2023-02-01T00:00:00", "2023-01-01T00:00:00", "-P1M")]
		[DataRow("2023-01-02T00:00:00", "2023-01-01T00:00:00", "-P1D")]
		[DataRow("2023-01-01T01:00:00", "2023-01-01T00:00:00", "-PT1H")]
		[DataRow("2023-01-01T00:01:00", "2023-01-01T00:00:00", "-PT1M")]
		[DataRow("2023-01-01T00:00:01", "2023-01-01T00:00:00", "-PT1S")]
		[DataRow("2023-01-01T00:00:00.250", "2023-01-01T00:00:00", "-PT0.25S")]
		public void Test_03_Negative_Components(string TP1, string TP2, string Expected)
		{
			Test(TP1, TP2, Expected);
		}

		[DataTestMethod]
		[DataRow("2025-01-01T00:00:00", "2023-12-01T00:00:00", "-P1Y1M")]
		[DataRow("2023-03-30T00:00:00", "2023-01-31T00:00:00", "-P58D")]
		[DataRow("2023-01-03T10:00:00", "2023-01-01T20:00:00", "-P1DT14H")]
		[DataRow("2023-01-01T15:45:00", "2023-01-01T10:50:00", "-PT4H55M")]
		[DataRow("2023-01-01T00:35:45", "2023-01-01T00:30:50", "-PT4M55S")]
		[DataRow("2023-01-01T00:00:35.100", "2023-01-01T00:00:30.500", "-PT4.600S")]
		public void Test_04_NegativeOverflow(string TP1, string TP2, string Expected)
		{
			Test(TP1, TP2, Expected);
		}

		[DataTestMethod]
		[DataRow("2094-09-12T11:30:12.372", "2094-11-07T23:42:58.571", "P1M26DT12H12M46.199S")]
		[DataRow("2094-11-07T23:42:58.571", "2094-09-12T11:30:12.372", "-P1M25DT12H12M46.199S")]
		[DataRow("1910-10-25T15:09:55.645", "1950-07-08T08:12:49.095", "P39Y8M12DT17H2M53.45S")]
		[DataRow("1950-07-08T08:12:49.095", "1910-10-25T15:09:55.645", "-P39Y8M13DT16H62M53.45S")]
		[DataRow("1921-08-18T13:30:10.533", "2070-08-11T19:00:21.164", "P148Y11M24DT5H30M10.631S")]
		[DataRow("1921-08-18", "2070-08-11", "P148Y11M24D")]
		[DataRow("2071-12-29T13:52:24.420", "2040-01-17T13:27:06.052", "-P31Y346DT24H25M18.368S")]
		[DataRow("2040-01-17T13:27:06.052", "2071-12-29T13:52:24.420", "P31Y346DT25M18.368S")]
		[DataRow("2040-01-17", "2071-12-29", "P31Y346D")]
		[DataRow("2092-08-29T15:15:17.037", "2037-05-03T20:58:44.669", "-P55Y117DT18H16M32.368S")]
		[DataRow("2027-04-30T16:46:55.549", "2028-01-01T02:49:19.514", "P245DT10H2M23.965S")]
		[DataRow("1933-01-29T10:32:54.592", "2001-02-01T03:51:34.757", "P68Y2DT17H18M40.165S")]
		public void Test_05_SpecialCases(string TP1, string TP2, string Expected)
		{
			Test(TP1, TP2, Expected);
		}

		[TestMethod]
		public void Test_06_Random()
		{
			Random Rnd = new();
			int i;

			for (i = 0; i < 10000000; i++)
			{
				DateTime TP1 = NextDateTime(Rnd);
				DateTime TP2 = NextDateTime(Rnd);
				Duration Diff = Duration.GetDurationBetween(TP1, TP2);

				try
				{
					this.Test_02_Overflow(XML.Encode(TP1), XML.Encode(TP2), Diff.ToString());
				}
				catch (Exception ex)
				{
					ConsoleOut.WriteLine(i.ToString());
					ConsoleOut.WriteLine(XML.Encode(TP1));
					ConsoleOut.WriteLine(XML.Encode(TP2));
					ConsoleOut.WriteLine(Diff.ToString());

					ExceptionDispatchInfo.Capture(ex).Throw();
				}
			}
		}

		private static DateTime NextDateTime(Random Rnd)
		{
			int Year = 1900 + Rnd.Next(0, 200);
			int Month = Rnd.Next(1, 13);
			int Day = Rnd.Next(1, DateTime.DaysInMonth(Year, Month) + 1);
			int Hour = Rnd.Next(0, 24);
			int Minute = Rnd.Next(0, 60);
			int Second = Rnd.Next(0, 60);
			int Millisecond = Rnd.Next(0, 1000);

			return new DateTime(Year, Month, Day, Hour, Minute, Second, Millisecond);
		}

		private static void Test(string TP1, string TP2, string Expected)
		{
			DateTime ParsedTP1 = XML.ParseDateTime(TP1);
			DateTime ParsedTP2 = XML.ParseDateTime(TP2);
			Duration ParsedExpected = Duration.Parse(Expected);
			Duration Estimate = Duration.GetDurationBetween(ParsedTP1, ParsedTP2);
			Assert.AreEqual(ParsedExpected, Estimate);

			DateTime Check = ParsedTP1 + Estimate;
			TimeSpan TS = ParsedTP2.Subtract(Check);
			Assert.IsTrue(TS.TotalMilliseconds < 1);
			// Round-off errors may result in a 1 ms diff.
		}

		[DataTestMethod]
		[DataRow("PT100M", "PT1H", 1)]
		[DataRow("P1Y", "PT1H", 1)]
		[DataRow("P1M", "PT1H", 1)]
		[DataRow("P1D", "PT1H", 1)]
		[DataRow("PT1H", "PT1H", 0)]
		[DataRow("PT1M", "PT1H", -1)]
		[DataRow("PT1S", "PT1H", -1)]
		public void Test_07_CompareTo(string s1, string s2, int Sign)
		{
			Duration D1 = Duration.Parse(s1);
			Duration D2 = Duration.Parse(s2);

			Assert.AreEqual(Sign, D1.CompareTo(D2));
		}
	}
}
