namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// Discovery data about a collection of entities.
	/// </summary>
	public class DiscoveryDataEntities : DiscoveryData
	{
		/// <summary>
		/// Discovery data about a collection of entities.
		/// </summary>
		/// <param name="Channel">Channel information.</param>
		/// <param name="Names">Names</param>
		/// <param name="Identities">Identities</param>
		public DiscoveryDataEntities(ChannelAddress Channel, string[] Names, byte[][] Identities)
			: base(Channel)
		{
			this.Names = Names;
			this.Identities = Identities;
		}

		/// <summary>
		/// Entity Names
		/// </summary>
		public string[] Names { get; }

		/// <summary>
		/// Identities
		/// </summary>
		public byte[][] Identities { get; }
	}
}
