using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.HostSettingObjects
{
	/// <summary>
	/// Double setting object.
	/// </summary>
	public class DoubleHostSetting : HostSetting
	{
		private double value = 0.0;

		/// <summary>
		/// Double setting object.
		/// </summary>
		public DoubleHostSetting()
		{
		}

		/// <summary>
		/// Double setting object.
		/// </summary>
		/// <param name="Host">Host name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public DoubleHostSetting(string Host, string Key, double Value)
			: base(Host, Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValue(0.0)]
		public double Value
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
