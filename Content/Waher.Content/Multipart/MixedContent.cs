using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Multipart
{
	/// <summary>
	/// Represents mixed content, encoded with multipart/mixed
	/// </summary>
	public class MixedContent : MultipartContent
	{
		/// <summary>
		/// Represents mixed content, encoded with multipart/mixed
		/// </summary>
		/// <param name="Content">Embedded content.</param>
		public MixedContent(EmbeddedContent[] Content)
			: base(Content)
		{
		}
	}
}
