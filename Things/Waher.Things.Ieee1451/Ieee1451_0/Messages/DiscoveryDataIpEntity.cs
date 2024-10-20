namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// NCAP address type.
	/// </summary>
	public enum AddressType
	{
		/// <summary>
		/// IP v4
		/// </summary>
		Ipv4 = 1,

		/// <summary>
		/// IP v6
		/// </summary>
		Ipv6 = 2
	}

	/// <summary>
	/// Discovery data about a single NCAP.
	/// </summary>
	public class DiscoveryDataIpEntity : DiscoveryDataEntity
	{
		/// <summary>
		/// Discovery data about a single NCAP.
		/// </summary>
		/// <param name="Channel">Channel information.</param>
		/// <param name="Name">Name</param>
		/// <param name="AddressType">Address type.</param>
		/// <param name="Address">Address</param>
		public DiscoveryDataIpEntity(ChannelAddress Channel, string Name,
			AddressType AddressType, byte[] Address)
			: base(Channel, Name)
		{
			this.AddressType = AddressType;
			this.Address = Address;
		}

		/// <summary>
		/// NCAP Address Type
		/// </summary>
		public AddressType AddressType { get; }

		/// <summary>
		/// NCAP Address
		/// </summary>
		public byte[] Address { get; }
	}
}
