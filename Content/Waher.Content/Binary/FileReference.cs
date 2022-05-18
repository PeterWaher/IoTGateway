using System.IO;

namespace Waher.Content.Binary
{
	/// <summary>
	/// File reference.
	/// </summary>
	public class FileReference
	{
		private readonly string fileName;
		private readonly string contentType;

		/// <summary>
		/// File reference.
		/// </summary>
		/// <param name="FileName">File Name</param>
		public FileReference(string FileName)
		{
			this.fileName = FileName;
			this.contentType = InternetContent.GetContentType(Path.GetExtension(FileName));
		}

		/// <summary>
		/// File Name
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Content-Type
		/// </summary>
		public string ContentType => this.contentType;
	}
}
