using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.UserSettingObjects
{
	/// <summary>
	/// TimeSpan user setting object.
	/// </summary>
	public class TimeSpanUserSetting : UserSetting
	{
		private TimeSpan value = TimeSpan.MinValue;

		/// <summary>
		/// TimeSpan user setting object.
		/// </summary>
		public TimeSpanUserSetting()
		{
		}

		/// <summary>
		/// TimeSpan user setting object.
		/// </summary>
		/// <param name="User">User name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public TimeSpanUserSetting(string User, string Key, TimeSpan Value)
			: base(User, Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValueTimeSpanMinValue]
		public TimeSpan Value
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
