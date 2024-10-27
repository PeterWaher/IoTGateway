namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// Discovery data.
	/// </summary>
	public class DiscoveryData
	{
		/// <summary>
		/// Discovery data.
		/// </summary>
		/// <param name="Channel">Channel information.</param>
		public DiscoveryData(ChannelAddress Channel)
		{
			this.Channel = Channel;
		}

		/// <summary>
		/// Channel information.
		/// </summary>
		public ChannelAddress Channel { get; }
	}
}
