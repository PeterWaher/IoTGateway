namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains information about a parsed contract.
	/// </summary>
	public class ParsedContract
	{
		/// <summary>
		/// Contract object
		/// </summary>
		public Contract Contract;

		/// <summary>
		/// If a status element was found.
		/// </summary>
		public bool HasStatus;

		/// <summary>
		/// If parameter values in the contract are valid.
		/// </summary>
		public bool ParametersValid;
	}
}
