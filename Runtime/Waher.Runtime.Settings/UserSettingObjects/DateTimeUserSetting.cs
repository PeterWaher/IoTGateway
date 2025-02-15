using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.UserSettingObjects
{
	/// <summary>
	/// DateTime user setting object.
	/// </summary>
	public class DateTimeUserSetting : UserSetting
	{
		private DateTime value = DateTime.MinValue;

		/// <summary>
		/// DateTime user setting object.
		/// </summary>
		public DateTimeUserSetting()
		{
		}

		/// <summary>
		/// DateTime user setting object.
		/// </summary>
		/// <param name="User">User name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public DateTimeUserSetting(string User, string Key, DateTime Value)
			: base(User, Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Value
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
