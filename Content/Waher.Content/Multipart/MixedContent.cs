using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Multipart
{
	/// <summary>
	/// Represents mixed content, encoded with multipart/mixed
	/// </summary>
	public class MixedContent
	{
		private readonly EmbeddedContent[] content;

		/// <summary>
		/// Represents mixed content, encoded with multipart/mixed
		/// </summary>
		/// <param name="Content">Embedded content.</param>
		public MixedContent(EmbeddedContent[] Content)
		{
			this.content = Content;
		}

		/// <summary>
		/// Embedded content.
		/// </summary>
		public EmbeddedContent[] Content => this.content;
	}
}
