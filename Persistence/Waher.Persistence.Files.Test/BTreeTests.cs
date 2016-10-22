using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Waher.Persistence.Files.Test
{
	[TestFixture]
	public class BTreeTests
	{
		private const string FileName = "Objects.db";
		private const int BlockSize = 16384;
		private const int BlocksInCache = 1000;

		private ObjectBTreeFile file;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			if (File.Exists(FileName))
				File.Delete(FileName);

			this.file = new ObjectBTreeFile(FileName, "Blobs", BlockSize, BlocksInCache);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			this.file.Dispose();
			this.file = null;
		}

		[Test]
		public async void Test_01_SaveBlock()
		{
			byte[] Block = new byte[BlockSize];
			int i;

			for (i = 0; i < BlockSize; i++)
				Block[i] = (byte)i;

			await this.file.SaveBlock(0, Block, 10000);
		}

		[Test]
		public async void Test_02_LoadBlock()
		{
			this.file.ClearCache();
			byte[] Block = await this.file.LoadBlock(0, 10000);
			int i;

			Assert.AreEqual(BlockSize, Block.Length);

			for (i = 0; i < BlockSize; i++)
				Assert.AreEqual((byte)i, Block[i]);
		}
	}
}
