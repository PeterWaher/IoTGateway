using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.SettingObjects
{
	/// <summary>
	/// DateTime setting object.
	/// </summary>
	public class DateTimeSetting : Setting
	{
		private DateTime value = DateTime.MinValue;

		/// <summary>
		/// DateTime setting object.
		/// </summary>
		public DateTimeSetting()
		{
		}

		/// <summary>
		/// DateTime setting object.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public DateTimeSetting(string Key, DateTime Value)
			: base(Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Value
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
