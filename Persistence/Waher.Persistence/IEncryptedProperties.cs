using Waher.Persistence.Attributes;

namespace Waher.Persistence
{
	/// <summary>
	/// Interface for objects containing encrypted properties. Mark the properties that are
	/// encrypted with the <see cref="EncryptedAttribute"/> attribute.
	/// </summary>
	public interface IEncryptedProperties
	{
		/// <summary>
		/// Array of properties that are encrypted.
		/// </summary>
		string[] EncryptedProperties { get; }
	}
}
