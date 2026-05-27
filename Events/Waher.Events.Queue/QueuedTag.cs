using Waher.Persistence.Attributes;

namespace Waher.Events.Queue
{
	/// <summary>
	/// Class representing a tag on a queued event.
	/// </summary>
	public class QueuedTag
	{
		private string name = string.Empty;
		private object value = null;

		/// <summary>
		/// Class representing a tag on a queued event.
		/// </summary>
		public QueuedTag()
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
