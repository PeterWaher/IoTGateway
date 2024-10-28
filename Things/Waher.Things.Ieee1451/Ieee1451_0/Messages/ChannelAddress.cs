using System.Text;
using Waher.Security;
using Waher.Things.Ieee1451.Ieee1451_1_6;

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

		/// <summary>
		/// Gets the topic name of the corresponding node.
		/// </summary>
		/// <param name="BaseTopic">Base topic</param>
		/// <returns>Full topic</returns>
		public string GetTopic(string BaseTopic)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(BaseTopic);
			sb.Append('/');
			sb.Append(Hashes.BinaryToString(this.NcapId));

			if (!MessageSwitch.IsZero(this.TimId))
			{
				sb.Append('/');
				sb.Append(Hashes.BinaryToString(this.TimId));

				if (this.ChannelId != 0)
				{
					sb.Append('/');
					sb.Append(this.ChannelId.ToString());
				}
			}

			return sb.ToString();
		}
	}
}
