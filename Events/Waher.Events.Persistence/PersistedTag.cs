using Waher.Persistence.Attributes;

namespace Waher.Events.Persistence
{
	/// <summary>
	/// Class representing a persisted tag.
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	public class PersistedTag
	{
		private string name = string.Empty;
		private object value = null;

		/// <summary>
		/// Class representing a persisted tag.
		/// </summary>
		public PersistedTag()
		{
		}

		/// <summary>
		/// Tag name.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Tag value.
		/// </summary>
		[DefaultValueNull]
		public object Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.name + "=" + this.value?.ToString();
		}
	}
}
