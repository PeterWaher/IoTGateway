namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 addresses
	/// </summary>
	public class ChannelAddress
	{
		/// <summary>
		/// Application ID
		/// </summary>
		public byte[] ApplicationId;

		/// <summary>
		/// NCAP ID
		/// </summary>
		public byte[] NcapId;

		/// <summary>
		/// TIM ID
		/// </summary>
		public byte[] TimId;

		/// <summary>
		/// Channel ID
		/// </summary>
		public ushort ChannelId;
	}
}
