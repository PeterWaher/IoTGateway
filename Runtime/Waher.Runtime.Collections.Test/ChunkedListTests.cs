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
			foreach (var item in List)
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
			foreach (var item in List)
			{
				Assert.AreEqual(expectedValue, item);
				expectedValue++;
			}

			Assert.AreEqual(5000, List.Count);
			Assert.AreEqual(-4999, List.FirstItem);
			Assert.AreEqual(0, List.LastItem);
		}
	}
}
