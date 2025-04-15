namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Represents a validated claim.
	/// </summary>
	public class ValidClaim
	{
		/// <summary>
		/// Represents a validated claim.
		/// </summary>
		/// <param name="Claim">Identifier of claim</param>
		/// <param name="Service">Service validating claim.</param>
		internal ValidClaim(string Claim, string Service)
		{
			this.Claim = Claim;
			this.Service = Service;
		}

		/// <summary>
		/// Identifier of claim
		/// </summary>
		public string Claim { get; }

		/// <summary>
		/// Service validating claim.
		/// </summary>
		public string Service { get; }
	}
}
