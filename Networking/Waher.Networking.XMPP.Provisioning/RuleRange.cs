namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Range of a rule change
	/// </summary>
	public enum RuleRange
	{
		/// <summary>
		/// Applies to caller only.
		/// </summary>
		Caller,

		/// <summary>
		/// Applies to the caller domain.
		/// </summary>
		Domain,

		/// <summary>
		/// Appplies to all future requests.
		/// </summary>
		All
	}
}
