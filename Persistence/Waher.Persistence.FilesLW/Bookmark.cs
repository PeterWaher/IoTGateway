using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Bookmark
	/// </summary>
	public class Bookmark
	{
		private readonly ObjectBTreeFile file;
		private readonly BlockInfo position;

		internal Bookmark(ObjectBTreeFile File, BlockInfo Position)
		{
			this.file = File;
			this.position = Position;
		}

		internal ObjectBTreeFile File => this.file;

		internal BlockInfo Position => this.position;
	}
}
