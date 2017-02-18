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
	/// Int64 setting object.
	/// </summary>
	public class Int64Setting : Setting
	{
		private long value = 0;

		/// <summary>
		/// Int64 setting object.
		/// </summary>
		public Int64Setting()
		{
		}

		/// <summary>
		/// Int64 setting object.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public Int64Setting(string Key, long Value)
			: base(Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValue(0L)]
		public long Value
		{
			get { return this.value; }
			set { this.value = value; }
		}
	}
}
