namespace Waher.Persistence.Files.Storage
{
	internal class BlockInfo
	{
		public BlockHeader Header;
		public byte[] Block;
		public uint BlockIndex;
		public int InternalPosition;
		public bool LastObject;
		public bool Match;

		public BlockInfo(BlockHeader Header, byte[] Block, uint BlockIndex, int InternalPosition, bool LastObject, bool Match)
		{
			this.Header = Header;
			this.Block = Block;
			this.BlockIndex = BlockIndex;
			this.InternalPosition = InternalPosition;
			this.LastObject = LastObject;
			this.Match = Match;
		}
	}
}
