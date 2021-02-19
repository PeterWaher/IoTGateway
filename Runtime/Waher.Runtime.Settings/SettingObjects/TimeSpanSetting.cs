using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.SettingObjects
{
	/// <summary>
	/// TimeSpan setting object.
	/// </summary>
	public class TimeSpanSetting : Setting
	{
		private TimeSpan value = TimeSpan.MinValue;

		/// <summary>
		/// TimeSpan setting object.
		/// </summary>
		public TimeSpanSetting()
		{
		}

		/// <summary>
		/// TimeSpan setting object.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public TimeSpanSetting(string Key, TimeSpan Value)
			: base(Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValueTimeSpanMinValue]
		public TimeSpan Value
		{
			get { return this.value; }
			set { this.value = value; }
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
