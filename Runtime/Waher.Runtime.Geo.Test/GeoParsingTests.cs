using System.Text.RegularExpressions;
using Waher.Layout.Layout2D.Functions;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Graphs;

namespace Waher.Runtime.Geo.Test
{
	[TestClass]
	public sealed class GeoParsingTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Expression).Assembly,
				typeof(Graph).Assembly,
				typeof(Legend).Assembly,
				typeof(GeoPosition).Assembly);
		}

		[DataTestMethod]
		[DataRow("10.2,20.3,100.1", 10.20, 20.30, 100.1)]
		[DataRow("-10.2,20.3,100.1", -10.20, 20.30, 100.1)]
		[DataRow("10.2,-20.3,100.1", 10.20, -20.30, 100.1)]
		[DataRow("10.2,20.3,-100.1", 10.20, 20.30, -100.1)]
		[DataRow("-10.2,-20.3,-100.1", -10.20, -20.30, -100.1)]
		[DataRow("10.2,20.3", 10.20, 20.30, double.NaN)]
		[DataRow("-10.2,20.3", -10.20, 20.30, double.NaN)]
		[DataRow("10.2,-20.3", 10.20, -20.30, double.NaN)]
		[DataRow("10.2,20.3", 10.20, 20.30, double.NaN)]
		[DataRow("-10.2,-20.3", -10.20, -20.30, double.NaN)]
		[DataRow("10,20,100", 10.0, 20.0, 100.0)]
		[DataRow("-10,20,100", -10.0, 20.0, 100.0)]
		[DataRow("10,-20,100", 10.0, -20.0, 100.0)]
		[DataRow("10,20,-100", 10.0, 20.0, -100.0)]
		[DataRow("-10,-20,-100", -10.0, -20.0, -100.0)]
		[DataRow("10,20", 10.0, 20.0, double.NaN)]
		[DataRow("-10,20", -10.0, 20.0, double.NaN)]
		[DataRow("10,-20", 10.0, -20.0, double.NaN)]
		[DataRow("10,20", 10.0, 20.0, double.NaN)]
		[DataRow("-10,-20", -10.0, -20.0, double.NaN)]
		public void Test_01_XML_RegEx(string Value, double Latitude, double Longitude, double Altitude)
		{
			Match M = GeoPosition.XmlGeoPositionPattern.Match(Value);

			Assert.IsTrue(M.Success);
			Assert.AreEqual(0, M.Index);
			Assert.AreEqual(Value.Length, M.Length);

			Assert.IsTrue(Expression.TryParse(M.Groups["Lat"].Value, out double Lat));
			Assert.AreEqual(Latitude, Lat);

			Assert.IsTrue(Expression.TryParse(M.Groups["Lon"].Value, out double Lon));
			Assert.AreEqual(Longitude, Lon);

			if (double.IsNaN(Altitude))
				Assert.IsTrue(string.IsNullOrEmpty(M.Groups["Alt"].Value));
			else
			{
				Assert.IsTrue(Expression.TryParse(M.Groups["Alt"].Value, out double Alt));
				Assert.AreEqual(Altitude, Alt);
			}

			Assert.IsTrue(GeoPosition.TryParseXml(Value, out GeoPosition Position));
			Assert.AreEqual(Value, Position.XmlValue);
		}

		[DataTestMethod]
		[DataRow("10.2,20.3,100.1", 10.2, 20.3, 100.1)]
		[DataRow("-10.2,20.3,100.1", -10.2, 20.3, 100.1)]
		[DataRow("10.2,-20.3,100.1", 10.2, -20.3, 100.1)]
		[DataRow("10.2,20.3,-100.1", 10.2, 20.3, -100.1)]
		[DataRow("-10.2,-20.3,-100.1", -10.2, -20.3, -100.1)]
		[DataRow("10.2,20.3", 10.2, 20.3, double.NaN)]
		[DataRow("-10.2,20.3", -10.2, 20.3, double.NaN)]
		[DataRow("10.2,-20.3", 10.2, -20.3, double.NaN)]
		[DataRow("10.2,20.3", 10.2, 20.3, double.NaN)]
		[DataRow("-10.2,-20.3", -10.2, -20.3, double.NaN)]
		[DataRow("10,20,100", 10.0, 20.0, 100.0)]
		[DataRow("-10,20,100", -10.0, 20.0, 100.0)]
		[DataRow("10,-20,100", 10.0, -20.0, 100.0)]
		[DataRow("10,20,-100", 10.0, 20.0, -100.0)]
		[DataRow("-10,-20,-100", -10.0, -20.0, -100.0)]
		[DataRow("10,20", 10.0, 20.0, double.NaN)]
		[DataRow("-10,20", -10.0, 20.0, double.NaN)]
		[DataRow("10,-20", 10.0, -20.0, double.NaN)]
		[DataRow("10,20", 10.0, 20.0, double.NaN)]
		[DataRow("-10,-20", -10.0, -20.0, double.NaN)]
		public void Test_02_XML_TryParse(string Value, double Latitude, double Longitude, double Altitude)
		{
			Assert.IsTrue(GeoPosition.TryParseXml(Value, out GeoPosition Pos));

			Assert.AreEqual(Latitude, Pos.Latitude);
			Assert.AreEqual(Longitude, Pos.Longitude);

			if (double.IsNaN(Altitude))
				Assert.IsFalse(Pos.Altitude.HasValue);
			else
			{
				Assert.IsTrue(Pos.Altitude.HasValue);
				Assert.AreEqual(Altitude, Pos.Altitude.Value);
			}

			Assert.AreEqual(Value, Pos.XmlValue);
		}

		[DataTestMethod]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E 100.1")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" E 100.1")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" W 100.1")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E -100.1")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" W -100.1")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" E")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" W")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" W")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E 100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" E 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" W 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E -100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" W -100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E 100.1 ft")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" E 100.1 ft")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" W 100.1 ft")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E -100.1 ft")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" W -100.1 ft")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E, Altitude: 100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" E, Altitude: 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" W, Altitude: 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E, Altitude: -100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" W, Altitude: -100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E Altitude: 100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" E Altitude: 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" W Altitude: 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E Altitude: -100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" W Altitude: -100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E Altitude 100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" E Altitude 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" W Altitude 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E Altitude -100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" W Altitude -100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E Alt: 100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" E Alt: 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" W Alt: 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E Alt: -100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" W Alt: -100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E, 100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" E, 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" W, 100.1 m")]
		[DataRow("10° 12' 18.5\" N 20° 18' 24.5\" E, -100.1 m")]
		[DataRow("10° 12' 18.5\" S 20° 18' 24.5\" W, -100.1 m")]
		[DataRow("10° 12' 18\" N 20° 18' 24\" E 100.1")]
		[DataRow("10° 12' 18\" S 20° 18' 24\" E 100.1")]
		[DataRow("10° 12' 18\" N 20° 18' 24\" W 100.1")]
		[DataRow("10° 12' 18\" N 20° 18' 24\" E -100.1")]
		[DataRow("10° 12' 18\" S 20° 18' 24\" W -100.1")]
		[DataRow("10° 12' 18\" N 20° 18' 24\" E")]
		[DataRow("10° 12' 18\" S 20° 18' 24\" E")]
		[DataRow("10° 12' 18\" N 20° 18' 24\" W")]
		[DataRow("10° 12' 18\" S 20° 18' 24\" W")]
		[DataRow("10° 12' N 20° 18' E 100.1")]
		[DataRow("10° 12' S 20° 18' E 100.1")]
		[DataRow("10° 12' N 20° 18' W 100.1")]
		[DataRow("10° 12' N 20° 18' E -100.1")]
		[DataRow("10° 12' S 20° 18' W -100.1")]
		[DataRow("10° 12' N 20° 18' E")]
		[DataRow("10° 12' S 20° 18' E")]
		[DataRow("10° 12' N 20° 18' W")]
		[DataRow("10° 12' S 20° 18' W")]
		[DataRow("10° N 20° E 100")]
		[DataRow("10° S 20° E 100")]
		[DataRow("10° N 20° W 100")]
		[DataRow("10° N 20° E -100")]
		[DataRow("10° S 20° W -100")]
		[DataRow("10° N 20° E")]
		[DataRow("10° S 20° E")]
		[DataRow("10° N 20° W")]
		[DataRow("10° N 20° E")]
		[DataRow("10° S 20° W")]
		public void Test_03_GPS_RegEx(string Value)
		{
			Match M = GeoPosition.GpsGeoPositionPattern.Match(Value);

			Assert.IsTrue(M.Success);
			Assert.AreEqual(0, M.Index);
			Assert.AreEqual(Value.Length, M.Length);
		}

		[DataTestMethod]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E 100.1", 10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" E 100.1", -10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" W 100.1", 10.20525, -20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E -100.1", 10.20525, 20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" W -100.1", -10.20525, -20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E", 10.20525, 20.31025, double.NaN, true)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" E", -10.20525, 20.31025, double.NaN, true)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" W", 10.20525, -20.31025, double.NaN, true)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" W", -10.20525, -20.31025, double.NaN, true)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E 100.1 m", 10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" E 100.1 m", -10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" W 100.1 m", 10.20525, -20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E -100.1 m", 10.20525, 20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" W -100.1 m", -10.20525, -20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E 100.1 ft", 10.20525, 20.31025, 100.1 * 0.3048, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" E 100.1 ft", -10.20525, 20.31025, 100.1 * 0.3048, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" W 100.1 ft", 10.20525, -20.31025, 100.1 * 0.3048, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E -100.1 ft", 10.20525, 20.31025, -100.1 * 0.3048, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" W -100.1 ft", -10.20525, -20.31025, -100.1 * 0.3048, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E, Altitude: 100.1 m", 10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" E, Altitude: 100.1 m", -10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" W, Altitude: 100.1 m", 10.20525, -20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E, Altitude: -100.1 m", 10.20525, 20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" W, Altitude: -100.1 m", -10.20525, -20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E Altitude: 100.1 m", 10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" E Altitude: 100.1 m", -10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" W Altitude: 100.1 m", 10.20525, -20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E Altitude: -100.1 m", 10.20525, 20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" W Altitude: -100.1 m", -10.20525, -20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E Altitude 100.1 m", 10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" E Altitude 100.1 m", -10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" W Altitude 100.1 m", 10.20525, -20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E Altitude -100.1 m", 10.20525, 20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" W Altitude -100.1 m", -10.20525, -20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E Alt: 100.1 m", 10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" E Alt: 100.1 m", -10.20525, 20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" W Alt: 100.1 m", 10.20525, -20.31025, 100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E Alt: -100.1 m", 10.20525, 20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" W Alt: -100.1 m", -10.20525, -20.31025, -100.1, false)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E, 100.1 m", 10.20525, 20.31025, 100.1, true)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" E, 100.1 m", -10.20525, 20.31025, 100.1, true)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" W, 100.1 m", 10.20525, -20.31025, 100.1, true)]
		[DataRow("10° 12' 18.9\" N 20° 18' 36.9\" E, -100.1 m", 10.20525, 20.31025, -100.1, true)]
		[DataRow("10° 12' 18.9\" S 20° 18' 36.9\" W, -100.1 m", -10.20525, -20.31025, -100.1, true)]
		[DataRow("10° 12' 18\" N 20° 18' 36\" E 100.1", 10.205, 20.31, 100.1, false)]
		[DataRow("10° 12' 18\" S 20° 18' 36\" E 100.1", -10.205, 20.31, 100.1, false)]
		[DataRow("10° 12' 18\" N 20° 18' 36\" W 100.1", 10.205, -20.31, 100.1, false)]
		[DataRow("10° 12' 18\" N 20° 18' 36\" E -100.1", 10.205, 20.31, -100.1, false)]
		[DataRow("10° 12' 18\" S 20° 18' 36\" W -100.1", -10.205, -20.31, -100.1, false)]
		[DataRow("10° 12' 18\" N 20° 18' 36\" E", 10.205, 20.31, double.NaN, true)]
		[DataRow("10° 12' 18\" S 20° 18' 36\" E", -10.205, 20.31, double.NaN, true)]
		[DataRow("10° 12' 18\" N 20° 18' 36\" W", 10.205, -20.31, double.NaN, true)]
		[DataRow("10° 12' 18\" S 20° 18' 36\" W", -10.205, -20.31, double.NaN, true)]
		[DataRow("10° 12' N 20° 18' E 100.1", 10.2, 20.3, 100.1, false)]
		[DataRow("10° 12' S 20° 18' E 100.1", -10.2, 20.3, 100.1, false)]
		[DataRow("10° 12' N 20° 18' W 100.1", 10.2, -20.3, 100.1, false)]
		[DataRow("10° 12' N 20° 18' E -100.1", 10.2, 20.3, -100.1, false)]
		[DataRow("10° 12' S 20° 18' W -100.1", -10.2, -20.3, -100.1, false)]
		[DataRow("10° 12' N 20° 18' E", 10.2, 20.3, double.NaN, true)]
		[DataRow("10° 12' S 20° 18' E", -10.2, 20.3, double.NaN, true)]
		[DataRow("10° 12' N 20° 18' W", 10.2, -20.3, double.NaN, true)]
		[DataRow("10° 12' S 20° 18' W", -10.2, -20.3, double.NaN, true)]
		[DataRow("10° N 20° E 100", 10, 20, 100, false)]
		[DataRow("10° S 20° E 100", -10, 20, 100, false)]
		[DataRow("10° N 20° W 100", 10, -20, 100, false)]
		[DataRow("10° N 20° E -100", 10, 20, -100, false)]
		[DataRow("10° S 20° W -100", -10, -20, -100, false)]
		[DataRow("10° N 20° E", 10, 20, double.NaN, true)]
		[DataRow("10° S 20° E", -10, 20, double.NaN, true)]
		[DataRow("10° N 20° W", 10, -20, double.NaN, true)]
		[DataRow("10° N 20° E", 10, 20, double.NaN, true)]
		[DataRow("10° S 20° W", -10, -20, double.NaN, true)]
		public void Test_04_GPS_TryParse(string Value, double Latitude, double Longitude, double Altitude, bool CheckToString)
		{
			Assert.IsTrue(GeoPosition.TryParseGps(Value, out GeoPosition Pos));

			Assert.AreEqual(Latitude, Pos.Latitude);
			Assert.AreEqual(Longitude, Pos.Longitude);

			if (double.IsNaN(Altitude))
				Assert.IsFalse(Pos.Altitude.HasValue);
			else
			{
				Assert.IsTrue(Pos.Altitude.HasValue);
				Assert.AreEqual(Altitude, Pos.Altitude.Value);
			}

			if (CheckToString)
				Assert.AreEqual(Value, Pos.HumanReadable);
		}

		[DataTestMethod]
		[DataRow("10° 20' N 30° 40' W")]
		public void Test_05_GPS_RoundingErrors(string Value)
		{
			Assert.IsTrue(GeoPosition.TryParseGps(Value, out GeoPosition Pos));
			Assert.AreEqual(Value, Pos.HumanReadable);
		}

	}
}
