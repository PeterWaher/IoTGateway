using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Persistence.Files.Test
{
	[TestClass]
	public class BlocksTests
	{
		private const int BlockSize = 16384;
		private const int BlocksInCache = 10000;

		private ObjectBTreeFile file;
		private FilesProvider provider;

		[ClassInitialize]
		public async void ClassInitialize()
		{
			if (File.Exists(BTreeTests.MasterFileName))
				File.Delete(BTreeTests.MasterFileName);

			if (File.Exists(BTreeTests.FileName))
				File.Delete(BTreeTests.FileName);

			if (File.Exists(BTreeTests.BlobFileName))
				File.Delete(BTreeTests.BlobFileName);

			if (File.Exists(BTreeTests.NamesFileName))
				File.Delete(BTreeTests.NamesFileName);

			this.provider = new FilesProvider("Data", "Default", BlockSize, BlocksInCache, Math.Max(BlockSize / 2, 1024), Encoding.UTF8, 10000, true);
			this.file = await this.provider.GetFile("Default");
		}

		[ClassCleanup]
		public void ClassCleanup()
		{
			this.provider.Dispose();
			this.provider = null;
			this.file = null;
		}

		[TestMethod]
		public async void Test_01_SaveBlock()
		{
			byte[] Block = new byte[BlockSize];
			int i;

			for (i = 0; i < BlockSize; i++)
				Block[i] = (byte)i;

			await this.file.SaveBlock(0, Block);
		}

		[TestMethod]
		public async void Test_02_LoadBlock()
		{
			this.file.ClearCache();
			byte[] Block = await this.file.LoadBlock(0);
			int i;

			Assert.AreEqual(BlockSize, Block.Length);

			for (i = 0; i < BlockSize; i++)
				Assert.AreEqual((byte)i, Block[i]);
		}
	}
}
