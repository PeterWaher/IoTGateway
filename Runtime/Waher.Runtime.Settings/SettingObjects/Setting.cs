using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.SettingObjects
{
	/// <summary>
	/// Base abstract class for settings.
	/// </summary>
	[TypeName(TypeNameSerialization.LocalName)]
	[CollectionName("Settings")]
	[ArchivingTime]		// No Limit
	[Index("Key")]
	public abstract class Setting
	{
		private string objectId = null;
		private string key = string.Empty;

		/// <summary>
		/// Base abstract class for settings.
		/// </summary>
		public Setting()
		{
		}

		/// <summary>
		/// Base abstract class for settings.
		/// </summary>
		/// <param name="Key">Key name.</param>
		public Setting(string Key)
		{
			this.key = Key;
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// Key name.
		/// </summary>
		public string Key
		{
			get { return this.key; }
			set { this.key = value; }
		}

		/// <summary>
		/// Gets the value of the setting, as an object.
		/// </summary>
		/// <returns>Value object.</returns>
		public abstract object GetValueObject();
	}
}
