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
		internal const string Folder = "Data";
		internal const int BlocksInCache = 1000;

		protected FilesProvider provider;

		[SetUp]
		public void SetUp()
		{
			this.provider = new FilesProvider("Data", "Default", 8192, 8192, Encoding.UTF8, 10000, true);
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
		}

		// TODO: Saving new objects with subobjects saved by reference.
		// TODO: Centralized block cache in FilesProvider that all files use.
		// TODO: FilesProvider: File with Key names. Master table for indices.
		// TODO: Multi-threaded stress test (with multiple indices).
		// TOOO: Test huge databases with more than uint.MaxValue objects.
		// TODO: Startup: Scan file if not shut down correctly. Rebuild in case file is corrupt
	}
}
