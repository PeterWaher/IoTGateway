using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.HostSettingObjects
{
	/// <summary>
	/// DateTime host setting object.
	/// </summary>
	public class DateTimeHostSetting : HostSetting
	{
		private DateTime value = DateTime.MinValue;

		/// <summary>
		/// DateTime host setting object.
		/// </summary>
		public DateTimeHostSetting()
		{
		}

		/// <summary>
		/// DateTime host setting object.
		/// </summary>
		/// <param name="Host">Host name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public DateTimeHostSetting(string Host, string Key, DateTime Value)
			: base(Host, Key)
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
