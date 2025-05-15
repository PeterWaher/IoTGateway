namespace Waher.Runtime.Collections.Test
{
	[TestClass]
	public class ChunkedListTests
	{
		[TestMethod]
		public void Test_001_DefaultConstructor()
		{
			ChunkedList<int> List = [];
			Assert.AreEqual(0, List.Count);
			Assert.IsFalse(List.IsReadOnly);
		}

		[TestMethod]
		public void Test_002_ConstructorWithInitialChunkSize()
		{
			ChunkedList<int> List = new(32);
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		public void Test_003_ConstructorWithInitialAndMaxChunkSize()
		{
			ChunkedList<int> List = new(32, 64);
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_004_ConstructorWithInvalidInitialChunkSize()
		{
			ChunkedList<int> List = new(0);
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_005_ConstructorWithMaxChunkSizeLessThanInitialChunkSize()
		{
			ChunkedList<int> List = new(64, 32);
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		public void Test_006_AddSingleItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			Assert.AreEqual(1, List.Count);
			Assert.IsTrue(List.Contains(1));
		}

		[TestMethod]
		public void Test_007_AddMultipleItemsWithinInitialChunkSize()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3,
				4
			};
			Assert.AreEqual(4, List.Count);
		}

		[TestMethod]
		public void Test_008_AddItemsExceedingInitialChunkSize()
		{
			ChunkedList<int> List = new(4, 8);
			for (int i = 0; i < 5; i++)
				List.Add(i);
			Assert.AreEqual(5, List.Count);
		}

		[TestMethod]
		public void Test_009_RemoveExistingItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			Assert.IsTrue(List.Remove(1));
			Assert.AreEqual(1, List.Count);
			Assert.IsFalse(List.Contains(1));
		}

		[TestMethod]
		public void Test_010_RemoveNonExistingItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			Assert.IsFalse(List.Remove(2));
		}

		[TestMethod]
		public void Test_011_ContainsItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			Assert.IsTrue(List.Contains(1));
			Assert.IsFalse(List.Contains(2));
		}

		[TestMethod]
		public void Test_012_Clear()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Clear();
			Assert.AreEqual(0, List.Count);
			Assert.IsFalse(List.Contains(1));
		}

		[TestMethod]
		public void Test_013_CopyTo()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			int[] Array = new int[2];
			List.CopyTo(Array, 0);
			Assert.AreEqual(1, Array[0]);
			Assert.AreEqual(2, Array[1]);
		}

		[TestMethod]
		public void Test_014_Enumerator()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			int Count = 0;
			foreach (int item in List)
				Count++;

			Assert.AreEqual(2, Count);
		}

		[TestMethod]
		public void Test_015_HasLastItem()
		{
			ChunkedList<int> List = [];
			Assert.IsFalse(List.HasLastItem);

			List.Add(1);
			Assert.IsTrue(List.HasLastItem);
		}

		[TestMethod]
		public void Test_016_LastItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			Assert.AreEqual(2, List.LastItem);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_017_LastItem_EmptyList()
		{
			ChunkedList<int> List = [];
			_ = List.LastItem; // Should throw InvalidOperationException
		}

		[TestMethod]
		public void Test_018_HasFirstItem()
		{
			ChunkedList<int> List = [];
			Assert.IsFalse(List.HasFirstItem);

			List.Add(1);
			Assert.IsTrue(List.HasFirstItem);
		}

		[TestMethod]
		public void Test_019_FirstItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			Assert.AreEqual(1, List.FirstItem);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_020_FirstItem_EmptyList()
		{
			ChunkedList<int> List = [];
			_ = List.FirstItem; // Should throw InvalidOperationException
		}

		[TestMethod]
		public void Test_021_AddFirstItem()
		{
			ChunkedList<int> List = [];
			List.AddFirstItem(1);
			Assert.AreEqual(1, List.Count);
			Assert.AreEqual(1, List.FirstItem);
			Assert.AreEqual(1, List.LastItem);

			List.AddFirstItem(2);
			Assert.AreEqual(2, List.Count);
			Assert.AreEqual(2, List.FirstItem);
			Assert.AreEqual(1, List.LastItem);
		}

		[TestMethod]
		public void Test_022_AddLastItem()
		{
			ChunkedList<int> List = [];
			List.AddLastItem(1);
			Assert.AreEqual(1, List.Count);
			Assert.AreEqual(1, List.FirstItem);
			Assert.AreEqual(1, List.LastItem);

			List.AddLastItem(2);
			Assert.AreEqual(2, List.Count);
			Assert.AreEqual(1, List.FirstItem);
			Assert.AreEqual(2, List.LastItem);
		}

		[TestMethod]
		public void Test_023_RemoveFirst()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			List.RemoveFirst();
			Assert.AreEqual(1, List.Count);
			Assert.AreEqual(2, List.FirstItem);
			Assert.AreEqual(2, List.LastItem);

			List.RemoveFirst();
			Assert.AreEqual(0, List.Count);
			Assert.IsFalse(List.HasFirstItem);
			Assert.IsFalse(List.HasLastItem);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_024_RemoveFirst_EmptyList()
		{
			ChunkedList<int> List = [];
			List.RemoveFirst(); // Should throw InvalidOperationException
		}

		[TestMethod]
		public void Test_025_RemoveLast()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			List.RemoveLast();
			Assert.AreEqual(1, List.Count);
			Assert.AreEqual(1, List.FirstItem);
			Assert.AreEqual(1, List.LastItem);

			List.RemoveLast();
			Assert.AreEqual(0, List.Count);
			Assert.IsFalse(List.HasFirstItem);
			Assert.IsFalse(List.HasLastItem);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_026_RemoveLast_EmptyList()
		{
			ChunkedList<int> List = [];
			List.RemoveLast(); // Should throw InvalidOperationException
		}

		[TestMethod]
		public void Test_027_AddRemoveManyElements()
		{
			ChunkedList<int> List = [];

			// Add elements to the end
			for (int i = 0; i < 10000; i++)
				List.AddLastItem(i);

			Assert.AreEqual(10000, List.Count);
			Assert.AreEqual(0, List.FirstItem);
			Assert.AreEqual(9999, List.LastItem);

			// Remove elements from the beginning
			for (int i = 0; i < 5000; i++)
				List.RemoveFirst();

			Assert.AreEqual(5000, List.Count);
			Assert.AreEqual(5000, List.FirstItem);
			Assert.AreEqual(9999, List.LastItem);

			// Add elements to the beginning
			for (int i = 0; i < 5000; i++)
				List.AddFirstItem(-i);

			Assert.AreEqual(10000, List.Count);
			Assert.AreEqual(-4999, List.FirstItem);
			Assert.AreEqual(9999, List.LastItem);

			// Remove elements from the end
			for (int i = 0; i < 5000; i++)
				List.RemoveLast();

			Assert.AreEqual(5000, List.Count);
			Assert.AreEqual(-4999, List.FirstItem);
			Assert.AreEqual(0, List.LastItem);
		}

		[TestMethod]
		public void Test_028_AddRemoveAndEnumerate()
		{
			ChunkedList<int> List = [];

			// Add elements to the end
			for (int i = 0; i < 10000; i++)
				List.AddLastItem(i);

			// Remove elements from the beginning
			for (int i = 0; i < 5000; i++)
				List.RemoveFirst();

			// Add elements to the beginning
			for (int i = 0; i < 5000; i++)
				List.AddFirstItem(-i);

			// Remove elements from the end
			for (int i = 0; i < 5000; i++)
				List.RemoveLast();

			// Enumerate remaining elements
			int expectedValue = -4999;
			foreach (int item in List)
			{
				Assert.AreEqual(expectedValue, item);
				expectedValue++;
			}

			Assert.AreEqual(5000, List.Count);
			Assert.AreEqual(-4999, List.FirstItem);
			Assert.AreEqual(0, List.LastItem);
		}
		[TestMethod]
		public void Test_029_Indexer_Get()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			List.Add(3);

			Assert.AreEqual(1, List[0]);
			Assert.AreEqual(2, List[1]);
			Assert.AreEqual(3, List[2]);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_030_Indexer_Get_OutOfRange()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			_ = List[1]; // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_031_Indexer_Set()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			List.Add(3);

			List[1] = 5;

			Assert.AreEqual(1, List[0]);
			Assert.AreEqual(5, List[1]);
			Assert.AreEqual(3, List[2]);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_032_Indexer_Set_OutOfRange()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List[1] = 5; // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_033_IndexOf()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			List.Add(3);

			Assert.AreEqual(0, List.IndexOf(1));
			Assert.AreEqual(1, List.IndexOf(2));
			Assert.AreEqual(2, List.IndexOf(3));
			Assert.AreEqual(-1, List.IndexOf(4));
		}

		[TestMethod]
		public void Test_034_Insert()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(3);

			List.Insert(1, 2);

			Assert.AreEqual(1, List[0]);
			Assert.AreEqual(2, List[1]);
			Assert.AreEqual(3, List[2]);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_035_Insert_OutOfRange()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Insert(2, 2); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_036_RemoveAt()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			List.Add(3);

			List.RemoveAt(1);

			Assert.AreEqual(1, List[0]);
			Assert.AreEqual(3, List[1]);
			Assert.AreEqual(2, List.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_037_RemoveAt_OutOfRange()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.RemoveAt(1); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_038_Insert_ChunkWithStartGreaterThanZero()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3,
				4
			};

			// Remove first two elements
			List.RemoveAt(0);
			List.RemoveAt(0);

			// Insert element at index 1 (in the chunk where Start > 0)
			List.Insert(1, 5);

			// Enumerate items to ensure all items are available and in the correct order
			int[] Expected = [3, 5, 4];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_039_Insert_ChunkNotFull()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3
			};

			// Insert element at index 1 (in the chunk that is not entirely full)
			List.Insert(1, 5);

			// Enumerate items to ensure all items are available and in the correct order
			int[] Expected = [1, 5, 2, 3];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_040_Insert_ChunkFull_0()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3,
				4
			};

			List.Insert(0, 5);

			int[] Expected = [5, 1, 2, 3, 4];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_041_Insert_ChunkFull_1()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3,
				4
			};

			List.Insert(1, 5);

			int[] Expected = [1, 5, 2, 3, 4];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_042_Insert_ChunkFull_2()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3,
				4
			};

			// Insert element at index 2 (in the chunk that is full, so it needs to be divided into two)
			List.Insert(2, 5);

			// Enumerate items to ensure all items are available and in the correct order
			int[] Expected = [1, 2, 5, 3, 4];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_043_Insert_ChunkFull_3()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3,
				4
			};

			// Insert element at index 2 (in the chunk that is full, so it needs to be divided into two)
			List.Insert(3, 5);

			// Enumerate items to ensure all items are available and in the correct order
			int[] Expected = [1, 2, 3, 5, 4];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_044_Insert_ChunkFull_4()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3,
				4
			};

			// Insert element at index 2 (in the chunk that is full, so it needs to be divided into two)
			List.Insert(4, 5);

			// Enumerate items to ensure all items are available and in the correct order
			int[] Expected = [1, 2, 3, 4, 5];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_045_IndexOf_Item_Index()
		{
			ChunkedList<int> List =
			[
				1,
				2,
				3,
				2,
				4
			];

			Assert.AreEqual(1, List.IndexOf(2, 0)); // First occurrence of 2
			Assert.AreEqual(3, List.IndexOf(2, 2)); // Second occurrence of 2
			Assert.AreEqual(-1, List.IndexOf(2, 4)); // No occurrence of 2 after index 4
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_046_IndexOf_Item_Index_OutOfRange()
		{
			ChunkedList<int> List = [1, 2];
			List.IndexOf(2, 3); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_047_IndexOf_Item_Index_Count()
		{
			ChunkedList<int> List = [1, 2, 3, 2, 4];

			Assert.AreEqual(1, List.IndexOf(2, 0, 5)); // First occurrence of 2
			Assert.AreEqual(3, List.IndexOf(2, 2, 3)); // Second occurrence of 2
			Assert.AreEqual(-1, List.IndexOf(2, 2, 1)); // No occurrence of 2 in the specified Range
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_048_IndexOf_Item_Index_Count_OutOfRange()
		{
			ChunkedList<int> List = [1, 2];
			List.IndexOf(2, 0, 3); // Should throw ArgumentOutOfRangeException
		}
		[TestMethod]
		public void Test_049_LastIndexOf_Item()
		{
			ChunkedList<int> List = [1, 2, 3, 2, 4];

			Assert.AreEqual(3, List.LastIndexOf(2)); // Last occurrence of 2
			Assert.AreEqual(4, List.LastIndexOf(4)); // Last occurrence of 4
			Assert.AreEqual(-1, List.LastIndexOf(5)); // No occurrence of 5
		}

		[TestMethod]
		public void Test_050_LastIndexOf_Item_Index()
		{
			ChunkedList<int> List = [1, 2, 3, 2, 4];

			Assert.AreEqual(3, List.LastIndexOf(2, 4)); // Last occurrence of 2 starting from index 4
			Assert.AreEqual(1, List.LastIndexOf(2, 2)); // Last occurrence of 2 starting from index 2
			Assert.AreEqual(-1, List.LastIndexOf(2, 0)); // No occurrence of 2 starting from index 0
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_051_LastIndexOf_Item_Index_OutOfRange()
		{
			ChunkedList<int> List = [1, 2];
			List.LastIndexOf(2, 3); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_052_LastIndexOf_Item_Index_Count()
		{
			ChunkedList<int> List = [1, 2, 3, 2, 4];

			Assert.AreEqual(3, List.LastIndexOf(2, 4, 5)); // Last occurrence of 2 in the Range
			Assert.AreEqual(1, List.LastIndexOf(2, 2, 3)); // Last occurrence of 2 in the Range
			Assert.AreEqual(-1, List.LastIndexOf(2, 2, 1)); // No occurrence of 2 in the specified Range
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_053_LastIndexOf_Item_Index_Count_OutOfRange()
		{
			ChunkedList<int> List = [1, 2];
			List.LastIndexOf(2, 0, 3); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_054_RemoveAt_ReduceChunks()
		{
			ChunkedList<int> List = new(4);

			// Add elements to fill multiple chunks
			for (int i = 0; i < 16; i++)
				List.Add(i);

			// Remove elements to reduce the number of chunks
			for (int i = 0; i < 8; i++)
				List.RemoveAt(0);

			// Check that the remaining elements are correct
			int[] Expected = [8, 9, 10, 11, 12, 13, 14, 15];
			int Index = 0;
			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_055_RemoveAt_ReduceChunks_Middle()
		{
			ChunkedList<int> List = new(4);

			// Add elements to fill multiple chunks
			for (int i = 0; i < 16; i++)
				List.Add(i);

			// Remove elements from the middle to reduce the number of chunks
			for (int i = 0; i < 8; i++)
				List.RemoveAt(4);

			// Check that the remaining elements are correct
			int[] Expected = [0, 1, 2, 3, 12, 13, 14, 15];
			int Index = 0;
			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_056_RemoveAt_ReduceChunks_End()
		{
			ChunkedList<int> List = new(4);

			// Add elements to fill multiple chunks
			for (int i = 0; i < 16; i++)
				List.Add(i);

			// Remove elements from the end to reduce the number of chunks
			for (int i = 0; i < 8; i++)
				List.RemoveAt(List.Count - 1);

			// Check that the remaining elements are correct
			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7];
			int Index = 0;
			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_057_AddRange_WithinSingleChunk()
		{
			ChunkedList<int> List = new(4);
			List<int> Range = [1, 2, 3];

			List.AddRange(Range);

			Assert.AreEqual(3, List.Count);
			int[] Expected = [1, 2, 3];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_058_AddRange_CrossChunkLimits()
		{
			ChunkedList<int> List = new(4);
			List<int> Range = [1, 2, 3, 4, 5, 6, 7, 8];

			List.AddRange(Range);

			Assert.AreEqual(8, List.Count);
			int[] Expected = [1, 2, 3, 4, 5, 6, 7, 8];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_059_AddRange_MultipleChunks()
		{
			ChunkedList<int> List = new(4);
			List<int> Range = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

			List.AddRange(Range);

			Assert.AreEqual(12, List.Count);
			int[] Expected = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_060_AddRange_EmptyCollection()
		{
			ChunkedList<int> List = new(4);
			List<int> Range = [];

			List.AddRange(Range);

			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		public void Test_061_AddRange_ExistingElements()
		{
			ChunkedList<int> List = new(4)
			{
				0
			};
			List<int> Range = [1, 2, 3, 4, 5, 6, 7, 8];

			List.AddRange(Range);

			Assert.AreEqual(9, List.Count);
			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7, 8];
			int Index = 0;

			foreach (int Item in List)
				Assert.AreEqual(Expected[Index++], Item);
		}

		[TestMethod]
		public void Test_062_CopyTo_Destination()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			int[] Destination = new int[8];
			List.CopyTo(Destination);

			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Destination[i]);
		}

		[TestMethod]
		public void Test_063_CopyTo_DestinationIndex()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			int[] Destination = new int[10];
			List.CopyTo(Destination, 2);

			int[] Expected = [0, 0, 0, 1, 2, 3, 4, 5, 6, 7];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Destination[i]);
		}

		[TestMethod]
		public void Test_064_CopyTo_Index_DestinationIndex_Count()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			int[] Destination = new int[10];
			List.CopyTo(2, Destination, 3, 4);

			int[] Expected = [0, 0, 0, 2, 3, 4, 5, 0, 0, 0];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Destination[i]);
		}

		[TestMethod]
		public void Test_065_CopyTo_ChunkLimits()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 12; i++)
				List.Add(i);

			int[] Destination = new int[12];
			List.CopyTo(Destination);

			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Destination[i]);
		}

		[TestMethod]
		public void Test_066_CopyTo_DestinationIndex_ChunkLimits()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 12; i++)
				List.Add(i);

			int[] Destination = new int[14];
			List.CopyTo(Destination, 2);

			int[] Expected = [0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Destination[i]);
		}

		[TestMethod]
		public void Test_067_CopyTo_Index_DestinationIndex_Count_ChunkLimits()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 12; i++)
				List.Add(i);

			int[] Destination = new int[14];
			List.CopyTo(2, Destination, 3, 8);

			int[] Expected = [0, 0, 0, 2, 3, 4, 5, 6, 7, 8, 9, 0, 0, 0];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Destination[i]);
		}

		[TestMethod]
		public void Test_068_ToArray_SingleChunk()
		{
			ChunkedList<int> List = new(4);
			for (int i = 0; i < 4; i++)
				List.Add(i);

			int[] Result = [.. List];
			int[] Expected = [0, 1, 2, 3];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_069_ToArray_MultipleChunks()
		{
			ChunkedList<int> List = new(4);
			for (int i = 0; i < 8; i++)
				List.Add(i);

			int[] Result = [.. List];
			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_070_ToArray_MultipleChunks_Larger()
		{
			ChunkedList<int> List = new(4);
			for (int i = 0; i < 12; i++)
				List.Add(i);

			int[] Result = [.. List];
			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_071_ToArray_EmptyList()
		{
			ChunkedList<int> List = new(4);
			int[] Result = [.. List];

			Assert.AreEqual(0, Result.Length);
		}

		[TestMethod]
		public void Test_072_ToArray_SingleElement()
		{
			ChunkedList<int> List = new(4)
			{
				1
			};

			int[] Result = [.. List];
			int[] Expected = [1];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_073_ToArray_MultipleChunks_WithGaps()
		{
			ChunkedList<int> List = new(4);
			for (int i = 0; i < 8; i++)
				List.Add(i);

			// Remove some elements to create gaps
			List.RemoveAt(2);
			List.RemoveAt(4);

			int[] Result = [.. List];
			int[] Expected = [0, 1, 3, 4, 6, 7];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_074_Update_SingleChunk_ChangeElements()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 4; i++)
				List.Add(i);

			static bool UpdateCallback(ref int Value, out bool Keep)
			{
				Value += 10;
				Keep = true;
				return true;
			}

			List.Update(UpdateCallback);

			int[] Expected = [10, 11, 12, 13];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_075_Update_MultipleChunks_ChangeElements()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			static bool UpdateCallback(ref int Value, out bool Keep)
			{
				Value += 10;
				Keep = true;
				return true;
			}

			List.Update(UpdateCallback);

			int[] Expected = [10, 11, 12, 13, 14, 15, 16, 17];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_076_UpdateSingleChunkDeleteElements()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 4; i++)
				List.Add(i);

			static bool UpdateCallback(ref int Value, out bool Keep)
			{
				Keep = Value % 2 != 0;
				return true;
			}

			List.Update(UpdateCallback);

			int[] Expected = [1, 3];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_077_Update_MultipleChunks_DeleteElements()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			static bool UpdateCallback(ref int Value, out bool Keep)
			{
				Keep = Value % 2 != 0;
				return true;
			}

			List.Update(UpdateCallback);

			int[] Expected = [1, 3, 5, 7];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_078_Update_MultipleChunks_ChangeAndDeleteElements()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			static bool UpdateCallback(ref int Value, out bool Keep)
			{
				Keep = Value % 2 != 0;
				Value += 10; // Change odd elements
				return true;
			}

			List.Update(UpdateCallback);

			int[] Expected = [11, 13, 15, 17];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_079_Update_EmptyList()
		{
			ChunkedList<int> List = new(4);

			static bool UpdateCallback(ref int Value, out bool Keep)
			{
				Keep = true;
				Value += 10;
				return true;
			}

			List.Update(UpdateCallback);

			int[] Result = [.. List];
			Assert.AreEqual(0, Result.Length);
		}

		[TestMethod]
		public void Test_080_Update_BreakProcedure()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			static bool UpdateCallback(ref int Value, out bool Keep)
			{
				Keep = true;
				if (Value == 4)
					return false; // Break the update procedure

				Value += 10;
				return true;
			}

			List.Update(UpdateCallback);

			int[] Expected = [10, 11, 12, 13, 4, 5, 6, 7];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_081_Sort_Default()
		{
			ChunkedList<int> List = new(4);

			for (int i = 7; i >= 0; i--)
				List.Add(i);

			List.Sort();

			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_082_Sort_Comparer()
		{
			ChunkedList<int> List = new(4);

			for (int i = 7; i >= 0; i--)
				List.Add(i);

			List.Sort(Comparer<int>.Create((x, y) => y.CompareTo(x)));

			int[] Expected = [7, 6, 5, 4, 3, 2, 1, 0];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_083_Sort_Comparison()
		{
			ChunkedList<int> List = new(4);

			for (int i = 7; i >= 0; i--)
				List.Add(i);

			List.Sort((x, y) => y.CompareTo(x));

			int[] Expected = [7, 6, 5, 4, 3, 2, 1, 0];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_084_Sort_Index_Count_Comparer()
		{
			ChunkedList<int> List = new(4);

			for (int i = 7; i >= 0; i--)
				List.Add(i);

			List.Sort(2, 4, Comparer<int>.Create((x, y) => x.CompareTo(y)));

			int[] Expected = [7, 6, 2, 3, 4, 5, 1, 0];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_085_Sort_MultipleChunks_Default()
		{
			ChunkedList<int> List = new(4);

			for (int i = 15; i >= 0; i--)
				List.Add(i);

			List.Sort();

			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_086_Sort_MultipleChunks_Comparer()
		{
			ChunkedList<int> List = new(4);

			for (int i = 15; i >= 0; i--)
				List.Add(i);

			List.Sort(Comparer<int>.Create((x, y) => y.CompareTo(x)));

			int[] Expected = [15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_087_Sort_MultipleChunks_Comparison()
		{
			ChunkedList<int> List = new(4);

			for (int i = 15; i >= 0; i--)
				List.Add(i);

			List.Sort((x, y) => y.CompareTo(x));

			int[] Expected = [15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_088_Sort_MultipleChunks_Index_Count_Comparer()
		{
			ChunkedList<int> List = new(4);

			for (int i = 15; i >= 0; i--)
				List.Add(i);

			List.Sort(4, 8, Comparer<int>.Create((x, y) => x.CompareTo(y)));

			int[] Expected = [15, 14, 13, 12, 4, 5, 6, 7, 8, 9, 10, 11, 3, 2, 1, 0];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_089_Reverse_SingleChunk()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 4; i++)
				List.Add(i);

			List.Reverse();

			int[] Expected = [3, 2, 1, 0];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_090_Reverse_MultipleChunks()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			List.Reverse();

			int[] Expected = [7, 6, 5, 4, 3, 2, 1, 0];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_091_Reverse_Subset_SingleChunk()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 4; i++)
				List.Add(i);

			List.Reverse(1, 2);

			int[] Expected = [0, 2, 1, 3];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_092_Reverse_Subset_MultipleChunks()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			List.Reverse(2, 4);

			int[] Expected = [0, 1, 5, 4, 3, 2, 6, 7];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_093_Reverse_EmptyList()
		{
			ChunkedList<int> List = new(4);

			List.Reverse();

			int[] Result = [.. List];
			Assert.AreEqual(0, Result.Length);
		}

		[TestMethod]
		public void Test_094_Reverse_SingleElement()
		{
			ChunkedList<int> List = new(4)
			{
				1
			};

			List.Reverse();

			int[] Expected = [1];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_095_Reverse_Subset_SingleElement()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 4; i++)
				List.Add(i);

			List.Reverse(1, 1);

			int[] Expected = [0, 1, 2, 3];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_096_GetSet_LastItem_SingleChunk()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 4; i++)
				List.Add(i);

			Assert.AreEqual(3, List.LastItem);

			List.LastItem = 10;
			Assert.AreEqual(10, List.LastItem);

			int[] Expected = [0, 1, 2, 10];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_097_GetSet_LastItem_MultipleChunks()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			Assert.AreEqual(7, List.LastItem);

			List.LastItem = 10;
			Assert.AreEqual(10, List.LastItem);

			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 10];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_098_GetSet_FirstItem_SingleChunk()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 4; i++)
				List.Add(i);

			Assert.AreEqual(0, List.FirstItem);

			List.FirstItem = 10;
			Assert.AreEqual(10, List.FirstItem);

			int[] Expected = [10, 1, 2, 3];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_099_GetSet_FirstItem_MultipleChunks()
		{
			ChunkedList<int> List = new(4);

			for (int i = 0; i < 8; i++)
				List.Add(i);

			Assert.AreEqual(0, List.FirstItem);

			List.FirstItem = 10;
			Assert.AreEqual(10, List.FirstItem);

			int[] Expected = [10, 1, 2, 3, 4, 5, 6, 7];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_100_Get_LastItem_EmptyList()
		{
			ChunkedList<int> List = new(4);
			_ = List.LastItem;
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_101_Get_FirstItem_EmptyList()
		{
			ChunkedList<int> List = new(4);
			_ = List.FirstItem;
		}

		[TestMethod]
		public void Test_102_Set_LastItem_EmptyList()
		{
			ChunkedList<int> List = new(4)
			{
				LastItem = 10
			};

			Assert.AreEqual(10, List.LastItem);

			int[] Expected = [10];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_103_Set_FirstItem_EmptyList()
		{
			ChunkedList<int> List = new(4)
			{
				FirstItem = 10
			};

			Assert.AreEqual(10, List.FirstItem);

			int[] Expected = [10];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_104_AddRangeFirst_SingleChunk_Array()
		{
			ChunkedList<int> List = new(4);

			for (int i = 4; i < 8; i++)
				List.Add(i);

			int[] NewElements = [ 0, 1, 2, 3 ];
			List.AddRangeFirst(NewElements);

			int[] Expected = [ 0, 1, 2, 3, 4, 5, 6, 7 ];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_105_AddRangeFirst_MultipleChunks_Array()
		{
			ChunkedList<int> List = new(4);

			for (int i = 8; i < 16; i++)
				List.Add(i);

			int[] NewElements = [0, 1, 2, 3, 4, 5, 6, 7];
			List.AddRangeFirst(NewElements);

			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_106_AddRangeFirst_SingleChunk_Enumerable()
		{
			ChunkedList<int> List = new(4);

			for (int i = 4; i < 8; i++)
				List.Add(i);

			List<int> NewElements = [0, 1, 2, 3];
			List.AddRangeFirst(NewElements);

			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_107_AddRangeFirst_MultipleChunks_Enumerable()
		{
			ChunkedList<int> List = new(4);
		
			for (int i = 8; i < 16; i++)
				List.Add(i);

			List<int> NewElements = [0, 1, 2, 3, 4, 5, 6, 7];
			List.AddRangeFirst(NewElements);

			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_108_AddRangeFirst_EmptyList_Array()
		{
			ChunkedList<int> List = new(4);

			int[] NewElements = [0, 1, 2, 3];
			List.AddRangeFirst(NewElements);

			int[] Expected = [0, 1, 2, 3];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_109_AddRangeFirst_EmptyList_Enumerable()
		{
			ChunkedList<int> List = new(4);

			List<int> NewElements = [0, 1, 2, 3];
			List.AddRangeFirst(NewElements);

			int[] Expected = [0, 1, 2, 3];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_110_AddRangeFirst_SingleElement_Array()
		{
			ChunkedList<int> List = new(4)
			{
				4
			};

			int[] NewElements = [0, 1, 2, 3];
			List.AddRangeFirst(NewElements);

			int[] Expected = [0, 1, 2, 3, 4];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

		[TestMethod]
		public void Test_111_AddRangeFirst_SingleElement_Enumerable()
		{
			ChunkedList<int> List = new(4)
			{
				4
			};

			List<int> NewElements = [0, 1, 2, 3];
			List.AddRangeFirst(NewElements);

			int[] Expected = [0, 1, 2, 3, 4];
			int[] Result = [.. List];

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], Result[i]);
		}

	}
}
