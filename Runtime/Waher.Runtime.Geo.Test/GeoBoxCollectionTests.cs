using System.Security.Authentication;

namespace Waher.Runtime.Geo.Test
{
	[TestClass]
	public sealed class GeoBoxCollectionTests
	{
		[TestMethod]
		public void Test_01_Add_SingleBox()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));

			collection.Add(box);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(1, collection.Count);
			Assert.IsTrue(collection.Contains(box));
		}

		[TestMethod]
		public void Test_02_Add_MultipleBoxes()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box1 = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));
			var box2 = new GeoBoundingBox(new GeoPosition(-10, -20), new GeoPosition(0, 0));
			var box3 = new GeoBoundingBox(new GeoPosition(50, 60), new GeoPosition(70, 80));

			collection.Add(box1);
			collection.Add(box2);
			collection.Add(box3);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(3, collection.Count);
			Assert.IsTrue(collection.Contains(box1));
			Assert.IsTrue(collection.Contains(box2));
			Assert.IsTrue(collection.Contains(box3));
		}

		[TestMethod]
		public void Test_03_Add_DuplicateBox()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));

			collection.Add(box);
			// Adding the same box again should not throw or increase count
			collection.Add(box);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(1, collection.Count);
		}

		[TestMethod]
		public void Test_04_Add_DuplicateBoxIdDifferentInstance()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box1 = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));
			var box2 = new GeoBoundingBox(box1.BoxId, new GeoPosition(11, 20), new GeoPosition(30, 40));

			collection.Add(box1);

			Console.WriteLine(collection.Export());

			Assert.ThrowsException<ArgumentException>(() => collection.Add(box2));
		}

		[TestMethod]
		public void Test_05_Remove_ExistingBox()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));

			collection.Add(box);
			bool removed = collection.Remove(box);

			Console.WriteLine(collection.Export());

			Assert.IsTrue(removed);
			Assert.AreEqual(0, collection.Count);
			Assert.IsFalse(collection.Contains(box));
		}

		[TestMethod]
		public void Test_06_Remove_NonExistentBox()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));

			bool removed = collection.Remove(box);

			Console.WriteLine(collection.Export());

			Assert.IsFalse(removed);
			Assert.AreEqual(0, collection.Count);
		}

		[TestMethod]
		public void Test_07_Clear_Collection()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>
			{
				new(new GeoPosition(10, 20), new GeoPosition(30, 40)),
				new(new GeoPosition(-10, -20), new GeoPosition(0, 0))
			};

			collection.Clear();

			Console.WriteLine(collection.Export());

			Assert.AreEqual(0, collection.Count);
		}

		[TestMethod]
		public void Test_08_CopyTo_Array()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box1 = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));
			var box2 = new GeoBoundingBox(new GeoPosition(-10, -20), new GeoPosition(0, 0));
			collection.Add(box1);
			collection.Add(box2);

			var array = new GeoBoundingBox[5];
			collection.CopyTo(array, 1);

			Console.WriteLine(collection.Export());

			Assert.IsNull(array[0]);
			Assert.IsNotNull(array[1]);
			Assert.IsNotNull(array[2]);
			Assert.IsNull(array[3]);
			Assert.IsNull(array[4]);
			Assert.IsTrue(array[1].Equals(box1) || array[1].Equals(box2));
			Assert.IsTrue(array[2].Equals(box1) || array[2].Equals(box2));
		}

		[TestMethod]
		public void Test_09_ToArray()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box1 = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));
			var box2 = new GeoBoundingBox(new GeoPosition(-10, -20), new GeoPosition(0, 0));
			collection.Add(box1);
			collection.Add(box2);

			var array = collection.ToArray();

			Console.WriteLine(collection.Export());

			Assert.AreEqual(2, array.Length);
			Assert.IsTrue(Array.Exists(array, b => b.Equals(box1)));
			Assert.IsTrue(Array.Exists(array, b => b.Equals(box2)));
		}

		[TestMethod]
		public void Test_10_Enumeration()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box1 = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));
			var box2 = new GeoBoundingBox(new GeoPosition(-10, -20), new GeoPosition(0, 0));
			collection.Add(box1);
			collection.Add(box2);

			var found = new List<GeoBoundingBox>();
			foreach (var box in collection)
				found.Add(box);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(2, found.Count);
			Assert.IsTrue(found.Exists(b => b.Equals(box1)));
			Assert.IsTrue(found.Exists(b => b.Equals(box2)));
		}

		[TestMethod]
		public void Test_11_Find_PositionInsideBox()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));
			collection.Add(box);

			var pos = new GeoPosition(20, 30);
			var results = collection.Find(pos);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(1, results.Length);
			Assert.IsTrue(results[0].Equals(box));
		}

		[TestMethod]
		public void Test_12_Find_PositionOutsideBox()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40));
			collection.Add(box);

			var pos = new GeoPosition(50, 60);
			var results = collection.Find(pos);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(0, results.Length);
		}

		[TestMethod]
		public void Test_13_Find_PositionOnBoundary_Inclusive()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40), true, true);
			collection.Add(box);

			var pos = new GeoPosition(10, 20);
			var results = collection.Find(pos);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(1, results.Length);
			Assert.IsTrue(results[0].Equals(box));
		}

		[TestMethod]
		public void Test_14_Find_PositionOnBoundary_Exclusive()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box = new GeoBoundingBox(new GeoPosition(10, 20), new GeoPosition(30, 40), false, false);
			collection.Add(box);

			var pos = new GeoPosition(10, 20);
			var results = collection.Find(pos);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(0, results.Length);
		}

		[TestMethod]
		public void Test_15_Find_MultipleBoxesContainingPosition()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var box1 = new GeoBoundingBox(new GeoPosition(0, 0), new GeoPosition(50, 50));
			var box2 = new GeoBoundingBox(new GeoPosition(10, 10), new GeoPosition(30, 30));
			var box3 = new GeoBoundingBox(new GeoPosition(20, 20), new GeoPosition(40, 40));
			collection.Add(box1);
			collection.Add(box2);
			collection.Add(box3);

			var pos = new GeoPosition(25, 25);
			var results = collection.Find(pos);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(3, results.Length);
			Assert.IsTrue(Array.Exists(results, b => b.Equals(box1)));
			Assert.IsTrue(Array.Exists(results, b => b.Equals(box2)));
			Assert.IsTrue(Array.Exists(results, b => b.Equals(box3)));
		}

		[TestMethod]
		public void Test_16_Find_EmptyCollection()
		{
			var collection = new GeoBoxCollection<GeoBoundingBox>();
			var pos = new GeoPosition(10, 20);

			var results = collection.Find(pos);

			Console.WriteLine(collection.Export());

			Assert.AreEqual(0, results.Length);
		}
		[TestMethod]
		public void Test_17_Add_Boxes_Exceeding_MaxCellCount_CreatesSubGrid()
		{
			// Use a small grid and cell size to force sub-grid creation quickly

			int GridSize = 2;
			int MaxCellCount = 2;
			GeoBoxCollection<GeoBoundingBox> Collection = new(GridSize, MaxCellCount);

			// All boxes overlap the same cell, so adding more than maxCellCount should trigger a sub-grid

			GeoPosition BaseMin = new(10, 10);
			GeoPosition BaseMax = new(20, 20);
			List<GeoBoundingBox> Boxes = [];

			for (int i = 0; i < MaxCellCount + 2; i++)
			{
				// Slightly offset each box to ensure they all overlap the same cell

				GeoPosition Min = new(10 + i * 0.01, 10 + i * 0.01);
				GeoPosition Max = new(20 - i * 0.01, 20 - i * 0.01);
				GeoBoundingBox Box = new(Min, Max);

				Boxes.Add(Box);
				Collection.Add(Box);
			}

			Console.WriteLine(Collection.Export());

			// All boxes should be present in the collection

			Assert.AreEqual(MaxCellCount + 2, Collection.Count);

			foreach (var box in Boxes)
				Assert.IsTrue(Collection.Contains(box));

			// All boxes should be found by a point in the overlapping region
			var testPoint = new GeoPosition(15, 15);
			var found = Collection.Find(testPoint);
			Assert.AreEqual(MaxCellCount + 2, found.Length);
			foreach (var box in Boxes)
				Assert.IsTrue(Array.Exists(found, b => b.Equals(box)));
		}

		[TestMethod]
		public void Test_18_Add_Boxes_Exceeding_MaxCellCount_MultipleCells()
		{
			int gridSize = 2;
			int maxCellCount = 2;
			var collection = new GeoBoxCollection<GeoBoundingBox>(gridSize, maxCellCount);

			// Add boxes that each fit into a different cell, then add more to one cell to force a split
			var boxes = new List<GeoBoundingBox>
			{
				new(new GeoPosition(0, 0), new GeoPosition(10, 10)),   // Cell 1
				new(new GeoPosition(0, 10), new GeoPosition(10, 20)),  // Cell 2
				new(new GeoPosition(10, 0), new GeoPosition(20, 10)),  // Cell 3
				new(new GeoPosition(10, 10), new GeoPosition(20, 20)), // Cell 4
			};

			foreach (var box in boxes)
				collection.Add(box);

			// Now add more boxes to the first cell to exceed maxCellCount
			for (int i = 0; i < maxCellCount + 1; i++)
			{
				var min = new GeoPosition(1 + i * 0.1, 1 + i * 0.1);
				var max = new GeoPosition(2 + i * 0.1, 2 + i * 0.1);
				var box = new GeoBoundingBox(min, max);
				collection.Add(box);
				boxes.Add(box);
			}

			Console.WriteLine(collection.Export());

			Assert.AreEqual(4 + maxCellCount + 1, collection.Count);

			// All boxes should be found by a point in their respective regions
			Assert.IsTrue(collection.Find(new GeoPosition(5, 5)).Length == 1); // Cell 1
			Assert.IsTrue(collection.Find(new GeoPosition(5, 15)).Length == 1); // Cell 2
			Assert.IsTrue(collection.Find(new GeoPosition(15, 5)).Length == 1); // Cell 3
			Assert.IsTrue(collection.Find(new GeoPosition(15, 15)).Length == 1); // Cell 4
		}

		[DataTestMethod]
		[DataRow(100, 2, 2, false)]
		[DataRow(100, 4, 4, false)]
		[DataRow(100, 8, 8, false)]
		[DataRow(100, 12, 12, false)]
		[DataRow(100, 16, 16, false)]
		[DataRow(500, 2, 2, false)]
		[DataRow(500, 4, 4, false)]
		[DataRow(500, 8, 8, false)]
		[DataRow(500, 12, 12, false)]
		[DataRow(500, 16, 16, false)]
		[DataRow(1000, 2, 2, false)]
		[DataRow(1000, 4, 4, false)]
		[DataRow(1000, 8, 8, false)]
		[DataRow(1000, 12, 12, false)]
		[DataRow(1000, 16, 16, false)]
		[DataRow(5000, 2, 2, false)]
		[DataRow(5000, 4, 4, false)]
		[DataRow(5000, 8, 8, false)]
		[DataRow(5000, 12, 12, false)]
		[DataRow(5000, 16, 16, false)]
		public void Test_19_IntegrityCheck(int N, int GridSize, int MaxCellCount, bool Export)
		{
			GeoBoxCollection<GeoBoundingBox> Collection = new(GridSize, MaxCellCount);
			GeoBoundingBox[] Boxes = new GeoBoundingBox[N];
			GeoBoundingBox Box;
			int i, j;

			for (i = 0; i < N; i++)
			{
				Box = GeoPositionCollectionBenchmarkingTests.RandomBoundingBox(10, 10);
				Boxes[i] = Box;

				Collection.Add(Box);

				if (Export)
				{
					Console.Out.WriteLine();
					Console.Out.WriteLine();
					Console.Out.WriteLine("Adding: " + Box.BoxId + " (" + Box.ToString() + ")");
					Console.Out.WriteLine(new string('=', 80));
					Console.Out.WriteLine();
					Console.Out.WriteLine();
					Console.Out.WriteLine(Collection.Export());
				}
			}

			Assert.AreEqual(N, Collection.Count);

			for (i = 0; i < N; i++)
				Assert.IsTrue(Collection.Contains(Boxes[i]));

			for (i = 0; i < N; i += 2)
			{
				Assert.IsTrue(Collection.Remove(Boxes[i]));

				if (Export)
				{
					Console.Out.WriteLine();
					Console.Out.WriteLine();
					Console.Out.WriteLine("Removing: " + Boxes[i].BoxId + " (" + Boxes[i].ToString() + ")");
					Console.Out.WriteLine(new string('=', 80));
					Console.Out.WriteLine();
					Console.Out.WriteLine();
					Console.Out.WriteLine(Collection.Export());
				}
			}

			Assert.AreEqual(N / 2, Collection.Count);

			for (i = 0; i < N; i++)
			{
				if ((i & 1) == 0)
					Assert.IsFalse(Collection.Contains(Boxes[i]), "Box not expected in collection after removal.");
				else
					Assert.IsTrue(Collection.Contains(Boxes[i]), "Box expected in collection.");
			}

			for (i = 0; i < N; i++)
			{
				GeoSpatialObjectReference Pos = GeoPositionCollectionBenchmarkingTests.RandomPosition();
				GeoBoundingBox[] Result = Collection.Find(Pos.Location);
				SortedDictionary<string, bool> Found = [];

				foreach (GeoBoundingBox Box2 in Result)
				{
					Assert.IsTrue(Box2.Contains(Pos.Location));
					Assert.IsFalse(Found.ContainsKey(Box2.BoxId), "Box found twice.");
					Found[Box2.BoxId] = true;
				}

				for (j = 0; j < N; j++)
				{
					GeoBoundingBox Box2 = Boxes[j];

					bool ShouldContain = ((j & 1) != 0) && Box2.Contains(Pos.Location);
					bool Contains = Array.Exists(Result, p => p.BoxId == Box2.BoxId);

					if (ShouldContain ^ Contains)
					{
						if (!Export)
						{
							Console.Out.WriteLine("Position: " + Pos.Location.ToString());
							Console.Out.WriteLine();
							Console.Out.WriteLine(Collection.Export());
						}

						Assert.Fail((ShouldContain ?
							"Box that should be found, was not found: " :
							"Box that should not be found, was found: ") + Box2.BoxId);
					}
				}
			}

			// TODO: Test altitudes as well
		}

	}
}
