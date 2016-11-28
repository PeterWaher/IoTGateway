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
	public class RetryLastBTreeTest
	{
		private ObjectBTreeFile file;
		private FilesProvider provider;
		private Random gen = new Random();
		private DateTime start;

		[SetUp]
		public void SetUp()
		{
			if (!File.Exists(BTreeTests.FileName + ".bak") || !File.Exists(BTreeTests.BlobFileName + ".bak"))
				throw new IgnoreException("No backup files to test against.");

			if (File.Exists(BTreeTests.FileName))
				File.Delete(BTreeTests.FileName);

			if (File.Exists(BTreeTests.BlobFileName))
				File.Delete(BTreeTests.BlobFileName);

			File.Copy(BTreeTests.FileName + ".bak", BTreeTests.FileName);
			File.Copy(BTreeTests.BlobFileName + ".bak", BTreeTests.BlobFileName);

			int BlockSize = this.LoadBlockSize();

			this.provider = new FilesProvider(BTreeTests.Folder, BTreeTests.CollectionName);
			this.file = new ObjectBTreeFile(BTreeTests.FileName, BTreeTests.CollectionName, BTreeTests.BlobFileName, BlockSize, 
				BTreeTests.BlocksInCache, Math.Max(BlockSize / 2, 1024), this.provider, Encoding.UTF8, 10000, true);
			this.start = DateTime.Now;

			BTreeTests.ExportXML(this.file, "Data\\BTreeBefore.xml").Wait();
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

		private Simple LoadSimple()
		{
			if (!File.Exists(BTreeTests.ObjFileName))
				throw new IgnoreException("No binary object file to test against.");

			byte[] Bin = File.ReadAllBytes(BTreeTests.ObjFileName);
			BinaryDeserializer Reader = new BinaryDeserializer(BTreeTests.CollectionName, Encoding.UTF8, Bin);
			IObjectSerializer Serializer = this.provider.GetObjectSerializer(typeof(Simple));
			return (Simple)Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);
		}

		private Guid LoadObjectId()
		{
			if (!File.Exists(BTreeTests.ObjIdFileName))
				throw new IgnoreException("No object id file to test against.");

			byte[] Bin = File.ReadAllBytes(BTreeTests.ObjIdFileName);

			return new Guid(Bin);
		}

		private int LoadBlockSize()
		{
			if (!File.Exists(BTreeTests.BlockSizeFileName))
				throw new IgnoreException("No block size file to test against.");

			return int.Parse(File.ReadAllText(BTreeTests.BlockSizeFileName));
		}

		[Test]
		public async void Test_01_Retry_SaveNew()
		{
			FileStatistics StatBefore = await this.file.ComputeStatistics();
			Simple Obj = this.LoadSimple();
			Guid ObjectId = await this.file.SaveNewObject(Obj);
			Assert.AreNotEqual(Guid.Empty, ObjectId);

			await BTreeTests.AssertConsistent(this.file, this.provider, (int)(StatBefore.NrObjects + 1), null, true);
		}

		[Test]
		public async void Test_02_Retry_Delete()
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
