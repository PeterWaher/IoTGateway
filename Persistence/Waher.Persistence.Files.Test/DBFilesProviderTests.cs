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
	public class DBFilesProviderTests
	{
		internal const int BlocksInCache = 10000;

		protected FilesProvider provider;
		private ObjectBTreeFile file;

		[TestInitialize]
		public async Task TestInitialize()
		{
			DBFilesBTreeTests.DeleteFiles();

#if LW
			this.provider = new FilesProvider("Data", DBFilesBTreeTests.CollectionName, 8192, BlocksInCache, 8192, Encoding.UTF8, 10000);
#else
			this.provider = new FilesProvider("Data", DBFilesBTreeTests.CollectionName, 8192, BlocksInCache, 8192, Encoding.UTF8, 10000, true);
#endif
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
		public async Task DBFiles_Provider_01_ByReference()
		{
			ByReference Obj = new ByReference()
			{
				Default = DBFilesBTreeTests.CreateDefault(100),
				Simple = DBFilesBTreeTests.CreateSimple(100)
			};

			await this.provider.Insert(Obj);

			ObjectBTreeFile File = await this.provider.GetFile("Default");
			await DBFilesBTreeTests.AssertConsistent(File, this.provider, 3, Obj, true);
			Console.Out.WriteLine(await DBFilesBTreeTests.ExportXML(File, "Data\\BTree.xml"));

			Assert.AreNotEqual(Guid.Empty, Obj.ObjectId);
			Assert.AreNotEqual(Guid.Empty, Obj.Default.ObjectId);
			Assert.AreNotEqual(Guid.Empty, Obj.Simple.ObjectId);

			ByReference Obj2 = await this.provider.TryLoadObject<ByReference>(Obj.ObjectId);
			Assert.IsNotNull(Obj2);

			DBFilesObjectSerializationTests.AssertEqual(Obj2.Default, Obj.Default);
			DBFilesObjectSerializationTests.AssertEqual(Obj2.Simple, Obj.Simple);
		}

		// TODO: Solve deadlocks.
		// TODO: Multi-threaded stress test (with multiple indices).
		// TOOO: Test huge databases with more than uint.MaxValue objects.
		// TODO: Startup: Scan file if not shut down correctly. Rebuild in case file is corrupt
	}
}
