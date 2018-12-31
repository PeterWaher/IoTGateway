using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Multipart
{
	/// <summary>
	/// Represents related content, encoded with multipart/related
	/// </summary>
	public class RelatedContent : MultipartContent
	{
		private string type;

		/// <summary>
		/// Represents related content, encoded with multipart/related
		/// </summary>
		/// <param name="Content">Embedded content.</param>
		/// <param name="Type">Principle content type of related content.</param>
		public RelatedContent(EmbeddedContent[] Content, string Type)
			: base(Content)
		{
			this.type = Type;
		}

		/// <summary>
		/// Principle content type of related content, if specified.
		/// </summary>
		public string Type => this.type;
	}
}
