namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Represents an invalidated claim.
	/// </summary>
	public class InvalidClaim
	{
		/// <summary>
		/// Represents an invalidated claim.
		/// </summary>
		/// <param name="Claim">Identifier of claim</param>
		/// <param name="Reason">Reason for invalidating claim.</param>
		/// <param name="ReasonLanguage">ISO code of language used for <paramref name="Reason"/>.</param>
		/// <param name="ReasonCode">A machine-readable code for the reason for invalidating 
		/// the claim. (Each service can define its own reason codes.)</param>
		/// <param name="Service">Service validating claim.</param>
		internal InvalidClaim(string Claim, string Reason, string ReasonLanguage, 
			string ReasonCode, string Service)
		{
			this.Claim = Claim;
			this.Reason = Reason;
			this.ReasonLanguage = ReasonLanguage;
			this.ReasonCode = ReasonCode;
			this.Service = Service;
		}

		/// <summary>
		/// Identifier of claim
		/// </summary>
		public string Claim { get; }

		/// <summary>
		/// Service invalidating claim.
		/// </summary>
		public string Service { get; }

		/// <summary>
		/// Reason for invalidating claim.
		/// </summary>
		public string Reason { get; }

		/// <summary>
		/// ISO code of language used for <see cref="Reason"/>.
		/// </summary>
		public string ReasonLanguage { get; }

		/// <summary>
		/// A machine-readable code for the reason for invalidating the claim.
		/// (Each service can define its own reason codes.)
		/// </summary>
		public string ReasonCode { get; }
	}
}
