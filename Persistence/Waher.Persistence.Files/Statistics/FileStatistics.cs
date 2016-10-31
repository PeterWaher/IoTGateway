using System;
using System.Collections.Generic;
using System.Text;
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
		private uint minObjSize = uint.MaxValue;
		private uint maxObjSize = uint.MinValue;
		private uint minDepth = uint.MaxValue;
		private uint maxDepth = uint.MinValue;
		private ulong sumObjSize = 0;
		private ulong nrBytesUsed = 0;
		private ulong nrBytesUnused = 0;
		private ulong nrBytesTotal = 0;
		private ulong nrBlockLoads = 0;
		private ulong nrCacheLoads = 0;
		private ulong nrBlockSaves = 0;
		private bool isCorrupt = false;
		private List<string> comments = null;
		private object synchObject = new object();

		/// <summary>
		/// Contains information about a file.
		/// </summary>
		public FileStatistics(uint BlockSize, ulong NrBlockLoads, ulong NrCacheLoads, ulong NrBlockSaves)
		{
			this.blockSize = BlockSize;
			this.nrBlockLoads = NrBlockLoads;
			this.nrCacheLoads = NrCacheLoads;
			this.nrBlockSaves = NrBlockSaves;
		}

		/// <summary>
		/// Block size
		/// </summary>
		public uint BlockSize
		{
			get
			{
				lock (this.synchObject)
				{
					return this.blockSize;
				}
			}
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
					return this.minDepth == this.maxDepth;
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

		internal void AddBlockStatistics(uint NrBytesUsed, uint NrBytesUnused)
		{
			lock (this.synchObject)
			{
				this.nrBlocks++;
				this.nrBytesUsed += NrBytesUsed;
				this.nrBytesUnused += NrBytesUnused;
				this.nrBytesTotal += NrBytesUsed + NrBytesUnused;
			}
		}

		internal void AddObjectStatistics(uint ObjectSize)
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

		internal void AddDepthStatistics(uint Depth)
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

	}
}
