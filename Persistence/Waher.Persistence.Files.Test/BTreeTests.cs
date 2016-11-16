using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Content;
using Waher.Persistence.Files.Test.Classes;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;
using Waher.Script;

namespace Waher.Persistence.Files.Test
{
	[TestFixture]
	public abstract class BTreeTests
	{
		internal const string FileName = "Data\\Objects.db";
		internal const string ObjFileName = "Data\\LastObject.bin";
		internal const string ObjIdFileName = "Data\\LastObjectId.bin";
		internal const string Folder = "Data";
		internal const string BlobFolder = "Data\\Blobs";
		internal const string CollectionName = "Default";
		internal const int BlocksInCache = 1000;
		private const int ObjectsToEnumerate = 100000;

		private ObjectBTreeFile file;
		private FilesProvider provider;
		private Random gen = new Random();
		private DateTime start;

		public abstract int BlockSize
		{
			get;
		}

		[SetUp]
		public void SetUp()
		{
			if (File.Exists(FileName + ".bak"))
				File.Delete(FileName + ".bak");

			if (File.Exists(FileName))
			{
				File.Copy(FileName, FileName + ".bak");
				File.Delete(FileName);
			}

			this.provider = new FilesProvider(Folder, CollectionName);
			this.file = new ObjectBTreeFile(FileName, CollectionName, BlobFolder, BlockSize, BlocksInCache, this.provider, Encoding.UTF8, 10000, true);
			this.start = DateTime.Now;
		}

		[TearDown]
		public void TearDown()
		{
			Console.Out.WriteLine("Elapsed time: " + (DateTime.Now - this.start).ToString());

			if (this.file != null)
			{
				this.file.Dispose();
				this.file = null;
			}

			if (this.provider != null)
			{
				this.provider.Dispose();
				this.provider = null;
			}
		}

		private Simple CreateSimple()
		{
			Simple Result = new Simple();

			Result.Boolean1 = this.gen.Next(2) == 1;
			Result.Boolean2 = this.gen.Next(2) == 1;
			Result.Byte = (byte)(this.gen.NextDouble() * 256);
			Result.Short = (short)(this.gen.NextDouble() * ((double)short.MaxValue - (double)short.MinValue + 1) + short.MinValue);
			Result.Int = (int)(this.gen.NextDouble() * ((double)int.MaxValue - (double)int.MinValue + 1) + int.MinValue);
			Result.Long = (long)(this.gen.NextDouble() * ((double)long.MaxValue - (double)long.MinValue + 1) + long.MinValue);
			Result.SByte = (sbyte)(this.gen.NextDouble() * ((double)sbyte.MaxValue - (double)sbyte.MinValue + 1) + sbyte.MinValue);
			Result.UShort = (ushort)(this.gen.NextDouble() * ((double)short.MaxValue + 1));
			Result.UInt = (uint)(this.gen.NextDouble() * ((double)short.MaxValue + 1));
			Result.ULong = (ulong)(this.gen.NextDouble() * ((double)short.MaxValue + 1));
			Result.Char = (char)(this.gen.Next(char.MaxValue));
			Result.Decimal = (decimal)this.gen.NextDouble();
			Result.Double = this.gen.NextDouble();
			Result.Single = (float)this.gen.NextDouble();
			Result.String = new string((char)this.gen.Next(32, 127), this.gen.Next(10, 100));
			Result.DateTime = new DateTime(1900, 1, 1).AddDays(this.gen.NextDouble() * 73049);
			Result.TimeSpan = new TimeSpan((long)(this.gen.NextDouble() * 36000000000));
			Result.Guid = Guid.NewGuid();

			switch (this.gen.Next(4))
			{
				case 0:
					Result.NormalEnum = NormalEnum.Option1;
					break;

				case 1:
					Result.NormalEnum = NormalEnum.Option2;
					break;

				case 2:
					Result.NormalEnum = NormalEnum.Option3;
					break;

				case 3:
					Result.NormalEnum = NormalEnum.Option4;
					break;
			}

			Result.FlagsEnum = (FlagsEnum)this.gen.Next(16);

			return Result;
		}

		[Test]
		public async Task Test_01_SaveNew()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
			Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTree.xml"));
		}

		[Test]
		[ExpectedException]
		[Ignore]
		public async void Test_01_X_Repeat_Test_01()
		{
			object LastObjectAdded = null;

			try
			{
				List<Simple> Objects = new List<Simple>();
				List<Guid> ObjectIds = new List<Guid>();
				int i, c = 0;

				Simple Obj = this.CreateSimple();
				Guid ObjectId = await this.file.SaveNewObject(LastObjectAdded = Obj);
				Assert.AreNotEqual(Guid.Empty, ObjectId);

				Objects.Add(Obj);
				ObjectIds.Add(ObjectId);
				c++;

				Simple Obj2 = await this.file.LoadObject<Simple>(ObjectIds[0]);
				ObjectSerializationTests.AssertEqual(Obj, Obj2);

				while (true)
				{
					this.TearDown();
					this.SetUp();

					Obj = this.CreateSimple();
					ObjectId = await this.file.SaveNewObject(LastObjectAdded = Obj);
					Assert.AreNotEqual(Guid.Empty, ObjectId);

					//FileStatistics Stat = await AssertConsistent(this.file, this.provider, null, Obj, false);

					Objects.Add(Obj);
					ObjectIds.Add(ObjectId);
					c++;

					for (i = 0; i < c; i++)
						Obj2 = await this.file.LoadObject<Simple>(ObjectIds[i]);

					ObjectSerializationTests.AssertEqual(Obj, Obj2);

					//Console.Out.Write(Stat.NrObjects.ToString() + " ");
					Console.Out.Write(c.ToString() + " ");
				}
			}
			catch (Exception ex)
			{
				SaveLastObject(this.provider, LastObjectAdded);

				ExceptionDispatchInfo.Capture(ex).Throw();
			}
		}

		internal static async Task<string> ExportXML(ObjectBTreeFile File, string XmlFileName)
		{
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings Settings = XML.WriterSettings(true, true);
			using (XmlWriter w = XmlWriter.Create(sb, Settings))
			{
				await File.ExportGraphXML(w);
				w.Flush();
			}

			string Xml = sb.ToString();
			if (!string.IsNullOrEmpty(XmlFileName))
				System.IO.File.WriteAllText(XmlFileName, Xml);

			return Xml;
		}

		internal static async Task<FileStatistics> AssertConsistent(ObjectBTreeFile File, FilesProvider Provider, int? ExpectedNrObjects, object LastObjectAdded,
			bool WriteStat)
		{
			FileStatistics Statistics = await File.ComputeStatistics();

			if (WriteStat)
			{
				Console.Out.WriteLine("Block Size: " + Statistics.BlockSize.ToString());
				Console.Out.WriteLine("#Blocks: " + Statistics.NrBlocks.ToString());
				Console.Out.WriteLine("#Bytes used: " + Statistics.NrBytesUsed.ToString());
				Console.Out.WriteLine("#Bytes unused: " + Statistics.NrBytesUnused.ToString());
				Console.Out.WriteLine("#Bytes total: " + Statistics.NrBytesTotal.ToString());
				Console.Out.WriteLine("#Block loads: " + Statistics.NrBlockLoads.ToString());
				Console.Out.WriteLine("#Cache loads: " + Statistics.NrCacheLoads.ToString());
				Console.Out.WriteLine("#Block saves: " + Statistics.NrBlockSaves.ToString());
				Console.Out.WriteLine("#Objects: " + Statistics.NrObjects.ToString());
				Console.Out.WriteLine("Smallest object: " + Statistics.MinObjectSize.ToString());
				Console.Out.WriteLine("Largest object: " + Statistics.MaxObjectSize.ToString());
				Console.Out.WriteLine("Average object: " + Statistics.AverageObjectSize.ToString("F1"));
				Console.Out.WriteLine("Usage: " + Statistics.Usage.ToString("F2") + " %");
				Console.Out.WriteLine("Min(Depth): " + Statistics.MinDepth.ToString());
				Console.Out.WriteLine("Max(Depth): " + Statistics.MaxDepth.ToString());
				Console.Out.WriteLine("Min(Objects/Block): " + Statistics.MinObjectsPerBlock.ToString());
				Console.Out.WriteLine("Max(Objects/Block): " + Statistics.MaxObjectsPerBlock.ToString());
				Console.Out.WriteLine("Avg(Objects/Block): " + Statistics.AverageObjectsPerBlock.ToString("F1"));
				Console.Out.WriteLine("Min(Bytes Used/Block): " + Statistics.MinBytesUsedPerBlock.ToString());
				Console.Out.WriteLine("Max(Bytes Used/Block): " + Statistics.MaxBytesUsedPerBlock.ToString());
				Console.Out.WriteLine("Avg(Bytes Used/Block): " + Statistics.AverageBytesUsedPerBlock.ToString("F1"));
				Console.Out.WriteLine("Is Corrupt: " + Statistics.IsCorrupt.ToString());
				Console.Out.WriteLine("Is Balanced: " + Statistics.IsBalanced.ToString());
				Console.Out.WriteLine("Has Comments: " + Statistics.HasComments.ToString());
			}

			if (Statistics.HasComments)
			{
				Console.Out.WriteLine();
				foreach (string Comment in Statistics.Comments)
					Console.Out.WriteLine(Comment);
			}

			try
			{
				if (Statistics.IsCorrupt || !Statistics.IsBalanced)
				{
					Console.Out.WriteLine();
					Console.Out.WriteLine(await ExportXML(File, "Data\\BTreeError.xml"));
					Console.Out.WriteLine();

					Assert.IsFalse(Statistics.IsCorrupt, "Database is corrupt.");
					Assert.IsTrue(Statistics.IsBalanced, "Database is unbalanced.");
				}

				if (ExpectedNrObjects.HasValue)
					Assert.AreEqual(ExpectedNrObjects.Value, Statistics.NrObjects);
			}
			catch (Exception ex)
			{
				SaveLastObject(Provider, LastObjectAdded);

				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			return Statistics;
		}

		private static void SaveLastObject(FilesProvider Provider, object LastObjectAdded)
		{
			if (LastObjectAdded != null)
			{
				IObjectSerializer Serializer = Provider.GetObjectSerializer(LastObjectAdded.GetType());
				BinarySerializer Writer = new BinarySerializer(CollectionName, Encoding.UTF8);
				Serializer.Serialize(Writer, false, false, LastObjectAdded);
				byte[] Bin = Writer.GetSerialization();
				System.IO.File.WriteAllBytes(ObjFileName, Bin);
			}
		}

		[Test]
		[ExpectedException(typeof(IOException))]
		public async void Test_02_SaveOld()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);
			await this.file.SaveNewObject(Obj);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[Test]
		public async void Test_03_LoadUntyped()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			GenericObject GenObj = (GenericObject)await this.file.LoadObject(ObjectId);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Boolean1, GenObj["Boolean1"]);
			Assert.AreEqual(Obj.Boolean2, GenObj["Boolean2"]);
			Assert.AreEqual(Obj.Byte, GenObj["Byte"]);
			Assert.AreEqual(Obj.Short, GenObj["Short"]);
			Assert.AreEqual(Obj.Int, GenObj["Int"]);
			Assert.AreEqual(Obj.Long, GenObj["Long"]);
			Assert.AreEqual(Obj.SByte, GenObj["SByte"]);
			Assert.AreEqual(Obj.UShort, GenObj["UShort"]);
			Assert.AreEqual(Obj.UInt, GenObj["UInt"]);
			Assert.AreEqual(Obj.ULong, GenObj["ULong"]);
			Assert.AreEqual(Obj.Char, GenObj["Char"]);
			Assert.AreEqual(Obj.Decimal, GenObj["Decimal"]);
			Assert.AreEqual(Obj.Double, GenObj["Double"]);
			Assert.AreEqual(Obj.Single, GenObj["Single"]);
			Assert.AreEqual(Obj.String, GenObj["String"]);
			Assert.AreEqual(Obj.DateTime, GenObj["DateTime"]);
			Assert.AreEqual(Obj.TimeSpan, GenObj["TimeSpan"]);
			Assert.AreEqual(Obj.Guid, GenObj["Guid"]);
			Assert.AreEqual(Obj.NormalEnum.ToString(), GenObj["NormalEnum"]);
			Assert.AreEqual((int)Obj.FlagsEnum, GenObj["FlagsEnum"]);
			Assert.AreEqual(Obj.ObjectId, GenObj.ObjectId);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[Test]
		public async void Test_03_LoadTyped()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			Simple Obj2 = await this.file.LoadObject<Simple>(ObjectId);

			ObjectSerializationTests.AssertEqual(Obj, Obj2);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[Test]
		public async void Test_04_LoadUntyped()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			GenericObject Obj2 = (GenericObject)await this.file.LoadObject(ObjectId);

			ObjectSerializationTests.AssertEqual(Obj, Obj2);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[Test]
		public async void Test_05_SaveNew_Multiple_NoSplit()
		{
			await this.TestMultiple(3, true, null);
		}

		private async Task TestMultiple(int c, bool AssertIndividually, int? LogStatisticsEvery)
		{
			DateTime Start = DateTime.Now;
			List<FileStatistics> Stat = null;
			List<double> Milliseconds = null;
			int i;
			Simple[] Objects = new Simple[c];
			Simple Obj2;

			for (i = 0; i < c; i++)
			{
				Objects[i] = this.CreateSimple();
				await this.file.SaveNewObject(Objects[i]);

				if (AssertIndividually)
				{
					Console.Out.WriteLine();
					Console.Out.WriteLine((i + 1).ToString() + " objects:");
					Console.Out.WriteLine(new string('-', 80));

					await AssertConsistent(this.file, this.provider, i + 1, Objects[i], true);
				}

				if (LogStatisticsEvery.HasValue && (i + 1) % LogStatisticsEvery.Value == 0)
				{
					if (Stat == null)
					{
						Milliseconds = new List<double>();
						Stat = new List<FileStatistics>();
					}

					Milliseconds.Add((DateTime.Now - Start).TotalMilliseconds / LogStatisticsEvery.Value);
					Stat.Add(await this.file.ComputeStatistics());

					Start = DateTime.Now;
				}
			}

			for (i = 0; i < c; i++)
			{
				Obj2 = await this.file.LoadObject<Simple>(Objects[i].ObjectId);
				ObjectSerializationTests.AssertEqual(Objects[i], Obj2);
			}

			if (!AssertIndividually)
				await AssertConsistent(this.file, this.provider, c, null, true);

			if (Stat != null)
			{
				Variables v = new Variables();
				v["Stat"] = Stat.ToArray();
				v["ms"] = Milliseconds.ToArray();
				v["StepSize"] = LogStatisticsEvery.Value;

				Expression Exp = new Expression("[ms,Stat.BlockSize,Stat.NrBlocks,Stat.NrBytesUsed,Stat.NrBytesUnused,Stat.NrBytesTotal," +
					"Stat.Usage,Stat.NrObjects,Stat.MinObjectSize,Stat.MaxObjectSize,Stat.AverageObjectSize,Stat.MinDepth,Stat.MaxDepth," +
					"Stat.NrBlockLoads,Stat.NrCacheLoads,Stat.NrBlockSaves,Stat.MinObjectsPerBlock,Stat.MaxObjectsPerBlock," +
					"Stat.AverageObjectsPerBlock,Stat.MinBytesUsedPerBlock,Stat.MaxBytesUsedPerBlock,Stat.AverageBytesUsedPerBlock]T");

				Console.Out.WriteLine("ms, BlockSize, NrBlocks, NrBytesUsed, NrBytesUnused, NrBytesTotal, " +
					"Usage, NrObjects, MinObjectSize, MaxObjectSize, AverageObjectSize, MinDepth, MaxDepth, " +
					"NrBlockLoads, NrCacheLoads, NrBlockSaves,Min(Obj/Block),Max(Obj/Block),Avg(Obj/Block)," +
					"Min(UsedBytes/Block),Max(UsedBytes/Block),Avg(UsedBytes/Block)");
				Console.Out.WriteLine(new string('-', 80));
				Console.Out.WriteLine(Exp.Evaluate(v).ToString());
			}
		}

		[Test]
		public async void Test_06_SaveNew_Multiple_NodeSplit()
		{
			await this.TestMultiple(100, true, null);
		}

		[Test]
		public async void Test_07_SaveNew_1000()
		{
			await this.TestMultiple(1000, false, null);
			await ExportXML(this.file, "Data\\BTree.xml");
		}

		[Test]
		public async void Test_08_SaveNew_10000()
		{
			await this.TestMultiple(10000, false, null);
			await ExportXML(this.file, "Data\\BTree.xml");
		}

		[Test]
		public async void Test_09_SaveNew_10000_Statistics()
		{
			await this.TestMultiple(10000, false, 100);
		}

		[Test]
		public async void Test_10_SaveNew_100000()
		{
			await this.TestMultiple(100000, false, null);
		}

		[Test]
		public async void Test_11_SaveNew_100000_Statistics()
		{
			await this.TestMultiple(100000, false, 1000);
		}

		[Test]
		public async void Test_12_SaveNew_1000000()
		{
			await this.TestMultiple(1000000, false, null);
		}

		[Test]
		public async void Test_13_SaveNew_1000000_Statistics()
		{
			await this.TestMultiple(1000000, false, 10000);
		}

		[Test]
		public async Task Test_14_Contains()
		{
			Simple Obj = this.CreateSimple();
			Simple Obj2 = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);
			Assert.IsTrue(this.file.Contains(Obj));
			Assert.IsFalse(this.file.Contains(Obj2));
		}

		[Test]
		public async Task Test_15_Count()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);
			Console.Out.WriteLine(this.file.Count.ToString());
		}

		[Test]
		public async Task Test_16_Clear()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);
			Assert.IsTrue(this.file.Contains(Obj));
			this.file.Clear();
			Assert.IsFalse(this.file.Contains(Obj));
		}

		[Test]
		public async Task Test_17_NormalEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Guid? Prev = null;

			foreach (GenericObject Obj in this.file)
			{
				if (Prev.HasValue)
					Assert.Less(Prev.Value, Obj.ObjectId);

				Prev = Obj.ObjectId;
				Assert.IsTrue(Objects.Remove(Obj.ObjectId));
			}

			Assert.AreEqual(0, Objects.Count);
		}

		[Test]
		public async Task Test_18_TypedEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Guid? Prev = null;
			Simple Obj;
			ulong Rank = 0;

			using (ObjectBTreeFileEnumerator<Simple> e = this.file.GetTypedEnumerator<Simple>(false))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						Assert.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					Assert.AreEqual(Rank++, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects.Count);
		}

		[Test]
		public async Task Test_19_LockedEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Guid? Prev = null;
			Simple Obj;
			ulong Rank = 0;

			using (ObjectBTreeFileEnumerator<Simple> e = this.file.GetTypedEnumerator<Simple>(true))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						Assert.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					Assert.AreEqual(Rank++, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects.Count);
		}

		[Test]
		[ExpectedException]
		public async Task Test_20_UnlockedChangeEnumeration()
		{
			await this.CreateObjects(Math.Min(ObjectsToEnumerate, 1000));
			Simple Obj;

			using (ObjectBTreeFileEnumerator<Simple> e = this.file.GetTypedEnumerator<Simple>(true))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					Obj = this.CreateSimple();
					await this.file.SaveNewObject(Obj);
				}
			}
		}

		[Test]
		public async Task Test_21_BackwardsEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Guid? Prev = null;
			Simple Obj;
			ulong Rank = ObjectsToEnumerate;

			using (ObjectBTreeFileEnumerator<Simple> e = this.file.GetTypedEnumerator<Simple>(true))
			{
				while (e.MovePrevious())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						Assert.Greater(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					Assert.AreEqual(--Rank, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects.Count);
		}

		private async Task<SortedDictionary<Guid, Simple>> CreateObjects(int NrObjects)
		{
			SortedDictionary<Guid, Simple> Result = new SortedDictionary<Guid, Simple>();

			while (NrObjects > 0)
			{
				Simple Obj = this.CreateSimple();
				Guid ObjectId = await this.file.SaveNewObject(Obj);
				Result[ObjectId] = Obj;
				NrObjects--;
			}

			return Result;
		}

		[Test]
		public async Task Test_22_SelectIthObject()
		{
			int c = ObjectsToEnumerate;
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(c);
			Simple[] Ordered = new Simple[c];
			Objects.Values.CopyTo(Ordered, 0);
			Guid? Prev = null;
			Simple Obj;
			Random gen = new Random();
			int i, j;

			for (i = 0; i < c; i++)
			{
				j = 0;
				Prev = null;

				if (i < 10 || (gen.Next(0, 2) == 0 && i <= c - 10))
				{
					using (ObjectBTreeFileEnumerator<Simple> e = this.file.GetTypedEnumerator<Simple>(true))
					{
						Assert.IsTrue(await e.GoToObject((uint)i));

						do
						{
							Obj = e.Current;
							if (Prev.HasValue)
								Assert.Less(Prev.Value, Obj.ObjectId);

							Prev = Obj.ObjectId;
							ObjectSerializationTests.AssertEqual(Ordered[i + j], Obj);

							Assert.AreEqual(i + j, e.CurrentRank);
							Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
						}
						while (e.MoveNext() && j++ < 10);
					}
				}
				else
				{
					using (ObjectBTreeFileEnumerator<Simple> e = this.file.GetTypedEnumerator<Simple>(true))
					{
						Assert.IsTrue(await e.GoToObject((uint)i));

						do
						{
							Obj = e.Current;
							if (Prev.HasValue)
								Assert.Greater(Prev.Value, Obj.ObjectId);

							Prev = Obj.ObjectId;
							ObjectSerializationTests.AssertEqual(Ordered[i - j], Obj);

							Assert.AreEqual(i - j, e.CurrentRank);
							Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
						}
						while (e.MovePrevious() && j++ < 10);
					}
				}
			}
		}

		[Test]
		public async Task Test_23_RankObject()
		{
			int c = ObjectsToEnumerate;
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(c);
			Simple[] Ordered = new Simple[c];
			Objects.Values.CopyTo(Ordered, 0);
			int i;

			for (i = 0; i < c; i++)
				Assert.AreEqual(i, await this.file.GetRank(Ordered[i].ObjectId));
		}

		[Test]
		public async Task Test_24_Reset()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Guid? Prev = null;
			Simple Obj;
			ulong Rank = 0;

			using (ObjectBTreeFileEnumerator<Simple> e = this.file.GetTypedEnumerator<Simple>(false))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						Assert.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.ContainsKey(Obj.ObjectId));

					Assert.AreEqual(Rank++, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}

				e.Reset();
				Prev = null;

				while (e.MovePrevious())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						Assert.Greater(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					Assert.AreEqual(--Rank, e.CurrentRank);
					Assert.AreEqual(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			Assert.AreEqual(0, Objects.Count);
		}

		[Test]
		[ExpectedException]
		public async Task Test_25_UpdateUnsavedObject()
		{
			Simple Obj = this.CreateSimple();
			await this.file.UpdateObject(Obj);
		}

		[Test]
		[ExpectedException]
		public async Task Test_26_UpdateUnexistentObject()
		{
			Simple Obj = this.CreateSimple();
			Obj.ObjectId = Guid.NewGuid();
			await this.file.UpdateObject(Obj);
		}

		[Test]
		public async Task Test_27_UpdateObject()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			Simple Obj2 = await this.file.LoadObject<Simple>(ObjectId);
			ObjectSerializationTests.AssertEqual(Obj, Obj2);

			Simple Obj3 = this.CreateSimple();
			Obj3.ObjectId = ObjectId;
			await this.file.UpdateObject(Obj3);

			Obj2 = await this.file.LoadObject<Simple>(ObjectId);
			ObjectSerializationTests.AssertEqual(Obj3, Obj2);

			await AssertConsistent(this.file, this.provider, null, null, true);
			Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTree.xml"));
		}

		[Test]
		public void Test_28_UpdateObjects_1000()
		{
			this.Test_UpdateObjects(1000).Wait();
		}

		[Test]
		public void Test_29_UpdateObjects_10000()
		{
			this.Test_UpdateObjects(10000).Wait();
		}

		[Test]
		public void Test_30_UpdateObjects_100000()
		{
			this.Test_UpdateObjects(100000).Wait();
		}

		private async Task Test_UpdateObjects(int c)
		{
			Simple[] Objects = new Simple[c];
			Simple Obj;
			int i;

			for (i = 0; i < c; i++)
			{
				Objects[i] = Obj = this.CreateSimple();
				await this.file.SaveNewObject(Obj);
			}

			//await AssertConsistent(this.file, this.provider, null, null, true);
			//Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeBeforeUpdates.xml"));

			for (i = 0; i < c; i++)
			{
				Obj = this.CreateSimple();
				Obj.ObjectId = Objects[i].ObjectId;

				await this.file.UpdateObject(Obj);

				Objects[i] = Obj;

				Obj = await this.file.LoadObject<Simple>(Obj.ObjectId);
				ObjectSerializationTests.AssertEqual(Objects[i], Obj);
			}

			await AssertConsistent(this.file, this.provider, null, null, true);
			//Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeAfterUpdates.xml"));
		}

		[Test]
		public async Task Test_31_DeleteObject()
		{
			await this.Test_DeleteObjects(3);
		}

		[Test]
		public async Task Test_32_DeleteObject_1000()
		{
			await this.Test_DeleteObjects(20);
		}

		private async Task Test_DeleteObjects(int c)
		{
			Random Gen = new Random();
			Simple[] Objects = new Simple[c];
			Simple Obj;
			int i;

			for (i = 0; i < c; i++)
			{
				Objects[i] = Obj = this.CreateSimple();
				await this.file.SaveNewObject(Obj);
			}

			while (c > 0)
			{
				i = Gen.Next(0, c);

				Obj = Objects[i];
				if (i < c - 1)
					Array.Copy(Objects, i + 1, Objects, i, c - i - 1);

				try
				{
					this.file.Dispose();
					File.Copy(FileName, FileName + ".bak", true);

					this.file = new ObjectBTreeFile(FileName, CollectionName, BlobFolder, BlockSize, BlocksInCache, this.provider, Encoding.UTF8, 10000, true);

					if (File.Exists(ObjIdFileName))
						File.Delete(ObjIdFileName);

					File.WriteAllBytes(ObjIdFileName, Obj.ObjectId.ToByteArray());

					Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeBefore.xml"));
					Console.Out.WriteLine(Obj.ObjectId);
					await this.file.DeleteObject(Obj);
					await AssertConsistent(this.file, this.provider, null, null, true);
				}
				catch (NotImplementedException ex)
				{
					Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeError.xml"));
					ExceptionDispatchInfo.Capture(ex).Throw();
				}

				c--;
			}

			await AssertConsistent(this.file, this.provider, null, null, true);

			Assert.AreEqual(0, this.file.Count);
		}

		/*
Register Empty Block:
	BytesUsed:=0xffff
	Truncate file if empty blocks at end. (maintain sorted list of empty blocks)

Analyze:
	Detect empty blocks
	Look for garbage at end of blocks.

Startup:
	Scan file for empty blocks asynchronously
	Rebuild in case file is corrupt

After deletion:
	Load remaining objects and see they are unchanged
 		 */
		// TODO: ICollection interfaces.
		// TODO: Delete Object
		// TODO: Multiple delete (test node merge)
		// TODO: Rotate right, if new right node is empty.
		// TODO: When analyzing file: Assure no nodes are empty.
		// TODO: Update Object
		// TODO: Update Object (incl. node split)
		// TODO: BLOBs
		// TODO: Update Object (normal -> BLOB)
		// TODO: Update Object (BLOB -> normal)
		// TODO: Multi-threaded stress test
		// TODO: Optimize by removing calls to BitConverter.
		// TOOO: Test huge databases with more than uint.MaxValue objects.
	}
}
