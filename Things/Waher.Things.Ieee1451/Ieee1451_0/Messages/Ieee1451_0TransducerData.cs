using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
    /// <summary>
    /// IEEE 1451.0 TEDS
    /// </summary>
    public class Ieee1451_0TransducerData
	{
		/// <summary>
		/// IEEE 1451.0 TEDS
		/// </summary>
		/// <param name="ChannelInfo">Address information.</param>
		/// <param name="Fields">TLV records available in TEDS</param>
		public Ieee1451_0TransducerData(Ieee1451_0ChannelId ChannelInfo, Field[] Fields)
		{
			this.ChannelInfo = ChannelInfo;
			this.Fields = Fields;
		}

		/// <summary>
		/// Transducer Data
		/// </summary>
		public Field[] Fields { get; }

		/// <summary>
		/// Address information.
		/// </summary>
		public Ieee1451_0ChannelId ChannelInfo { get; }
	}
}
