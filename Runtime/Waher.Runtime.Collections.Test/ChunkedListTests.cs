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
	}
}
