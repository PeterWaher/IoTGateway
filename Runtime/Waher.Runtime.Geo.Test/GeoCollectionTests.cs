namespace Waher.Runtime.Geo.Test
{
	[TestClass]
	public sealed class GeoCollectionTests
	{
		[TestMethod]
		public void Test_01_Add_SingleObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			Collection.Add(Object);

			Assert.AreEqual(1, Collection.Count);

			List<IGeoSpatialObjectReference> Objects = [.. Collection];
			Assert.AreEqual(1, Objects.Count);
			Assert.AreEqual("37.7749,-122.4194", Objects[0].GeoId);
		}

		[TestMethod]
		public void Test_02_Add_MultipleObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);
			GeoSpatialObject Object3 = new("40.7128,-74.0060", true);

			Collection.Add(Object1);
			Collection.Add(Object2);
			Collection.Add(Object3);

			Assert.AreEqual(3, Collection.Count);

			List<IGeoSpatialObjectReference> Objects = [.. Collection];
			Assert.AreEqual(3, Objects.Count);
			Assert.IsTrue(Objects.Exists(o => o.GeoId == "37.7749,-122.4194"));
			Assert.IsTrue(Objects.Exists(o => o.GeoId == "34.0522,-118.2437"));
			Assert.IsTrue(Objects.Exists(o => o.GeoId == "40.7128,-74.0060"));
		}

		[TestMethod]
		public void Test_03_Add_DuplicateObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject DuplicateObject = new("37.7749,-122.4194", true);

			Collection.Add(Object1);
			Assert.ThrowsException<ArgumentException>(() => Collection.Add(DuplicateObject));
		}

		[TestMethod]
		public void Test_04_Add_ManyObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			int ObjectCount = 1000;

			for (int i = 0; i < ObjectCount; i++)
			{
				Collection.Add(new GeoSpatialObject(i.ToString(),
					new GeoPosition(37.7749 + i * 0.01, -122.4194 + i * 0.01)));
			}

			Assert.AreEqual(ObjectCount, Collection.Count);

			List<IGeoSpatialObjectReference> Objects = [.. Collection];
			Assert.AreEqual(ObjectCount, Objects.Count);

			for (int i = 0; i < ObjectCount; i++)
				Assert.IsTrue(Objects.Exists(o =>
				{
					return
						o.GeoId == i.ToString() &&
						o.Location.Latitude == 37.7749 + i * 0.01 &&
						o.Location.Longitude == -122.4194 + i * 0.01;
				}));
		}

		[TestMethod]
		public void Test_05_Add_BoundaryConditions()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];

			// Add objects with extreme GPS coordinates
			GeoSpatialObject ObjectMin = new("-90.0000,-180.0000", true);
			GeoSpatialObject ObjectMax = new("90.0000,180.0000", true);

			Collection.Add(ObjectMin);
			Collection.Add(ObjectMax);

			Assert.AreEqual(2, Collection.Count);

			List<IGeoSpatialObjectReference> Objects = [.. Collection];
			Assert.AreEqual(2, Objects.Count);
			Assert.IsTrue(Objects.Exists(o => o.GeoId == "-90.0000,-180.0000"));
			Assert.IsTrue(Objects.Exists(o => o.GeoId == "90.0000,180.0000"));
		}

		[TestMethod]
		public void Test_06_Add_EnumerateObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);
			GeoSpatialObject Object3 = new("40.7128,-74.0060", true);

			Collection.Add(Object1);
			Collection.Add(Object2);
			Collection.Add(Object3);

			List<string> GeoIds = [];
			foreach (IGeoSpatialObjectReference Obj in Collection)
			{
				GeoIds.Add(Obj.GeoId);
			}

			Assert.AreEqual(3, GeoIds.Count);
			Assert.IsTrue(GeoIds.Contains("37.7749,-122.4194"));
			Assert.IsTrue(GeoIds.Contains("34.0522,-118.2437"));
			Assert.IsTrue(GeoIds.Contains("40.7128,-74.0060"));
		}

		[TestMethod]
		public void Test_07_Clear_EmptyCollection()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];

			// Ensure the collection is initially empty
			Assert.AreEqual(0, Collection.Count);

			// Clear the collection
			Collection.Clear();

			// Verify the collection is still empty
			Assert.AreEqual(0, Collection.Count);

			// Verify enumeration returns no elements
			List<IGeoSpatialObjectReference> Objects = [.. Collection];
			Assert.AreEqual(0, Objects.Count);
		}

		[TestMethod]
		public void Test_08_Clear_SingleObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add a single object
			Collection.Add(Object);
			Assert.AreEqual(1, Collection.Count);

			// Clear the collection
			Collection.Clear();

			// Verify the collection is empty
			Assert.AreEqual(0, Collection.Count);

			// Verify enumeration returns no elements
			List<IGeoSpatialObjectReference> Objects = [.. Collection];
			Assert.AreEqual(0, Objects.Count);
		}

		[TestMethod]
		public void Test_09_Clear_MultipleObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);
			GeoSpatialObject Object3 = new("40.7128,-74.0060", true);

			// Add multiple objects
			Collection.Add(Object1);
			Collection.Add(Object2);
			Collection.Add(Object3);
			Assert.AreEqual(3, Collection.Count);

			// Clear the collection
			Collection.Clear();

			// Verify the collection is empty
			Assert.AreEqual(0, Collection.Count);

			// Verify enumeration returns no elements
			List<IGeoSpatialObjectReference> Objects = [.. Collection];
			Assert.AreEqual(0, Objects.Count);
		}

		[TestMethod]
		public void Test_10_Clear_ManyObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			int ObjectCount = 1000;

			// Add many objects
			for (int i = 0; i < ObjectCount; i++)
				Collection.Add(new GeoSpatialObject(new GeoPosition(37.7749 + i * 0.01, -122.4194 + i * 0.01)));

			Assert.AreEqual(ObjectCount, Collection.Count);

			// Clear the collection
			Collection.Clear();

			// Verify the collection is empty
			Assert.AreEqual(0, Collection.Count);

			// Verify enumeration returns no elements
			List<IGeoSpatialObjectReference> Objects = [.. Collection];
			Assert.AreEqual(0, Objects.Count);
		}

		[TestMethod]
		public void Test_11_Clear_ThenAddObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);

			// Add objects
			Collection.Add(Object1);
			Collection.Add(Object2);
			Assert.AreEqual(2, Collection.Count);

			// Clear the collection
			Collection.Clear();
			Assert.AreEqual(0, Collection.Count);

			// Add new objects after clearing
			GeoSpatialObject Object3 = new("40.7128,-74.0060", true);
			Collection.Add(Object3);
			Assert.AreEqual(1, Collection.Count);

			// Verify the new object is present
			List<IGeoSpatialObjectReference> Objects = [.. Collection];
			Assert.AreEqual(1, Objects.Count);
			Assert.AreEqual("40.7128,-74.0060", Objects[0].GeoId);
		}

		[TestMethod]
		public void Test_12_Contains_ObjectExists()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add the object to the collection
			Collection.Add(Object);

			// Verify that the object exists in the collection
			Assert.IsTrue(Collection.Contains(new GeoSpatialObjectReference(Object)));
		}

		[TestMethod]
		public void Test_13_Contains_ObjectDoesNotExist()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Do not add the object to the collection

			// Verify that the object does not exist in the collection
			Assert.IsFalse(Collection.Contains(new GeoSpatialObjectReference(Object)));
		}

		[TestMethod]
		public void Test_14_Contains_EmptyCollection()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Verify that the object does not exist in an empty collection
			Assert.IsFalse(Collection.Contains(new GeoSpatialObjectReference(Object)));
		}

		[TestMethod]
		public void Test_15_Contains_MultipleObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);
			GeoSpatialObject Object3 = new("40.7128,-74.0060", true);

			// Add multiple objects to the collection
			Collection.Add(Object1);
			Collection.Add(Object2);

			// Verify that the objects exist in the collection
			Assert.IsTrue(Collection.Contains(new GeoSpatialObjectReference(Object1)));
			Assert.IsTrue(Collection.Contains(new GeoSpatialObjectReference(Object2)));

			// Verify that an object not added does not exist
			Assert.IsFalse(Collection.Contains(new GeoSpatialObjectReference(Object3)));
		}

		[TestMethod]
		public void Test_16_Contains_DuplicateGeoIdDifferentObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("37.7749,-122.4194", true); // Same GeoId, different instance

			// Add the first object to the collection
			Collection.Add(Object1);

			// Verify that the collection contains the first object
			Assert.IsTrue(Collection.Contains(new GeoSpatialObjectReference(Object1)));

			// Verify that the collection does not consider the second object as contained
			Assert.IsFalse(Collection.Contains(new GeoSpatialObjectReference(Object2)));
		}

		[TestMethod]
		public void Test_17_Contains_AfterClear()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add the object to the collection
			Collection.Add(Object);

			// Clear the collection
			Collection.Clear();

			// Verify that the object no longer exists in the collection
			Assert.IsFalse(Collection.Contains(new GeoSpatialObjectReference(Object)));
		}

		[TestMethod]
		public void Test_18_Contains_AfterRemove()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add the object to the collection
			Collection.Add(Object);

			// Remove the object from the collection
			Collection.Remove(new GeoSpatialObjectReference(Object));

			// Verify that the object no longer exists in the collection
			Assert.IsFalse(Collection.Contains(new GeoSpatialObjectReference(Object)));
		}

		[TestMethod]
		public void Test_19_CopyTo_EmptyCollection()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObjectReference[] Destination = new GeoSpatialObjectReference[10];

			// Copy from an empty collection
			Collection.CopyTo(Destination, 0);

			// Verify that the destination array remains empty
			for (int i = 0; i < Destination.Length; i++)
			{
				Assert.IsNull(Destination[i]);
			}
		}

		[TestMethod]
		public void Test_20_CopyTo_SingleObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add a single object to the collection
			Collection.Add(Object);

			GeoSpatialObjectReference[] Destination = new GeoSpatialObjectReference[10];

			// Copy to the destination array
			Collection.CopyTo(Destination, 0);

			// Verify that the object was copied correctly
			Assert.AreEqual(1, Collection.Count);
			Assert.IsNotNull(Destination[0]);
			Assert.AreEqual("37.7749,-122.4194", Destination[0].GeoId);

			// Verify that the rest of the array remains empty
			for (int i = 1; i < Destination.Length; i++)
			{
				Assert.IsNull(Destination[i]);
			}
		}

		[TestMethod]
		public void Test_21_CopyTo_MultipleObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);
			GeoSpatialObject Object3 = new("40.7128,-74.0060", true);

			// Add multiple objects to the collection
			Collection.Add(Object1);
			Collection.Add(Object2);
			Collection.Add(Object3);

			GeoSpatialObjectReference[] Destination = new GeoSpatialObjectReference[10];

			// Copy to the destination array
			Collection.CopyTo(Destination, 0);

			// Verify that the objects were copied correctly
			Assert.AreEqual(3, Collection.Count);
			Assert.AreEqual("37.7749,-122.4194", Destination[0].GeoId);
			Assert.AreEqual("34.0522,-118.2437", Destination[1].GeoId);
			Assert.AreEqual("40.7128,-74.0060", Destination[2].GeoId);

			// Verify that the rest of the array remains empty
			for (int i = 3; i < Destination.Length; i++)
			{
				Assert.IsNull(Destination[i]);
			}
		}

		[TestMethod]
		public void Test_22_CopyTo_WithOffset()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);

			// Add objects to the collection
			Collection.Add(Object1);
			Collection.Add(Object2);

			GeoSpatialObjectReference[] Destination = new GeoSpatialObjectReference[10];

			// Copy to the destination array with an offset
			Collection.CopyTo(Destination, 5);

			// Verify that the objects were copied correctly
			Assert.AreEqual("37.7749,-122.4194", Destination[5].GeoId);
			Assert.AreEqual("34.0522,-118.2437", Destination[6].GeoId);

			// Verify that the rest of the array remains empty
			for (int i = 0; i < 5; i++)
			{
				Assert.IsNull(Destination[i]);
			}
			for (int i = 7; i < Destination.Length; i++)
			{
				Assert.IsNull(Destination[i]);
			}
		}

		[TestMethod]
		public void Test_23_CopyTo_NullDestination()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add an object to the collection
			Collection.Add(Object);

			// Attempt to copy to a null destination array
			Assert.ThrowsException<ArgumentNullException>(() => Collection.CopyTo(null, 0));
		}

		[TestMethod]
		public void Test_24_CopyTo_NegativeOffset()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add an object to the collection
			Collection.Add(Object);

			GeoSpatialObjectReference[] Destination = new GeoSpatialObjectReference[10];

			// Attempt to copy with a negative offset
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => Collection.CopyTo(Destination, -1));
		}

		[TestMethod]
		public void Test_25_CopyTo_InsufficientArraySize()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);

			// Add objects to the collection
			Collection.Add(Object1);
			Collection.Add(Object2);

			GeoSpatialObjectReference[] Destination = new GeoSpatialObjectReference[1];

			// Attempt to copy to an array that is too small
			Assert.ThrowsException<ArgumentException>(() => Collection.CopyTo(Destination, 0));
		}

		[TestMethod]
		public void Test_26_ToArray_EmptyCollection()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];

			// Convert an empty collection to an array
			IGeoSpatialObjectReference[] Result = [.. Collection];

			// Verify that the resulting array is empty
			Assert.AreEqual(0, Result.Length);
		}

		[TestMethod]
		public void Test_27_ToArray_SingleObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add a single object to the collection
			Collection.Add(Object);

			// Convert the collection to an array
			IGeoSpatialObjectReference[] Result = [.. Collection];

			// Verify that the resulting array contains the single object
			Assert.AreEqual(1, Result.Length);
			Assert.AreEqual("37.7749,-122.4194", Result[0].GeoId);
		}

		[TestMethod]
		public void Test_28_ToArray_MultipleObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);
			GeoSpatialObject Object3 = new("40.7128,-74.0060", true);

			// Add multiple objects to the collection
			Collection.Add(Object1);
			Collection.Add(Object2);
			Collection.Add(Object3);

			// Convert the collection to an array
			IGeoSpatialObjectReference[] Result = [.. Collection];

			// Verify that the resulting array contains all objects
			Assert.AreEqual(3, Result.Length);
			Assert.IsTrue(Array.Exists(Result, o => o.GeoId == "37.7749,-122.4194"));
			Assert.IsTrue(Array.Exists(Result, o => o.GeoId == "34.0522,-118.2437"));
			Assert.IsTrue(Array.Exists(Result, o => o.GeoId == "40.7128,-74.0060"));
		}

		[TestMethod]
		public void Test_29_ToArray_AfterClear()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add an object to the collection
			Collection.Add(Object);

			// Clear the collection
			Collection.Clear();

			// Convert the collection to an array
			IGeoSpatialObjectReference[] Result = [.. Collection];

			// Verify that the resulting array is empty
			Assert.AreEqual(0, Result.Length);
		}

		[TestMethod]
		public void Test_30_ToArray_AfterRemove()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new(new GeoPosition(37.7749, -122.4194));

			// Add an object to the collection
			Collection.Add(Object);

			// Remove the object from the collection
			Collection.Remove(new GeoSpatialObjectReference(Object));

			// Convert the collection to an array
			IGeoSpatialObjectReference[] Result = [.. Collection];

			// Verify that the resulting array is empty
			Assert.AreEqual(0, Result.Length);
		}

		[TestMethod]
		public void Test_31_ToArray_ManyObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			int ObjectCount = 1000;

			// Add many objects to the collection
			for (int i = 0; i < ObjectCount; i++)
			{
				Collection.Add(new GeoSpatialObject(i.ToString(),
					new GeoPosition(37.7749 + i * 0.01, -122.4194 + i * 0.01)));
			}

			// Convert the collection to an array
			IGeoSpatialObjectReference[] Result = [.. Collection];

			// Verify that the resulting array contains all objects
			Assert.AreEqual(ObjectCount, Result.Length);

			for (int i = 0; i < ObjectCount; i++)
			{
				Assert.IsTrue(Array.Exists(Result, o =>
				{
					return
						o.GeoId == i.ToString() &&
						o.Location.Latitude == 37.7749 + i * 0.01 &&
						o.Location.Longitude == -122.4194 + i * 0.01;
				}));

			}
		}

		[TestMethod]
		public void Test_32_Enumeration_EmptyCollection()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];

			// Enumerate an empty collection
			List<IGeoSpatialObjectReference> EnumeratedObjects = [];
			foreach (IGeoSpatialObjectReference Obj in Collection)
			{
				EnumeratedObjects.Add(Obj);
			}

			// Verify that no objects were enumerated
			Assert.AreEqual(0, EnumeratedObjects.Count);
		}

		[TestMethod]
		public void Test_33_Enumeration_SingleObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add a single object to the collection
			Collection.Add(Object);

			// Enumerate the collection
			List<IGeoSpatialObjectReference> EnumeratedObjects = [];
			foreach (IGeoSpatialObjectReference Obj in Collection)
			{
				EnumeratedObjects.Add(Obj);
			}

			// Verify that the single object was enumerated
			Assert.AreEqual(1, EnumeratedObjects.Count);
			Assert.AreEqual("37.7749,-122.4194", EnumeratedObjects[0].GeoId);
		}

		[TestMethod]
		public void Test_34_Enumeration_MultipleObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);
			GeoSpatialObject Object3 = new("40.7128,-74.0060", true);

			// Add multiple objects to the collection
			Collection.Add(Object1);
			Collection.Add(Object2);
			Collection.Add(Object3);

			// Enumerate the collection
			List<IGeoSpatialObjectReference> EnumeratedObjects = [];
			foreach (IGeoSpatialObjectReference Obj in Collection)
			{
				EnumeratedObjects.Add(Obj);
			}

			// Verify that all objects were enumerated
			Assert.AreEqual(3, EnumeratedObjects.Count);
			Assert.IsTrue(EnumeratedObjects.Exists(o => o.GeoId == "37.7749,-122.4194"));
			Assert.IsTrue(EnumeratedObjects.Exists(o => o.GeoId == "34.0522,-118.2437"));
			Assert.IsTrue(EnumeratedObjects.Exists(o => o.GeoId == "40.7128,-74.0060"));
		}

		[TestMethod]
		public void Test_35_Enumeration_ModifyDuringEnumeration_Add()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new(new GeoPosition(37.7749, -122.4194));
			GeoSpatialObject Object2 = new(new GeoPosition(34.0522, -118.2437));

			// Add an object to the collection
			Collection.Add(Object1);

			// Attempt to modify the collection during enumeration
			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				foreach (IGeoSpatialObjectReference _ in Collection)
					Collection.Add(Object2);
			});
		}

		[TestMethod]
		public void Test_36_Enumeration_ModifyDuringEnumeration_Remove()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new(new GeoPosition(37.7749, -122.4194));

			// Add an object to the collection
			Collection.Add(Object1);

			// Attempt to modify the collection during enumeration
			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				foreach (IGeoSpatialObjectReference _ in Collection)
					Collection.Remove(new GeoSpatialObjectReference(Object1));
			});
		}

		[TestMethod]
		public void Test_37_Enumeration_AfterModification()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);

			// Add an object to the collection
			Collection.Add(Object1);

			// Enumerate the collection
			List<IGeoSpatialObjectReference> EnumeratedObjectsBefore = [];
			foreach (IGeoSpatialObjectReference Obj in Collection)
			{
				EnumeratedObjectsBefore.Add(Obj);
			}

			// Modify the collection
			Collection.Add(Object2);

			// Enumerate the collection again
			List<IGeoSpatialObjectReference> EnumeratedObjectsAfter = [];
			foreach (IGeoSpatialObjectReference Obj in Collection)
			{
				EnumeratedObjectsAfter.Add(Obj);
			}

			// Verify the first enumeration contains only the first object
			Assert.AreEqual(1, EnumeratedObjectsBefore.Count);
			Assert.AreEqual("37.7749,-122.4194", EnumeratedObjectsBefore[0].GeoId);

			// Verify the second enumeration contains both objects
			Assert.AreEqual(2, EnumeratedObjectsAfter.Count);
			Assert.IsTrue(EnumeratedObjectsAfter.Exists(o => o.GeoId == "37.7749,-122.4194"));
			Assert.IsTrue(EnumeratedObjectsAfter.Exists(o => o.GeoId == "34.0522,-118.2437"));
		}

		[TestMethod]
		public void Test_38_Remove_ExistingObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add the object to the collection
			Collection.Add(Object);

			// Remove the object
			bool Removed = Collection.Remove(new GeoSpatialObjectReference(Object));

			// Verify that the object was removed
			Assert.IsTrue(Removed);
			Assert.AreEqual(0, Collection.Count);
			Assert.IsFalse(Collection.Contains(new GeoSpatialObjectReference(Object)));
		}

		[TestMethod]
		public void Test_39_Remove_NonExistentObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Attempt to remove an object that was not added
			bool Removed = Collection.Remove(new GeoSpatialObjectReference(Object));

			// Verify that the object was not removed
			Assert.IsFalse(Removed);
			Assert.AreEqual(0, Collection.Count);
		}

		[TestMethod]
		public void Test_40_Remove_FromEmptyCollection()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Attempt to remove an object from an empty collection
			bool Removed = Collection.Remove(new GeoSpatialObjectReference(Object));

			// Verify that the object was not removed
			Assert.IsFalse(Removed);
			Assert.AreEqual(0, Collection.Count);
		}

		[TestMethod]
		public void Test_41_Remove_MultipleObjects()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("34.0522,-118.2437", true);
			GeoSpatialObject Object3 = new("40.7128,-74.0060", true);

			// Add multiple objects to the collection
			Collection.Add(Object1);
			Collection.Add(Object2);
			Collection.Add(Object3);

			// Remove one object
			bool Removed = Collection.Remove(new GeoSpatialObjectReference(Object2));

			// Verify that the object was removed
			Assert.IsTrue(Removed);
			Assert.AreEqual(2, Collection.Count);
			Assert.IsFalse(Collection.Contains(new GeoSpatialObjectReference(Object2)));

			// Verify that the other objects are still in the collection
			Assert.IsTrue(Collection.Contains(new GeoSpatialObjectReference(Object1)));
			Assert.IsTrue(Collection.Contains(new GeoSpatialObjectReference(Object3)));
		}

		[TestMethod]
		public void Test_42_Remove_DuplicateGeoIdDifferentObject()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object1 = new("37.7749,-122.4194", true);
			GeoSpatialObject Object2 = new("37.7749,-122.4194", true); // Same GeoId, different instance

			// Add the first object to the collection
			Collection.Add(Object1);

			// Attempt to remove the second object
			bool Removed = Collection.Remove(new GeoSpatialObjectReference(Object2));

			// Verify that the object was not removed
			Assert.IsFalse(Removed);
			Assert.AreEqual(1, Collection.Count);
			Assert.IsTrue(Collection.Contains(new GeoSpatialObjectReference(Object1)));
		}

		[TestMethod]
		public void Test_43_Remove_AfterClear()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add the object to the collection
			Collection.Add(Object);

			// Clear the collection
			Collection.Clear();

			// Attempt to remove the object
			bool Removed = Collection.Remove(new GeoSpatialObjectReference(Object));

			// Verify that the object was not removed
			Assert.IsFalse(Removed);
			Assert.AreEqual(0, Collection.Count);
		}

		[TestMethod]
		public void Test_44_Remove_AfterReAdd()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];
			GeoSpatialObject Object = new("37.7749,-122.4194", true);

			// Add and remove the object
			Collection.Add(Object);
			Collection.Remove(new GeoSpatialObjectReference(Object));

			// Re-add the object
			Collection.Add(Object);

			// Remove the object again
			bool Removed = Collection.Remove(new GeoSpatialObjectReference(Object));

			// Verify that the object was removed
			Assert.IsTrue(Removed);
			Assert.AreEqual(0, Collection.Count);
			Assert.IsFalse(Collection.Contains(new GeoSpatialObjectReference(Object)));
		}

		[TestMethod]
		public void Test_45_Find_AllPointsInBoundingBox()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("Point1", new GeoPosition(10.0, 20.0)),
				new GeoSpatialObject("Point2", new GeoPosition(15.0, 25.0)),
				new GeoSpatialObject("Point3", new GeoPosition(20.0, 30.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(5.0, 15.0), new GeoPosition(25.0, 35.0));

			GeoSpatialObjectReference[] Results = Collection.Find(Box);

			Assert.AreEqual(3, Results.Length);
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point1"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point2"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point3"));
		}

		[TestMethod]
		public void Test_46_Find_NoPointsInBoundingBox()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("Point1", new GeoPosition(10.0, 20.0)),
				new GeoSpatialObject("Point2", new GeoPosition(15.0, 25.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(30.0, 40.0), new GeoPosition(35.0, 45.0));

			GeoSpatialObjectReference[] Results = Collection.Find(Box);

			Assert.AreEqual(0, Results.Length);
		}

		[TestMethod]
		public void Test_47_Find_PointsInSpecificQuadrants()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("NW", new GeoPosition(15.0, -15.0)),
				new GeoSpatialObject("NE", new GeoPosition(15.0, 15.0)),
				new GeoSpatialObject("SW", new GeoPosition(-15.0, -15.0)),
				new GeoSpatialObject("SE", new GeoPosition(-15.0, 15.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(-20.0, -20.0), new GeoPosition(20.0, 20.0));

			GeoSpatialObjectReference[] Results = Collection.Find(Box);

			Assert.AreEqual(4, Results.Length);
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "NW"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "NE"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "SW"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "SE"));
		}

		[TestMethod]
		public void Test_48_Find_BoundingBoxExcludesEdges()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("PointOnMin", new GeoPosition(10.0, 20.0)),
				new GeoSpatialObject("PointOnMax", new GeoPosition(30.0, 40.0)),
				new GeoSpatialObject("PointInside", new GeoPosition(20.0, 30.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(10.0, 20.0), new GeoPosition(30.0, 40.0), false, false);

			GeoSpatialObjectReference[] Results = Collection.Find(Box);

			Assert.AreEqual(1, Results.Length);
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "PointInside"));
		}

		[TestMethod]
		public void Test_49_Find_BoundingBoxIncludesEdges()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("PointOnMin", new GeoPosition(10.0, 20.0)),
				new GeoSpatialObject("PointOnMax", new GeoPosition(30.0, 40.0)),
				new GeoSpatialObject("PointInside", new GeoPosition(20.0, 30.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(10.0, 20.0), new GeoPosition(30.0, 40.0), true, true);

			GeoSpatialObjectReference[] Results = Collection.Find(Box);

			Assert.AreEqual(3, Results.Length);
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "PointOnMin"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "PointOnMax"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "PointInside"));
		}

		[TestMethod]
		public void Test_50_Find_EmptyCollection()
		{
			GeoCollection<GeoSpatialObjectReference> Collection = [];

			GeoBoundingBox Box = new(new GeoPosition(10.0, 20.0), new GeoPosition(30.0, 40.0));

			GeoSpatialObjectReference[] Results = Collection.Find(Box);

			Assert.AreEqual(0, Results.Length);
		}

		[TestMethod]
		public void Test_51_Find_BoundingBoxWithExtremeCoordinates()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("PointMin", new GeoPosition(-90.0, -180.0)),
				new GeoSpatialObject("PointMax", new GeoPosition(90.0, 180.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(-90.0, -180.0), new GeoPosition(90.0, 180.0));

			GeoSpatialObjectReference[] Results = Collection.Find(Box);

			Assert.AreEqual(2, Results.Length);
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "PointMin"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "PointMax"));
		}

		[TestMethod]
		public void Test_52_Find_AltitudeWithinRange()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("Point1", new GeoPosition(10.0, 20.0, 100.0)),
				new GeoSpatialObject("Point2", new GeoPosition(15.0, 25.0, 200.0)),
				new GeoSpatialObject("Point3", new GeoPosition(20.0, 30.0, 300.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(5.0, 15.0, 150.0), new GeoPosition(25.0, 35.0, 250.0));

			var Results = Collection.Find(Box);

			Assert.AreEqual(1, Results.Length);
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point2"));
		}

		[TestMethod]
		public void Test_53_Find_AltitudeOutsideRange()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("Point1", new GeoPosition(10.0, 20.0, 100.0)),
				new GeoSpatialObject("Point2", new GeoPosition(15.0, 25.0, 200.0)),
				new GeoSpatialObject("Point3", new GeoPosition(20.0, 30.0, 300.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(5.0, 15.0, 400.0), new GeoPosition(25.0, 35.0, 500.0));

			var Results = Collection.Find(Box);

			Assert.AreEqual(0, Results.Length);
		}

		[TestMethod]
		public void Test_54_Find_AltitudeOnBoundary_Inclusive()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("Point1", new GeoPosition(10.0, 20.0, 100.0)),
				new GeoSpatialObject("Point2", new GeoPosition(15.0, 25.0, 200.0)),
				new GeoSpatialObject("Point3", new GeoPosition(20.0, 30.0, 300.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(5.0, 15.0, 100.0), new GeoPosition(25.0, 35.0, 300.0), true, true);

			var Results = Collection.Find(Box);

			Assert.AreEqual(3, Results.Length);
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point1"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point2"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point3"));
		}

		[TestMethod]
		public void Test_55_Find_AltitudeOnBoundary_Exclusive()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("Point1", new GeoPosition(10.0, 20.0, 100.0)),
				new GeoSpatialObject("Point2", new GeoPosition(15.0, 25.0, 200.0)),
				new GeoSpatialObject("Point3", new GeoPosition(20.0, 30.0, 300.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(5.0, 15.0, 100.0), new GeoPosition(25.0, 35.0, 300.0), false, false);

			var Results = Collection.Find(Box);

			Assert.AreEqual(1, Results.Length);
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point2"));
		}

		[TestMethod]
		public void Test_56_Find_AltitudeMixedRange()
		{
			GeoCollection<GeoSpatialObjectReference> Collection =
			[
				new GeoSpatialObject("Point1", new GeoPosition(10.0, 20.0, 50.0)),
				new GeoSpatialObject("Point2", new GeoPosition(15.0, 25.0, 150.0)),
				new GeoSpatialObject("Point3", new GeoPosition(20.0, 30.0, 250.0)),
				new GeoSpatialObject("Point4", new GeoPosition(25.0, 35.0, 350.0)),
			];

			GeoBoundingBox Box = new(new GeoPosition(5.0, 15.0, 100.0), new GeoPosition(30.0, 40.0, 300.0));

			var Results = Collection.Find(Box);

			Assert.AreEqual(2, Results.Length);
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point2"));
			Assert.IsTrue(Array.Exists(Results, p => p.GeoId == "Point3"));
		}

	}
}
