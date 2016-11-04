using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Storage
{
	internal abstract class BlockSplitter
	{
		public byte[] LeftBlock;
		public byte[] RightBlock;
		public byte[] ParentObject = null;
		public uint LeftSizeSubtree = 0;
		public int LeftPos = ObjectBTreeFile.BlockHeaderSize;
		public uint RightSizeSubtree = 0;
		public int RightPos = ObjectBTreeFile.BlockHeaderSize;
		public int TotPos = ObjectBTreeFile.BlockHeaderSize;

		public BlockSplitter()
		{
		}

		public uint LeftLastBlockIndex
		{
			get { return BitConverter.ToUInt32(this.LeftBlock, 6); }
			set { Array.Copy(BitConverter.GetBytes(value), 0, this.LeftBlock, 6, 4); }
		}

		public uint RightLastBlockIndex
		{
			get { return BitConverter.ToUInt32(this.RightBlock, 6); }
			set { Array.Copy(BitConverter.GetBytes(value), 0, this.RightBlock, 6, 4); }
		}

		public abstract void NextBlock(uint BlockLink, byte[] Block, int Pos, int Len, uint ChildSize);

	}
}
