namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
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
			: base(Channel, Names, ToIdentities(Channels))
		{
			this.Channels = Channels;
		}

		private static byte[][] ToIdentities(ushort[] Ids)
		{
			int i, c = Ids?.Length ?? 0;
			byte[][] Result = new byte[c][];
			ushort j;

			for (i = 0; i < c; i++)
			{
				j = Ids[i];

				Result[i] = new byte[]
				{
					(byte)(j >> 8),
					(byte)j
				};
			}

			return Result;
		}

		/// <summary>
		/// Entity Names
		/// </summary>
		public ushort[] Channels { get; }
	}
}
