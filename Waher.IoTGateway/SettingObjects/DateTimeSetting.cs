using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.SettingObjects
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
	}
}
