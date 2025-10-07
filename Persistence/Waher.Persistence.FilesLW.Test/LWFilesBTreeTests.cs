﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Exceptions;
using Waher.Persistence.Files.Statistics;
using Waher.Persistence.Serialization;
using Waher.Content;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Security;

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
		internal const string DefaultFileName = "Data\\Default.btree";
		internal const string DefaultBlobFileName = "Data\\Default.blob";
		internal const string DefaultLabelsFileName = "Data\\Default.labels";
		internal const string CollectionName = "Default";
		internal const string ObjFileName = "Data\\LastObject.bin";
		internal const string ObjIdFileName = "Data\\LastObjectId.bin";
		internal const string BlockSizeFileName = "Data\\BlockSize.bin";
#if LW
		internal const string Index1FileName = "Data\\Default.btree.Byte.-DateTime.index";
		internal const string Index2FileName = "Data\\Default.btree.ShortString.index";
		internal const string Index3FileName = "Data\\Default.btree.CIString.index";
#else
		internal const string Index1FileName = "Data\\Default.btree.50104c1cdc9b0754886b272fc1aaa550747dadf4.index";
		internal const string Index2FileName = "Data\\Default.btree.40059d366b589d4071aba631a3aa4fc1dc03e357.index";
		internal const string Index3FileName = "Data\\Default.btree.f1c0be1209b1cdc095103cf364d416abeb3fdf2f.index";
#endif
		internal const string Folder = "Data";
		internal const int BlocksInCache = 10000;
		internal const int ObjectsToEnumerate = 1000;

		protected static readonly Random gen = new();
		protected ObjectBTreeFile file;
		protected FilesProvider provider;
		protected DateTime start;

		public abstract int BlockSize
		{
			get;
		}

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(DBFilesBTreeTests).Assembly,
				typeof(Expression).Assembly,
				typeof(Script.Persistence.SQL.Select).Assembly,
				typeof(Duration).Assembly);

			Types.SetModuleParameter("Data", "Data");
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			DeleteFiles();

#if LW
			this.provider = await FilesProvider.CreateAsync(Folder, CollectionName, this.BlockSize, BlocksInCache, Math.Max(this.BlockSize / 2, 1024), Encoding.UTF8, 10000);
#else
			this.provider = await FilesProvider.CreateAsync(Folder, CollectionName, this.BlockSize, BlocksInCache, Math.Max(this.BlockSize / 2, 1024), Encoding.UTF8, 10000, true);
#endif
			this.file = await this.provider.GetFile(CollectionName);
			this.start = DateTime.Now;
		}

		internal static void DeleteFiles()
		{
			if (File.Exists(MasterFileName + ".bak"))
				File.Delete(MasterFileName + ".bak");

			if (File.Exists(MasterFileName))
			{
				File.Copy(MasterFileName, MasterFileName + ".bak");
				File.Delete(MasterFileName);
			}

			if (File.Exists(DefaultFileName + ".bak"))
				File.Delete(DefaultFileName + ".bak");

			if (File.Exists(DefaultFileName))
			{
				File.Copy(DefaultFileName, DefaultFileName + ".bak");
				File.Delete(DefaultFileName);
			}

			if (File.Exists(DefaultBlobFileName + ".bak"))
				File.Delete(DefaultBlobFileName + ".bak");

			if (File.Exists(DefaultBlobFileName))
			{
				File.Copy(DefaultBlobFileName, DefaultBlobFileName + ".bak");
				File.Delete(DefaultBlobFileName);
			}

			if (File.Exists(DefaultLabelsFileName + ".bak"))
				File.Delete(DefaultLabelsFileName + ".bak");

			if (File.Exists(DefaultLabelsFileName))
			{
				File.Copy(DefaultLabelsFileName, DefaultLabelsFileName + ".bak");
				File.Delete(DefaultLabelsFileName);
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

			if (File.Exists(Index3FileName + ".bak"))
				File.Delete(Index3FileName + ".bak");

			if (File.Exists(Index3FileName))
			{
				File.Copy(Index3FileName, Index3FileName + ".bak");
				File.Delete(Index3FileName);
			}
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			ConsoleOut.WriteLine("Elapsed time: " + (DateTime.Now - this.start).ToString());

			if (this.provider is not null)
			{
				await this.provider.DisposeAsync();
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
			Simple Result = new()
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
				DateTimeOffset = new DateTimeOffset(new DateTime(1900, 1, 1), TimeSpan.Zero).AddDays(gen.NextDouble() * 73049),
				TimeSpan = new TimeSpan((long)(gen.NextDouble() * 36000000000)),
				Guid = Guid.NewGuid(),
				String = GenerateRandomString(MaxStringLength),
				ShortString = GenerateRandomString(10)
			};

			Result.CIString = Result.ShortString;

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

		internal static string GenerateRandomString(int MaxStringLength)
		{
			int i, c = gen.Next(10, MaxStringLength);
			char[] ch = new char[c];

			for (i = 0; i < c; i++)
				ch[i] = (char)gen.Next(32, 127);

			return new string(ch);
		}

		internal static Default CreateDefault(int MaxStringLength)
		{
			Default Result = new()
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
				Guid = Guid.NewGuid(),
				String = GenerateRandomString(MaxStringLength)
			};

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
			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
			ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTree.xml", false));
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		[Ignore]
		public async Task DBFiles_BTree_Test_01_X_Repeat_DBFiles_BTree_Test_01()
		{
			object LastObjectAdded = null;

			try
			{
				List<Simple> Objects = [];
				List<Guid> ObjectIds = [];
				int i, c = 0;

				Simple Obj = CreateSimple(this.MaxStringLength);
				Guid ObjectId = await this.file.SaveNewObject(LastObjectAdded = Obj, false, null);
				AssertEx.NotSame(Guid.Empty, ObjectId);

				Objects.Add(Obj);
				ObjectIds.Add(ObjectId);
				c++;

				Simple Obj2 = await this.file.LoadObject<Simple>(ObjectIds[0]);
				DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

				while (true)
				{
					await this.TestCleanup();
					await this.TestInitialize();

					Obj = CreateSimple(this.MaxStringLength);
					ObjectId = await this.file.SaveNewObject(LastObjectAdded = Obj, false, null);
					AssertEx.NotSame(Guid.Empty, ObjectId);

					//FileStatistics Stat = await AssertConsistent(this.file, this.provider, null, Obj, false);

					Objects.Add(Obj);
					ObjectIds.Add(ObjectId);
					c++;

					for (i = 0; i < c; i++)
						Obj2 = await this.file.LoadObject<Simple>(ObjectIds[i]);

					DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

					//ConsoleOut.Write(Stat.NrObjects.ToString() + " ");
					ConsoleOut.Write(c.ToString() + " ");
				}
			}
			catch (Exception ex)
			{
				await SaveLastObject(this.provider, LastObjectAdded);

				ExceptionDispatchInfo.Capture(ex).Throw();
			}
		}

		internal static async Task<string> ExportXML(ObjectBTreeFile File, string XmlFileName, bool Locked)
		{
			string Xml = await File.ExportGraphXML(false, Locked);

			if (!string.IsNullOrEmpty(XmlFileName))
				System.IO.File.WriteAllText(XmlFileName, Xml);

			return Xml;
		}

		internal static async Task<FileStatistics> AssertConsistent(ObjectBTreeFile File, FilesProvider Provider, int? ExpectedNrObjects, object LastObjectAdded,
			bool WriteStat)
		{
			await File.BeginWrite();
			try
			{
				return await AssertConsistentLocked(File, Provider, ExpectedNrObjects, LastObjectAdded, WriteStat);
			}
			finally
			{
				await File.EndWrite();
			}
		}

		internal static async Task<FileStatistics> AssertConsistentLocked(ObjectBTreeFile File, FilesProvider Provider, int? ExpectedNrObjects, object LastObjectAdded,
			bool WriteStat)
		{
			KeyValuePair<FileStatistics, Dictionary<Guid, bool>> P = await File.ComputeStatisticsLocked();
			FileStatistics Statistics = P.Key;

			if (WriteStat)
			{
				ConsoleOut.WriteLine("Block Size: " + Statistics.BlockSize.ToString());
				ConsoleOut.WriteLine("#Blocks: " + Statistics.NrBlocks.ToString());
				ConsoleOut.WriteLine("#BLOB Blocks: " + Statistics.NrBlobBlocks.ToString());
				ConsoleOut.WriteLine("#Bytes used: " + Statistics.NrBytesUsed.ToString());
				ConsoleOut.WriteLine("#Bytes unused: " + Statistics.NrBytesUnused.ToString());
				ConsoleOut.WriteLine("#Bytes total: " + Statistics.NrBytesTotal.ToString());
				ConsoleOut.WriteLine("#BLOB Bytes used: " + Statistics.NrBlobBytesUsed.ToString());
				ConsoleOut.WriteLine("#BLOB Bytes unused: " + Statistics.NrBlobBytesUnused.ToString());
				ConsoleOut.WriteLine("#BLOB Bytes total: " + Statistics.NrBlobBytesTotal.ToString());
				ConsoleOut.WriteLine("#Block loads: " + Statistics.NrBlockLoads.ToString());
				ConsoleOut.WriteLine("#Cache loads: " + Statistics.NrCacheLoads.ToString());
				ConsoleOut.WriteLine("#Block saves: " + Statistics.NrBlockSaves.ToString());
				ConsoleOut.WriteLine("#BLOB Block loads: " + Statistics.NrBlobBlockLoads.ToString());
				ConsoleOut.WriteLine("#BLOB Block saves: " + Statistics.NrBlobBlockSaves.ToString());
				ConsoleOut.WriteLine("#Objects: " + Statistics.NrObjects.ToString());
				ConsoleOut.WriteLine("Smallest object: " + Statistics.MinObjectSize.ToString());
				ConsoleOut.WriteLine("Largest object: " + Statistics.MaxObjectSize.ToString());
				ConsoleOut.WriteLine("Average object: " + Statistics.AverageObjectSize.ToString("F1"));
				ConsoleOut.WriteLine("Usage: " + Statistics.Usage.ToString("F2") + " %");
				ConsoleOut.WriteLine("Min(Depth): " + Statistics.MinDepth.ToString());
				ConsoleOut.WriteLine("Max(Depth): " + Statistics.MaxDepth.ToString());
				ConsoleOut.WriteLine("Min(Objects/Block): " + Statistics.MinObjectsPerBlock.ToString());
				ConsoleOut.WriteLine("Max(Objects/Block): " + Statistics.MaxObjectsPerBlock.ToString());
				ConsoleOut.WriteLine("Avg(Objects/Block): " + Statistics.AverageObjectsPerBlock.ToString("F1"));
				ConsoleOut.WriteLine("Min(Bytes Used/Block): " + Statistics.MinBytesUsedPerBlock.ToString());
				ConsoleOut.WriteLine("Max(Bytes Used/Block): " + Statistics.MaxBytesUsedPerBlock.ToString());
				ConsoleOut.WriteLine("Avg(Bytes Used/Block): " + Statistics.AverageBytesUsedPerBlock.ToString("F1"));
				ConsoleOut.WriteLine("Is Corrupt: " + Statistics.IsCorrupt.ToString());
				ConsoleOut.WriteLine("Is Balanced: " + Statistics.IsBalanced.ToString());
				ConsoleOut.WriteLine("Has Comments: " + Statistics.HasComments.ToString());
			}

			if (Statistics.HasComments)
			{
				ConsoleOut.WriteLine();
				foreach (string Comment in Statistics.Comments)
					ConsoleOut.WriteLine(Comment);
			}

			try
			{
				if (Statistics.IsCorrupt || !Statistics.IsBalanced)
				{
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine(await ExportXML(File, "Data\\BTreeError.xml", true));
					ConsoleOut.WriteLine();

					Assert.IsFalse(Statistics.IsCorrupt, "Database is corrupt.");
					Assert.IsTrue(Statistics.IsBalanced, "Database is unbalanced.");
				}

				if (ExpectedNrObjects.HasValue)
					AssertEx.Same(ExpectedNrObjects.Value, Statistics.NrObjects);
			}
			catch (Exception ex)
			{
				await SaveLastObject(Provider, LastObjectAdded);

				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			foreach (IndexBTreeFile Index in File.Indices)
				await AssertConsistentLocked(Index.IndexFileLocked, Provider, ExpectedNrObjects, null, WriteStat);

			return Statistics;
		}

		private static async Task SaveLastObject(FilesProvider Provider, object LastObjectAdded)
		{
			if (LastObjectAdded is not null)
			{
				IObjectSerializer Serializer = await Provider.GetObjectSerializer(LastObjectAdded.GetType());
				BinarySerializer Writer = new(CollectionName, Encoding.UTF8);
				await Serializer.Serialize(Writer, false, false, LastObjectAdded, null);
				byte[] Bin = Writer.GetSerialization();
				File.WriteAllBytes(ObjFileName, Bin);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(KeyAlreadyExistsException))]
		public async Task DBFiles_BTree_Test_02_SaveOld()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
			AssertEx.NotSame(Guid.Empty, ObjectId);
			await this.file.SaveNewObject(Obj, false, null);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_03_LoadUntyped()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
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
			AssertEx.Same(Obj.DateTimeOffset, GenObj["DateTimeOffset"]);
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
			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			Simple Obj2 = await this.file.LoadObject<Simple>(ObjectId);

			DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_04_LoadUntyped()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			GenericObject Obj2 = (GenericObject)await this.file.LoadObject(ObjectId);

			DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_05_SaveNew_Multiple_NoSplit()
		{
			await this.TestMultiple(3, 1, 1, true, null);
		}

		private async Task TestMultiple(int NrObjects, int ArraySize, int BulkSize, bool AssertIndividually, int? LogStatisticsEvery)
		{
			DateTime Start = DateTime.Now;
			List<FileStatistics> Stat = null;
			List<double> Milliseconds = null;
			int i = 0, j;
			Simple[] Objects = new Simple[NrObjects];
			Simple[] Block = new Simple[ArraySize];
			Simple Obj2;
			ObjectSerializer Serializer = await this.provider.GetObjectSerializerEx(typeof(Simple));

			if (BulkSize > 1)
				await this.provider.StartBulk();

			while (i < NrObjects)
			{
				for (j = 0; j < ArraySize; j++)
				{
					Block[j] = CreateSimple(this.MaxStringLength);
					Objects[i++] = Block[j];
				}

				if (ArraySize > 1)
					await this.file.SaveNewObjects(Block, Serializer, false, null);
				else
					await this.file.SaveNewObject(Block[0], false, null);

				if (BulkSize > 1 && i % BulkSize == 0)
				{
					await this.provider.EndBulk();
					await this.provider.StartBulk();
				}

				if (AssertIndividually)
				{
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine(i.ToString() + " objects:");
					ConsoleOut.WriteLine(new string('-', 80));

					await AssertConsistent(this.file, this.provider, i, Objects[i - 1], true);
				}

				if (LogStatisticsEvery.HasValue && i % LogStatisticsEvery.Value == 0)
				{
					if (Stat is null)
					{
						Milliseconds = [];
						Stat = [];
					}

					Milliseconds.Add((DateTime.Now - Start).TotalMilliseconds / LogStatisticsEvery.Value);
					Stat.Add((await this.file.ComputeStatistics()).Key);

					Start = DateTime.Now;
				}
			}

			if (BulkSize > 1)
				await this.provider.EndBulk();

			for (i = 0; i < NrObjects; i++)
			{
				Obj2 = await this.file.LoadObject<Simple>(Objects[i].ObjectId);
				DBFilesObjectSerializationTests.AssertEqual(Objects[i], Obj2);
			}

			if (!AssertIndividually)
				await AssertConsistent(this.file, this.provider, NrObjects, null, true);

			if (Stat is not null)
			{
				Variables v = new()
				{
					{ "Stat", Stat.ToArray() },
					{ "ms", Milliseconds.ToArray() },
					{ "StepSize", LogStatisticsEvery.Value }
				};

				Expression Exp = new("[ms,Stat.BlockSize,Stat.NrBlocks,Stat.NrBytesUsed,Stat.NrBytesUnused,Stat.NrBytesTotal," +
					"Stat.Usage,Stat.NrObjects,Stat.MinObjectSize,Stat.MaxObjectSize,Stat.AverageObjectSize,Stat.MinDepth,Stat.MaxDepth," +
					"Stat.NrBlockLoads,Stat.NrCacheLoads,Stat.NrBlockSaves,Stat.MinObjectsPerBlock,Stat.MaxObjectsPerBlock," +
					"Stat.AverageObjectsPerBlock,Stat.MinBytesUsedPerBlock,Stat.MaxBytesUsedPerBlock,Stat.AverageBytesUsedPerBlock]T");

				ConsoleOut.WriteLine("ms, BlockSize, NrBlocks, NrBytesUsed, NrBytesUnused, NrBytesTotal, " +
					"Usage, NrObjects, MinObjectSize, MaxObjectSize, AverageObjectSize, MinDepth, MaxDepth, " +
					"NrBlockLoads, NrCacheLoads, NrBlockSaves,Min(Obj/Block),Max(Obj/Block),Avg(Obj/Block)," +
					"Min(UsedBytes/Block),Max(UsedBytes/Block),Avg(UsedBytes/Block)");
				ConsoleOut.WriteLine(new string('-', 80));
				ConsoleOut.WriteLine((await Exp.EvaluateAsync(v)).ToString());
			}
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_06_SaveNew_Multiple_NodeSplit()
		{
			await this.TestMultiple(100, 1, 1, true, null);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_07_SaveNew_1000()
		{
			await this.TestMultiple(1000, 100, 1000, false, null);
			await ExportXML(this.file, "Data\\BTree.xml", false);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_08_SaveNew_10000()
		{
			await this.TestMultiple(10000, 100, 1000, false, null);
			await ExportXML(this.file, "Data\\BTree.xml", false);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_09_SaveNew_10000_Statistics()
		{
			await this.TestMultiple(10000, 100, 100, false, 100);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_10_SaveNew_100000()
		{
			await this.TestMultiple(100000, 100, 1000, false, null);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_11_SaveNew_100000_Statistics()
		{
			await this.TestMultiple(100000, 100, 1000, false, 1000);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_12_SaveNew_1000000()
		{
			await this.TestMultiple(1000000, 100, 1000, false, null);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_13_SaveNew_1000000_Statistics()
		{
			await this.TestMultiple(1000000, 100, 1000, false, 10000);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_14_Contains()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Simple Obj2 = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
			AssertEx.NotSame(Guid.Empty, ObjectId);
			Assert.IsTrue(await this.file.ContainsAsync(Obj));
			Assert.IsFalse(await this.file.ContainsAsync(Obj2));
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_15_Count()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
			AssertEx.NotSame(Guid.Empty, ObjectId);
			ConsoleOut.WriteLine((await this.file.CountAsync).ToString());
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_16_Clear()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
			AssertEx.NotSame(Guid.Empty, ObjectId);
			Assert.IsTrue(await this.file.ContainsAsync(Obj));
			await this.file.ClearAsync();
			Assert.IsFalse(await this.file.ContainsAsync(Obj));
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_17_NormalEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Guid? Prev = null;

			await this.file.BeginRead();
			try
			{
				ObjectBTreeFileCursor<object> e = await this.file.GetEnumeratorAsyncLocked();

				while (await e.MoveNextAsyncLocked())
				{
					Simple Obj = (Simple)e.Current;
					if (Prev.HasValue)
						AssertEx.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));
				}
			}
			finally
			{
				await this.file.EndRead();
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

			await this.file.BeginRead();
			try
			{
				ObjectBTreeFileCursor<Simple> e = await this.file.GetTypedEnumeratorAsyncLocked<Simple>();

				while (await e.MoveNextAsyncLocked())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						AssertEx.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					AssertEx.Same(Rank++, await e.GetCurrentRankLocked());
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}
			}
			finally
			{
				await this.file.EndRead();
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

			await this.file.BeginRead();
			try
			{
				ObjectBTreeFileCursor<Simple> e = await this.file.GetTypedEnumeratorAsyncLocked<Simple>();

				while (await e.MoveNextAsyncLocked())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						AssertEx.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					AssertEx.Same(Rank++, await e.GetCurrentRankLocked());
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}
			}
			finally
			{
				await this.file.EndRead();
			}

			AssertEx.Same(0, Objects.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task DBFiles_BTree_Test_20_UnlockedChangeEnumeration()
		{
			await this.CreateObjects(Math.Min(ObjectsToEnumerate, 1000));
			Simple Obj;

			ObjectBTreeFileCursor<Simple> e = await this.file.GetTypedEnumeratorAsyncLocked<Simple>();

			while (await e.MoveNextAsyncLocked())
			{
				Simple _ = e.Current;
				Obj = CreateSimple(this.MaxStringLength);
				await this.file.SaveNewObject(Obj, false, null);
			}
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_21_BackwardsEnumeration()
		{
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(ObjectsToEnumerate);
			Guid? Prev = null;
			Simple Obj;
			ulong Rank = ObjectsToEnumerate;

			await this.file.BeginRead();
			try
			{
				ObjectBTreeFileCursor<Simple> e = await this.file.GetTypedEnumeratorAsyncLocked<Simple>();

				while (await e.MovePreviousAsyncLocked())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						AssertEx.Greater(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					AssertEx.Same(--Rank, await e.GetCurrentRankLocked());
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}
			}
			finally
			{
				await this.file.EndRead();
			}

			AssertEx.Same(0, Objects.Count);
		}

		private async Task<SortedDictionary<Guid, Simple>> CreateObjects(int NrObjects)
		{
			SortedDictionary<Guid, Simple> Result = [];

			await this.provider.StartBulk();

			while (NrObjects > 0)
			{
				Simple Obj = CreateSimple(this.MaxStringLength);
				Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
				Result[ObjectId] = Obj;
				NrObjects--;
			}

			await this.provider.EndBulk();

			return Result;
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_22_SelectIthObject()
		{
			int c = ObjectsToEnumerate;
			SortedDictionary<Guid, Simple> Objects = await this.CreateObjects(c);
			Simple[] Ordered = new Simple[c];
			Objects.Values.CopyTo(Ordered, 0);
			Guid? Prev;
			Simple Obj;
			Random gen = new();
			int i, j;

			for (i = 0; i < c; i++)
			{
				j = 0;
				Prev = null;

				if (i < 10 || (gen.Next(0, 2) == 0 && i <= c - 10))
				{
					await this.file.BeginRead();
					try
					{
						ObjectBTreeFileCursor<Simple> e = await this.file.GetTypedEnumeratorAsyncLocked<Simple>();

						Assert.IsTrue(await e.GoToObjectLocked((uint)i));

						do
						{
							Obj = e.Current;
							if (Prev.HasValue)
								AssertEx.Less(Prev.Value, Obj.ObjectId);

							Prev = Obj.ObjectId;
							DBFilesObjectSerializationTests.AssertEqual(Ordered[i + j], Obj);

							AssertEx.Same(i + j, await e.GetCurrentRankLocked());
							AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
						}
						while (await e.MoveNextAsyncLocked() && j++ < 10);
					}
					finally
					{
						await this.file.EndRead();
					}
				}
				else
				{
					await this.file.BeginRead();
					try
					{
						ObjectBTreeFileCursor<Simple> e = await this.file.GetTypedEnumeratorAsyncLocked<Simple>();

						Assert.IsTrue(await e.GoToObjectLocked((uint)i));

						do
						{
							Obj = e.Current;
							if (Prev.HasValue)
								AssertEx.Greater(Prev.Value, Obj.ObjectId);

							Prev = Obj.ObjectId;
							DBFilesObjectSerializationTests.AssertEqual(Ordered[i - j], Obj);

							AssertEx.Same(i - j, await e.GetCurrentRankLocked());
							AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
						}
						while (await e.MovePreviousAsyncLocked() && j++ < 10);
					}
					finally
					{
						await this.file.EndRead();
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

			await this.file.BeginRead();
			try
			{
				ObjectBTreeFileCursor<Simple> e = await this.file.GetTypedEnumeratorAsyncLocked<Simple>();

				while (await e.MoveNextAsyncLocked())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						AssertEx.Less(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.ContainsKey(Obj.ObjectId));

					AssertEx.Same(Rank++, await e.GetCurrentRankLocked());
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}

				e.Reset();
				Prev = null;

				while (await e.MovePreviousAsyncLocked())
				{
					Obj = e.Current;
					if (Prev.HasValue)
						AssertEx.Greater(Prev.Value, Obj.ObjectId);

					Prev = Obj.ObjectId;
					Assert.IsTrue(Objects.Remove(Obj.ObjectId));

					AssertEx.Same(--Rank, await e.GetCurrentRankLocked());
					AssertEx.Same(Obj.ObjectId, e.CurrentObjectId);
				}
			}
			finally
			{
				await this.file.EndRead();
			}

			AssertEx.Same(0, Objects.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(SerializationException))]
		public async Task DBFiles_BTree_Test_25_UpdateUnsavedObject()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			await this.file.UpdateObject(Obj, false, null);
		}

		[TestMethod]
		[ExpectedException(typeof(KeyNotFoundException))]
		public async Task DBFiles_BTree_Test_26_UpdateUnexistentObject()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Obj.ObjectId = Guid.NewGuid();
			await this.file.UpdateObject(Obj, false, null);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_27_UpdateObject()
		{
			Simple Obj = CreateSimple(this.MaxStringLength);
			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			Simple Obj2 = await this.file.LoadObject<Simple>(ObjectId);
			DBFilesObjectSerializationTests.AssertEqual(Obj, Obj2);

			ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTreeBefore.xml", false));

			Simple Obj3 = CreateSimple(this.MaxStringLength);
			Obj3.ObjectId = ObjectId;
			await this.file.UpdateObject(Obj3, false, null);

			ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTreeAfter.xml", false));

			Obj2 = await this.file.LoadObject<Simple>(ObjectId);
			DBFilesObjectSerializationTests.AssertEqual(Obj3, Obj2);

			await AssertConsistent(this.file, this.provider, null, null, true);
			ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTree.xml", false));
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_28_UpdateObjects_1000()
		{
			await this.DBFiles_BTree_Test_UpdateObjects(1000);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_29_UpdateObjects_10000()
		{
			await this.DBFiles_BTree_Test_UpdateObjects(10000);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_30_UpdateObjects_100000()
		{
			await this.DBFiles_BTree_Test_UpdateObjects(100000);
		}

		private async Task DBFiles_BTree_Test_UpdateObjects(int c)
		{
			Simple[] Objects = new Simple[c];
			Simple Obj;
			int i;

			for (i = 0; i < c; i++)
			{
				Objects[i] = Obj = CreateSimple(this.MaxStringLength);
				await this.file.SaveNewObject(Obj, false, null);
			}

			//await AssertConsistent(this.file, this.provider, null, null, true);
			//ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTreeBeforeUpdates.xml"));

			for (i = 0; i < c; i++)
			{
				Obj = CreateSimple(this.MaxStringLength);
				Obj.ObjectId = Objects[i].ObjectId;

				await this.file.UpdateObject(Obj, false, null);

				Objects[i] = Obj;

				Obj = await this.file.LoadObject<Simple>(Obj.ObjectId);
				DBFilesObjectSerializationTests.AssertEqual(Objects[i], Obj);
			}

			await AssertConsistent(this.file, this.provider, null, null, true);
			//ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTreeAfterUpdates.xml"));

			for (i = 0; i < c; i++)
			{
				Obj = await this.file.LoadObject<Simple>(Objects[i].ObjectId);
				DBFilesObjectSerializationTests.AssertEqual(Objects[i], Obj);
			}
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_31_DeleteObject()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(3, 1, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_32_DeleteObject_100()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(100, 1, true);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_33_DeleteObject_1000()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(1000, 100, false);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_34_DeleteObject_10000()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(10000, 1000, false);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_35_DeleteObject_100000()
		{
			await this.DBFiles_BTree_Test_DeleteObjects(100000, 1000, false);
		}

		[TestMethod]
		[Ignore]
		public async Task DBFiles_BTree_Test_36_DeleteObject_1000_UntilFailure()
		{
			while (true)
			{
				await this.DBFiles_BTree_Test_DeleteObjects(1000, 1, true);
			}
		}

		private async Task DBFiles_BTree_Test_DeleteObjects(int c, int BulkSize, bool CheckForEachObject)
		{
			Random Gen = new();
			Simple[] Objects = new Simple[c];
			Simple Obj;
			int i;

			await this.file.ClearAsync();

			if (BulkSize > 1)
				await this.provider.StartBulk();

			for (i = 0; i < c; i++)
			{
				if (BulkSize > 1 && i % BulkSize == 0)
				{
					await this.provider.EndBulk();
					await this.provider.StartBulk();
				}

				Objects[i] = Obj = CreateSimple(this.MaxStringLength);
				await this.file.SaveNewObject(Obj, false, null);
			}

			if (BulkSize > 1)
			{
				await this.provider.EndBulk();
				await this.provider.StartBulk();
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

						File.Copy(DefaultFileName, DefaultFileName + ".bak", true);
						File.Copy(DefaultBlobFileName, DefaultBlobFileName + ".bak", true);
						File.Copy(DefaultLabelsFileName, DefaultLabelsFileName + ".bak", true);

						this.file = await this.provider.GetFile(CollectionName);

						if (File.Exists(ObjIdFileName))
							File.Delete(ObjIdFileName);

						File.WriteAllBytes(ObjIdFileName, Obj.ObjectId.ToByteArray());

						if (File.Exists(BlockSizeFileName))
							File.Delete(BlockSizeFileName);

						File.WriteAllText(BlockSizeFileName, this.BlockSize.ToString());

						ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTreeBefore.xml", false));
						ConsoleOut.WriteLine(Obj.ObjectId);
						await this.file.DeleteObject(Obj, false, null);
						//ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTreeAfter.xml"));
						await AssertConsistent(this.file, this.provider, null, null, true);

						for (i = 0; i < c; i++)
						{
							Obj = await this.file.LoadObject<Simple>(Objects[i].ObjectId);
							DBFilesObjectSerializationTests.AssertEqual(Objects[i], Obj);
						}
					}
					catch (Exception ex)
					{
						if (this.file is not null)
							ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTreeError.xml", false));

						ExceptionDispatchInfo.Capture(ex).Throw();
					}
				}
				else
					await this.file.DeleteObject(Obj, false, null);

				if (BulkSize > 1 && c % BulkSize == 0)
				{
					await this.provider.EndBulk();
					await this.provider.StartBulk();
				}
			}

			if (BulkSize > 1)
				await this.provider.EndBulk();

			FileStatistics Stat = await AssertConsistent(this.file, this.provider, null, null, true);

			AssertEx.Same(0, await this.file.GetObjectCount(0, true));
			AssertEx.Same(1, Stat.NrBlocks);
			AssertEx.Same(0, Stat.NrBlobBlocks);
		}

		[TestMethod]
		public async Task DBFiles_BTree_Test_37_BinaryLifeCycle()
		{
			DockerBlob Obj = new()
			{
				AccountName = "Some user",
				FileName = "Some file"
			};

			Guid ObjectId = await this.file.SaveNewObject(Obj, false, null);
			AssertEx.NotSame(Guid.Empty, ObjectId);

			await AssertConsistent(this.file, this.provider, 1, Obj, true);
			ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTree.xml", false));

			Obj.Function = HashFunction.SHA256;
			Obj.Digest = Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes("Hello World."));

			await this.file.UpdateObject(Obj, false, null);

			ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTreeAfter.xml", false));

			DockerBlob Obj2 = await this.file.LoadObject<DockerBlob>(ObjectId);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Function, Obj2.Function);
			Assert.AreEqual(Convert.ToBase64String(Obj.Digest), Convert.ToBase64String(Obj2.Digest));
			Assert.AreEqual(Obj.FileName, Obj2.FileName);
			Assert.AreEqual(Obj.AccountName, Obj2.AccountName);

			await AssertConsistent(this.file, this.provider, null, null, true);
			ConsoleOut.WriteLine(await ExportXML(this.file, "Data\\BTree.xml", false));

		}

		// TODO: Delete Object (check if rare error persists.)
	}
}
