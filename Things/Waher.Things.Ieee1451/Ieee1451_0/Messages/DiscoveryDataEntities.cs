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
		public DiscoveryDataEntities(ChannelAddress Channel, string[] Names)
			: base(Channel)
		{
			this.Names = Names;
		}

		/// <summary>
		/// Entity Names
		/// </summary>
		public string[] Names { get; }
	}
}
