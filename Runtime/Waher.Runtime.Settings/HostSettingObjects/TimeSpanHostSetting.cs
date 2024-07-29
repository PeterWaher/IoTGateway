using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.HostSettingObjects
{
	/// <summary>
	/// TimeSpan setting object.
	/// </summary>
	public class TimeSpanHostSetting : HostSetting
	{
		private TimeSpan value = TimeSpan.MinValue;

		/// <summary>
		/// TimeSpan setting object.
		/// </summary>
		public TimeSpanHostSetting()
		{
		}

		/// <summary>
		/// TimeSpan setting object.
		/// </summary>
		/// <param name="Host">Host name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public TimeSpanHostSetting(string Host, string Key, TimeSpan Value)
			: base(Host, Key)
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
