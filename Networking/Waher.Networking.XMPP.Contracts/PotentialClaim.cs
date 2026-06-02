namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Represents a potential claim.
	/// </summary>
	public class PotentialClaim : ValidClaim
	{
		/// <summary>
		/// Represents a potential claim.
		/// </summary>
		/// <param name="Claim">Identifier of claim</param>
		/// <param name="Value">Value of claim</param>
		/// <param name="Service">Service validating claim.</param>
		internal PotentialClaim(string Claim, string Value, string Service)
			: base(Claim, Service)
		{
			this.Value = Value;
		}

		/// <summary>
		/// Value of claim
		/// </summary>
		public string Value { get; }
	}
}
