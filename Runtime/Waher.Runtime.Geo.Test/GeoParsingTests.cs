using System.Text.RegularExpressions;
using Waher.Runtime.Inventory;
using Waher.Script;

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
				typeof(GeoPosition).Assembly);
		}

		[DataTestMethod]
		[DataRow("10.20,20.30,100.1", 10.20, 20.30, 100.1)]
		[DataRow("-10.20,20.30,100.1", -10.20, 20.30, 100.1)]
		[DataRow("10.20,-20.30,100.1", 10.20, -20.30, 100.1)]
		[DataRow("10.20,20.30,-100.1", 10.20, 20.30, -100.1)]
		[DataRow("-10.20,-20.30,-100.1", -10.20, -20.30, -100.1)]
		[DataRow("10.20,20.30", 10.20, 20.30, double.NaN)]
		[DataRow("-10.20,20.30", -10.20, 20.30, double.NaN)]
		[DataRow("10.20,-20.30", 10.20, -20.30, double.NaN)]
		[DataRow("10.20,20.30", 10.20, 20.30, double.NaN)]
		[DataRow("-10.20,-20.30", -10.20, -20.30, double.NaN)]
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
		}

		[DataTestMethod]
		[DataRow("10.20,20.30,100.1", 10.20, 20.30, 100.1)]
		[DataRow("-10.20,20.30,100.1", -10.20, 20.30, 100.1)]
		[DataRow("10.20,-20.30,100.1", 10.20, -20.30, 100.1)]
		[DataRow("10.20,20.30,-100.1", 10.20, 20.30, -100.1)]
		[DataRow("-10.20,-20.30,-100.1", -10.20, -20.30, -100.1)]
		[DataRow("10.20,20.30", 10.20, 20.30, double.NaN)]
		[DataRow("-10.20,20.30", -10.20, 20.30, double.NaN)]
		[DataRow("10.20,-20.30", 10.20, -20.30, double.NaN)]
		[DataRow("10.20,20.30", 10.20, 20.30, double.NaN)]
		[DataRow("-10.20,-20.30", -10.20, -20.30, double.NaN)]
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
		}
	}
}
