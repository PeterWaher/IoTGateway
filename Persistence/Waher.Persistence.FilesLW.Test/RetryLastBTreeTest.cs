using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;
using Waher.Runtime.Inventory;

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
	public class RetryLastBTreeTest
	{
		private ObjectBTreeFile file;
		private FilesProvider provider;
		private Random gen = new Random();
		private DateTime start;

		[TestInitialize]
		public async Task TestInitialize()
		{
			if (!File.Exists(BTreeTests.MasterFileName + ".bak") ||
				!File.Exists(BTreeTests.FileName + ".bak") ||
				!File.Exists(BTreeTests.BlobFileName + ".bak") ||
				!File.Exists(BTreeTests.NamesFileName + ".bak"))
			{
				Assert.Inconclusive("No backup files to test against.");
			}

			if (File.Exists(BTreeTests.MasterFileName))
				File.Delete(BTreeTests.MasterFileName);

			if (File.Exists(BTreeTests.FileName))
				File.Delete(BTreeTests.FileName);

			if (File.Exists(BTreeTests.BlobFileName))
				File.Delete(BTreeTests.BlobFileName);

			if (File.Exists(BTreeTests.NamesFileName))
				File.Delete(BTreeTests.NamesFileName);

			File.Copy(BTreeTests.MasterFileName + ".bak", BTreeTests.MasterFileName);
			File.Copy(BTreeTests.FileName + ".bak", BTreeTests.FileName);
			File.Copy(BTreeTests.BlobFileName + ".bak", BTreeTests.BlobFileName);
			File.Copy(BTreeTests.NamesFileName + ".bak", BTreeTests.NamesFileName);

			int BlockSize = this.LoadBlockSize();

			this.provider = new FilesProvider(BTreeTests.Folder, BTreeTests.CollectionName, BlockSize, 10000, Math.Max(BlockSize / 2, 1024), Encoding.UTF8, 10000, true);
			this.file = await this.provider.GetFile(BTreeTests.CollectionName);
			this.start = DateTime.Now;

			BTreeTests.ExportXML(this.file, "Data\\BTreeBefore.xml").Wait();
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

		private Simple LoadSimple()
		{
			if (!File.Exists(BTreeTests.ObjFileName))
				Assert.Inconclusive("No binary object file to test against.");

			byte[] Bin = File.ReadAllBytes(BTreeTests.ObjFileName);
			BinaryDeserializer Reader = new BinaryDeserializer(BTreeTests.CollectionName, Encoding.UTF8, Bin);
			IObjectSerializer Serializer = this.provider.GetObjectSerializer(typeof(Simple));
			return (Simple)Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);
		}

		private Guid LoadObjectId()
		{
			if (!File.Exists(BTreeTests.ObjIdFileName))
				Assert.Inconclusive("No object id file to test against.");

			byte[] Bin = File.ReadAllBytes(BTreeTests.ObjIdFileName);

			return new Guid(Bin);
		}

		private int LoadBlockSize()
		{
			if (!File.Exists(BTreeTests.BlockSizeFileName))
				Assert.Inconclusive("No block size file to test against.");

			return int.Parse(File.ReadAllText(BTreeTests.BlockSizeFileName));
		}

		[TestMethod]
		[Ignore]
		public async Task Test_01_Retry_SaveNew()
		{
			FileStatistics StatBefore = await this.file.ComputeStatistics();
			Simple Obj = this.LoadSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			await BTreeTests.AssertConsistent(this.file, this.provider, (int)(StatBefore.NrObjects + 1), null, true);
		}

		[TestMethod]
		[Ignore]
		public async Task Test_02_Retry_Delete()
		{
			FileStatistics StatBefore = await this.file.ComputeStatistics();
			Guid ObjectId = this.LoadObjectId();

			try
			{
				await this.file.DeleteObject(ObjectId);
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(await BTreeTests.ExportXML(this.file, "Data\\BTreeError.xml"));
				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			Console.Out.WriteLine(await BTreeTests.ExportXML(this.file, "Data\\BTreeAfter.xml"));

			await BTreeTests.AssertConsistent(this.file, this.provider, (int)(StatBefore.NrObjects - 1), null, true);
		}
	}
}
