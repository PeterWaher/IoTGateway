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
	public class ProviderTests
	{
		internal const int BlocksInCache = 10000;

		protected FilesProvider provider;
		private ObjectBTreeFile file;

		[TestInitialize]
		public async Task TestInitialize()
		{
			if (File.Exists(BTreeTests.MasterFileName + ".bak"))
				File.Delete(BTreeTests.MasterFileName + ".bak");

			if (File.Exists(BTreeTests.MasterFileName))
			{
				File.Copy(BTreeTests.MasterFileName, BTreeTests.MasterFileName + ".bak");
				File.Delete(BTreeTests.MasterFileName);
			}

			if (File.Exists(BTreeTests.FileName + ".bak"))
				File.Delete(BTreeTests.FileName + ".bak");

			if (File.Exists(BTreeTests.FileName))
			{
				File.Copy(BTreeTests.FileName, BTreeTests.FileName + ".bak");
				File.Delete(BTreeTests.FileName);
			}

			if (File.Exists(BTreeTests.BlobFileName + ".bak"))
				File.Delete(BTreeTests.BlobFileName + ".bak");

			if (File.Exists(BTreeTests.BlobFileName))
			{
				File.Copy(BTreeTests.BlobFileName, BTreeTests.BlobFileName + ".bak");
				File.Delete(BTreeTests.BlobFileName);
			}

			this.provider = new FilesProvider("Data", BTreeTests.CollectionName, 8192, BlocksInCache, 8192, Encoding.UTF8, 10000, true);
			this.file = await this.provider.GetFile("Default");
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.provider != null)
			{
				this.provider.Dispose();
				this.provider = null;
				this.file = null;
			}
		}

		[TestMethod]
		public async Task Test_01_ByReference()
		{
			ByReference Obj = new ByReference()
			{
				Default = BTreeTests.CreateDefault(100),
				Simple = BTreeTests.CreateSimple(100)
			};

			await this.provider.Insert(Obj);

			ObjectBTreeFile File = await this.provider.GetFile("Default");
			await BTreeTests.AssertConsistent(File, this.provider, 3, Obj, true);
			Console.Out.WriteLine(await BTreeTests.ExportXML(File, "Data\\BTree.xml"));

			Assert.AreNotEqual(Guid.Empty, Obj.ObjectId);
			Assert.AreNotEqual(Guid.Empty, Obj.Default.ObjectId);
			Assert.AreNotEqual(Guid.Empty, Obj.Simple.ObjectId);

			ByReference Obj2 = await this.provider.LoadObject<ByReference>(Obj.ObjectId);

			ObjectSerializationTests.AssertEqual(Obj2.Default, Obj.Default);
			ObjectSerializationTests.AssertEqual(Obj2.Simple, Obj.Simple);
		}

		// TODO: Solve deadlocks.
		// TODO: Multi-threaded stress test (with multiple indices).
		// TOOO: Test huge databases with more than uint.MaxValue objects.
		// TODO: Startup: Scan file if not shut down correctly. Rebuild in case file is corrupt
	}
}
