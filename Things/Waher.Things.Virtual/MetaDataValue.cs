using Waher.Persistence.Attributes;

namespace Waher.Things.Virtual
{
	/// <summary>
	/// Class representing a meta-data value.
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	public class MetaDataValue
	{
		private string name = string.Empty;
		private object value = null;

		/// <summary>
		/// Class representing a persisted tag.
		/// </summary>
		public MetaDataValue()
		{
		}

		/// <summary>
		/// Tag name.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Tag value.
		/// </summary>
		[DefaultValueNull]
		public object Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.name + "=" + this.value?.ToString();
		}
	}
}
