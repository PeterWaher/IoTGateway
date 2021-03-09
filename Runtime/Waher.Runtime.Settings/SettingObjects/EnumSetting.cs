using System;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.Settings.SettingObjects
{
	/// <summary>
	/// Enumeration setting object.
	/// </summary>
	public class EnumSetting : Setting
	{
		private Enum value = null;
		private Type enumType = null;
		private string enumTypeName = string.Empty;
		private string enumValue = string.Empty;

		/// <summary>
		/// Enumeration setting object.
		/// </summary>
		public EnumSetting()
		{
		}

		/// <summary>
		/// Enumeration setting object.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">Value.</param>
		public EnumSetting(string Key, Enum Value)
			: base(Key)
		{
			this.Value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		[IgnoreMember]
		public Enum Value
		{
			get
			{
				if (this.value is null)
					this.value = (Enum)Enum.Parse(this.enumType, this.enumValue);

				return this.value;
			}

			set
			{
				this.enumType = value.GetType();
				this.enumTypeName = this.enumType.FullName;
				this.enumValue = value.ToString();
				this.value = null;
			}
		}

		/// <summary>
		/// Enumeration type name
		/// </summary>
		[DefaultValueStringEmpty]
		public string EnumTypeName
		{
			get => this.enumTypeName;
			set
			{
				this.enumType = Types.GetType(value);
				if (this.enumType is null)
					throw new InvalidOperationException("Enumeration type not recognized by " + typeof(Types).Namespace + ": " + value);

				this.enumTypeName = value;
				this.value = null;
			}
		}

		/// <summary>
		/// Enumeration value
		/// </summary>
		[DefaultValueStringEmpty]
		public string EnumValue
		{
			get => this.enumValue;
			set
			{
				this.enumValue = value;
				this.value = null;
			}
		}

		/// <summary>
		/// Gets the value of the setting, as an object.
		/// </summary>
		/// <returns>Value object.</returns>
		public override object GetValueObject()
		{
			return this.Value;
		}
	}
}
