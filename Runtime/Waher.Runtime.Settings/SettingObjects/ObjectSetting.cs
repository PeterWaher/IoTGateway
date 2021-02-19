using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.SettingObjects
{
	/// <summary>
	/// Object setting object.
	/// </summary>
	public class ObjectSetting : Setting
	{
		private object value = null;

		/// <summary>
		/// String setting object.
		/// </summary>
		public ObjectSetting()
		{
		}

		/// <summary>
		/// Object setting object.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public ObjectSetting(string Key, object Value)
			: base(Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValueNull]
		public object Value
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
