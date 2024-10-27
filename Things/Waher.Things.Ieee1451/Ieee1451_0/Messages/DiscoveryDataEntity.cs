namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// Discovery data about a single entity.
	/// </summary>
	public class DiscoveryDataEntity : DiscoveryData
	{
		/// <summary>
		/// Discovery data about a single entity.
		/// </summary>
		/// <param name="Channel">Channel information.</param>
		/// <param name="Name">Name</param>
		public DiscoveryDataEntity(ChannelAddress Channel, string Name)
			: base(Channel)
		{
			this.Name = Name;
		}

		/// <summary>
		/// Entity Name
		/// </summary>
		public string Name { get; }
	}
}
