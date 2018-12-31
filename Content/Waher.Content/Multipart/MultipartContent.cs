using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Multipart
{
	/// <summary>
	/// Abstract base class for multipart content
	/// </summary>
	public abstract class MultipartContent
	{
		private readonly EmbeddedContent[] content;

		/// <summary>
		/// Abstract base class for multipart content
		/// </summary>
		/// <param name="Content">Embedded content.</param>
		public MultipartContent(EmbeddedContent[] Content)
		{
			this.content = Content;
		}

		/// <summary>
		/// Embedded content.
		/// </summary>
		public EmbeddedContent[] Content => this.content;
	}
}
