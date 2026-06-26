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

			int[] NewElements = [0, 1, 2, 3];
			List.AddRangeFirst(NewElements);

			int[] Expected = [0, 1, 2, 3, 4, 5, 6, 7];
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

		[TestMethod]
		public void Test_112_InsertRange_EmptyCollection_DoesNotChangeCountOrIndexedValues()
		{
			ChunkedList<int> List = new(4);
			for (int i = 0; i < 8; i++)
				List.Add(i);

			int InitialChunkCount = CountChunks(List);
			int[] NewElements = [];
			List.InsertRange(4, NewElements);

			Assert.AreEqual(InitialChunkCount, CountChunks(List));
			AssertListByIndexer(List, [0, 1, 2, 3, 4, 5, 6, 7]);
		}

		[TestMethod]
		public void Test_113_InsertRange_FullInternalChunk_AtChunkStart_SplitsAndKeepsIndexedOrder()
		{
			ChunkedList<int> List = CreateList(12, 4);
			int InitialChunkCount = CountChunks(List);

			List.InsertRange(4, [100, 101]);

			Assert.IsTrue(CountChunks(List) > InitialChunkCount, "Expected InsertRange to split an internal chunk.");
			AssertListByIndexer(List, [0, 1, 2, 3, 100, 101, 4, 5, 6, 7, 8, 9, 10, 11]);
		}

		[TestMethod]
		public void Test_114_InsertRange_FullInternalChunk_BeforeMidpoint_SplitsAndKeepsIndexedOrder()
		{
			ChunkedList<int> List = CreateList(12, 4);
			int InitialChunkCount = CountChunks(List);

			List.InsertRange(5, [100, 101, 102]);

			Assert.IsTrue(CountChunks(List) > InitialChunkCount, "Expected InsertRange to split an internal chunk.");
			AssertListByIndexer(List, [0, 1, 2, 3, 4, 100, 101, 102, 5, 6, 7, 8, 9, 10, 11]);
		}

		[TestMethod]
		public void Test_115_InsertRange_FullInternalChunk_AfterMidpoint_SplitsAndKeepsIndexedOrder()
		{
			ChunkedList<int> List = CreateList(12, 4);
			int InitialChunkCount = CountChunks(List);

			List.InsertRange(7, [100, 101, 102]);

			Assert.IsTrue(CountChunks(List) > InitialChunkCount, "Expected InsertRange to split an internal chunk.");
			AssertListByIndexer(List, [0, 1, 2, 3, 4, 5, 6, 100, 101, 102, 7, 8, 9, 10, 11]);
		}

		[TestMethod]
		public void Test_116_InsertRange_LongRange_InsideFullInternalChunk_CausesRepeatedSplits()
		{
			ChunkedList<int> List = CreateList(16, 4);
			int InitialChunkCount = CountChunks(List);

			List.InsertRange(6, [200, 201, 202, 203, 204, 205, 206, 207, 208, 209]);

			Assert.IsTrue(CountChunks(List) > InitialChunkCount, "Expected a long InsertRange into an internal chunk to create additional chunks.");
			AssertListByIndexer(List,
			[
				0, 1, 2, 3, 4, 5,
				200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
				6, 7, 8, 9, 10, 11, 12, 13, 14, 15
			]);
		}

		[TestMethod]
		public void Test_117_InsertRange_InternalChunkSplit_WhenEarlierChunkHasStartOffset()
		{
			ChunkedList<int> List = CreateList(12, 4);
			List.RemoveAt(0);
			List.RemoveAt(0);
			int InitialChunkCount = CountChunks(List);

			List.InsertRange(3, [100, 101, 102]);

			Assert.IsTrue(CountChunks(List) > InitialChunkCount, "Expected InsertRange to split an internal chunk after traversing a chunk with Start > 0.");
			AssertListByIndexer(List, [2, 3, 4, 100, 101, 102, 5, 6, 7, 8, 9, 10, 11]);
		}

		[TestMethod]
		public void Test_118_InsertRange_SplitLastChunkThenAppend_DoesNotLoseSplitOffChunk()
		{
			ChunkedList<int> List = new(4)
			{
				0,
				1,
				2,
				3
			};

			// This InsertRange splits the current last chunk. The split-off chunk must
			// become the new last chunk, otherwise later appends use a stale tail pointer.
			List.InsertRange(2, [100]);
			AssertCountEqualsChunkElementCount(List);
			AssertListByIndexer(List, [0, 1, 100, 2, 3]);

			// Appending enough items after the stale-tail split used to overwrite the
			// old last chunk's Next pointer, orphaning the split-off chunk.
			List.InsertRange(List.Count, [200, 201, 202]);

			AssertCountEqualsChunkElementCount(List);
			AssertListByIndexer(List, [0, 1, 100, 2, 3, 200, 201, 202]);
		}

		[TestMethod]
		public void Test_119_InsertRange_ParserLikeIndexedTraversal_DoesNotLoseChunksAfterTailSplit()
		{
			ParserNode Root = new(0);
			ParserNode A = new(1);
			ParserNode B = new(2);
			ParserNode C = new(3);
			ParserNode D = new(4);
			ParserNode E = new(5);
			ParserNode F = new(6);
			ParserNode G = new(7);
			ParserNode X = new(8);
			ParserNode Y = new(9);
			ParserNode Z = new(10);

			// Root's children fill the first chunk and the second chunk exactly:
			// [Root, A, B, C] [D, E, F, G]
			Root.Children = [A, B, C, D, E, F, G];

			// Processing D inserts X before E/F/G and splits the current last chunk.
			D.Children = [X];

			// Processing G appends enough nodes to reuse the stale lastChunk pointer if
			// Insert did not update lastChunk when D split the tail chunk.
			G.Children = [Y, Z];

			ChunkedList<ParserNode> Todo = new(4)
			{
				Root
			};

			int i = 0;
			while (i < Todo.Count)
			{
				ParserNode N = Todo[i++];

				if (N.Children.Length > 0)
				{
					Todo.InsertRange(i, N.Children);
					AssertCountEqualsChunkElementCount(Todo);
				}
			}

			AssertNodeListByIndexer(Todo, [0, 1, 2, 3, 4, 8, 5, 6, 7, 9, 10]);
		}

		[TestMethod]
		public void Test_120_InsertRange_SplitLastChunkThenAppend_AllCountedIndexesRemainReadable()
		{
			ChunkedList<int> List = new(4)
			{
				0,
				1,
				2,
				3
			};

			List.InsertRange(2, [100]);
			List.InsertRange(List.Count, [200, 201, 202]);

			AssertAllCountedIndexesCanBeRead(List);
			AssertListByIndexer(List, [0, 1, 100, 2, 3, 200, 201, 202]);
		}

		[TestMethod]
		public void Test_121_LastChunk_ReturnsTailChunkNodeAfterListSpansMultipleChunks()
		{
			ChunkedList<int> List = CreateList(8, 4);

			Assert.IsTrue(CountChunks(List) > 1, "Test setup requires more than one chunk.");
			Assert.AreNotSame(List.FirstChunk, List.LastChunk, "FirstChunk and LastChunk must not expose the same node when the list spans multiple chunks.");
			AssertCountEqualsChunkElementCount(List);
			AssertListByIndexer(List, [0, 1, 2, 3, 4, 5, 6, 7]);
		}

		[TestMethod]
		public void Test_122_FirstItemSetter_WhenFirstChunkHasStartOffset_UpdatesLogicalFirstItem()
		{
			ChunkedList<int> List = new(4)
			{
				0,
				1,
				2,
				3
			};

			List.RemoveFirst();
			Assert.AreEqual(1, List.FirstItem);

			List.FirstItem = 100;

			Assert.AreEqual(100, List.FirstItem, "FirstItem setter must write to firstChunk.Start, not to physical index zero.");
			AssertListByIndexer(List, [100, 2, 3]);
			AssertCountEqualsChunkElementCount(List);
		}

		[TestMethod]
		public void Test_123_Insert_FullChunkOfSizeOne_SplitsWithoutCorruptingList()
		{
			ChunkedList<int> List = new(1)
			{
				1
			};

			List.Insert(0, 0);

			AssertListByIndexer(List, [0, 1]);
			AssertCountEqualsChunkElementCount(List);
			AssertAllCountedIndexesCanBeRead(List);
		}

		[TestMethod]
		public void Test_124_IndexOf_WhenFirstChunkHasStartOffset_ReturnsLogicalIndex()
		{
			ChunkedList<int> List = new(4)
			{
				0,
				1,
				2,
				3
			};

			List.RemoveFirst();

			AssertListByIndexer(List, [1, 2, 3]);
			Assert.AreEqual(0, List.IndexOf(1), "IndexOf must return the logical list index, not the physical array slot.");
			Assert.AreEqual(1, List.IndexOf(2), "IndexOf must subtract the chunk Start offset from the physical array index.");
			Assert.AreEqual(2, List.IndexOf(3), "IndexOf must subtract the chunk Start offset from the physical array index.");
		}

		[TestMethod]
		public void Test_125_IndexOf_WithStartIndexInsideEarlierChunk_SearchesFollowingChunks()
		{
			ChunkedList<int> List = CreateList(8, 4);

			AssertListByIndexer(List, [0, 1, 2, 3, 4, 5, 6, 7]);
			Assert.AreEqual(4, List.IndexOf(4, 2), "IndexOf must reset the per-chunk offset after searching the tail of the starting chunk.");
			Assert.AreEqual(5, List.IndexOf(5, 2, 4), "IndexOf with a start index and count must search across following chunks using logical indexes.");
			Assert.AreEqual(-1, List.IndexOf(6, 2, 4), "IndexOf must honor Count while searching across following chunks.");
		}

		[TestMethod]
		public void Test_126_LastIndexOf_WhenFirstChunkHasStartOffset_ReturnsLogicalIndex()
		{
			ChunkedList<int> List = new(4)
			{
				0,
				1,
				2,
				3
			};

			List.RemoveFirst();

			AssertListByIndexer(List, [1, 2, 3]);
			Assert.AreEqual(2, List.LastIndexOf(3), "LastIndexOf must return logical indexes when the chunk Start offset is non-zero.");
			Assert.AreEqual(1, List.LastIndexOf(2), "LastIndexOf must subtract the chunk Start offset from the physical array index.");
			Assert.AreEqual(0, List.LastIndexOf(1), "LastIndexOf must subtract the chunk Start offset from the physical array index.");
		}

		[TestMethod]
		public void Test_127_LastIndexOf_StartingAtFirstElementOfLaterChunk_ReturnsBoundaryIndex()
		{
			ChunkedList<int> List = CreateList(8, 4);

			AssertListByIndexer(List, [0, 1, 2, 3, 4, 5, 6, 7]);
			Assert.AreEqual(4, List.LastIndexOf(4, 4));
		}

		[TestMethod]
		public void Test_128_EmptyInitialArrayConstructor_AddFirstMaintainsFirstAndLastInvariants()
		{
			ChunkedList<int> List = new(Array.Empty<int>());

			List.AddFirstItem(10);

			Assert.AreEqual(1, List.Count);
			Assert.IsTrue(List.HasFirstItem);
			Assert.IsTrue(List.HasLastItem);
			Assert.AreEqual(10, List.FirstItem);
			Assert.AreEqual(10, List.LastItem);
			Assert.AreEqual(10, List.RemoveFirst());
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		public void Test_129_EmptyInitialArrayConstructor_AddLastAllowsRemoveFirst()
		{
			ChunkedList<int> List = new(Array.Empty<int>());

			List.AddLastItem(10);

			Assert.AreEqual(1, List.Count);
			AssertCountEqualsChunkElementCount(List);
			Assert.IsTrue(List.HasFirstItem);
			Assert.IsTrue(List.HasLastItem);
			Assert.AreEqual(10, List.FirstItem);
			Assert.AreEqual(10, List.RemoveFirst());
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		public void Test_130_SortEmptyThenAdd_PreservesFirstAndLastItemInvariants()
		{
			ChunkedList<int> List = new(4);

			List.Sort();
			List.Add(1);

			Assert.AreEqual(1, List.Count);
			Assert.IsTrue(List.HasFirstItem, "Adding to a sorted empty list must leave firstChunk pointing at a non-empty chunk.");
			Assert.IsTrue(List.HasLastItem);
			Assert.AreEqual(1, List.FirstItem);
			Assert.AreEqual(1, List.LastItem);
			AssertListByIndexer(List, [1]);
			AssertCountEqualsChunkElementCount(List);
		}

		[TestMethod]
		public void Test_131_ReverseEmptyThenAdd_PreservesFirstAndLastItemInvariants()
		{
			ChunkedList<int> List = new(4);

			List.Reverse();
			List.Add(1);

			Assert.AreEqual(1, List.Count);
			Assert.IsTrue(List.HasFirstItem, "Adding to a reversed empty list must leave firstChunk pointing at a non-empty chunk.");
			Assert.IsTrue(List.HasLastItem);
			Assert.AreEqual(1, List.FirstItem);
			Assert.AreEqual(1, List.LastItem);
			AssertListByIndexer(List, [1]);
			AssertCountEqualsChunkElementCount(List);
		}

		[TestMethod]
		public void Test_132_ClearAfterSortEmptyThenAdd_DoesNotCreateZeroSizedAppendChunk()
		{
			ChunkedList<int> List = new(4);

			List.Sort();
			List.Clear();
			List.Add(1);

			AssertListByIndexer(List, [1]);
			Assert.IsTrue(List.HasFirstItem);
			Assert.IsTrue(List.HasLastItem);
			Assert.AreEqual(1, List.FirstItem);
			Assert.AreEqual(1, List.LastItem);
			AssertCountEqualsChunkElementCount(List);
		}

		[TestMethod]
		public void Test_133_IndexOf_EmptyList_ReturnsMinusOne()
		{
			ChunkedList<int> List = [];

			Assert.AreEqual(-1, List.IndexOf(123));
		}

		[TestMethod]
		public void Test_134_LastIndexOf_EmptyList_ReturnsMinusOne()
		{
			ChunkedList<int> List = [];

			Assert.AreEqual(-1, List.LastIndexOf(123));
		}

		[TestMethod]
		public void Test_135_Insert_FullSizeOneChunk_DoesNotLeaveEmptyTail()
		{
			ChunkedList<int> List = new(1)
			{
				10
			};

			List.Insert(0, 5);

			Assert.AreEqual(2, List.Count);
			Assert.AreEqual(5, List[0]);
			Assert.AreEqual(10, List[1]);

			Assert.IsTrue(List.HasLastItem);
			Assert.AreEqual(10, List.LastItem);
		}

		[TestMethod]
		public void Test_136_LastIndexOf_IndexEqualToCount_Throws()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3
			};

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => 
				List.LastIndexOf(3, List.Count));
		}

		[TestMethod]
		public void Test_137_LastIndexOf_IndexEqualToCountWithZeroCount_Throws()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3
			};

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				List.LastIndexOf(3, List.Count, 0));
		}

		[TestMethod]
		public void Test_138_AddRange_Self_DuplicatesOriginalOnlyOnce()
		{
			ChunkedList<int> List = new(2)
			{
				1,
				2
			};

			List.AddRange(List);

			AssertListByIndexer(List, [1, 2, 1, 2]);
		}

		[TestMethod]
		public void Test_139_InsertRange_EmptyCollection_IndexGreaterThanCount_Throws()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3
			};

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				List.InsertRange(4, Array.Empty<int>()));
		}

		[TestMethod]
		public void Test_140_InsertRange_Self_DuplicatesOriginalSnapshotOnly()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3
			};

			List.InsertRange(1, List);

			AssertListByIndexer(List, [1, 1, 2, 3, 2, 3]);
		}

		[TestMethod]
		public void Test_141_SortSingleItemThenClearThenInsert_DoesNotCreateBrokenSizeOneChunk()
		{
			ChunkedList<int> List = new(4)
			{
				10
			};

			List.Sort();
			List.Clear();
			List.Add(10);

			List.Insert(0, 5);

			Assert.AreEqual(2, List.Count);
			Assert.AreEqual(5, List[0]);
			Assert.AreEqual(10, List[1]);
			Assert.AreEqual(10, List.LastItem);
		}

		[TestMethod]
		public void Test_142_SortComparison_EmptyList_DoesNotCallComparison()
		{
			ChunkedList<string> List = [];
			int Comparisons = 0;

			List.Sort((x, y) =>
			{
				Comparisons++;
				throw new InvalidOperationException("Empty list should not compare elements.");
			});

			Assert.AreEqual(0, Comparisons);
		}

		[TestMethod]
		public void Test_143_CopyTo_NegativeSourceIndex_Throws()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3
			};

			int[] Destination = new int[3];

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				List.CopyTo(-1, Destination, 0, 1));
		}

		[TestMethod]
		public void Test_144_AddRange_ArrayNegativeCount_Throws()
		{
			ChunkedList<int> List = new(4)
			{
				10
			};

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				List.AddRange([1, 2, 3], 1, -1));
		}

		[TestMethod]
		public void Test_145_AddRange_ArrayIndexEqualLengthPlusOneWithZeroCount_Throws()
		{
			ChunkedList<int> List = new(4)
			{
				10
			};

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				List.AddRange([1, 2, 3], 4, 0));
		}

		[TestMethod]
		public void Test_146_AddRange_LazySelfEnumerable_DoesNotEnumerateNewlyAppendedItems()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2
			};

			IEnumerable<int> LazySelf = Enumerable.Select(List, x => x);

			List.AddRange(LazySelf);

			AssertListByIndexer(List, [1, 2, 1, 2]);
		}

		[TestMethod]
		[Timeout(1000)]
		public void Test_147_InsertRange_LazySelfEnumerable_DoesNotEnumerateNewlyInsertedItems()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3
			};

			IEnumerable<int> LazySelf = Enumerable.Select(List, x => x);

			List.InsertRange(1, LazySelf);

			AssertListByIndexer(List, [1, 1, 2, 3, 2, 3]);
		}

		[TestMethod]
		public void Test_148_ReverseRange_EmptyListIndexOne_Throws()
		{
			ChunkedList<int> List = [];

			Assert.ThrowsException<ArgumentOutOfRangeException>(()=>
				List.Reverse(1, 0));
		}

		[TestMethod]
		public void Test_149_ReverseRange_EmptyListIndexOne_Throws()
		{
			ChunkedList<int> List = [];

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				List.Reverse(0, 1));
		}

		[TestMethod]
		public void Test_150_SortRange_EmptyListIndexOne_Throws()
		{
			ChunkedList<int> List = [];

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				List.Sort(1, 0, Comparer<int>.Default));
		}

		[TestMethod]
		public void Test_151_SortRange_EmptyListIndexOne_Throws()
		{
			ChunkedList<int> List =
			[
				1
			];

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
				List.Sort(0, 2, Comparer<int>.Default));
		}

		[TestMethod]
		public void Test_152_AddRange_NullArray_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.AddRange((int[]?)null));
		}

		[TestMethod]
		public void Test_153_InsertRange_NullEnumerable_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.InsertRange(0, null));
		}
		[TestMethod]
		public void Test_154_AddRange_ArraySegmentAcrossChunks_DoesNotCopyPastRequestedCount()
		{
			ChunkedList<int> List = new(4)
			{
				10
			};

			int[] Source = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

			List.AddRange(Source, 2, 5);

			AssertListByIndexer(List, [10, 2, 3, 4, 5, 6]);
		}

		[TestMethod]
		public void Test_155_AddRange_ArraySegmentEndingAtArrayEnd_DoesNotThrowOrOverCopy()
		{
			ChunkedList<int> List = new(4)
			{
				10
			};

			int[] Source = [0, 1, 2, 3, 4, 5, 6];

			List.AddRange(Source, 2, 5);

			AssertListByIndexer(List, [10, 2, 3, 4, 5, 6]);
		}

		[TestMethod]
		public void Test_156_AddRange_ChunkedListSourceWithStartOffsetAndFullDestination_DoesNotOverCopy()
		{
			ChunkedList<int> Source = new(4)
			{
				1,
				2,
				3,
				4
			};

			Source.RemoveFirst();
			Source.RemoveFirst();

			ChunkedList<int> Destination = new(4)
			{
				10,
				11,
				12,
				13
			};

			Destination.AddRange(Source);

			AssertListByIndexer(Destination, [10, 11, 12, 13, 3, 4]);
		}

		[TestMethod]
		public void Test_157_AddRange_NullChunkedList_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.AddRange((ChunkedList<int>?)null));
		}

		[TestMethod]
		public void Test_158_AddRangeFirst_NullArray_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.AddRangeFirst((int[]?)null));
		}

		[TestMethod]
		public void Test_159_AddRangeFirst_NullEnumerable_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.AddRangeFirst((IEnumerable<int>?)null));
		}

		[TestMethod]
		public void Test_160_AddRangeFirst_EmptyArray_DoesNotChangeList()
		{
			ChunkedList<int> List = new(4)
			{
				1,
				2,
				3
			};

			List.AddRangeFirst([]);

			AssertListByIndexer(List, [1, 2, 3]);
		}

		[TestMethod]
		public void Test_161_SortComparison_NullComparisonOnEmptyList_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.Sort((Comparison<int>?)null));
		}

		[TestMethod]
		public void Test_162_SortComparison_NullComparisonOnSingleItemList_ThrowsArgumentNullException()
		{
			ChunkedList<int> List =
			[
				1
			];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.Sort((Comparison<int>?)null));
		}

		[TestMethod]
		public void Test_163_Constructor_NullInitialElements_ThrowsArgumentNullException()
		{
			Assert.ThrowsException<ArgumentNullException>(() =>
				_ = new ChunkedList<int>((int[]?)null));
		}

		[TestMethod]
		public void Test_164_Constructor_ArrayInput_IsCopiedNotAliased()
		{
			int[] Source = [1, 2, 3];

			ChunkedList<int> List = [.. Source];

			Source[1] = 99;

			Assert.AreEqual(3, List.Count);
			Assert.AreEqual(1, List[0]);
			Assert.AreEqual(2, List[1]);
			Assert.AreEqual(3, List[2]);
		}

		[TestMethod]
		public void Test_165_LastIndexOf_EmptyList_IndexMinusOne_ReturnsMinusOne()
		{
			ChunkedList<int> List = [];

			Assert.AreEqual(-1, List.LastIndexOf(123, -1));
		}

		[TestMethod]
		public void Test_166_LastIndexOf_EmptyList_IndexMinusOneCountZero_ReturnsMinusOne()
		{
			ChunkedList<int> List = [];

			Assert.AreEqual(-1, List.LastIndexOf(123, -1, 0));
		}

		[TestMethod]
		public void Test_167_SortComparer_NullComparer_UsesDefaultComparer()
		{
			ChunkedList<int> List = new(4)
			{
				3,
				1,
				2
			};

			List.Sort((IComparer<int>?)null);

			AssertListByIndexer(List, [1, 2, 3]);
		}

		[TestMethod]
		public void Test_168_SortComparer_NullComparer_MultipleChunks_UsesDefaultComparer()
		{
			ChunkedList<int> List = new(4);

			for (int i = 9; i >= 0; i--)
				List.Add(i);

			List.Sort((IComparer<int>?)null);

			AssertListByIndexer(List, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
		}

		[TestMethod]
		public void Test_169_AddRangeFirst_NonCollectionEnumerable_DisposesEnumerator()
		{
			ChunkedList<int> List = [];
			DisposableEnumerable Source = new(1, 2, 3);

			List.AddRangeFirst(Source);

			Assert.IsTrue(Source.Disposed);
			AssertListByIndexer(List, [1, 2, 3]);
		}

		[TestMethod]
		public void Test_170_ForEach_NullCallback_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [1, 2, 3];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.ForEach(null));
		}

		[TestMethod]
		public void Test_171_ForEachChunk_NullCallback_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [1, 2, 3];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.ForEachChunk(null));
		}

		[TestMethod]
		public void Test_172_Update_NullCallback_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [1, 2, 3];

			Assert.ThrowsException<ArgumentNullException>(() =>
				List.Update(null));
		}

		[TestMethod]
		public async Task Test_173_ForEachAsync_NullCallback_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [1, 2, 3];

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
				await List.ForEachAsync(null));
		}

		[TestMethod]
		public async Task Test_174_ForEachChunkAsync_NullCallback_ThrowsArgumentNullException()
		{
			ChunkedList<int> List = [1, 2, 3];

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
				await List.ForEachChunkAsync(null));
		}
		private sealed class ParserNode
		{
			public ParserNode(int Id)
			{
				this.Id = Id;
				this.Children = [];
			}

			public int Id { get; }

			public ParserNode[] Children { get; set; }

			public override string ToString()
			{
				return this.Id.ToString();
			}
		}

		private static ChunkedList<int> CreateList(int Count, int ChunkSize)
		{
			ChunkedList<int> List = new(ChunkSize);

			for (int i = 0; i < Count; i++)
				List.Add(i);

			return List;
		}

		private static void AssertListByIndexer(ChunkedList<int> List, params int[] Expected)
		{
			Assert.AreEqual(Expected.Length, List.Count, "Unexpected list count.");

			for (int i = 0; i < Expected.Length; i++)
				Assert.AreEqual(Expected[i], List[i], "Unexpected value at index " + i.ToString() + ".");
		}

		private static void AssertNodeListByIndexer(ChunkedList<ParserNode> List, params int[] ExpectedIds)
		{
			Assert.AreEqual(ExpectedIds.Length, List.Count, "Unexpected list count.");

			for (int i = 0; i < ExpectedIds.Length; i++)
				Assert.AreEqual(ExpectedIds[i], List[i].Id, "Unexpected node at index " + i.ToString() + ".");
		}

		private static void AssertCountEqualsChunkElementCount<T>(ChunkedList<T> List)
		{
			int ChunkElementCount = 0;

			List.ForEachChunk((Chunk, Offset, Count) =>
			{
				ChunkElementCount += Count;
				return true;
			});

			Assert.AreEqual(List.Count, ChunkElementCount, "ChunkedList.Count does not match the number of elements reachable through the chunk chain.");
		}

		private static void AssertAllCountedIndexesCanBeRead<T>(ChunkedList<T> List)
		{
			for (int i = 0; i < List.Count; i++)
			{
				try
				{
					_ = List[i];
				}
				catch (ArgumentOutOfRangeException ex)
				{
					Assert.Fail("Indexer threw " + ex.GetType().Name + " for index " + i.ToString() + " even though Count is " + List.Count.ToString() + ". " + ex.Message);
				}
			}
		}

		private static int CountChunks(ChunkedList<int> List)
		{
			int Result = 0;

			List.ForEachChunk((Chunk, Offset, Count) =>
			{
				Result++;
				Assert.IsTrue(Count > 0, "ForEachChunk should only report non-empty chunks.");
				return true;
			});

			return Result;
		}

	}
}
