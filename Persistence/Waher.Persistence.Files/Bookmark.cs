using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Bookmark
	/// </summary>
	public class Bookmark
	{
		private ObjectBTreeFile file;
		private BlockInfo position;

		internal Bookmark(ObjectBTreeFile File, BlockInfo Position)
		{
			this.file = File;
			this.position = Position;
		}

		internal ObjectBTreeFile File
		{
			get { return this.file; }
		}

		internal BlockInfo Position
		{
			get { return this.position; }
		}
	}
}
