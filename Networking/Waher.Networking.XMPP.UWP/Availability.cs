namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Resource availability.
	/// </summary>
	public enum Availability
	{
		/// <summary>
		/// The entity or resource is online.
		/// </summary>
		Online,

		/// <summary>
		/// The entity or resource is offline.
		/// </summary>
		Offline,

		/// <summary>
		/// The entity or resource is temporarily away.
		/// </summary>
		Away,

		/// <summary>
		/// The entity or resource is actively interested in chatting.
		/// </summary>
		Chat,

		/// <summary>
		/// The entity or resource is busy.
		/// </summary>
		DoNotDisturb,

		/// <summary>
		/// The entity or resource is away for an extended period.
		/// </summary>
		ExtendedAway
	}
}
