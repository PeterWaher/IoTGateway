using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Represents an alternative field in a legal identity.
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	public class AlternativeField
	{
		/// <summary>
		/// Represents an alternative field in a legal identity.
		/// </summary>
		public AlternativeField()
		{
		}

		/// <summary>
		/// Represents an alternative field in a legal identity.
		/// </summary>
		/// <param name="Key">Alternative field name.</param>
		/// <param name="Value">Alternative field Value.</param>
		public AlternativeField(string Key, string Value)
		{
			this.Key = Key;
			this.Value = Value;
		}

		/// <summary>
		/// Alternative field name.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Key = string.Empty;

		/// <summary>
		/// Alternative field Value.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Value = string.Empty;
	}
}
