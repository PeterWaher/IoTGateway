using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.UserSettingObjects
{
	/// <summary>
	/// Base abstract class for user settings.
	/// </summary>
	[TypeName(TypeNameSerialization.LocalName)]
	[CollectionName("UserSettings")]
	[ArchivingTime]		// No Limit
	[Index("User", "Key")]
	[Index("Key", "User")]
	public abstract class UserSetting
	{
		private string objectId = null;
		private string user = string.Empty;
		private string key = string.Empty;

		/// <summary>
		/// Base abstract class for user settings.
		/// </summary>
		public UserSetting()
		{
		}

		/// <summary>
		/// Base abstract class for user settings.
		/// </summary>
		/// <param name="User">User name.</param>
		/// <param name="Key">Key name.</param>
		public UserSetting(string User, string Key)
		{
			this.user = User;
			this.key = Key;
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// User name.
		/// </summary>
		public string User
		{
			get => this.user;
			set => this.user = value;
		}

		/// <summary>
		/// Key name.
		/// </summary>
		public string Key
		{
			get => this.key;
			set => this.key = value;
		}

		/// <summary>
		/// Gets the value of the setting, as an object.
		/// </summary>
		/// <returns>Value object.</returns>
		public abstract object GetValueObject();
	}
}
