using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
	public class DBFilesStringDictionaryTests
	{
		internal const string FileName = "Data\\Dictionary.btree";
		internal const string Folder = "Data";
		internal const string BlobFileName = "Data\\Dictionary.blob";
		internal const string CollectionName = "Default";
		internal const int BlocksInCache = 10000;
		internal const int ObjectsToEnumerate = 1000;

		protected StringDictionary file;
		protected FilesProvider provider;
		protected DateTime start;

		[TestInitialize]
		public void TestInitialize()
		{
			if (File.Exists(DBFilesBTreeTests.MasterFileName + ".bak"))
				File.Delete(DBFilesBTreeTests.MasterFileName + ".bak");

			if (File.Exists(DBFilesBTreeTests.MasterFileName))
			{
				File.Copy(DBFilesBTreeTests.MasterFileName, DBFilesBTreeTests.MasterFileName + ".bak");
				File.Delete(DBFilesBTreeTests.MasterFileName);
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

#if LW
			this.provider = new FilesProvider(Folder, CollectionName, 8192, BlocksInCache, 8192, Encoding.UTF8, 10000);
#else
			this.provider = new FilesProvider(Folder, CollectionName, 8192, BlocksInCache, 8192, Encoding.UTF8, 10000, true);
#endif
			this.file = new StringDictionary(FileName, BlobFileName, CollectionName, this.provider, false);
			this.start = DateTime.Now;
		}

		[TestCleanup]
		public void TestCleanup()
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

		[TestMethod]
		public async Task DBFiles_StringDictionary_01_Set()
		{
			await Test_Set(100);
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_02_Set_BLOB()
		{
			await Test_Set(100000);
		}

		private async Task Test_Set(int MaxLen)
		{
			byte[] ByteArray = this.GetBytes(MaxLen, 0);
			Simple Obj = DBFilesBTreeTests.CreateSimple(MaxLen);

			this.file["Key1"] = "Value1";
			this.file["Key2"] = "Value2";
			this.file["Key3"] = "Value3";
			this.file["Key4"] = null;
			this.file["Key5"] = Obj;
			this.file["Key6"] = ByteArray;
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key4"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key5"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key6"));
			AssertEx.Same(this.file["Key1"], "Value1");
			AssertEx.Same(this.file["Key2"], "Value2");
			AssertEx.Same(this.file["Key3"], "Value3");
			Assert.IsNull(this.file["Key4"]);
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj);
			AssertEx.Same(this.file["Key6"], ByteArray);
		}

		private byte[] GetBytes(int NrBytes, int Offset)
		{
			byte[] Result = new byte[NrBytes];
			int i;

			for (i = 0; i < NrBytes; i++)
				Result[i] = (byte)(i + Offset);

			return Result;
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_03_Add()
		{
			await Test_Add(100);
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_04_Add_BLOB()
		{
			await Test_Add(100000);
		}

		private async Task Test_Add(int MaxLen)
		{
			byte[] ByteArray = this.GetBytes(MaxLen, 0);
			Simple Obj = DBFilesBTreeTests.CreateSimple(MaxLen);

			await this.file.AddAsync("Key1", "Value1");
			await this.file.AddAsync("Key2", "Value2");
			await this.file.AddAsync("Key3", "Value3");
			await this.file.AddAsync("Key4", null);
			await this.file.AddAsync("Key5", Obj);
			await this.file.AddAsync("Key6", ByteArray);
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key4"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key5"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key6"));
			AssertEx.Same(this.file["Key1"], "Value1");
			AssertEx.Same(this.file["Key2"], "Value2");
			AssertEx.Same(this.file["Key3"], "Value3");
			Assert.IsNull(this.file["Key4"]);
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj);
			AssertEx.Same(this.file["Key6"], ByteArray);
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_05_Reset()
		{
			await Test_Reset(100);
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_06_Reset_BLOB()
		{
			await Test_Reset(100000);
		}

		private async Task Test_Reset(int MaxLen)
		{
			byte[] ByteArray1 = this.GetBytes(MaxLen, 0);
			byte[] ByteArray2 = this.GetBytes(MaxLen, MaxLen);
			Simple Obj1 = DBFilesBTreeTests.CreateSimple(MaxLen);
			Simple Obj2 = DBFilesBTreeTests.CreateSimple(MaxLen);

			this.file["Key1"] = "Value1_1";
			this.file["Key2"] = "Value2_1";
			this.file["Key3"] = "Value3_1";
			this.file["Key4"] = null;
			this.file["Key5"] = Obj1;
			this.file["Key6"] = ByteArray1;
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key4"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key5"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key6"));
			AssertEx.Same(this.file["Key1"], "Value1_1");
			AssertEx.Same(this.file["Key2"], "Value2_1");
			AssertEx.Same(this.file["Key3"], "Value3_1");
			Assert.IsNull(this.file["Key4"]);
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj1);
			AssertEx.Same(this.file["Key6"], ByteArray1);

			this.file["Key1"] = "Value1_2";
			this.file["Key2"] = "Value2_2";
			this.file["Key3"] = "Value3_2";
			this.file["Key4"] = null;
			this.file["Key5"] = Obj2;
			this.file["Key6"] = ByteArray2;
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key4"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key5"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key6"));
			AssertEx.Same(this.file["Key1"], "Value1_2");
			AssertEx.Same(this.file["Key2"], "Value2_2");
			AssertEx.Same(this.file["Key3"], "Value3_2");
			Assert.IsNull(this.file["Key4"]);
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj2);
			AssertEx.Same(this.file["Key6"], ByteArray2);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task DBFiles_StringDictionary_07_Readd()
		{
			await this.Test_Readd(100);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task DBFiles_StringDictionary_08_Readd_BLOB()
		{
			await this.Test_Readd(100000);
		}

		private async Task Test_Readd(int MaxLen)
		{
			await this.Test_Add(MaxLen);
			await this.file.AddAsync("Key1", "Value1_2");
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_09_Remove()
		{
			await this.Test_Remove(100);
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_10_Remove_BLOB()
		{
			await this.Test_Remove(100000);
		}

		private async Task Test_Remove(int MaxLen)
		{
			byte[] ByteArray = this.GetBytes(MaxLen, 0);
			Simple Obj = DBFilesBTreeTests.CreateSimple(MaxLen);

			await this.file.AddAsync("Key1", "Value1");
			await this.file.AddAsync("Key2", "Value2");
			await this.file.AddAsync("Key3", "Value3");
			await this.file.AddAsync("Key4", null);
			await this.file.AddAsync("Key5", Obj);
			await this.file.AddAsync("Key6", ByteArray);
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key4"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key5"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key6"));
			AssertEx.Same(this.file["Key1"], "Value1");
			AssertEx.Same(this.file["Key2"], "Value2");
			AssertEx.Same(this.file["Key3"], "Value3");
			Assert.IsNull(this.file["Key4"]);
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj);
			AssertEx.Same(this.file["Key6"], ByteArray);

			Assert.IsTrue(await this.file.RemoveAsync("Key2"));
			Assert.IsFalse(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsFalse(await this.file.RemoveAsync("Key2"));
			AssertEx.Same(this.file["Key1"], "Value1");
			AssertEx.Same(this.file["Key3"], "Value3");
			Assert.IsNull(this.file["Key4"]);
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj);
			AssertEx.Same(this.file["Key6"], ByteArray);

			Assert.IsTrue(await this.file.RemoveAsync("Key1"));
			Assert.IsFalse(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsFalse(await this.file.RemoveAsync("Key1"));
			AssertEx.Same(this.file["Key3"], "Value3");
			Assert.IsNull(this.file["Key4"]);
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj);
			AssertEx.Same(this.file["Key6"], ByteArray);

			Assert.IsTrue(await this.file.RemoveAsync("Key3"));
			Assert.IsFalse(await this.file.ContainsKeyAsync("Key3"));
			Assert.IsFalse(await this.file.RemoveAsync("Key3"));
			Assert.IsNull(this.file["Key4"]);
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj);
			AssertEx.Same(this.file["Key6"], ByteArray);

			Assert.IsTrue(await this.file.RemoveAsync("Key4"));
			Assert.IsFalse(await this.file.ContainsKeyAsync("Key4"));
			Assert.IsFalse(await this.file.RemoveAsync("Key4"));
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj);
			AssertEx.Same(this.file["Key6"], ByteArray);

			Assert.IsTrue(await this.file.RemoveAsync("Key5"));
			Assert.IsFalse(await this.file.ContainsKeyAsync("Key5"));
			Assert.IsFalse(await this.file.RemoveAsync("Key5"));
			AssertEx.Same(this.file["Key6"], ByteArray);

			Assert.IsTrue(await this.file.RemoveAsync("Key6"));
			Assert.IsFalse(await this.file.ContainsKeyAsync("Key6"));
			Assert.IsFalse(await this.file.RemoveAsync("Key6"));
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_11_CopyTo()
		{
			await this.Test_CopyTo(100);
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_12_CopyTo_BLOB()
		{
			await this.Test_CopyTo(100000);
		}

		private async Task Test_CopyTo(int MaxLen)
		{
			byte[] ByteArray = this.GetBytes(MaxLen, 0);
			Simple Obj = DBFilesBTreeTests.CreateSimple(MaxLen);

			this.file["Key1"] = "Value1";
			this.file["Key2"] = "Value2";
			this.file["Key3"] = "Value3";
			this.file["Key4"] = null;
			this.file["Key5"] = Obj;
			this.file["Key6"] = ByteArray;
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key4"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key5"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key6"));
			AssertEx.Same(this.file["Key1"], "Value1");
			AssertEx.Same(this.file["Key2"], "Value2");
			AssertEx.Same(this.file["Key3"], "Value3");
			Assert.IsNull(this.file["Key4"]);
			DBFilesObjectSerializationTests.AssertEqual(this.file["Key5"] as Simple, Obj);
			AssertEx.Same(this.file["Key6"], ByteArray);

			int c = this.file.Count;
			AssertEx.Same(6, c);

			KeyValuePair<string, object>[] A = new KeyValuePair<string, object>[c];
			this.file.CopyTo(A, 0);

			AssertEx.Same(A[0].Key, "Key1");
			AssertEx.Same(A[1].Key, "Key2");
			AssertEx.Same(A[2].Key, "Key3");
			AssertEx.Same(A[3].Key, "Key4");
			AssertEx.Same(A[4].Key, "Key5");
			AssertEx.Same(A[5].Key, "Key6");

			AssertEx.Same(A[0].Value, "Value1");
			AssertEx.Same(A[1].Value, "Value2");
			AssertEx.Same(A[2].Value, "Value3");
			Assert.IsNull(A[3].Value);
			DBFilesObjectSerializationTests.AssertEqual(A[4].Value as Simple, Obj);
			AssertEx.Same(A[5].Value, ByteArray);
		}

		[TestMethod]
		public void DBFiles_StringDictionary_13_DataTypes()
		{
			DateTime DT = DateTime.Now;
			DateTimeOffset DTO = DateTimeOffset.Now;

			this.file["Key1"] = true;
			this.file["Key2"] = (byte)1;
			this.file["Key3"] = (short)2;
			this.file["Key4"] = (int)3;
			this.file["Key5"] = (long)4;
			this.file["Key6"] = (sbyte)5;
			this.file["Key7"] = (ushort)6;
			this.file["Key8"] = (uint)7;
			this.file["Key9"] = (ulong)8;
			this.file["Key10"] = (decimal)9;
			this.file["Key11"] = (double)10;
			this.file["Key12"] = (float)11;
			this.file["Key13"] = DT;
			this.file["Key14"] = TimeSpan.Zero;
			this.file["Key15"] = 'a';
			this.file["Key16"] = "Hello";
			this.file["Key17"] = NormalEnum.Option2;
			this.file["Key18"] = Guid.Empty.ToByteArray();
			this.file["Key19"] = Guid.Empty;
			this.file["Key20"] = DTO;
			this.file["Key21"] = new CaseInsensitiveString("Hello");
			this.file["Key22"] = null;

			AssertEx.Same(this.file["Key1"], true);
			AssertEx.Same(this.file["Key2"], (byte)1);
			AssertEx.Same(this.file["Key3"], (short)2);
			AssertEx.Same(this.file["Key4"], (int)3);
			AssertEx.Same(this.file["Key5"], (long)4);
			AssertEx.Same(this.file["Key6"], (sbyte)5);
			AssertEx.Same(this.file["Key7"], (ushort)6);
			AssertEx.Same(this.file["Key8"], (uint)7);
			AssertEx.Same(this.file["Key9"], (ulong)8);
			AssertEx.Same(this.file["Key10"], (decimal)9);
			AssertEx.Same(this.file["Key11"], (double)10);
			AssertEx.Same(this.file["Key12"], (float)11);
			AssertEx.Same(this.file["Key13"], DT);
			AssertEx.Same(this.file["Key14"], TimeSpan.Zero);
			AssertEx.Same(this.file["Key15"], 'a');
			AssertEx.Same(this.file["Key16"], "Hello");
			AssertEx.Same(this.file["Key17"], "Option2");
			AssertEx.Same(this.file["Key18"], Guid.Empty.ToByteArray());
			AssertEx.Same(this.file["Key19"], Guid.Empty);
			AssertEx.Same(this.file["Key20"], DTO);
			AssertEx.Same(this.file["Key21"], new CaseInsensitiveString("Hello"));
			Assert.IsNull(this.file["Key22"]);
		}

		[TestMethod]
		public async Task DBFiles_StringDictionary_14_ToArray()
		{
			this.file["Default"] = 1UL;
			this.file["ObjectId"] = 2UL;
			this.file["Boolean1"] = 3UL;
			this.file["Boolean2"] = 4UL;
			this.file["Byte"] = 5UL;
			this.file["Short"] = 6UL;
			this.file["Int"] = 7UL;
			this.file["Long"] = 8UL;
			this.file["SByte"] = 9UL;
			this.file["UShort"] = 10UL;
			this.file["UInt"] = 11UL;
			this.file["ULong"] = 12UL;
			this.file["Char"] = 13UL;
			this.file["Decimal"] = 14UL;
			this.file["Double"] = 15UL;
			this.file["Single"] = 16UL;
			this.file["String"] = 17UL;
			this.file["ShortString"] = 18UL;
			this.file["DateTime"] = 19UL;
			this.file["TimeSpan"] = 20UL;
			this.file["Guid"] = 21UL;
			this.file["NormalEnum"] = 22UL;
			this.file["FlagsEnum"] = 23UL;
			this.file["Waher.Persistence.FilesLW.Test.Classes.Simple"] = 24UL;

			KeyValuePair<string, object>[] Records = await this.file.ToArrayAsync();

			AssertEx.Same(24, Records.Length);
		}

		[TestMethod]
		public void DBFiles_StringDictionary_15_10000_Items()
		{
			int i;

			for (i = 1; i <= 10000; i++)
				this.file["Key" + i.ToString()] = i;

			Assert.AreEqual(10000, this.file.Count);

			for (i = 1; i <= 10000; i++)
				AssertEx.Same(this.file["Key" + i.ToString()], i);
		}

		[TestMethod]
		public void DBFiles_StringDictionary_16_10000_Items_BLOB()
		{
			int i;

			for (i = 1; i <= 10000; i++)
				this.file["Key" + i.ToString()] = this.GetBytes(10000, i);

			Assert.AreEqual(10000, this.file.Count);

			for (i = 1; i <= 10000; i++)
				AssertEx.Same(this.file["Key" + i.ToString()], this.GetBytes(10000, i));
		}

		[TestMethod]
		public void DBFiles_StringDictionary_17_10000_Replace()
		{
			int[] Last = new int[1024];
			int i;

			for (i = 1; i <= 10000; i++)
				this.file["Key" + (i & 1023).ToString()] = Last[i & 1023] = i;

			Assert.AreEqual(1024, this.file.Count);

			for (i = 0; i <= 1023; i++)
				AssertEx.Same(this.file["Key" + i.ToString()], Last[i]);
		}

		[TestMethod]
		public void DBFiles_StringDictionary_18_10000_Replace_BLOB()
		{
			byte[][] Last = new byte[1024][];
			int i;

			for (i = 1; i <= 10000; i++)
				this.file["Key" + (i & 1023).ToString()] = Last[i & 1023] = this.GetBytes(10000, i);

			Assert.AreEqual(1024, this.file.Count);

			for (i = 0; i <= 1023; i++)
				AssertEx.Same(this.file["Key" + i.ToString()], Last[i]);
		}

	}
}
