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
	public class ProviderTests
	{
		internal const string FileName = "Data\\Default.btree";
		internal const string Folder = "Data";
		internal const string BlobFileName = "Data\\Default.blob";
		internal const string CollectionName = "Default";
		internal const int BlocksInCache = 1000;

		protected FilesProvider provider;

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

			if (File.Exists(BlobFileName + ".bak"))
				File.Delete(BlobFileName + ".bak");

			if (File.Exists(BlobFileName))
			{
				File.Copy(BlobFileName, BlobFileName + ".bak");
				File.Delete(BlobFileName);
			}

			this.provider = new FilesProvider("Data", CollectionName, 8192, 8192, Encoding.UTF8, 10000, true);
		}

		[TearDown]
		public void TearDown()
		{
			if (this.provider != null)
			{
				this.provider.Dispose();
				this.provider = null;
			}
		}

		[Test]
		public async void Test_01_ByReference()
		{
			ByReference Obj = new Classes.ByReference();
			Obj.Default = BTreeTests.CreateDefault(100);
			Obj.Simple = BTreeTests.CreateSimple(100);

			await this.provider.Insert(Obj);

			ObjectBTreeFile File = this.provider.GetFile("Default");
			await BTreeTests.AssertConsistent(File, this.provider, 3, Obj, true);
			Console.Out.WriteLine(await BTreeTests.ExportXML(File, "Data\\BTree.xml"));

			Assert.AreNotEqual(Guid.Empty, Obj.ObjectId);
			Assert.AreNotEqual(Guid.Empty, Obj.Default.ObjectId);
			Assert.AreNotEqual(Guid.Empty, Obj.Simple.ObjectId);

			ByReference Obj2 = await this.provider.LoadObject<ByReference>(Obj.ObjectId);

			ObjectSerializationTests.AssertEqual(Obj2.Default, Obj.Default);
			ObjectSerializationTests.AssertEqual(Obj2.Simple, Obj.Simple);
		}

		// TODO: Centralized block cache in FilesProvider that all files use.
		// TODO: FilesProvider: File with Key names. Master table for indices.
		// TODO: Multi-threaded stress test (with multiple indices).
		// TOOO: Test huge databases with more than uint.MaxValue objects.
		// TODO: Startup: Scan file if not shut down correctly. Rebuild in case file is corrupt
	}
}
