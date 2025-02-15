namespace Waher.Runtime.Settings.UserSettingObjects
{
	/// <summary>
	/// Boolean user setting object.
	/// </summary>
	public class BooleanUserSetting : UserSetting
	{
		private bool value = false;

		/// <summary>
		/// Boolean user setting object.
		/// </summary>
		public BooleanUserSetting()
		{
		}

		/// <summary>
		/// Boolean user setting object.
		/// </summary>
		/// <param name="User">User name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public BooleanUserSetting(string User, string Key, bool Value)
			: base(User, Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		public bool Value
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
