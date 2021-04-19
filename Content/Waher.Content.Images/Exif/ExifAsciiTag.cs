using System;

namespace Waher.Content.Images.Exif
{
	/// <summary>
	/// Class for ASCII-valued EXIF meta-data tags.
	/// </summary>
	public class ExifAsciiTag : ExifTag
	{
		private readonly string value;

		/// <summary>
		/// Class for ASCII-valued EXIF meta-data tags.
		/// </summary>
		/// <param name="TagId">Tag ID</param>
		/// <param name="Name">Tag Name</param>
		/// <param name="Value">Tag Value</param>
		public ExifAsciiTag(int TagId, ExifTagName Name, string Value)
			: base(TagId, Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// EXIF Tag Value
		/// </summary>
		public override object Value => this.value;

		/// <summary>
		/// Typed EXIF Tag Value
		/// </summary>
		public string TypedValue => this.value;
	}
}
