namespace Waher.Things.Ieee1451.Ieee1451_0
{
	/// <summary>
	/// Network Service Message Type
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// Command (1)
		/// </summary>
		Command = 1,

		/// <summary>
		/// Reply (2)
		/// </summary>
		Reply = 2,

		/// <summary>
		/// Announcement (3)
		/// </summary>
		Announcement = 3,

		/// <summary>
		/// Notification (4)
		/// </summary>
		Notification = 4,

		/// <summary>
		/// Callback (5)
		/// </summary>
		Callback = 5
	}
}
