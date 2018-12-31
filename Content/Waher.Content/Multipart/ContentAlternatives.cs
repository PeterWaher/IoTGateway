using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Multipart
{
	/// <summary>
	/// Represents alternative versions of the same content, encoded with 
	/// multipart/alternative
	/// </summary>
	public class ContentAlternatives
	{
		private readonly EmbeddedContent[] content;

		/// <summary>
		/// Represents mixed content, encoded with multipart/mixed
		/// </summary>
		/// <param name="Content">Embedded content.</param>
		public ContentAlternatives(EmbeddedContent[] Content)
		{
			this.content = Content;
		}

		/// <summary>
		/// Embedded content.
		/// </summary>
		public EmbeddedContent[] Content => this.content;
	}
}
