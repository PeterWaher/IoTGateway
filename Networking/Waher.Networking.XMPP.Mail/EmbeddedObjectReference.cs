using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Mail
{
	/// <summary>
	/// Contains a reference to an object embedded in a mail.
	/// </summary>
	public class EmbeddedObjectReference
	{
		private readonly string contentType;
		private readonly string description;
		private readonly string fileName;
		private readonly string name;
		private readonly string contentId;
		private readonly string embeddedObjectId;
		private readonly int size;

		/// <summary>
		/// Contains a reference to an object embedded in a mail.
		/// </summary>
		public EmbeddedObjectReference(string ContentType, string Description, string FileName, string Name, 
			string ContentId, string EmbeddedObjectId, int Size)
		{
			this.contentType = ContentType;
			this.description = Description;
			this.fileName = FileName;
			this.name = Name;
			this.contentId = ContentId;
			this.embeddedObjectId = EmbeddedObjectId;
			this.size = Size;
		}

		/// <summary>
		/// Content-Type of embedded object
		/// </summary>
		public string ContentType => this.contentType;

		/// <summary>
		/// Description, if available.
		/// </summary>
		public string Description => this.description;

		/// <summary>
		/// Filename, if available.
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Name, if available.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Content-ID of embedded object, if available.
		/// </summary>
		public string Id => this.contentId;

		/// <summary>
		/// ID of embedded object, in broker.
		/// </summary>
		public string EmbeddedObjectId => this.embeddedObjectId;

		/// <summary>
		/// Size of embedded object.
		/// </summary>
		public int Size => this.size;

	}
}
