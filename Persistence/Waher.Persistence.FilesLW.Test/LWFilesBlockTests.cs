using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !LW
namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public class BlocksTests
	{
		private const int BlockSize = 16384;
		private const int BlocksInCache = 10000;

		private static ObjectBTreeFile file;
		private static FilesProvider provider;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext Context)
		{
			DBFilesBTreeTests.DeleteFiles();

#if LW
			provider = await FilesProvider.CreateAsync("Data", "Default", BlockSize, BlocksInCache, Math.Max(BlockSize / 2, 1024), Encoding.UTF8, 10000);
#else
			provider = await FilesProvider.CreateAsync("Data", "Default", BlockSize, BlocksInCache, Math.Max(BlockSize / 2, 1024), Encoding.UTF8, 10000, true);
#endif
			file = await provider.GetFile("Default");
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (provider != null)
			{
				provider.Dispose();
				provider = null;
			}

			file = null;
		}

		[TestMethod]
		public async Task DBFiles_Block_Test_01_SaveBlock()
		{
			byte[] Block = new byte[BlockSize];
			int i;

			for (i = 0; i < BlockSize; i++)
				Block[i] = (byte)i;

			await file.SaveBlock(0, Block);
		}

		[TestMethod]
		public async Task DBFiles_Block_Test_02_LoadBlock()
		{
			file.ClearCache();
			byte[] Block = await file.LoadBlock(0);
			int i;

			Assert.AreEqual(BlockSize, Block.Length);

			for (i = 0; i < BlockSize; i++)
				Assert.AreEqual((byte)i, Block[i]);
		}
	}
}
