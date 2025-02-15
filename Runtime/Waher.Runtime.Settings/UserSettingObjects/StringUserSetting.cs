using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.UserSettingObjects
{
	/// <summary>
	/// String user setting object.
	/// </summary>
	public class StringUserSetting : UserSetting
	{
		private string value = string.Empty;

		/// <summary>
		/// String user setting object.
		/// </summary>
		public StringUserSetting()
		{
		}

		/// <summary>
		/// String user setting object.
		/// </summary>
		/// <param name="User">User name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public StringUserSetting(string User, string Key, string Value)
			: base(User, Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// Gets the value of the setting, as an object.
		/// </summary>
		/// <returns>Value object.</returns>
		public override object GetValueObject()
		{
			return this.value;
		}
	}
}
