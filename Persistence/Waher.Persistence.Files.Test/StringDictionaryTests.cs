using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
	public class StringDictionaryTests
	{
		internal const string FileName = "Data\\Dictionary.btree";
		internal const string Folder = "Data";
		internal const string BlobFileName = "Data\\Dictionary.blob";
		internal const string CollectionName = "Default";
		internal const int BlocksInCache = 10000;
		internal const int ObjectsToEnumerate = 10000;

		protected StringDictionary file;
		protected FilesProvider provider;
		protected DateTime start;

		[TestInitialize]
		public void TestInitialize()
		{
			if (File.Exists(BTreeTests.MasterFileName + ".bak"))
				File.Delete(BTreeTests.MasterFileName + ".bak");

			if (File.Exists(BTreeTests.MasterFileName))
			{
				File.Copy(BTreeTests.MasterFileName, BTreeTests.MasterFileName + ".bak");
				File.Delete(BTreeTests.MasterFileName);
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

			this.provider = new FilesProvider(Folder, CollectionName, 8192, BlocksInCache, 8192, Encoding.UTF8, 10000, true);
			this.file = new StringDictionary(0, FileName, BlobFileName, CollectionName, this.provider, false);

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
		public async Task Test_01_Set()
		{
			this.file["Key1"] = "Value1";
			this.file["Key2"] = "Value2";
			this.file["Key3"] = "Value3";
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.AreEqual(this.file["Key1"], "Value1");
			Assert.AreEqual(this.file["Key2"], "Value2");
			Assert.AreEqual(this.file["Key3"], "Value3");
		}

		[TestMethod]
		public async Task Test_02_Add()
		{
			await this.file.AddAsync("Key1", "Value1");
			await this.file.AddAsync("Key2", "Value2");
			await this.file.AddAsync("Key3", "Value3");
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.AreEqual(this.file["Key1"], "Value1");
			Assert.AreEqual(this.file["Key2"], "Value2");
			Assert.AreEqual(this.file["Key3"], "Value3");
		}

		[TestMethod]
		public async Task Test_03_Reset()
		{
			this.file["Key1"] = "Value1_1";
			this.file["Key2"] = "Value2_1";
			this.file["Key3"] = "Value3_1";
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.AreEqual(this.file["Key1"], "Value1_1");
			Assert.AreEqual(this.file["Key2"], "Value2_1");
			Assert.AreEqual(this.file["Key3"], "Value3_1");

			this.file["Key1"] = "Value1_2";
			this.file["Key2"] = "Value2_2";
			this.file["Key3"] = "Value3_2";
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.AreEqual(this.file["Key1"], "Value1_2");
			Assert.AreEqual(this.file["Key2"], "Value2_2");
			Assert.AreEqual(this.file["Key3"], "Value3_2");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task Test_04_Readd()
		{
			await this.file.AddAsync("Key1", "Value1_1");
			await this.file.AddAsync("Key2", "Value2_1");
			await this.file.AddAsync("Key3", "Value3_1");
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.AreEqual(this.file["Key1"], "Value1_1");
			Assert.AreEqual(this.file["Key2"], "Value2_1");
			Assert.AreEqual(this.file["Key3"], "Value3_1");
			await this.file.AddAsync("Key1", "Value1_2");
		}

		[TestMethod]
		public async Task Test_05_Remove()
		{
			await this.file.AddAsync("Key1", "Value1_1");
			await this.file.AddAsync("Key2", "Value2_1");
			await this.file.AddAsync("Key3", "Value3_1");
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsTrue(await this.file.ContainsKeyAsync("Key3"));
			Assert.AreEqual(this.file["Key1"], "Value1_1");
			Assert.AreEqual(this.file["Key2"], "Value2_1");
			Assert.AreEqual(this.file["Key3"], "Value3_1");

			Assert.IsTrue(await this.file.RemoveAsync("Key2"));
			Assert.IsFalse(await this.file.ContainsKeyAsync("Key2"));
			Assert.IsFalse(await this.file.RemoveAsync("Key2"));
			Assert.AreEqual(this.file["Key1"], "Value1_1");
			Assert.AreEqual(this.file["Key3"], "Value3_1");

			Assert.IsTrue(await this.file.RemoveAsync("Key1"));
			Assert.IsFalse(await this.file.ContainsKeyAsync("Key1"));
			Assert.IsFalse(await this.file.RemoveAsync("Key1"));
			Assert.AreEqual(this.file["Key3"], "Value3_1");

			Assert.IsTrue(await this.file.RemoveAsync("Key3"));
			Assert.IsFalse(await this.file.ContainsKeyAsync("Key3"));
			Assert.IsFalse(await this.file.RemoveAsync("Key3"));
		}

		[TestMethod]
		public async Task Test_06_CopyTo()
		{
			await this.Test_01_Set();

			int c = this.file.Count;
			Assert.AreEqual(3, c);

			KeyValuePair<string, object>[] A = new KeyValuePair<string, object>[c];
			this.file.CopyTo(A, 0);

			Assert.AreEqual(A[0].Key, "Key1");
			Assert.AreEqual(A[1].Key, "Key2");
			Assert.AreEqual(A[2].Key, "Key3");

			Assert.AreEqual(A[0].Value, "Value1");
			Assert.AreEqual(A[1].Value, "Value2");
			Assert.AreEqual(A[2].Value, "Value3");
		}

		[TestMethod]
		public void Test_07_DataTypes()
		{
			Simple Obj = BTreeTests.CreateSimple(100);

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
			this.file["Key13"] = (DateTime)DateTime.Today;
			this.file["Key14"] = TimeSpan.Zero;
			this.file["Key15"] = 'a';
			this.file["Key16"] = "Hello";
			this.file["Key17"] = NormalEnum.Option2;
			this.file["Key18"] = Guid.Empty.ToByteArray();

			Assert.AreEqual(this.file["Key1"], true);
			Assert.AreEqual(this.file["Key2"], (byte)1);
			Assert.AreEqual(this.file["Key3"], (short)2);
			Assert.AreEqual(this.file["Key4"], (int)3);
			Assert.AreEqual(this.file["Key5"], (long)4);
			Assert.AreEqual(this.file["Key6"], (sbyte)5);
			Assert.AreEqual(this.file["Key7"], (ushort)6);
			Assert.AreEqual(this.file["Key8"], (uint)7);
			Assert.AreEqual(this.file["Key9"], (ulong)8);
			Assert.AreEqual(this.file["Key10"], (decimal)9);
			Assert.AreEqual(this.file["Key11"], (double)10);
			Assert.AreEqual(this.file["Key12"], (float)11);
			Assert.AreEqual(this.file["Key13"], (DateTime)DateTime.Today);
			Assert.AreEqual(this.file["Key14"], TimeSpan.Zero);
			Assert.AreEqual(this.file["Key15"], 'a');
			Assert.AreEqual(this.file["Key16"], "Hello");
			Assert.AreEqual(this.file["Key17"], "Option2");
			Assert.AreEqual(this.file["Key18"], Guid.Empty.ToByteArray());
		}

		// TODO: BLOB values.
		// TODO: Object values.
		// TODO: null values.
	}
}
