namespace Waher.Persistence.Files.Storage
{
	internal class MergeResult
	{
		public byte[] ResultBlock = null;
		public byte[] Separator = null;
		public byte[] Residue = null;
		public uint ResultBlockSize = 0;
		public uint ResidueSize = 0;

		public MergeResult()
		{
		}
	}
}
