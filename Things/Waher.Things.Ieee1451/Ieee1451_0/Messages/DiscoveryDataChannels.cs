﻿namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// Discovery data about a collection of channels.
	/// </summary>
	public class DiscoveryDataChannels : DiscoveryDataEntities
	{
		/// <summary>
		/// Discovery data about a collection of channels.
		/// </summary>
		/// <param name="Channel">Channel information.</param>
		/// <param name="Names">Names</param>
		/// <param name="Channels">Channels</param>
		public DiscoveryDataChannels(ChannelAddress Channel, string[] Names, ushort[] Channels)
			: base(Channel, Names)
		{
			this.Channels = Channels;
		}

		/// <summary>
		/// Entity Names
		/// </summary>
		public ushort[] Channels { get; }
	}
}
