using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Statistics
{
	/// <summary>
	/// Contains information about a file.
	/// </summary>
	public class FileStatistics
	{
		private uint blockSize;
		private uint nrObjects = 0;
		private uint nrBlocks = 0;
		private uint nrBlobBlocks = 0;
		private uint minObjSize = uint.MaxValue;
		private uint maxObjSize = uint.MinValue;
		private uint minDepth = uint.MaxValue;
		private uint maxDepth = uint.MinValue;
		private uint minObjPerBlock = uint.MaxValue;
		private uint maxObjPerBlock = uint.MinValue;
		private uint minBytesUsedPerBlock = uint.MaxValue;
		private uint maxBytesUsedPerBlock = uint.MinValue;
		private ulong sumObjSize = 0;
		private ulong nrBytesUsed = 0;
		private ulong nrBytesUnused = 0;
		private ulong nrBytesTotal = 0;
		private ulong nrBlobBytesUsed = 0;
		private ulong nrBlobBytesUnused = 0;
		private ulong nrBlobBytesTotal = 0;
		private ulong nrBlockLoads = 0;
		private ulong nrCacheLoads = 0;
		private ulong nrBlockSaves = 0;
		private ulong nrBlobBlockLoads = 0;
		private ulong nrBlobBlockSaves = 0;
		private ulong nrFullFileScans;
		private ulong nrSearches;
		private bool isCorrupt = false;
		private bool isBalanced = true;
		private List<string> comments = null;
		private object synchObject = new object();

		/// <summary>
		/// Contains information about a file.
		/// </summary>
		public FileStatistics(uint BlockSize, ulong NrBlockLoads, ulong NrCacheLoads, ulong NrBlockSaves, 
			ulong NrBlobBlockLoads, ulong NrBlobBlockSaves, ulong NrFullFileScans, ulong NrSearches)
		{
			this.blockSize = BlockSize;
			this.nrBlockLoads = NrBlockLoads;
			this.nrCacheLoads = NrCacheLoads;
			this.nrBlockSaves = NrBlockSaves;
			this.nrBlobBlockLoads = NrBlobBlockLoads;
			this.nrBlobBlockSaves = NrBlobBlockSaves;
			this.nrFullFileScans = NrFullFileScans;
			this.nrSearches = NrSearches;
		}

		/// <summary>
		/// Block size
		/// </summary>
		public uint BlockSize
		{
			get { return this.blockSize; }
		}

		/// <summary>
		/// Number of searches performed against the file.
		/// </summary>
		public ulong NrSearches
		{
			get { return this.nrSearches; }
		}

		/// <summary>
		/// Number of searches performed, resulting in full file scans.
		/// </summary>
		public ulong NrFullFileScans
		{
			get { return this.nrFullFileScans; }
		}

		/// <summary>
		/// Number of blocks
		/// </summary>
		public uint NrBlocks
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBlocks;
				}
			}
		}

		/// <summary>
		/// Number of bytes used.
		/// </summary>
		public ulong NrBytesUsed
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBytesUsed;
				}
			}
		}

		/// <summary>
		/// Number of bytes unused.
		/// </summary>
		public ulong NrBytesUnused
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBytesUnused;
				}
			}
		}

		/// <summary>
		/// Total number of bytes in file.
		/// </summary>
		public ulong NrBytesTotal
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBytesTotal;
				}
			}
		}

		/// <summary>
		/// Number of BLOB blocks
		/// </summary>
		public uint NrBlobBlocks
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBlobBlocks;
				}
			}
		}

		/// <summary>
		/// Number of BLOB bytes used.
		/// </summary>
		public ulong NrBlobBytesUsed
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBlobBytesUsed;
				}
			}
		}

		/// <summary>
		/// Number of BLOB bytes unused.
		/// </summary>
		public ulong NrBlobBytesUnused
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBlobBytesUnused;
				}
			}
		}

		/// <summary>
		/// Total number of BLOB bytes in file.
		/// </summary>
		public ulong NrBlobBytesTotal
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBlobBytesTotal;
				}
			}
		}

		/// <summary>
		/// Usage, in percent.
		/// </summary>
		public double Usage
		{
			get
			{
				lock (this.synchObject)
				{
					return (100.0 * (this.nrBytesUsed + this.nrBlobBytesUsed)) / (this.nrBytesTotal + this.nrBlobBytesTotal);
				}
			}
		}

		/// <summary>
		/// Number of objects stored.
		/// </summary>
		public ulong NrObjects
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrObjects;
				}
			}
		}

		/// <summary>
		/// Size of smallest object.
		/// </summary>
		public uint MinObjectSize
		{
			get
			{
				lock (this.synchObject)
				{
					return this.minObjSize;
				}
			}
		}

		/// <summary>
		/// Size of largest object.
		/// </summary>
		public uint MaxObjectSize
		{
			get
			{
				lock (this.synchObject)
				{
					return this.maxObjSize;
				}
			}
		}

		/// <summary>
		/// Smallest number of objects in a block.
		/// </summary>
		public uint MinObjectsPerBlock
		{
			get
			{
				lock (this.synchObject)
				{
					return this.minObjPerBlock;
				}
			}
		}

		/// <summary>
		/// Largest number of objects in a block.
		/// </summary>
		public uint MaxObjectsPerBlock
		{
			get
			{
				lock (this.synchObject)
				{
					return this.maxObjPerBlock;
				}
			}
		}

		/// <summary>
		/// Smallest number of bytes used in a block.
		/// </summary>
		public uint MinBytesUsedPerBlock
		{
			get
			{
				lock (this.synchObject)
				{
					return this.minBytesUsedPerBlock;
				}
			}
		}

		/// <summary>
		/// Largest number of bytes used in a block.
		/// </summary>
		public uint MaxBytesUsedPerBlock
		{
			get
			{
				lock (this.synchObject)
				{
					return this.maxBytesUsedPerBlock;
				}
			}
		}

		/// <summary>
		/// Average size of object.
		/// </summary>
		public double AverageObjectSize
		{
			get
			{
				lock (this.synchObject)
				{
					return ((double)this.sumObjSize) / this.nrObjects;
				}
			}
		}

		/// <summary>
		/// Average number of objects per block.
		/// </summary>
		public double AverageObjectsPerBlock
		{
			get
			{
				lock (this.synchObject)
				{
					return ((double)this.nrObjects) / this.nrBlocks;
				}
			}
		}

		/// <summary>
		/// Average bytes used per block.
		/// </summary>
		public double AverageBytesUsedPerBlock
		{
			get
			{
				lock (this.synchObject)
				{
					return ((double)this.nrBytesUsed) / this.nrBlocks;
				}
			}
		}

		/// <summary>
		/// Depth of most shallow leaf.
		/// </summary>
		public uint MinDepth
		{
			get
			{
				lock (this.synchObject)
				{
					return this.minDepth;
				}
			}
		}

		/// <summary>
		/// Depth of deepest leaf.
		/// </summary>
		public uint MaxDepth
		{
			get
			{
				lock (this.synchObject)
				{
					return this.maxDepth;
				}
			}
		}

		/// <summary>
		/// Number of blocks load operations performed.
		/// </summary>
		public ulong NrBlockLoads
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBlockLoads;
				}
			}
		}

		/// <summary>
		/// Number of blocks load operations performed, where result was fetched from internal cache.
		/// </summary>
		public ulong NrCacheLoads
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrCacheLoads;
				}
			}
		}

		/// <summary>
		/// Number of blocks save operations performed.
		/// </summary>
		public ulong NrBlockSaves
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBlockSaves;
				}
			}
		}

		/// <summary>
		/// Number of BLOB blocks load operations performed.
		/// </summary>
		public ulong NrBlobBlockLoads
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBlobBlockLoads;
				}
			}
		}

		/// <summary>
		/// Number of BLOB blocks save operations performed.
		/// </summary>
		public ulong NrBlobBlockSaves
		{
			get
			{
				lock (this.synchObject)
				{
					return this.nrBlobBlockSaves;
				}
			}
		}

		/// <summary>
		/// If the file is corrupt.
		/// </summary>
		public bool IsCorrupt
		{
			get
			{
				lock (this.synchObject)
				{
					return this.isCorrupt;
				}
			}
		}

		/// <summary>
		/// If the file is balanced.
		/// </summary>
		public bool IsBalanced
		{
			get
			{
				lock (this.synchObject)
				{
					return this.isBalanced && this.minDepth == this.maxDepth;
				}
			}

			internal set
			{
				lock (this.synchObject)
				{
					this.isBalanced = value;
				}
			}
		}

		/// <summary>
		/// Any comments logged when scanning the file. If no comments, this property is null.
		/// </summary>
		public string[] Comments
		{
			get
			{
				lock (this.synchObject)
				{
					return this.comments.ToArray();
				}
			}
		}

		/// <summary>
		/// If comments have been logged.
		/// </summary>
		public bool HasComments
		{
			get
			{
				lock (this.synchObject)
				{
					return this.comments != null;
				}
			}
		}

		internal void ReportBlockStatistics(uint NrBytesUsed, uint NrBytesUnused, uint NrObjects)
		{
			lock (this.synchObject)
			{
				this.nrBlocks++;
				this.nrBytesUsed += NrBytesUsed;
				this.nrBytesUnused += NrBytesUnused;
				this.nrBytesTotal += NrBytesUsed + NrBytesUnused;

				if (NrObjects < this.minObjPerBlock)
					this.minObjPerBlock = NrObjects;

				if (NrObjects > this.maxObjPerBlock)
					this.maxObjPerBlock = NrObjects;

				if (NrBytesUsed < this.minBytesUsedPerBlock)
					this.minBytesUsedPerBlock = NrBytesUsed;

				if (NrBytesUsed > this.maxBytesUsedPerBlock)
					this.maxBytesUsedPerBlock = NrBytesUsed;
			}
		}

		internal void ReportBlobBlockStatistics(uint NrBytesUsed, uint NrBytesUnused)
		{
			lock (this.synchObject)
			{
				this.nrBlobBlocks++;
				this.nrBlobBytesUsed += NrBytesUsed;
				this.nrBlobBytesUnused += NrBytesUnused;
				this.nrBlobBytesTotal += NrBytesUsed + NrBytesUnused;

				if (NrBytesUsed < this.minBytesUsedPerBlock)
					this.minBytesUsedPerBlock = NrBytesUsed;

				if (NrBytesUsed > this.maxBytesUsedPerBlock)
					this.maxBytesUsedPerBlock = NrBytesUsed;
			}
		}

		internal void ReportObjectStatistics(uint ObjectSize)
		{
			lock (this.synchObject)
			{
				if (ObjectSize < this.minObjSize)
					this.minObjSize = ObjectSize;

				if (ObjectSize > this.maxObjSize)
					this.maxObjSize = ObjectSize;

				this.sumObjSize += ObjectSize;
				this.nrObjects++;
			}
		}

		internal void ReportDepthStatistics(uint Depth)
		{
			lock (this.synchObject)
			{
				if (Depth < this.minDepth)
					this.minDepth = Depth;

				if (Depth > this.maxDepth)
					this.maxDepth = Depth;
			}
		}

		internal void LogError(string Message)
		{
			lock (this.synchObject)
			{
				this.isCorrupt = true;

				if (this.comments == null)
					this.comments = new List<string>();

				this.comments.Add(Message);
			}
		}

		internal void LogComment(string Message)
		{
			lock (this.synchObject)
			{
				if (this.comments == null)
					this.comments = new List<string>();

				this.comments.Add(Message);
			}
		}

		public void ToString(StringBuilder Output, bool WriteStat)
		{
			lock (this.synchObject)
			{
				if (WriteStat)
				{
					Output.AppendLine("Block Size: " + this.blockSize.ToString());
					Output.AppendLine("#Blocks: " + this.nrBlocks.ToString());
					Output.AppendLine("#BLOB Blocks: " + this.nrBlobBlocks.ToString());
					Output.AppendLine("#Bytes used: " + this.nrBytesUsed.ToString());
					Output.AppendLine("#Bytes unused: " + this.nrBytesUnused.ToString());
					Output.AppendLine("#Bytes total: " + this.nrBytesTotal.ToString());
					Output.AppendLine("#BLOB Bytes used: " + this.nrBlobBytesUsed.ToString());
					Output.AppendLine("#BLOB Bytes unused: " + this.nrBlobBytesUnused.ToString());
					Output.AppendLine("#BLOB Bytes total: " + this.nrBlobBytesTotal.ToString());
					Output.AppendLine("#Block loads: " + this.nrBlockLoads.ToString());
					Output.AppendLine("#Cache loads: " + this.nrCacheLoads.ToString());
					Output.AppendLine("#Block saves: " + this.nrBlockSaves.ToString());
					Output.AppendLine("#BLOB Block loads: " + this.nrBlobBlockLoads.ToString());
					Output.AppendLine("#BLOB Block saves: " + this.nrBlobBlockSaves.ToString());
					Output.AppendLine("#Objects: " + this.nrObjects.ToString());
					Output.AppendLine("Smallest object: " + this.minObjSize.ToString());
					Output.AppendLine("Largest object: " + this.maxObjSize.ToString());
					Output.AppendLine("Average object: " + this.AverageObjectSize.ToString("F1"));
					Output.AppendLine("Usage: " + this.Usage.ToString("F2") + " %");
					Output.AppendLine("Min(Depth): " + this.minDepth.ToString());
					Output.AppendLine("Max(Depth): " + this.maxDepth.ToString());
					Output.AppendLine("Min(Objects/Block): " + this.minObjPerBlock.ToString());
					Output.AppendLine("Max(Objects/Block): " + this.maxObjPerBlock.ToString());
					Output.AppendLine("Avg(Objects/Block): " + this.AverageObjectsPerBlock.ToString("F1"));
					Output.AppendLine("Min(Bytes Used/Block): " + this.minBytesUsedPerBlock.ToString());
					Output.AppendLine("Max(Bytes Used/Block): " + this.maxBytesUsedPerBlock.ToString());
					Output.AppendLine("Avg(Bytes Used/Block): " + this.AverageBytesUsedPerBlock.ToString("F1"));
					Output.AppendLine("Is Corrupt: " + this.isCorrupt.ToString());
					Output.AppendLine("Is Balanced: " + this.isBalanced.ToString());
					Output.AppendLine("Has Comments: " + this.HasComments.ToString());
				}

				if (this.HasComments)
				{
					Output.AppendLine();
					foreach (string Comment in this.Comments)
						Output.AppendLine(Comment);
				}
			}
		}

	}
}
