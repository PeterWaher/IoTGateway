using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Persistence.Serialization;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;
using Waher.Runtime.Inventory;
using Waher.Script;

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
	public abstract class DBFilesBTreeTests
	{
		internal const string MasterFileName = "Data\\Files.master";
		internal const string FileName = "Data\\Default.btree";
		internal const string BlobFileName = "Data\\Default.blob";
		internal const string NamesFileName = "Data\\Default.names";
		internal const string CollectionName = "Default";
		internal const string ObjFileName = "Data\\LastObject.bin";
		internal const string ObjIdFileName = "Data\\LastObjectId.bin";
		internal const string BlockSizeFileName = "Data\\BlockSize.bin";
		internal const string Folder = "Data";
		internal const int BlocksInCache = 10000;
		internal const int ObjectsToEnumerate = 1000;

		protected static Random gen = new Random();
		protected ObjectBTreeFile file;
		protected FilesProvider provider;
		protected DateTime start;

		public abstract int BlockSize
		{
			get;
		}

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(typeof(FilesProvider).Assembly, typeof(DBFilesBTreeTests).Assembly, typeof(Expression).Assembly);
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			if (File.Exists(MasterFileName + ".bak"))
				File.Delete(MasterFileName + ".bak");

			if (File.Exists(MasterFileName))
			{
				File.Copy(MasterFileName, MasterFileName + ".bak");
				File.Delete(MasterFileName);
			}

			if (File.Exists(FileName + ".bak"))
				File.Delete(FileName + ".bak");

			if (File.Exists(FileName))
			{
				File.Copy(FileName, FileName + ".bak");
				File.Delete(FileName);
			}

			if (File.Exists(BlobFileName + ".bak"))
				File.Delete(BlobFileName + ".bak");

			if (File.Exists(BlobFileName))
			{
				File.Copy(BlobFileName, BlobFileName + ".bak");
				File.Delete(BlobFileName);
			}

			if (File.Exists(NamesFileName + ".bak"))
				File.Delete(NamesFileName + ".bak");

			if (File.Exists(NamesFileName))
			{
				File.Copy(NamesFileName, NamesFileName + ".bak");
				File.Delete(NamesFileName);
			}

			this.provider = new FilesProvider(Folder, CollectionName, this.BlockSize, BlocksInCache, Math.Max(BlockSize / 2, 1024), Encoding.UTF8, 10000, true);
			this.file = await this.provider.GetFile(CollectionName);
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

		internal static Simple CreateSimple(int MaxStringLength)
		{
			Simple Result = new Simple()
			{
				Boolean1 = gen.Next(2) == 1,
				Boolean2 = gen.Next(2) == 1,
				Byte = (byte)(gen.NextDouble() * 256),
				Short = (short)(gen.NextDouble() * ((double)short.MaxValue - (double)short.MinValue + 1) + short.MinValue),
				Int = (int)(gen.NextDouble() * ((double)int.MaxValue - (double)int.MinValue + 1) + int.MinValue),
				Long = (long)(gen.NextDouble() * ((double)long.MaxValue - (double)long.MinValue + 1) + long.MinValue),
				SByte = (sbyte)(gen.NextDouble() * ((double)sbyte.MaxValue - (double)sbyte.MinValue + 1) + sbyte.MinValue),
				UShort = (ushort)(gen.NextDouble() * ((double)short.MaxValue + 1)),
				UInt = (uint)(gen.NextDouble() * ((double)short.MaxValue + 1)),
				ULong = (ulong)(gen.NextDouble() * ((double)short.MaxValue + 1)),
				Char = (char)(gen.Next(char.MaxValue)),
				Decimal = (decimal)gen.NextDouble(),
				Double = gen.NextDouble(),
				Single = (float)gen.NextDouble(),
				DateTime = new DateTime(1900, 1, 1).AddDays(gen.NextDouble() * 73049),
				TimeSpan = new TimeSpan((long)(gen.NextDouble() * 36000000000)),
				Guid = Guid.NewGuid()
			};

			int i, c = gen.Next(10, MaxStringLength);
			char[] ch = new char[c];

			for (i = 0; i < c; i++)
				ch[i] = (char)gen.Next(32, 127);

			Result.String = new string(ch);

			c = 10;
			ch = new char[c];

			for (i = 0; i < c; i++)
				ch[i] = (char)gen.Next(32, 127);

			Result.ShortString = new string(ch);

			switch (gen.Next(4))
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

			Result.FlagsEnum = (FlagsEnum)gen.Next(16);

			return Result;
		}

		internal static Default CreateDefault(int MaxStringLength)
		{
			Default Result = new Default()
			{
				Boolean1 = gen.Next(2) == 1,
				Boolean2 = gen.Next(2) == 1,
				Byte = (byte)(gen.NextDouble() * 256),
				Short = (short)(gen.NextDouble() * ((double)short.MaxValue - (double)short.MinValue + 1) + short.MinValue),
				Int = (int)(gen.NextDouble() * ((double)int.MaxValue - (double)int.MinValue + 1) + int.MinValue),
				Long = (long)(gen.NextDouble() * ((double)long.MaxValue - (double)long.MinValue + 1) + long.MinValue),
				SByte = (sbyte)(gen.NextDouble() * ((double)sbyte.MaxValue - (double)sbyte.MinValue + 1) + sbyte.MinValue),
				UShort = (ushort)(gen.NextDouble() * ((double)short.MaxValue + 1)),
				UInt = (uint)(gen.NextDouble() * ((double)short.MaxValue + 1)),
				ULong = (ulong)(gen.NextDouble() * ((double)short.MaxValue + 1)),
				Char = (char)(gen.Next(char.MaxValue)),
				Decimal = (decimal)gen.NextDouble(),
				Double = gen.NextDouble(),
				Single = (float)gen.NextDouble(),
				DateTime = new DateTime(1900, 1, 1).AddDays(gen.NextDouble() * 73049),
				TimeSpan = new TimeSpan((long)(gen.NextDouble() * 36000000000)),
				Guid = Guid.NewGuid()
			};

			int i, c = gen.Next(10, MaxStringLength);
			char[] ch = new char[c];

			for (i = 0; i < c; i++)
				ch[i] = (char)gen.Next(32, 127);

			Result.String = new string(ch);

			switch (gen.Next(4))
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

			Result.FlagsEnum = (FlagsEnum)gen.Next(16);

			return Result;
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_01_SaveNew()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
			Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTree.xml"));
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		[Ignore]
		public async Task DBFiles_BTree_Test_01_X_Repeat_DBFiles_BTree_Test_01()
		{
			object LastObjectAdded = null;

			try
			{
				List<Simple> Objects = new List<Simple>();
				List<Guid> ObjectIds = new List<Guid>();
				int i, c = 0;

				Simple Obj = CreateSimple(this.MaxStringLength);
				Guid ObjectId = await this.file.SaveNewObject(LastObjectAdded = Obj);
				AssertEx.NotSame(Guid.Empty, ObjectId);

				Objects.Add(Obj);
				ObjectIds.Add(ObjectId);
				c++;

				Simple Obj2 = await this.file.LoadObject<Simple>(ObjectIds[0]);
				DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

				while (true)
				{
					this.TestCleanup();
					await this.TestInitialize();

					Obj = CreateSimple(this.MaxStringLength);
					ObjectId = await this.file.SaveNewObject(LastObjectAdded = Obj);
					AssertEx.NotSame(Guid.Empty, ObjectId);

					//FileStatistics Stat = await AssertConsistent(this.file, this.provider, null, Obj, false);

					Objects.Add(Obj);
					ObjectIds.Add(ObjectId);
					c++;

					for (i = 0; i < c; i++)
						Obj2 = await this.file.LoadObject<Simple>(ObjectIds[i]);

					DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

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
			string Xml = await File.ExportGraphXML(false);

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
				Console.Out.WriteLine("#BLOB Blocks: " + Statistics.NrBlobBlocks.ToString());
				Console.Out.WriteLine("#Bytes used: " + Statistics.NrBytesUsed.ToString());
				Console.Out.WriteLine("#Bytes unused: " + Statistics.NrBytesUnused.ToString());
				Console.Out.WriteLine("#Bytes total: " + Statistics.NrBytesTotal.ToString());
				Console.Out.WriteLine("#BLOB Bytes used: " + Statistics.NrBlobBytesUsed.ToString());
				Console.Out.WriteLine("#BLOB Bytes unused: " + Statistics.NrBlobBytesUnused.ToString());
				Console.Out.WriteLine("#BLOB Bytes total: " + Statistics.NrBlobBytesTotal.ToString());
				Console.Out.WriteLine("#Block loads: " + Statistics.NrBlockLoads.ToString());
				Console.Out.WriteLine("#Cache loads: " + Statistics.NrCacheLoads.ToString());
				Console.Out.WriteLine("#Block saves: " + Statistics.NrBlockSaves.ToString());
				Console.Out.WriteLine("#BLOB Block loads: " + Statistics.NrBlobBlockLoads.ToString());
				Console.Out.WriteLine("#BLOB Block saves: " + Statistics.NrBlobBlockSaves.ToString());
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
					AssertEx.Same(ExpectedNrObjects.Value, Statistics.NrObjects);
			}
			catch (Exception ex)
			{
				SaveLastObject(Provider, LastObjectAdded);

				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			foreach (IndexBTreeFile Index in File.Indices)
				await AssertConsistent(Index.IndexFile, Provider, ExpectedNrObjects, null, WriteStat);

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

		[TestMethod]
		[ExpectedException(typeof(IOException))]
		public async Task DBFiles_BTree_Test_02_SaveOld()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			AssertEx.NotSame(Guid.Empty, ObjectId);
			await this.file.SaveNewObject(Obj);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_03_LoadUntyped()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			GenericObject GenObj = (GenericObject)await this.file.LoadObject(ObjectId);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Boolean1, GenObj["Boolean1"]);
			AssertEx.Same(Obj.Boolean2, GenObj["Boolean2"]);
			AssertEx.Same(Obj.Byte, GenObj["Byte"]);
			AssertEx.Same(Obj.Short, GenObj["Short"]);
			AssertEx.Same(Obj.Int, GenObj["Int"]);
			AssertEx.Same(Obj.Long, GenObj["Long"]);
			AssertEx.Same(Obj.SByte, GenObj["SByte"]);
			AssertEx.Same(Obj.UShort, GenObj["UShort"]);
			AssertEx.Same(Obj.UInt, GenObj["UInt"]);
			AssertEx.Same(Obj.ULong, GenObj["ULong"]);
			AssertEx.Same(Obj.Char, GenObj["Char"]);
			AssertEx.Same(Obj.Decimal, GenObj["Decimal"]);
			AssertEx.Same(Obj.Double, GenObj["Double"]);
			AssertEx.Same(Obj.Single, GenObj["Single"]);
			AssertEx.Same(Obj.String, GenObj["String"]);
			AssertEx.Same(Obj.DateTime, GenObj["DateTime"]);
			AssertEx.Same(Obj.TimeSpan, GenObj["TimeSpan"]);
			AssertEx.Same(Obj.Guid, GenObj["Guid"]);
			AssertEx.Same(Obj.NormalEnum.ToString(), GenObj["NormalEnum"]);
			AssertEx.Same((int)Obj.FlagsEnum, GenObj["FlagsEnum"]);
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_03_LoadTyped()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			Simple Obj2 = await this.file.LoadObject<Simple>(ObjectId);

			DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_04_LoadUntyped()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			GenericObject Obj2 = (GenericObject)await this.file.LoadObject(ObjectId);

			DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_05_SaveNew_Multiple_NoSplit()
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
				Objects[i] = CreateSimple(this.MaxStringLength);
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
				DBFilesObjectSerializationTests.AssertEqual(Objects[i], Obj2);
			}

			if (!AssertIndividually)
				await AssertConsistent(this.file, this.provider, c, null, true);

			if (Stat != null)
			{
				Variables v = new Variables()
				{
					{ "Stat", Stat.ToArray() },
					{ "ms", Milliseconds.ToArray() },
					{ "StepSize", LogStatisticsEvery.Value }
				};

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

		[TestMethod]
		public async Task DBFiles_BTree_Test_06_SaveNew_Multiple_NodeSplit()
		{
			await this.TestMultiple(100, true, null);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_07_SaveNew_1000()
		{
			await this.TestMultiple(1000, false, null);
			await ExportXML(this.file, "Data\\BTree.xml");
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_08_SaveNew_10000()
		{
			await this.TestMultiple(10000, false, null);
			await ExportXML(this.file, "Data\\BTree.xml");
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_09_SaveNew_10000_Statistics()
		{
			await this.TestMultiple(10000, false, 100);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_10_SaveNew_100000()
		{
			await this.TestMultiple(100000, false, null);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_11_SaveNew_100000_Statistics()
		{
			await this.TestMultiple(100000, false, 1000);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_12_SaveNew_1000000()
		{
			await this.TestMultiple(1000000, false, null);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_13_SaveNew_1000000_Statistics()
		{
			await this.TestMultiple(1000000, false, 10000);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_14_Contains()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Simple Obj2 = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			AssertEx.NotSame(Guid.Empty, ObjectId);
			Assert.IsTrue(this.file.Contains(Obj));
			Assert.IsFalse(this.file.Contains(Obj2));
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_15_Count()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			AssertEx.NotSame(Guid.Empty, ObjectId);
			Console.Out.WriteLine(this.file.Count.ToString());
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_16_Clear()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			AssertEx.NotSame(Guid.Empty, ObjectId);
			Assert.IsTrue(this.file.Contains(Obj));
			this.file.Clear();
			Assert.IsFalse(this.file.Contains(Obj));
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_17_NormalEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Guid? Prev = null;

			foreach (GenericObject Obj in this.file)
			{
				if (Prev.HasValue)
					AssertEx.Less(Prev.Value, Obj.ObjectId);

				Prev = Obj.ObjectId;
				Assert.IsTrue(Objects.Remove(Obj.ObjectId));
			}

			AssertEx.Same(0, Objects.Count);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_18_TypedEnumeration()
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
						AssertEx.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					AssertEx.Same(Rank++, e.CurrentRank);
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			AssertEx.Same(0, Objects.Count);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_19_LockedEnumeration()
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
						AssertEx.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					AssertEx.Same(Rank++, e.CurrentRank);
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			AssertEx.Same(0, Objects.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(AggregateException))]
		public async Task DBFiles_BTree_Test_20_UnlockedChangeEnumeration()
		{
			await this.CreateObjects(Math.Min(ObjectsToEnumerate, 1000));
			Simple Obj;

			using (ObjectBTreeFileEnumerator<Simple> e = this.file.GetTypedEnumerator<Simple>(true))
			{
				while (e.MoveNext())
				{
					Obj = e.Current;
					Obj = CreateSimple(this.MaxStringLength);
					await this.file.SaveNewObject(Obj);
				}
			}
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_21_BackwardsEnumeration()
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
						AssertEx.Greater(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					AssertEx.Same(--Rank, e.CurrentRank);
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			AssertEx.Same(0, Objects.Count);
		}

		private async Task<SortedDictionary<Guid, Simple>> CreateObjects(int NrObjects)
		{
			SortedDictionary<Guid, Simple> Result = new SortedDictionary<Guid, Simple>();

			while (NrObjects > 0)
			{
				Simple Obj = CreateSimple(this.MaxStringLength);
				Guid ObjectId = await this.file.SaveNewObject(Obj);
				Result[ObjectId] = Obj;
				NrObjects--;
			}

			return Result;
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_22_SelectIthObject()
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
								AssertEx.Less(Prev.Value, Obj.ObjectId);

							Prev = Obj.ObjectId;
							DBFilesObjectSerializationTests.AssertEqual(Ordered[i + j], Obj);

							AssertEx.Same(i + j, e.CurrentRank);
							AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
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
								AssertEx.Greater(Prev.Value, Obj.ObjectId);

							Prev = Obj.ObjectId;
							DBFilesObjectSerializationTests.AssertEqual(Ordered[i - j], Obj);

							AssertEx.Same(i - j, e.CurrentRank);
							AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
						}
						while (e.MovePrevious() && j++ < 10);
					}
				}
			}
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_23_RankObject()
		{
			int c = ObjectsToEnumerate;
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(c);
			Simple[] Ordered = new Simple[c];
			Objects.Values.CopyTo(Ordered, 0);
			int i;

			for (i = 0; i < c; i++)
				AssertEx.Same(i, await this.file.GetRank(Ordered[i].ObjectId));
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_24_Reset()
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
						AssertEx.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.ContainsKey(Obj.ObjectId));

					AssertEx.Same(Rank++, e.CurrentRank);
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}

				e.Reset();
				Prev = null;

				while (e.MovePrevious())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						AssertEx.Greater(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					AssertEx.Same(--Rank, e.CurrentRank);
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}
			}

			AssertEx.Same(0, Objects.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public async Task DBFiles_BTree_Test_25_UpdateUnsavedObject()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			await this.file.UpdateObject(Obj);
		}

		[TestMethod]
		[ExpectedException(typeof(KeyNotFoundException))]
		public async Task DBFiles_BTree_Test_26_UpdateUnexistentObject()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Obj.ObjectId = Guid.NewGuid();
			await this.file.UpdateObject(Obj);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_27_UpdateObject()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			Simple Obj2 = await this.file.LoadObject<Simple>(ObjectId);
			DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

			Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeBefore.xml"));

			Simple Obj3 = CreateSimple(this.MaxStringLength);
			Obj3.ObjectId = ObjectId;
			await this.file.UpdateObject(Obj3);

			Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeAfter.xml"));

			Obj2 = await this.file.LoadObject<Simple>(ObjectId);
			DBFilesObjectSerializationTests.AssertEqual(Obj3, Obj2);

			await AssertConsistent(this.file, this.provider, null, null, true);
			Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTree.xml"));
		}

		[TestMethod]
		public void DBFiles_BTree_Test_28_UpdateObjects_1000()
		{
			this.DBFiles_BTree_Test_UpdateObjects(1000).Wait();
		}

		[TestMethod]
		public void DBFiles_BTree_Test_29_UpdateObjects_10000()
		{
			this.DBFiles_BTree_Test_UpdateObjects(10000).Wait();
		}

		[TestMethod]
		public void DBFiles_BTree_Test_30_UpdateObjects_100000()
		{
			this.DBFiles_BTree_Test_UpdateObjects(100000).Wait();
		}

		private async Task DBFiles_BTree_Test_UpdateObjects(int c)
		{
			Simple[] Objects = new Simple[c];
			Simple Obj;
			int i;

			for (i = 0; i < c; i++)
			{
				Objects[i] = Obj = CreateSimple(this.MaxStringLength);
				await this.file.SaveNewObject(Obj);
			}

			//await AssertConsistent(this.file, this.provider, null, null, true);
			//Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeBeforeUpdates.xml"));

			for (i = 0; i < c; i++)
			{
				Obj = CreateSimple(this.MaxStringLength);
				Obj.ObjectId = Objects[i].ObjectId;

				await this.file.UpdateObject(Obj);

				Objects[i] = Obj;

				Obj = await this.file.LoadObject<Simple>(Obj.ObjectId);
				DBFilesObjectSerializationTests.AssertEqual(Objects[i], Obj);
			}

			await AssertConsistent(this.file, this.provider, null, null, true);
			//Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeAfterUpdates.xml"));

			for (i = 0; i < c; i++)
			{
				Obj = await this.file.LoadObject<Simple>(Objects[i].ObjectId);
				DBFilesObjectSerializationTests.AssertEqual(Objects[i], Obj);
			}
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_31_DeleteObject()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(3, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_32_DeleteObject_100()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(100, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_33_DeleteObject_1000()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(1000, false);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_34_DeleteObject_10000()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(10000, false);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_35_DeleteObject_100000()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(100000, false);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_36_DeleteObject_1000_UntilFailure()
		{
			while (true)
			{
				await this.DBFiles_BTree_Test_DeleteObjects(1000, true);
			}
		}

		private async Task DBFiles_BTree_Test_DeleteObjects(int c, bool CheckForEachObject)
		{
			Random Gen = new Random();
			Simple[] Objects = new Simple[c];
			Simple Obj;
			int i;

			for (i = 0; i < c; i++)
			{
				Objects[i] = Obj = CreateSimple(this.MaxStringLength);
				await this.file.SaveNewObject(Obj);
			}

			while (c > 0)
			{
				i = Gen.Next(0, c);

				Obj = Objects[i];
				c--;
				if (i < c)
					Array.Copy(Objects, i + 1, Objects, i, c - i);

				if (CheckForEachObject)
				{
					try
					{
						this.provider.CloseFile(this.file.CollectionName);
						this.file = null;

						File.Copy(FileName, FileName + ".bak", true);
						File.Copy(BlobFileName, BlobFileName + ".bak", true);
						File.Copy(NamesFileName, NamesFileName + ".bak", true);

						this.file = await this.provider.GetFile(CollectionName);

						if (File.Exists(ObjIdFileName))
							File.Delete(ObjIdFileName);

						File.WriteAllBytes(ObjIdFileName, Obj.ObjectId.ToByteArray());

						if (File.Exists(BlockSizeFileName))
							File.Delete(BlockSizeFileName);

						File.WriteAllText(BlockSizeFileName, this.BlockSize.ToString());

						Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeBefore.xml"));
						Console.Out.WriteLine(Obj.ObjectId);
						await this.file.DeleteObject(Obj);
						//Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeAfter.xml"));
						await AssertConsistent(this.file, this.provider, null, null, true);

						for (i = 0; i < c; i++)
						{
							Obj = await this.file.LoadObject<Simple>(Objects[i].ObjectId);
							DBFilesObjectSerializationTests.AssertEqual(Objects[i], Obj);
						}
					}
					catch (Exception ex)
					{
						if (this.file != null)
							Console.Out.WriteLine(await ExportXML(this.file, "Data\\BTreeError.xml"));

						ExceptionDispatchInfo.Capture(ex).Throw();
					}
				}
				else
					await this.file.DeleteObject(Obj);
			}

			FileStatistics Stat = await AssertConsistent(this.file, this.provider, null, null, true);

			AssertEx.Same(0, this.file.Count);
			AssertEx.Same(1, Stat.NrBlocks);
			AssertEx.Same(0, Stat.NrBlobBlocks);
		}

		// TODO: Delete Object (check if rare error persists.)
	}
}
