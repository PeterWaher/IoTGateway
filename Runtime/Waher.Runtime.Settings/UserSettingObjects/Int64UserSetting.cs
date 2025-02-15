using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.UserSettingObjects
{
	/// <summary>
	/// Int64 user setting object.
	/// </summary>
	public class Int64UserSetting : UserSetting
	{
		private long value = 0;

		/// <summary>
		/// Int64 user setting object.
		/// </summary>
		public Int64UserSetting()
		{
		}

		/// <summary>
		/// Int64 user setting object.
		/// </summary>
		/// <param name="User">User name.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public Int64UserSetting(string User, string Key, long Value)
			: base(User, Key)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[DefaultValue(0L)]
		public long Value
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
