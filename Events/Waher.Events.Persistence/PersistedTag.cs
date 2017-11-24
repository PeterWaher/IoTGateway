using System;
using System.Collections.Generic;
using Waher.Persistence.Attributes;

namespace Waher.Events
{
	/// <summary>
	/// Class representing a persisted tag.
	/// </summary>
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
		/// Free-text event message.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Object related to the event.
		/// </summary>
		[DefaultValueNull]
		public object Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.name + "=" + this.value?.ToString();
		}
	}
}
