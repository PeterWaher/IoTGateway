namespace Waher.Things.Authorities
{
	/// <summary>
	/// Origin of request has maximum authority.
	/// </summary>
	public class MaximumAuthority : Authority
	{
		/// <summary>
		/// Origin of request has maximum authority.
		/// </summary>
		/// <param name="From">Source of request.</param>
		public MaximumAuthority(string From)
			: base(From)
		{
		}

		/// <summary>
		/// Origin of request has maximum authority.
		/// </summary>
		/// <param name="From">Source of request.</param>
		/// <param name="DeviceTokens">Device tokens.</param>
		/// <param name="ServiceTokens">Service tokens.</param>
		/// <param name="UserTokens">User tokens.</param>
		public MaximumAuthority(string From, string[] DeviceTokens, string[] ServiceTokens,
			string[] UserTokens)
			: base(From, DeviceTokens, ServiceTokens, UserTokens)
		{
		}

		/// <summary>
		/// If the origin has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the origin has the corresponding privilege.</returns>
		public override bool HasPrivilege(string Privilege)
		{
			return true;
		}
	}
}
