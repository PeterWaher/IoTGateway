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
		public BooleanHostSetting(string Host, string Key, bool Value)
			: base(Host, Key)
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
