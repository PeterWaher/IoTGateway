namespace Waher.Runtime.Collections.Test
{
	[TestClass]
	public class ChunkedListTests
	{
		[TestMethod]
		public void Test_01_DefaultConstructor()
		{
			ChunkedList<int> List = [];
			Assert.AreEqual(0, List.Count);
			Assert.IsFalse(List.IsReadOnly);
		}

		[TestMethod]
		public void Test_02_ConstructorWithInitialChunkSize()
		{
			ChunkedList<int> List = new(32);
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		public void Test_03_ConstructorWithInitialAndMaxChunkSize()
		{
			ChunkedList<int> List = new(32, 64);
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_04_ConstructorWithInvalidInitialChunkSize()
		{
			ChunkedList<int> List = new(0);
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_05_ConstructorWithMaxChunkSizeLessThanInitialChunkSize()
		{
			ChunkedList<int> List = new(64, 32);
			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		public void Test_06_AddSingleItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			Assert.AreEqual(1, List.Count);
			Assert.IsTrue(List.Contains(1));
		}

		[TestMethod]
		public void Test_07_AddMultipleItemsWithinInitialChunkSize()
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
		public void Test_08_AddItemsExceedingInitialChunkSize()
		{
			ChunkedList<int> List = new(4, 8);
			for (int i = 0; i < 5; i++)
				List.Add(i);
			Assert.AreEqual(5, List.Count);
		}

		[TestMethod]
		public void Test_09_RemoveExistingItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			Assert.IsTrue(List.Remove(1));
			Assert.AreEqual(1, List.Count);
			Assert.IsFalse(List.Contains(1));
		}

		[TestMethod]
		public void Test_10_RemoveNonExistingItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			Assert.IsFalse(List.Remove(2));
		}

		[TestMethod]
		public void Test_11_ContainsItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			Assert.IsTrue(List.Contains(1));
			Assert.IsFalse(List.Contains(2));
		}

		[TestMethod]
		public void Test_12_Clear()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Clear();
			Assert.AreEqual(0, List.Count);
			Assert.IsFalse(List.Contains(1));
		}

		[TestMethod]
		public void Test_13_CopyTo()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			var array = new int[2];
			List.CopyTo(array, 0);
			Assert.AreEqual(1, array[0]);
			Assert.AreEqual(2, array[1]);
		}

		[TestMethod]
		public void Test_14_Enumerator()
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
		public void Test_15_HasLastItem()
		{
			ChunkedList<int> List = [];
			Assert.IsFalse(List.HasLastItem);

			List.Add(1);
			Assert.IsTrue(List.HasLastItem);
		}

		[TestMethod]
		public void Test_16_LastItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			Assert.AreEqual(2, List.LastItem);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_17_LastItem_EmptyList()
		{
			ChunkedList<int> List = [];
			_ = List.LastItem; // Should throw InvalidOperationException
		}

		[TestMethod]
		public void Test_18_HasFirstItem()
		{
			ChunkedList<int> List = [];
			Assert.IsFalse(List.HasFirstItem);

			List.Add(1);
			Assert.IsTrue(List.HasFirstItem);
		}

		[TestMethod]
		public void Test_19_FirstItem()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Add(2);
			Assert.AreEqual(1, List.FirstItem);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_20_FirstItem_EmptyList()
		{
			ChunkedList<int> List = [];
			_ = List.FirstItem; // Should throw InvalidOperationException
		}

		[TestMethod]
		public void Test_21_AddFirstItem()
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
		public void Test_22_AddLastItem()
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
		public void Test_23_RemoveFirst()
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
		public void Test_24_RemoveFirst_EmptyList()
		{
			ChunkedList<int> List = [];
			List.RemoveFirst(); // Should throw InvalidOperationException
		}

		[TestMethod]
		public void Test_25_RemoveLast()
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
		public void Test_26_RemoveLast_EmptyList()
		{
			ChunkedList<int> List = [];
			List.RemoveLast(); // Should throw InvalidOperationException
		}

		[TestMethod]
		public void Test_27_AddRemoveManyElements()
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
		public void Test_28_AddRemoveAndEnumerate()
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
		public void Test_29_Indexer_Get()
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
		public void Test_30_Indexer_Get_OutOfRange()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			_ = List[1]; // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_31_Indexer_Set()
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
		public void Test_32_Indexer_Set_OutOfRange()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List[1] = 5; // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_33_IndexOf()
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
		public void Test_34_Insert()
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
		public void Test_35_Insert_OutOfRange()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.Insert(2, 2); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_36_RemoveAt()
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
		public void Test_37_RemoveAt_OutOfRange()
		{
			ChunkedList<int> List = [];
			List.Add(1);
			List.RemoveAt(1); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_38_Insert_ChunkWithStartGreaterThanZero()
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
		public void Test_39_Insert_ChunkNotFull()
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
		public void Test_40_Insert_ChunkFull_0()
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
		public void Test_41_Insert_ChunkFull_1()
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
		public void Test_42_Insert_ChunkFull_2()
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
		public void Test_43_Insert_ChunkFull_3()
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
		public void Test_44_Insert_ChunkFull_4()
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
		public void Test_45_IndexOf_Item_Index()
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
		public void Test_46_IndexOf_Item_Index_OutOfRange()
		{
			ChunkedList<int> List = [1, 2];
			List.IndexOf(2, 3); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_47_IndexOf_Item_Index_Count()
		{
			ChunkedList<int> List = [1, 2, 3, 2, 4];

			Assert.AreEqual(1, List.IndexOf(2, 0, 5)); // First occurrence of 2
			Assert.AreEqual(3, List.IndexOf(2, 2, 3)); // Second occurrence of 2
			Assert.AreEqual(-1, List.IndexOf(2, 2, 1)); // No occurrence of 2 in the specified Range
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_48_IndexOf_Item_Index_Count_OutOfRange()
		{
			ChunkedList<int> List = [1, 2];
			List.IndexOf(2, 0, 3); // Should throw ArgumentOutOfRangeException
		}
		[TestMethod]
		public void Test_49_LastIndexOf_Item()
		{
			ChunkedList<int> List = [1, 2, 3, 2, 4];

			Assert.AreEqual(3, List.LastIndexOf(2)); // Last occurrence of 2
			Assert.AreEqual(4, List.LastIndexOf(4)); // Last occurrence of 4
			Assert.AreEqual(-1, List.LastIndexOf(5)); // No occurrence of 5
		}

		[TestMethod]
		public void Test_50_LastIndexOf_Item_Index()
		{
			ChunkedList<int> List = [1, 2, 3, 2, 4];

			Assert.AreEqual(3, List.LastIndexOf(2, 4)); // Last occurrence of 2 starting from index 4
			Assert.AreEqual(1, List.LastIndexOf(2, 2)); // Last occurrence of 2 starting from index 2
			Assert.AreEqual(-1, List.LastIndexOf(2, 0)); // No occurrence of 2 starting from index 0
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_51_LastIndexOf_Item_Index_OutOfRange()
		{
			ChunkedList<int> List = [1, 2];
			List.LastIndexOf(2, 3); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_52_LastIndexOf_Item_Index_Count()
		{
			ChunkedList<int> List = [1, 2, 3, 2, 4];

			Assert.AreEqual(3, List.LastIndexOf(2, 4, 5)); // Last occurrence of 2 in the Range
			Assert.AreEqual(1, List.LastIndexOf(2, 2, 3)); // Last occurrence of 2 in the Range
			Assert.AreEqual(-1, List.LastIndexOf(2, 2, 1)); // No occurrence of 2 in the specified Range
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_53_LastIndexOf_Item_Index_Count_OutOfRange()
		{
			ChunkedList<int> List = [1, 2];
			List.LastIndexOf(2, 0, 3); // Should throw ArgumentOutOfRangeException
		}

		[TestMethod]
		public void Test_54_RemoveAt_ReduceChunks()
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
		public void Test_55_RemoveAt_ReduceChunks_Middle()
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
		public void Test_56_RemoveAt_ReduceChunks_End()
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
		public void Test_57_AddRange_WithinSingleChunk()
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
		public void Test_58_AddRange_CrossChunkLimits()
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
		public void Test_59_AddRange_MultipleChunks()
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
		public void Test_60_AddRange_EmptyCollection()
		{
			ChunkedList<int> List = new(4);
			List<int> Range = [];

			List.AddRange(Range);

			Assert.AreEqual(0, List.Count);
		}

		[TestMethod]
		public void Test_61_AddRange_ExistingElements()
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
	}
}
