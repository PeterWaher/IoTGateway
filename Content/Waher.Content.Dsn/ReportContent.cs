using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Multipart;

namespace Waher.Content.Dsn
{
	/// <summary>
	/// Represents report content, encoded with multipart/report
	/// </summary>
	public class ReportContent : MultipartContent
	{
		/// <summary>
		/// Represents report content, encoded with multipart/report
		/// </summary>
		/// <param name="Content">Embedded content.</param>
		public ReportContent(EmbeddedContent[] Content)
			: base(Content)
		{
		}
	}
}
