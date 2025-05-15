namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// How altitudes are managed by a Geo-spatial parameter
	/// </summary>
	public enum AltitudeUse
	{
		/// <summary>
		/// Altitude is required by the parameter.
		/// </summary>
		Required,

		/// <summary>
		/// Altitude is optional by the parameter.
		/// </summary>
		Optional,

		/// <summary>
		/// Altitude is prohibited by the parameter.
		/// </summary>
		Prohibited
	}
}
