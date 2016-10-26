using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Storage
{
	internal class BlockHeader
	{
		public ushort BytesUsed;
		public uint SizeSubtree;
		public uint LastBlockLink;
		public uint ParentBlockLink;

		public BlockHeader(BinaryDeserializer Deserializer)
		{
			this.BytesUsed = Deserializer.ReadUInt16();         // 0: Counts number of bytes used in block, excluding the node header, and the last block link (14 bytes in total).
			this.SizeSubtree = Deserializer.ReadUInt32();       // 2: Order statistic tree, counting number of objects rooted at the current node.
			this.LastBlockLink = Deserializer.ReadUInt32();     // 6: Last block link.
			this.ParentBlockLink = Deserializer.ReadUInt32();   // 10: Parent block link.
		}
	}
}
