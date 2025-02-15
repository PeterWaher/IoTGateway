using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.UserSettingObjects
{
	/// <summary>
	/// Double user setting object.
	/// </summary>
	public class DoubleUserSetting : UserSetting
	{
		private double value = 0.0;

		/// <summary>
		/// Double user setting object.
		/// </summary>
		public DoubleUserSetting()
		{
		}

		/// <summary>
		/// Double user setting object.
		/// </summary>
		/// <param name="User">User name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public DoubleUserSetting(string User, string Key, double Value)
			: base(User, Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValue(0.0)]
		public double Value
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
