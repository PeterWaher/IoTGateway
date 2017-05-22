using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.Serialization;
using Waher.Persistence.Filters;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;

#if !LW
using Waher.Persistence.Files.Test.Classes;

namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
using Waher.Persistence.FilesLW.Test.Classes;

namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public abstract class IndexTests
	{
		internal const string Index1FileName = "Data\\Default.btree.50104c1cdc9b0754886b272fc1aaa550747dadf4.index";
		internal const string Index2FileName = "Data\\Default.btree.40059d366b589d4071aba631a3aa4fc1dc03e357.index";
		internal const string ObjFileName = "Data\\LastObject.bin";
		internal const string ObjIdFileName = "Data\\LastObjectId.bin";
		internal const string BlockSizeFileName = "Data\\BlockSize.bin";
		internal const int BlocksInCache = 10000;
		internal const int ObjectsToEnumerate = 10000;

		protected ObjectBTreeFile file;
		protected IndexBTreeFile index1;
		protected IndexBTreeFile index2;
		protected FilesProvider provider;
		protected Random gen = new Random();
		protected DateTime start;

		public abstract int BlockSize
		{
			get;
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			if (File.Exists(BTreeTests.MasterFileName + ".bak"))
				File.Delete(BTreeTests.MasterFileName + ".bak");

			if (File.Exists(BTreeTests.MasterFileName))
			{
				File.Copy(BTreeTests.MasterFileName, BTreeTests.MasterFileName + ".bak");
				File.Delete(BTreeTests.MasterFileName);
			}

			if (File.Exists(BTreeTests.FileName + ".bak"))
				File.Delete(BTreeTests.FileName + ".bak");

			if (File.Exists(BTreeTests.FileName))
			{
				File.Copy(BTreeTests.FileName, BTreeTests.FileName + ".bak");
				File.Delete(BTreeTests.FileName);
			}

			if (File.Exists(BTreeTests.BlobFileName + ".bak"))
				File.Delete(BTreeTests.BlobFileName + ".bak");

			if (File.Exists(BTreeTests.BlobFileName))
			{
				File.Copy(BTreeTests.BlobFileName, BTreeTests.BlobFileName + ".bak");
				File.Delete(BTreeTests.BlobFileName);
			}

			if (File.Exists(BTreeTests.NamesFileName + ".bak"))
				File.Delete(BTreeTests.NamesFileName + ".bak");

			if (File.Exists(BTreeTests.NamesFileName))
			{
				File.Copy(BTreeTests.NamesFileName, BTreeTests.NamesFileName + ".bak");
				File.Delete(BTreeTests.NamesFileName);
			}

			if (File.Exists(Index1FileName + ".bak"))
				File.Delete(Index1FileName + ".bak");

			if (File.Exists(Index1FileName))
			{
				File.Copy(Index1FileName, Index1FileName + ".bak");
				File.Delete(Index1FileName);
			}

			if (File.Exists(Index2FileName + ".bak"))
				File.Delete(Index2FileName + ".bak");

			if (File.Exists(Index2FileName))
			{
				File.Copy(Index2FileName, Index2FileName + ".bak");
				File.Delete(Index2FileName);
			}

			this.provider = new FilesProvider(BTreeTests.Folder, BTreeTests.CollectionName, BlockSize, BlocksInCache, Math.Max(BlockSize / 2, 1024), Encoding.UTF8, 10000, true);
			this.file = await this.provider.GetFile(BTreeTests.CollectionName);

			this.index1 = await this.provider.GetIndexFile(this.file, RegenerationOptions.DontRegenerate, "Byte", "-DateTime");
			this.index2 = await this.provider.GetIndexFile(this.file, RegenerationOptions.DontRegenerate, "ShortString");

			this.start = DateTime.Now;
		}

		[TestCleanup]
		public void TestCleanup()
		{
			Console.Out.WriteLine("Elapsed time: " + (DateTime.Now - this.start).ToString());

			if (this.provider != null)
			{
				this.provider.Dispose();
				this.provider = null;
				this.file = null;
			}
		}

		public virtual int MaxStringLength
		{
			get { return 100; }
		}

		[TestMethod]
		public async Task Test_01_NormalEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			SortedDictionary<Guid, bool> Objects2 = new SortedDictionary<Guid, bool>();
			GenericObject Prev = null;

			foreach (GenericObject Obj in this.index1)
			{
				Objects2[Obj.ObjectId] = true;

				if (Prev != null)
					AssertEx.Less(this.Index1Compare(Prev, Obj), 0);

				Prev = Obj;
				Assert.IsTrue(Objects.Remove(Obj.ObjectId));
			}

			Assert.AreEqual(0, Objects.Count);

			Prev = null;
			foreach (GenericObject Obj in this.index2)
			{
				if (Prev != null)
					AssertEx.Less(this.Index2Compare(Prev, Obj), 0);

				Prev = Obj;
				Assert.IsTrue(Objects2.Remove(Obj.ObjectId));
			}

			Assert.AreEqual(0, Objects2.Count);
		}

		private int Index1Compare(GenericObject Obj1, GenericObject Obj2)
		{
			int i = ((IComparable)Obj1["Byte"]).CompareTo(Obj2["Byte"]);
			if (i != 0)
				return i;

			i = ((IComparable)Obj1["DateTime"]).CompareTo(Obj2["DateTime"]);
			if (i != 0)
				return -i;

			return 0;
		}

		private int Index2Compare(GenericObject Obj1, GenericObject Obj2)
		{
			return ((IComparable)Obj1["ShortString"]).CompareTo(Obj2["ShortString"]);
		}

		[TestMethod]
		public async Task Test_02_TypedEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			SortedDictionary<Guid, bool> Objects2 = new SortedDictionary<Guid, bool>();
			Simple Prev = null;
			Simple Obj;
			ulong Rank = 0;

			using (IndexBTreeFileEnumerator<Simple> e = this.index1.GetTypedEnumerator<Simple>(false))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					Objects2[Obj.ObjectId] = true;

					if (Prev != null)
						AssertEx.Less(this.Index1Compare(Prev, Obj), 0);

					Prev = Obj;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					Assert.AreEqual(Rank++, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects.Count);

			Prev = null;
			Rank = 0;
			using (IndexBTreeFileEnumerator<Simple> e = this.index2.GetTypedEnumerator<Simple>(false))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					if (Prev != null)
						AssertEx.Less(this.Index2Compare(Prev, Obj), 0);

					Prev = Obj;
					Assert.IsTrue(Objects2.Remove(Obj.ObjectId));

					Assert.AreEqual(Rank++, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects2.Count);
		}

		private int Index1Compare(Simple Obj1, Simple Obj2)
		{
			int i = Obj1.Byte.CompareTo(Obj2.Byte);
			if (i != 0)
				return i;

			i = Obj1.DateTime.CompareTo(Obj2.DateTime);
			if (i != 0)
				return -i;

			return 0;
		}

		private int Index2Compare(Simple Obj1, Simple Obj2)
		{
			return Obj1.ShortString.CompareTo(Obj2.ShortString);
		}

		[TestMethod]
		public async Task Test_03_LockedEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			SortedDictionary<Guid, bool> Objects2 = new SortedDictionary<Guid, bool>();
			Simple Prev = null;
			Simple Obj;
			ulong Rank = 0;

			using (IndexBTreeFileEnumerator<Simple> e = this.index1.GetTypedEnumerator<Simple>(true))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					Objects2[Obj.ObjectId] = true;

					if (Prev != null)
						AssertEx.Less(this.Index1Compare(Prev, Obj), 0);

					Prev = Obj;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					Assert.AreEqual(Rank++, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects.Count);

			Prev = null;
			Rank = 0;
			using (IndexBTreeFileEnumerator<Simple> e = this.index2.GetTypedEnumerator<Simple>(true))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					if (Prev != null)
						AssertEx.Less(this.Index2Compare(Prev, Obj), 0);

					Prev = Obj;
					Assert.IsTrue(Objects2.Remove(Obj.ObjectId));

					Assert.AreEqual(Rank++, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects2.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public async Task Test_04_UnlockedChangeEnumeration()
		{
			await this.CreateObjects(Math.Min(ObjectsToEnumerate, 1000));
			Simple Obj;

			using (IndexBTreeFileEnumerator<Simple> e = this.index1.GetTypedEnumerator<Simple>(true))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					Obj = BTreeTests.CreateSimple(this.MaxStringLength);
					await this.file.SaveNewObject(Obj);
				}
			}
		}

		[TestMethod]
		public async Task Test_05_BackwardsEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			SortedDictionary<Guid, bool> Objects2 = new SortedDictionary<Guid, bool>();
			Simple Prev = null;
			Simple Obj;
			ulong Rank = ObjectsToEnumerate;

			using (IndexBTreeFileEnumerator<Simple> e = this.index1.GetTypedEnumerator<Simple>(true))
			{
				while (e.MovePrevious())
				{
					Obj = e.Current;
					Objects2[Obj.ObjectId] = true;
					if (Prev != null)
						AssertEx.Greater(this.Index1Compare(Prev, Obj), 0);

					Prev = Obj;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					Assert.AreEqual(--Rank, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects.Count);

			Prev = null;
			Rank = ObjectsToEnumerate;
			using (IndexBTreeFileEnumerator<Simple> e = this.index2.GetTypedEnumerator<Simple>(true))
			{
				while (e.MovePrevious())
				{
					Obj = e.Current;
					if (Prev != null)
						AssertEx.Greater(this.Index2Compare(Prev, Obj), 0);

					Prev = Obj;
					Assert.IsTrue(Objects2.Remove(Obj.ObjectId));

					Assert.AreEqual(--Rank, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects2.Count);
		}

		private async Task<SortedDictionary<Guid, Simple>> CreateObjects(int NrObjects)
		{
			SortedDictionary<Guid, Simple> Result = new SortedDictionary<Guid, Simple>();

			while (NrObjects > 0)
			{
				Simple Obj = BTreeTests.CreateSimple(this.MaxStringLength);
				Guid ObjectId = await this.file.SaveNewObject(Obj);
				Result[ObjectId] = Obj;
				NrObjects--;
			}

			await BTreeTests.AssertConsistent(this.file, this.provider, null, null, true);

			return Result;
		}

		[TestMethod]
		public async Task Test_06_SelectIthObject()
		{
			int c = ObjectsToEnumerate;
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(c);
			SortedDictionary<string, Simple> ObjectsSorted = this.SortObjects(Objects);
			Simple[] Ordered = new Simple[c];
			ObjectsSorted.Values.CopyTo(Ordered, 0);
			Simple Prev = null;
			Simple Obj;
			Random gen = new Random();
			int i, j;

			for (i = 0; i < c; i++)
			{
				j = 0;
				Prev = null;

				if (i < 10 || (gen.Next(0, 2) == 0 && i <= c - 10))
				{
					using (IndexBTreeFileEnumerator<Simple> e = this.index1.GetTypedEnumerator<Simple>(true))
					{
						Assert.IsTrue(await e.GoToObject((uint)i));

						do
						{
							Obj = e.Current;
							if (Prev != null)
								AssertEx.Less(this.Index1Compare(Prev, Obj), 0);

							Prev = Obj;
							ObjectSerializationTests.AssertEqual(Ordered[i + j], Obj);

							Assert.AreEqual(i + j, e.CurrentRank);
							Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
						}
						while (e.MoveNext() && j++ < 10);
					}
				}
				else
				{
					using (IndexBTreeFileEnumerator<Simple> e = this.index1.GetTypedEnumerator<Simple>(true))
					{
						Assert.IsTrue(await e.GoToObject((uint)i));

						do
						{
							Obj = e.Current;
							if (Prev != null)
								AssertEx.Greater(this.Index1Compare(Prev, Obj), 0);

							Prev = Obj;
							ObjectSerializationTests.AssertEqual(Ordered[i - j], Obj);

							Assert.AreEqual(i - j, e.CurrentRank);
							Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
						}
						while (e.MovePrevious() && j++ < 10);
					}
				}
			}
		}

		private SortedDictionary<string, Simple> SortObjects(SortedDictionary<Guid, Simple> Objects)
		{
			SortedDictionary<string, Simple> ObjectsSorted = new SortedDictionary<string, Simple>();

			foreach (Simple Obj in Objects.Values)
				ObjectsSorted[Obj.Byte.ToString("D3") + " " + (long.MaxValue - Obj.DateTime.Ticks).ToString()] = Obj;

			return ObjectsSorted;
		}

		[TestMethod]
		public async Task Test_07_RankObject()
		{
			int c = ObjectsToEnumerate;
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(c);
			SortedDictionary<string, Simple> ObjectsSorted = this.SortObjects(Objects);
			Simple[] Ordered = new Simple[c];
			ObjectsSorted.Values.CopyTo(Ordered, 0);
			int i;

			for (i = 0; i < c; i++)
				Assert.AreEqual(i, await this.index1.GetRank(Ordered[i].ObjectId));
		}

		[TestMethod]
		public async Task Test_08_Reset()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Prev = null;
			Simple Obj;
			ulong Rank = 0;

			using (IndexBTreeFileEnumerator<Simple> e = this.index1.GetTypedEnumerator<Simple>(false))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					if (Prev != null)
						AssertEx.Less(this.Index1Compare(Prev, Obj), 0);

					Prev = Obj;
					Assert.IsTrue(Objects.ContainsKey(Obj.ObjectId));

					Assert.AreEqual(Rank++, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}

				e.Reset();
				Prev = null;

				while (e.MovePrevious())
				{
					Obj = e.Current;
					if (Prev != null)
						AssertEx.Greater(this.Index1Compare(Prev, Obj), 0);

					Prev = Obj;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					Assert.AreEqual(--Rank, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects.Count);
		}

		[TestMethod]
		public void Test_09_UpdateObjects_1000()
		{
			this.Test_UpdateObjects(1000).Wait();
		}

		[TestMethod]
		public void Test_10_UpdateObjects_10000()
		{
			this.Test_UpdateObjects(10000).Wait();
		}

		[TestMethod]
		public void Test_11_UpdateObjects_100000()
		{
			Assert.Inconclusive();
			this.Test_UpdateObjects(100000).Wait();
		}

		private async Task Test_UpdateObjects(int c)
		{
			Dictionary<Guid, Simple> Ordered = new Dictionary<Guid, Simple>();
			Simple[] Objects = new Simple[c];
			Simple Obj;
			int i;

			for (i = 0; i < c; i++)
			{
				Objects[i] = Obj = BTreeTests.CreateSimple(this.MaxStringLength);
				await this.file.SaveNewObject(Obj);
				Ordered[Obj.ObjectId] = Obj;
			}

			await BTreeTests.AssertConsistent(this.file, this.provider, null, null, true);

			for (i = 0; i < c; i++)
			{
				Obj = BTreeTests.CreateSimple(this.MaxStringLength);
				Obj.ObjectId = Objects[i].ObjectId;

				await this.file.UpdateObject(Obj);

				Objects[i] = Obj;

				Obj = await this.file.LoadObject<Simple>(Obj.ObjectId);
				ObjectSerializationTests.AssertEqual(Objects[i], Obj);
			}

			await BTreeTests.AssertConsistent(this.file, this.provider, null, null, true);

			for (i = 0; i < c; i++)
			{
				Obj = await this.file.LoadObject<Simple>(Objects[i].ObjectId);
				ObjectSerializationTests.AssertEqual(Objects[i], Obj);
			}

			using (IndexBTreeFileEnumerator<Simple> e = this.index1.GetTypedEnumerator<Simple>(true))
			{
				Obj = null;

				while (e.MoveNext())
				{
					Assert.AreEqual(e.Current.ObjectId, e.CurrentObjectId);
					Assert.IsTrue(Ordered.Remove(e.Current.ObjectId));

					if (Obj != null)
						AssertEx.Less(this.Index1Compare(Obj, e.Current), 0);

					Obj = e.Current;
				}
			}

			Assert.AreEqual(0, Ordered.Count);
		}

		[TestMethod]
		public async Task Test_12_DeleteObject()
		{
			await this.Test_DeleteObjects(3);
		}

		[TestMethod]
		public async Task Test_13_DeleteObject_100()
		{
			await this.Test_DeleteObjects(100);
		}

		[TestMethod]
		public async Task Test_14_DeleteObject_1000()
		{
			await this.Test_DeleteObjects(1000);
		}

		[TestMethod]
		public async Task Test_15_DeleteObject_10000()
		{
			await this.Test_DeleteObjects(10000);
		}

		[TestMethod]
		public async Task Test_16_DeleteObject_100000()
		{
			Assert.Inconclusive();
			await this.Test_DeleteObjects(100000);
		}

		private async Task Test_DeleteObjects(int c)
		{
			Random Gen = new Random();
			Simple[] Objects = new Simple[c];
			Simple Obj;
			int i;

			for (i = 0; i < c; i++)
			{
				Objects[i] = Obj = BTreeTests.CreateSimple(this.MaxStringLength);
				await this.file.SaveNewObject(Obj);
			}

			while (c > 0)
			{
				i = Gen.Next(0, c);

				Obj = Objects[i];
				c--;
				if (i < c)
					Array.Copy(Objects, i + 1, Objects, i, c - i);

				await this.file.DeleteObject(Obj);
			}

			FileStatistics Stat = await BTreeTests.AssertConsistent(this.file, this.provider, null, null, true);

			Assert.AreEqual(0, this.file.Count);
			Assert.AreEqual(0, this.index1.IndexFile.Count);
			Assert.AreEqual(1, Stat.NrBlocks);
			Assert.AreEqual(0, Stat.NrBlobBlocks);
		}

		[TestMethod]
		public async Task Test_17_Clear()
		{
			Simple Obj = BTreeTests.CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);
			Assert.IsTrue(this.file.Contains(Obj));
			this.file.Clear();
			Assert.IsFalse(this.file.Contains(Obj));
			Assert.AreEqual(0, this.file.Count);
			Assert.AreEqual(0, this.index1.IndexFile.Count);
		}

		[TestMethod]
		public async Task Test_18_FindFirst()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			int i;

			for (i = 0; i < 256; i++)
			{
				using (IndexBTreeFileEnumerator<Simple> e = await this.index1.FindFirstGreaterOrEqualTo<Simple>(true,
					new KeyValuePair<string, object>("Byte", i)))
				{
					while (e.MoveNext())
					{
						AssertEx.GreaterOrEqual(e.Current.Byte, i);
						if (e.Current.Byte > i)
							break;
					}

					e.Reset();

					while (e.MovePrevious())
					{
						AssertEx.Less(e.Current.Byte, i);
						if (e.Current.Byte < i - 1)
							break;
					}
				}
			}
		}

		[TestMethod]
		public async Task Test_19_FindLast()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			int i;

			for (i = 0; i < 256; i++)
			{
				using (IndexBTreeFileEnumerator<Simple> e = await this.index1.FindLastLesserOrEqualTo<Simple>(true,
					new KeyValuePair<string, object>("Byte", i)))
				{
					while (e.MovePrevious())
					{
						AssertEx.LessOrEqual(e.Current.Byte, i);
						if (e.Current.Byte < i)
							break;
					}

					e.Reset();

					while (e.MoveNext())
					{
						AssertEx.Greater(e.Current.Byte, i);
						if (e.Current.Byte > i + 1)
							break;
					}
				}
			}
		}

		[TestMethod]
		public async Task Test_20_Search_FilterEqualTo()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterFieldEqualTo("Byte", 100), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.AreEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.AreNotEqual(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_21_Search_FilterNotEqualTo()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterFieldNotEqualTo("Byte", 100), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.AreNotEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.AreEqual(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_22_Search_FilterGreaterThanOrEqualTo()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterFieldGreaterOrEqualTo("Byte", 100), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.GreaterOrEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					AssertEx.Less(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_23_Search_FilterLesserThanOrEqualTo()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterFieldLesserOrEqualTo("Byte", 100), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.LessOrEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					AssertEx.Greater(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_24_Search_FilterGreaterThan()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterFieldGreaterThan("Byte", 100), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Greater(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					AssertEx.LessOrEqual(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_25_Search_FilterLesserThan()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterFieldLesserThan("Byte", 100), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Less(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					AssertEx.GreaterOrEqual(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_26_Search_FilterLikeRegEx()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterFieldLikeRegEx("ShortString", "A.*B.*"), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.IsTrue(Obj.ShortString.StartsWith("A"));
					Assert.IsTrue(Obj.ShortString.Contains("B"));
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.IsFalse(Obj2.ShortString.StartsWith("A") && Obj2.ShortString.Contains("B"));
			}
		}

		[TestMethod]
		public async Task Test_27_Search_FilterLikeRegEx_2()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterFieldLikeRegEx("ShortString", "[AB].*"), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.IsTrue(Obj.ShortString.StartsWith("A") || Obj.ShortString.StartsWith("B"));
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.IsFalse(Obj2.ShortString.StartsWith("A") || Obj2.ShortString.StartsWith("B"));
			}
		}

		[TestMethod]
		public async Task Test_28_Search_FilterNot_EqualTo()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterNot(new FilterFieldEqualTo("Byte", 100)), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.AreNotEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.AreEqual(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_29_Search_FilterNot_NotEqualTo()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterNot(new FilterFieldNotEqualTo("Byte", 100)), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.AreEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.AreNotEqual(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_30_Search_FilterNot_GreaterThanOrEqualTo()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterNot(new FilterFieldGreaterOrEqualTo("Byte", 100)), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Less(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					AssertEx.GreaterOrEqual(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_31_Search_FilterNot_LesserThanOrEqualTo()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterNot(new FilterFieldLesserOrEqualTo("Byte", 100)), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.GreaterOrEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					AssertEx.LessOrEqual(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_32_Search_FilterNot_GreaterThan()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterNot(new FilterFieldGreaterThan("Byte", 100)), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.LessOrEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					AssertEx.Greater(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_33_Search_FilterNot_LesserThan()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterNot(new FilterFieldLesserThan("Byte", 100)), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.GreaterOrEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					AssertEx.Less(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_34_Search_FilterNot_LikeRegEx()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterNot(new FilterFieldLikeRegEx("ShortString", "A.*B.*")), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.IsFalse(Obj.ShortString.StartsWith("A") && Obj.ShortString.Contains("B"));
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.IsTrue(Obj2.ShortString.StartsWith("A") && Obj2.ShortString.Contains("B"));
			}
		}

		[TestMethod]
		public async Task Test_35_Search_FilterNot_LikeRegEx_2()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterNot(new FilterFieldLikeRegEx("ShortString", "[AB].*")), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.IsFalse(Obj.ShortString.StartsWith("A") || Obj.ShortString.StartsWith("B"));
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.IsTrue(Obj2.ShortString.StartsWith("A") || Obj2.ShortString.StartsWith("B"));
			}
		}

		[TestMethod]
		public async Task Test_36_Search_FilterNot_Not_EqualTo()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterNot(new FilterNot(new FilterFieldEqualTo("Byte", 100))), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.AreEqual(Obj.Byte, 100);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.AreNotEqual(Obj2.Byte, 100);
			}
		}

		[TestMethod]
		public async Task Test_37_Search_FilterOr()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterOr(
				new FilterFieldEqualTo("Byte", 100),
				new FilterFieldEqualTo("Byte", 200),
				new FilterFieldLikeRegEx("ShortString", "A.*")), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.IsTrue(Obj.Byte == 100 || Obj.Byte == 200 || Obj.ShortString.StartsWith("A"));
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.IsFalse(Obj2.Byte == 100 || Obj2.Byte == 200 || Obj2.ShortString.StartsWith("A"));
			}
		}

		[TestMethod]
		public async Task Test_38_Search_OneRange_Closed()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterOrEqualTo("Byte", 100),
				new FilterFieldLesserOrEqualTo("Byte", 200)), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.GreaterOrEqual(Obj.Byte, 100);
					AssertEx.LessOrEqual(Obj.Byte, 200);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.IsTrue(Obj2.Byte < 100 || Obj2.Byte > 200);
			}
		}

		[TestMethod]
		public async Task Test_39_Search_OneRange_Open()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterThan("Byte", 100),
				new FilterFieldLesserThan("Byte", 200)), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Greater(Obj.Byte, 100);
					AssertEx.Less(Obj.Byte, 200);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.IsTrue(Obj2.Byte <= 100 || Obj2.Byte >= 200);
			}
		}

		[TestMethod]
		public async Task Test_40_Search_TwoRanges_Closed()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Obj;
			bool b;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterOrEqualTo("Byte", 100),
				new FilterFieldLesserOrEqualTo("Byte", 200),
				new FilterFieldGreaterOrEqualTo("DateTime", MinDT),
				new FilterFieldLesserOrEqualTo("DateTime", MaxDT)), true))
			{
				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.GreaterOrEqual(Obj.Byte, 100);
					AssertEx.LessOrEqual(Obj.Byte, 200);
					AssertEx.GreaterOrEqual(Obj.DateTime, MinDT);
					AssertEx.LessOrEqual(Obj.DateTime, MaxDT);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
				{
					b = Obj2.Byte < 100 || Obj2.Byte > 200 || Obj2.DateTime < MinDT || Obj2.DateTime > MaxDT;
					Assert.IsTrue(b);
				}
			}
		}

		private static readonly DateTime MinDT = new DateTime(1950, 6, 7, 8, 9, 10);
		private static readonly DateTime MaxDT = new DateTime(1990, 2, 3, 4, 5, 6);

		[TestMethod]
		public async Task Test_41_Search_TwoRanges_Open()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Obj;
			bool b;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterThan("Byte", 100),
				new FilterFieldLesserThan("Byte", 200),
				new FilterFieldGreaterThan("DateTime", MinDT),
				new FilterFieldLesserThan("DateTime", MaxDT)), true))
			{
				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Greater(Obj.Byte, 100);
					AssertEx.Less(Obj.Byte, 200);
					AssertEx.Greater(Obj.DateTime, MinDT);
					AssertEx.Less(Obj.DateTime, MaxDT);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
				{
					b = Obj2.Byte <= 100 || Obj2.Byte >= 200 || Obj2.DateTime <= MinDT || Obj2.DateTime >= MaxDT;
					Assert.IsTrue(b);
				}
			}
		}

		[TestMethod]
		public async Task Test_42_Search_TwoRanges_Open_AdditionalFilter()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Obj;
			bool b;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterThan("Byte", 100),
				new FilterFieldLesserThan("Byte", 200),
				new FilterFieldGreaterThan("DateTime", MinDT),
				new FilterFieldLesserThan("DateTime", MaxDT),
				new FilterFieldLikeRegEx("ShortString", "[A-Z].*")), true))
			{
				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Greater(Obj.Byte, 100);
					AssertEx.Less(Obj.Byte, 200);
					AssertEx.Greater(Obj.DateTime, MinDT);
					AssertEx.Less(Obj.DateTime, MaxDT);
					Assert.IsTrue(Obj.ShortString[0] >= 'A' && Obj.ShortString[0] <= 'Z');
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
				{
					b = Obj2.Byte <= 100 || Obj2.Byte >= 200 || Obj2.DateTime <= MinDT || Obj2.DateTime >= MaxDT || Obj2.ShortString[0] < 'A' || Obj2.ShortString[0] > 'Z';
					Assert.IsTrue(b);
				}
			}
		}

		[TestMethod]
		public async Task Test_43_Search_TwoOpenRanges_MinMin()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Obj;
			bool b;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterThan("Byte", 100),
				new FilterFieldGreaterThan("DateTime", MinDT)), true))
			{
				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Greater(Obj.Byte, 100);
					AssertEx.Greater(Obj.DateTime, MinDT);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
				{
					b = Obj2.Byte <= 100 || Obj2.DateTime <= MinDT;
					Assert.IsTrue(b);
				}
			}
		}

		[TestMethod]
		public async Task Test_44_Search_TwoOpenRanges_MinMax()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Obj;
			bool b;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterThan("Byte", 100),
				new FilterFieldLesserThan("DateTime", MaxDT)), true))
			{
				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Greater(Obj.Byte, 100);
					AssertEx.Less(Obj.DateTime, MaxDT);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
				{
					b = Obj2.Byte <= 100 || Obj2.DateTime >= MaxDT;
					Assert.IsTrue(b);
				}
			}
		}

		[TestMethod]
		public async Task Test_45_Search_TwoOpenRanges_MaxMin()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Obj;
			bool b;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldLesserThan("Byte", 200),
				new FilterFieldGreaterThan("DateTime", MinDT)), true))
			{
				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Less(Obj.Byte, 200);
					AssertEx.Greater(Obj.DateTime, MinDT);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
				{
					b = Obj2.Byte >= 200 || Obj2.DateTime <= MinDT;
					Assert.IsTrue(b);
				}
			}
		}

		[TestMethod]
		public async Task Test_46_Search_TwoOpenRanges_MaxMax()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Obj;
			bool b;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldLesserThan("Byte", 200),
				new FilterFieldLesserThan("DateTime", MaxDT)), true))
			{
				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.Less(Obj.Byte, 200);
					AssertEx.Less(Obj.DateTime, MaxDT);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
				{
					b = Obj2.Byte >= 200 || Obj2.DateTime >= MaxDT;
					Assert.IsTrue(b);
				}
			}
		}

		[TestMethod]
		public async Task Test_47_Search_TwoRanges_Normalization()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Obj;
			bool b;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterThan("DateTime", MinDT),
				new FilterFieldLesserThan("DateTime", MaxDT),
				new FilterOr(
					new FilterFieldGreaterThan("Byte", 200),
					new FilterFieldLesserThan("Byte", 100))), true))
			{
				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.IsTrue(Obj.Byte < 100 || Obj.Byte > 200);
					AssertEx.Greater(Obj.DateTime, MinDT);
					AssertEx.Less(Obj.DateTime, MaxDT);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
				{
					b = Obj2.DateTime <= MinDT || Obj2.DateTime >= MaxDT || (Obj2.Byte >= 100 && Obj2.Byte <= 200);
					Assert.IsTrue(b);
				}
			}
		}

		[TestMethod]
		public async Task Test_48_Search_Or_AvoidMultipleFullFileScans()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			FileStatistics StatBefore = await this.file.ComputeStatistics();

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterOr(
				new FilterFieldLikeRegEx("ShortString", "[AB].*"),
				new FilterFieldLikeRegEx("ShortString", "[XY].*")), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.IsTrue("ABXY".IndexOf(Obj.ShortString[0]) >= 0);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects.Values)
					Assert.IsFalse("ABXY".IndexOf(Obj2.ShortString[0]) >= 0);
			}

			FileStatistics StatAfter = await this.file.ComputeStatistics();
			ulong NrFullFileScans = StatAfter.NrFullFileScans - StatBefore.NrFullFileScans;
			Assert.AreEqual(1, NrFullFileScans);
		}

		[TestMethod]
		public async Task Test_49_Search_SortOrder()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Simple Prev = null;
			Simple Obj;
			bool b;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterThan("DateTime", MinDT),
				new FilterFieldLesserThan("DateTime", MaxDT),
				new FilterOr(
					new FilterFieldGreaterThan("Byte", 200),
					new FilterFieldLesserThan("Byte", 100))), true, "SByte", "ShortString"))
			{
				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					Assert.IsTrue(Obj.Byte < 100 || Obj.Byte > 200);
					AssertEx.Greater(Obj.DateTime, MinDT);
					AssertEx.Less(Obj.DateTime, MaxDT);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					if (Prev != null)
					{
						Assert.IsTrue(Prev.SByte < Obj.SByte ||
							(Prev.SByte == Obj.SByte && string.Compare(Prev.ShortString, Obj.ShortString) <= 0));
					}

					Prev = Obj;
				}

				foreach (Simple Obj2 in Objects.Values)
				{
					b = Obj2.DateTime <= MinDT || Obj2.DateTime >= MaxDT || (Obj2.Byte >= 100 && Obj2.Byte <= 200);
					Assert.IsTrue(b);
				}
			}
		}

		[TestMethod]
		public async Task Test_50_Search_Paging()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			List<Simple> Ordered = new List<Simple>();
			int i;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterAnd(
				new FilterFieldGreaterThan("DateTime", MinDT),
				new FilterFieldLesserThan("DateTime", MaxDT),
				new FilterOr(
					new FilterFieldGreaterThan("Byte", 200),
					new FilterFieldLesserThan("Byte", 100))), true, "SByte", "ShortString"))
			{
				while (await Cursor.MoveNextAsync())
				{
					Ordered.Add(Cursor.Current);
				}
			}

			i = 20;

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(20, 10, new FilterAnd(
				new FilterFieldGreaterThan("DateTime", MinDT),
				new FilterFieldLesserThan("DateTime", MaxDT),
				new FilterOr(
					new FilterFieldGreaterThan("Byte", 200),
					new FilterFieldLesserThan("Byte", 100))), true, "SByte", "ShortString"))
			{
				while (await Cursor.MoveNextAsync())
				{
					ObjectSerializationTests.AssertEqual(Ordered[i++], Cursor.Current);
				}
			}

			AssertEx.LessOrEqual(i, 40);
		}

		[TestMethod]
		public async Task Test_51_Search_DefaultValue()
		{
			SortedDictionary<Guid, Default> Objects = await this.CreateDefaultObjects(ObjectsToEnumerate);

			using (ICursor<Default> Cursor = await this.file.Find<Default>(0, int.MaxValue, new FilterFieldLesserOrEqualTo("Byte", 10), true))
			{
				Default Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.LessOrEqual(Obj.Byte, 10);
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}

				foreach (Default Obj2 in Objects.Values)
					AssertEx.Greater(Obj2.Byte, 10);
			}
		}

		private async Task<SortedDictionary<Guid, Default>> CreateDefaultObjects(int NrObjects)
		{
			SortedDictionary<Guid, Default> Result = new SortedDictionary<Guid, Default>();

			while (NrObjects > 0)
			{
				Default Obj = BTreeTests.CreateDefault(this.MaxStringLength);
				Guid ObjectId = await this.file.SaveNewObject(Obj);
				Result[ObjectId] = Obj;
				NrObjects--;
			}

			await BTreeTests.AssertConsistent(this.file, this.provider, null, null, true);

			return Result;
		}

		[TestMethod]
		public async Task Test_52_Search_MixedTypes()
		{
			SortedDictionary<Guid, Default> Objects1 = await this.CreateDefaultObjects(ObjectsToEnumerate);
			SortedDictionary<Guid, Simple> Objects2 = await this.CreateObjects(ObjectsToEnumerate);

			using (ICursor<Default> Cursor = await this.file.Find<Default>(0, int.MaxValue, new FilterFieldLesserOrEqualTo("Byte", 10), true))
			{
				Default Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.LessOrEqual(Obj.Byte, 10);
					Assert.IsTrue(Objects1.Remove(Obj.ObjectId));
				}

				foreach (Default Obj2 in Objects1.Values)
					AssertEx.Greater(Obj2.Byte, 10);
			}

			using (ICursor<Simple> Cursor = await this.file.Find<Simple>(0, int.MaxValue, new FilterFieldLesserOrEqualTo("Byte", 10), true))
			{
				Simple Obj;

				while (await Cursor.MoveNextAsync())
				{
					Obj = Cursor.Current;
					Assert.IsNotNull(Obj);
					AssertEx.LessOrEqual(Obj.Byte, 10);
					Assert.IsTrue(Objects2.Remove(Obj.ObjectId));
				}

				foreach (Simple Obj2 in Objects2.Values)
					AssertEx.Greater(Obj2.Byte, 10);
			}
		}

	}
}
