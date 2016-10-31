using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Content;
using Waher.Persistence.Files.Test.Classes;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;

namespace Waher.Persistence.Files.Test
{
	[TestFixture]
	public class BTreeTests
	{
		private const string FileName = "Data\\Objects.db";
		private const int BlockSize = 1024;
		private const int BlocksInCache = 1000;

		private ObjectBTreeFile file;
		private FilesProvider provider;
		private Random gen = new Random();

		[SetUp]
		public void SetUp()
		{
			if (File.Exists(FileName))
				File.Delete(FileName);

			this.provider = new FilesProvider("Data", "Default");
			this.file = new ObjectBTreeFile(FileName, "Default", "Blobs", BlockSize, BlocksInCache, this.provider, Encoding.UTF8, 10000);
		}

		[TearDown]
		public void TearDown()
		{
			this.file.Dispose();
			this.file = null;

			this.provider.Dispose();
			this.provider = null;
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
			Result.String = Guid.NewGuid().ToString();
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
		public async void Test_01_SaveNew()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNew(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			await this.AssertConsistent();
		}

		private async Task AssertConsistent()
		{
			FileStatistics Statistics = await this.file.ComputeStatistics();

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
			Console.Out.WriteLine("Usage: " + (((double)Statistics.NrBytesUsed) / Statistics.NrBytesTotal * 100).ToString("F2") + " %");
			Console.Out.WriteLine("Is Corrupt: " + Statistics.IsCorrupt.ToString());
			Console.Out.WriteLine("Has Comments: " + Statistics.HasComments.ToString());

			if (Statistics.HasComments)
			{
				Console.Out.WriteLine();
				foreach (string Comment in Statistics.Comments)
					Console.Out.WriteLine(Comment);
			}

			if (Statistics.IsCorrupt)
			{
				StringBuilder sb = new StringBuilder();
				XmlWriterSettings Settings = XML.WriterSettings(true, true);
				using (XmlWriter w = XmlWriter.Create(sb, Settings))
				{
					await this.file.ExportGraphML(w);
					w.Flush();
				}

				File.WriteAllText("BTreeError.graphml", sb.ToString());

				Console.Out.WriteLine();
				Console.Out.WriteLine(sb.ToString());
				Console.Out.WriteLine();

				Assert.Fail("Database is corrupt.");
			}
		}

		[Test]
		[ExpectedException(typeof(IOException))]
		public async void Test_02_SaveOld()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNew(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);
			await this.file.SaveNew(Obj);

			await this.AssertConsistent();
		}

		[Test]
		public async void Test_03_LoadUntyped()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNew(Obj);
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

			await this.AssertConsistent();
		}

		[Test]
		public async void Test_03_LoadTyped()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNew(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			Simple Obj2 = await this.file.LoadObject<Simple>(ObjectId);

			ObjectSerializationTests.AssertEqual(Obj, Obj2);

			await this.AssertConsistent();
		}

		[Test]
		public async void Test_04_LoadUntyped()
		{
			Simple Obj = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNew(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			GenericObject Obj2 = (GenericObject)await this.file.LoadObject(ObjectId);

			ObjectSerializationTests.AssertEqual(Obj, Obj2);

			await this.AssertConsistent();
		}

		[Test]
		public async void Test_05_SaveNew_Multiple_NoSplit()
		{
			await this.TestMultiple(3, true);
		}

		private async Task TestMultiple(int c, bool AssertIndividually)
		{
			int i;
			Simple[] Objects = new Simple[c];
			Simple Obj2;

			for (i = 0; i < c; i++)
			{
				Objects[i] = this.CreateSimple();
				await this.file.SaveNew(Objects[i]);

				Console.Out.WriteLine();
				Console.Out.WriteLine((i + 1).ToString() + " objects:");
				Console.Out.WriteLine(new string('-', 80));

				if (AssertIndividually)
					await this.AssertConsistent();
			}

			for (i = 0; i < c; i++)
			{
				Obj2 = await this.file.LoadObject<Simple>(Objects[i].ObjectId);
				ObjectSerializationTests.AssertEqual(Objects[i], Obj2);
			}

			if (!AssertIndividually)
				await this.AssertConsistent();
		}

		[Test]
		public async void Test_06_SaveNew_Multiple_NodeSplit()
		{
			await this.TestMultiple(50, true);
		}

		// TODO: GraphML: http://graphml.graphdrawing.org/
		// TODO: Count property: Total number of objects in file.
		// TODO: IEnumerable, ICollection interfaces.
		// TODO: Delete Object
		// TODO: Multiple save (test node split) Enumerate, load all
		// TODO: Multiple delete (test node merge)
		// TODO: Update Object
		// TODO: Update Object (incl. node split)
		// TODO: Select i'th element.
		// TODO: Enumerate
		// TODO: Start enumeration from i'th element.
		// TODO: BLOBs
		// TODO: Update Object (normal -> BLOB)
		// TODO: Update Object (BLOB -> normal)
		// TODO: Statistics (nr objects, size, used vs. unused space)
		// TODO: Stress test
		// TODO: Check that node counts are correct after all tests.
	}
}
