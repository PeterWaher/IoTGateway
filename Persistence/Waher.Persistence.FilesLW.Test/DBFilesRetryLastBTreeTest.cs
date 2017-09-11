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
	public class DBFilesRetryLastBTreeTest
	{
		private ObjectBTreeFile file;
		private FilesProvider provider;
		private Random gen = new Random();
		private DateTime start;

		[TestInitialize]
		public async Task TestInitialize()
		{
			if (!File.Exists(DBFilesBTreeTests.MasterFileName + ".bak") ||
				!File.Exists(DBFilesBTreeTests.FileName + ".bak") ||
				!File.Exists(DBFilesBTreeTests.BlobFileName + ".bak") ||
				!File.Exists(DBFilesBTreeTests.NamesFileName + ".bak"))
			{
				Assert.Inconclusive("No backup files to test against.");
			}

			if (File.Exists(DBFilesBTreeTests.MasterFileName))
				File.Delete(DBFilesBTreeTests.MasterFileName);

			if (File.Exists(DBFilesBTreeTests.FileName))
				File.Delete(DBFilesBTreeTests.FileName);

			if (File.Exists(DBFilesBTreeTests.BlobFileName))
				File.Delete(DBFilesBTreeTests.BlobFileName);

			if (File.Exists(DBFilesBTreeTests.NamesFileName))
				File.Delete(DBFilesBTreeTests.NamesFileName);

			File.Copy(DBFilesBTreeTests.MasterFileName + ".bak", DBFilesBTreeTests.MasterFileName);
			File.Copy(DBFilesBTreeTests.FileName + ".bak", DBFilesBTreeTests.FileName);
			File.Copy(DBFilesBTreeTests.BlobFileName + ".bak", DBFilesBTreeTests.BlobFileName);
			File.Copy(DBFilesBTreeTests.NamesFileName + ".bak", DBFilesBTreeTests.NamesFileName);

			int BlockSize = this.LoadBlockSize();

			this.provider = new FilesProvider(DBFilesBTreeTests.Folder, DBFilesBTreeTests.CollectionName, BlockSize, 10000, Math.Max(BlockSize / 2, 1024), Encoding.UTF8, 10000, true);
			this.file = await this.provider.GetFile(DBFilesBTreeTests.CollectionName);
			this.start = DateTime.Now;

			DBFilesBTreeTests.ExportXML(this.file, "Data\\BTreeBefore.xml").Wait();
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
			if (!File.Exists(DBFilesBTreeTests.ObjFileName))
				Assert.Inconclusive("No binary object file to test against.");

			byte[] Bin = File.ReadAllBytes(DBFilesBTreeTests.ObjFileName);
			BinaryDeserializer Reader = new BinaryDeserializer(DBFilesBTreeTests.CollectionName, Encoding.UTF8, Bin);
			IObjectSerializer Serializer = this.provider.GetObjectSerializer(typeof(Simple));
			return (Simple)Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);
		}

		private Guid LoadObjectId()
		{
			if (!File.Exists(DBFilesBTreeTests.ObjIdFileName))
				Assert.Inconclusive("No object id file to test against.");

			byte[] Bin = File.ReadAllBytes(DBFilesBTreeTests.ObjIdFileName);

			return new Guid(Bin);
		}

		private int LoadBlockSize()
		{
			if (!File.Exists(DBFilesBTreeTests.BlockSizeFileName))
				Assert.Inconclusive("No block size file to test against.");

			return int.Parse(File.ReadAllText(DBFilesBTreeTests.BlockSizeFileName));
		}

		[TestMethod]
		[Ignore]
		public async Task Test_01_Retry_SaveNew()
		{
			FileStatistics StatBefore = await this.file.ComputeStatistics();
			Simple Obj = this.LoadSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			await DBFilesBTreeTests.AssertConsistent(this.file, this.provider, (int)(StatBefore.NrObjects + 1), null, true);
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
				Console.Out.WriteLine(await DBFilesBTreeTests.ExportXML(this.file, "Data\\BTreeError.xml"));
				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			Console.Out.WriteLine(await DBFilesBTreeTests.ExportXML(this.file, "Data\\BTreeAfter.xml"));

			await DBFilesBTreeTests.AssertConsistent(this.file, this.provider, (int)(StatBefore.NrObjects - 1), null, true);
		}
	}
}
