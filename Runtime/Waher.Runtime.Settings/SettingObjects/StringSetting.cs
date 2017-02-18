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
	/// String setting object.
	/// </summary>
	public class StringSetting : Setting
	{
		private string value = string.Empty;

		/// <summary>
		/// String setting object.
		/// </summary>
		public StringSetting()
		{
		}

		/// <summary>
		/// String setting object.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public StringSetting(string Key, string Value)
			: base(Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Value
		{
			get { return this.value; }
			set { this.value = value; }
		}
	}
}
