using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Storage
{
	internal class BlockSplitterMiddle : BlockSplitter
	{
		public int MiddlePos;

		public BlockSplitterMiddle(int PayloadSize)
			: base()
		{
			this.MiddlePos = PayloadSize / 2 + ObjectBTreeFile.BlockHeaderSize;    // Instead of median value, select the value residing in the middle of the block. These are not the same, since object values might be of different sizes.
		}

		public override void NextBlock(uint BlockLink, byte[] Block, int Pos, int Len, uint ChildSize)
		{
			int c = 4 + Len;

			if (this.ParentObject != null)
			{
				if (this.RightSizeSubtree < uint.MaxValue)
				{
					this.RightSizeSubtree++;

					this.RightSizeSubtree += ChildSize;
					if (this.RightSizeSubtree < ChildSize)
						this.RightSizeSubtree = uint.MaxValue;
				}

				Array.Copy(BitConverter.GetBytes(BlockLink), 0, RightBlock, RightPos, 4);
				this.RightPos += 4;
				Array.Copy(Block, Pos, RightBlock, RightPos, Len);
				this.RightPos += Len;
				this.TotPos += c;
			}
			else if (this.TotPos + c <= this.MiddlePos)
			{
				if (this.LeftSizeSubtree < uint.MaxValue)
				{
					this.LeftSizeSubtree++;

					this.LeftSizeSubtree += ChildSize;
					if (this.LeftSizeSubtree < ChildSize)
						this.LeftSizeSubtree = uint.MaxValue;
				}

				Array.Copy(BitConverter.GetBytes(BlockLink), 0, LeftBlock, LeftPos, 4);
				this.LeftPos += 4;
				Array.Copy(Block, Pos, LeftBlock, LeftPos, Len);
				this.LeftPos += Len;
				this.TotPos += c;
			}
			else 
			{
				this.LeftLastBlockIndex = BlockLink;
				this.LeftSizeSubtree += ChildSize;
				if (this.LeftSizeSubtree < ChildSize)
					this.LeftSizeSubtree = uint.MaxValue;

				this.ParentObject = new byte[Len];
				Array.Copy(Block, Pos, this.ParentObject, 0, Len);
			}
		}

	}
}
