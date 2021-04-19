using System;
using System.Text;

namespace Waher.Content.Images.Exif
{
	/// <summary>
	/// Abstract base class for EXIF meta-data tags.
	/// </summary>
	public abstract class ExifTag
	{
		private readonly ExifTagName name;
		private readonly int tagId;

		/// <summary>
		/// Abstract base class for EXIF meta-data tags.
		/// </summary>
		/// <param name="TagId">Tag ID</param>
		/// <param name="Name">Tag Name</param>
		public ExifTag(int TagId, ExifTagName Name)
		{
			this.tagId = TagId;
			this.name = Name;
		}

		/// <summary>
		/// EXIF Tag ID
		/// </summary>
		public int TagID => this.tagId;

		/// <summary>
		/// EXIF Tag Name
		/// </summary>
		public ExifTagName Name => this.name;

		/// <summary>
		/// EXIF Tag Value
		/// </summary>
		public abstract object Value { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (this.name == ExifTagName.Unknown)
				sb.Append(this.tagId.ToString());
			else
				sb.Append(this.name.ToString());
			
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
