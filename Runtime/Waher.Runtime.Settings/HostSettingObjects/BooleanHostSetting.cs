using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.HostSettingObjects
{
	/// <summary>
	/// Boolean setting object.
	/// </summary>
	public class BooleanHostSetting : HostSetting
	{
		private bool value = false;

		/// <summary>
		/// Boolean setting object.
		/// </summary>
		public BooleanHostSetting()
		{
		}

		/// <summary>
		/// Boolean setting object.
		/// </summary>
		/// <param name="Host">Host name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public BooleanHostSetting(string Name, string Key, bool Value)
			: base(Name, Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		public bool Value
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
