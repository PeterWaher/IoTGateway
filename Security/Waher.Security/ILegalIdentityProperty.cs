namespace Waher.Security
{
	/// <summary>
	/// Interface for legal identity properties.
	/// </summary>
	public interface ILegalIdentityProperty
	{
		/// <summary>
		/// Name of property
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Property value
		/// </summary>
		string Value { get; }
	}
}
