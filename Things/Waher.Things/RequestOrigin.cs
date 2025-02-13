using System.Threading.Tasks;

namespace Waher.Things
{
	/// <summary>
	/// Tokens available in request.
	/// </summary>
	public class RequestOrigin : IRequestOrigin
	{
		/// <summary>
		/// Empty request origin.
		/// </summary>
		public static readonly RequestOrigin Empty = new RequestOrigin(string.Empty, new string[0], new string[0], new string[0], null);

		private readonly IRequestOrigin authority;
		private readonly string[] deviceTokens;
		private readonly string[] serviceTokens;
		private readonly string[] userTokens;
		private readonly string from;

		/// <summary>
		/// Tokens available in request.
		/// </summary>
		/// <param name="From">Address of caller.</param>
		/// <param name="DeviceTokens">Device tokens, or null.</param>
		/// <param name="ServiceTokens">Service tokens, or null.</param>
		/// <param name="UserTokens">User tokens, or null.</param>
		/// <param name="Authority">Optional authority for privilege authorization.</param>
		public RequestOrigin(string From, string[] DeviceTokens, string[] ServiceTokens, string[] UserTokens,
			IRequestOrigin Authority)
		{
			this.from = From;
			this.deviceTokens = DeviceTokens;
			this.serviceTokens = ServiceTokens;
			this.userTokens = UserTokens;
			this.authority = Authority;
		}

		/// <summary>
		/// Address of caller.
		/// </summary>
		public string From => this.from;

		/// <summary>
		/// Device tokens, or null.
		/// </summary>
		public string[] DeviceTokens => this.deviceTokens;

		/// <summary>
		/// Service tokens, or null.
		/// </summary>
		public string[] ServiceTokens => this.serviceTokens;

		/// <summary>
		/// User tokens, or null.
		/// </summary>
		public string[] UserTokens => this.userTokens;

		/// <summary>
		/// Optional authority for privilege authorization
		/// </summary>
		public IRequestOrigin Authority => this.authority;

		/// <summary>
		/// Origin of request.
		/// </summary>
		public Task<RequestOrigin> GetOrigin()
		{
			return Task.FromResult(this);
		}

		/// <summary>
		/// If the origin has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the origin has the corresponding privilege.</returns>
		public bool HasPrivilege(string Privilege)
		{
			return this.authority?.HasPrivilege(Privilege) ?? false;
		}
	}
}
