using System;

namespace Waher.Content.Images.Exif
{
	/// <summary>
	/// Base class for typed EXIF meta-data tags.
	/// </summary>
	public class ExifTypedTag<T> : ExifTag
	{
		private readonly T value;

		/// <summary>
		/// Base class for typed EXIF meta-data tags.
		/// </summary>
		/// <param name="TagId">Tag ID</param>
		/// <param name="Value">Tag Value</param>
		public ExifTypedTag(ExifTagName TagId, T Value)
			: base(TagId)
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
		public T TypedValue => this.value;
	}
}
