using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Storage
{
	internal class BlockSplitterLast : BlockSplitter
	{
		public int BlockSize;

		public BlockSplitterLast(int BlockSize)
			: base()
		{
			this.BlockSize = BlockSize;		
		}

		public override void NextBlock(uint BlockLink, byte[] Block, int Pos, int Len, uint ChildSize)
		{
			int c = 4 + Len;

			if (!(this.ParentObject is null))
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
			else if (this.TotPos + c <= this.BlockSize)
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
