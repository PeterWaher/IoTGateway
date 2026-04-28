using System.Threading.Tasks;

namespace Waher.Things.Authorities
{
	/// <summary>
	/// Abstract base class for authorities.
	/// </summary>
	public abstract class Authority : IRequestOrigin
	{
		private readonly string from;
		private readonly string[] deviceTokens;
		private readonly string[] serviceTokens;
		private readonly string[] userTokens;

		/// <summary>
		/// Abstract base class for authorities.
		/// </summary>
		/// <param name="From">Source of request.</param>
		public Authority(string From)
			: this(From, null, null, null)
		{
		}

		/// <summary>
		/// Abstract base class for authorities.
		/// </summary>
		/// <param name="From">Source of request.</param>
		/// <param name="DeviceTokens">Device tokens.</param>
		/// <param name="ServiceTokens">Service tokens.</param>
		/// <param name="UserTokens">User tokens.</param>
		public Authority(string From, string[] DeviceTokens, string[] ServiceTokens,
			string[] UserTokens)
		{
			this.from = From;
			this.deviceTokens = DeviceTokens;
			this.serviceTokens = ServiceTokens;
			this.userTokens = UserTokens;
		}

		/// <summary>
		/// Origin of request.
		/// </summary>
		public Task<RequestOrigin> GetOrigin()
		{
			return Task.FromResult(new RequestOrigin(this.from, this.deviceTokens, 
				this.serviceTokens, this.userTokens, this));
		}

		/// <summary>
		/// If the origin has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the origin has the corresponding privilege.</returns>
		public abstract bool HasPrivilege(string Privilege);
	}
}
