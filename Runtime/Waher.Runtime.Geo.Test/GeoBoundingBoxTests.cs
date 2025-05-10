namespace Waher.Runtime.Geo.Test
{
	[TestClass]
	public sealed class GeoBoundingBoxTests
	{
		[TestMethod]
		public void Test_01_GeoBoundingBox_Constructor()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);

			GeoBoundingBox BoundingBox = new(Min, Max);

			Assert.AreEqual(Min, BoundingBox.Min);
			Assert.AreEqual(Max, BoundingBox.Max);
			Assert.IsTrue(BoundingBox.IncludeMin);
			Assert.IsTrue(BoundingBox.IncludeMax);
		}

		[TestMethod]
		public void Test_02_GeoBoundingBox_Constructor_WithIncludeFlags()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);

			GeoBoundingBox BoundingBox = new(Min, Max, false, true);

			Assert.AreEqual(Min, BoundingBox.Min);
			Assert.AreEqual(Max, BoundingBox.Max);
			Assert.IsFalse(BoundingBox.IncludeMin);
			Assert.IsTrue(BoundingBox.IncludeMax);
		}

		[TestMethod]
		public void Test_03_GeoBoundingBox_Equals()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);

			GeoBoundingBox Box1 = new(Min, Max);
			GeoBoundingBox Box2 = new(Min, Max);
			GeoBoundingBox Box3 = new(new(15.0, 25.0), Max);

			Assert.IsTrue(Box1.Equals(Box2));
			Assert.IsFalse(Box1.Equals(Box3));
			Assert.IsFalse(Box1.Equals(null));
		}

		[TestMethod]
		public void Test_04_GeoBoundingBox_OperatorEquality()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);

			GeoBoundingBox Box1 = new(Min, Max);
			GeoBoundingBox Box2 = new(Min, Max);
			GeoBoundingBox Box3 = new(new(15.0, 25.0), Max);
			GeoBoundingBox? NullBox1 = null;
			GeoBoundingBox? NullBox2 = null;

			Assert.IsTrue(Box1 == Box2);
			Assert.IsFalse(Box1 == Box3);
			Assert.IsFalse(Box1 == NullBox1);
			Assert.IsFalse(NullBox2 == Box3);
			Assert.IsTrue(NullBox1 == NullBox2);
		}

		[TestMethod]
		public void Test_05_GeoBoundingBox_OperatorInequality()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);

			GeoBoundingBox Box1 = new(Min, Max);
			GeoBoundingBox Box2 = new(Min, Max);
			GeoBoundingBox Box3 = new(new(15.0, 25.0), Max);
			GeoBoundingBox? NullBox1 = null;
			GeoBoundingBox? NullBox2 = null;

			Assert.IsFalse(Box1 != Box2);
			Assert.IsTrue(Box1 != Box3);
			Assert.IsTrue(Box1 != NullBox1);
			Assert.IsTrue(NullBox2 != Box3);
			Assert.IsFalse(NullBox1 != NullBox2);
		}

		[TestMethod]
		public void Test_06_GeoBoundingBox_Contains_PointInside()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);
			GeoBoundingBox BoundingBox = new(Min, Max);

			GeoPosition InsidePoint = new(20.0, 30.0);

			Assert.IsTrue(BoundingBox.Contains(InsidePoint));
		}

		[TestMethod]
		public void Test_07_GeoBoundingBox_Contains_PointOutside()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);
			GeoBoundingBox BoundingBox = new(Min, Max);

			GeoPosition OutsidePoint = new(5.0, 15.0);

			Assert.IsFalse(BoundingBox.Contains(OutsidePoint));
		}

		[TestMethod]
		public void Test_08_GeoBoundingBox_Contains_PointOnBoundary()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);

			GeoBoundingBox BoundingBox = new(Min, Max);

			GeoPosition BoundaryPoint = new(10.0, 20.0);

			Assert.IsTrue(BoundingBox.Contains(BoundaryPoint));
		}

		[TestMethod]
		public void Test_09_GeoBoundingBox_ToString()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);

			GeoBoundingBox BoundingBox = new(Min, Max);

			string Expected = $"{Min} - {Max}";
			Assert.AreEqual(Expected, BoundingBox.ToString());
		}

		[TestMethod]
		public void Test_10_GeoBoundingBox_GetHashCode()
		{
			GeoPosition Min = new(10.0, 20.0);
			GeoPosition Max = new(30.0, 40.0);

			GeoBoundingBox Box1 = new(Min, Max);
			GeoBoundingBox Box2 = new(Min, Max);

			Assert.AreEqual(Box1.GetHashCode(), Box2.GetHashCode());
		}
	}
}
