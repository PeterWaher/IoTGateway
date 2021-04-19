using System;
using System.Text;

namespace Waher.Content.Images.Exif
{
	/// <summary>
	/// Abstract base class for EXIF meta-data tags.
	/// </summary>
	public abstract class ExifTag
	{
		private readonly ExifTagName tagId;

		/// <summary>
		/// Abstract base class for EXIF meta-data tags.
		/// </summary>
		/// <param name="TagId">Tag ID</param>
		public ExifTag(ExifTagName TagId)
		{
			this.tagId = TagId;
		}

		/// <summary>
		/// EXIF Tag ID
		/// </summary>
		public ExifTagName TagID => this.tagId;

		/// <summary>
		/// EXIF Tag Value
		/// </summary>
		public abstract object Value { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.tagId.ToString());
			sb.Append("=");

			object Value = this.Value;

			if (Value is Array A)
			{
				bool First = true;

				sb.Append('[');

				foreach (object Item in A)
				{
					if (First)
						First = false;
					else
						sb.Append(", ");

					sb.Append(Item.ToString());
				}

				sb.Append(']');
			}
			else
				sb.Append(Value.ToString());

			return sb.ToString();
		}
	}
}
