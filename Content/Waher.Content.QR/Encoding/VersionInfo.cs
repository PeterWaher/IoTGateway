using System;

namespace Waher.Content.QR.Encoding
{
	/// <summary>
	/// Contains information about one version of encoding.
	/// </summary>
	public class VersionInfo
	{
		private readonly CorrectionLevel level;
		private readonly int version;
		private readonly int ecBytesPerBlock;
		private readonly int blocksPerGroup1;
		private readonly int dataBytesPerBlock1;
		private readonly int blocksPerGroup2;
		private readonly int dataBytesPerBlock2;
		private readonly int totalBlocks;
		private readonly int totalEcBytes;
		private readonly int totalDataBytes;
		private readonly int totalBytes;

		internal VersionInfo(CorrectionLevel Level, int Version, int EcBytesPerBlock, 
			int BlocksPerGroup1, int DataBytesPerBlock1, int BlocksPerGroup2, 
			int DataBytesPerBlock2)
		{
			this.level = Level;
			this.version = Version;
			this.ecBytesPerBlock = EcBytesPerBlock;
			this.blocksPerGroup1 = BlocksPerGroup1;
			this.dataBytesPerBlock1 = DataBytesPerBlock1;
			this.blocksPerGroup2 = BlocksPerGroup2;
			this.dataBytesPerBlock2 = DataBytesPerBlock2;
			this.totalBlocks = this.blocksPerGroup1 + this.blocksPerGroup2;
			this.totalEcBytes = this.ecBytesPerBlock * (this.blocksPerGroup1 + this.blocksPerGroup2);
			this.totalDataBytes = this.blocksPerGroup1 * this.dataBytesPerBlock1 + this.blocksPerGroup2 * this.dataBytesPerBlock2;
			this.totalBytes = this.totalDataBytes + this.totalEcBytes;
		}

		/// <summary>
		/// Error correction level.
		/// </summary>
		public CorrectionLevel Level => this.level;

		/// <summary>
		/// Version number
		/// </summary>
		public int Version => this.version;

		/// <summary>
		/// Error Correction bytes per block
		/// </summary>
		public int EcBytesPerBlock => this.ecBytesPerBlock;

		/// <summary>
		/// Blocks in Group 1
		/// </summary>
		public int BlocksPerGroup1 => this.blocksPerGroup1;

		/// <summary>
		/// Data bytes per block in group 1
		/// </summary>
		public int DataBytesPerBlock1 => this.dataBytesPerBlock1;

		/// <summary>
		/// Blocks in Group 2
		/// </summary>
		public int BlocksPerGroup2 => this.blocksPerGroup2;

		/// <summary>
		/// Data bytes per block in group 2
		/// </summary>
		public int DataBytesPerBlock2 => this.dataBytesPerBlock2;

		/// <summary>
		/// Total number of blocks
		/// </summary>
		public int TotalBlocks => this.totalBlocks;

		/// <summary>
		/// Total number of error correction bytes
		/// </summary>
		public int TotalEcBytes => this.totalEcBytes;

		/// <summary>
		/// Total number of data bytes
		/// </summary>
		public int TotalDataBytes => this.totalDataBytes;

		/// <summary>
		/// Total number of bytes
		/// </summary>
		public int TotalBytes => this.totalBytes;
	}
}
