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
	/// Boolean setting object.
	/// </summary>
	public class BooleanSetting : Setting
	{
		private bool value = false;

		/// <summary>
		/// Boolean setting object.
		/// </summary>
		public BooleanSetting()
		{
		}

		/// <summary>
		/// Boolean setting object.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public BooleanSetting(string Key, bool Value)
			: base(Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		public bool Value
		{
			get { return this.value; }
			set { this.value = value; }
		}
	}
}
