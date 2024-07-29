using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.HostSettingObjects
{
	/// <summary>
	/// Object setting object.
	/// </summary>
	public class ObjectHostSetting : HostSetting
	{
		private object value = null;

		/// <summary>
		/// String setting object.
		/// </summary>
		public ObjectHostSetting()
		{
		}

		/// <summary>
		/// Object setting object.
		/// </summary>
		/// <param name="Host">Host name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public ObjectHostSetting(string Host, string Key, object Value)
			: base(Host, Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValueNull]
		public object Value
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
