using System;

namespace Waher.Things.Authorities
{
	/// <summary>
	/// Origin of request has view-only authority.
	/// </summary>
	public class ViewOnlyAuthority : Authority
	{
		private readonly string[] dataSourceIds;

		/// <summary>
		/// Origin of request has view-only authority in a all data sources.
		/// </summary>
		/// <param name="From">Source of request.</param>
		public ViewOnlyAuthority(string From)
			: this(From, null, null, null, (string[])null)
		{
		}

		/// <summary>
		/// Origin of request has view-only authority in a all data sources.
		/// </summary>
		/// <param name="From">Source of request.</param>
		/// <param name="DeviceTokens">Device tokens.</param>
		/// <param name="ServiceTokens">Service tokens.</param>
		/// <param name="UserTokens">User tokens.</param>
		public ViewOnlyAuthority(string From, string[] DeviceTokens, string[] ServiceTokens,
			string[] UserTokens)
			: this(From, DeviceTokens, ServiceTokens, UserTokens, null)
		{
		}

		/// <summary>
		/// Origin of request has view-only authority in a specific set of data sources.
		/// </summary>
		/// <param name="From">Source of request.</param>
		/// <param name="DataSourceIds">Data Source IDs the origin is authorized to view.</param>
		public ViewOnlyAuthority(string From, params string[] DataSourceIds)
			: this(From, null, null, null, DataSourceIds)
		{
		}

		/// <summary>
		/// Origin of request has view-only authority in a specific set of data sources.
		/// </summary>
		/// <param name="From">Source of request.</param>
		/// <param name="DeviceTokens">Device tokens.</param>
		/// <param name="ServiceTokens">Service tokens.</param>
		/// <param name="UserTokens">User tokens.</param>
		/// <param name="DataSourceIds">Data Source IDs the origin is authorized to view.</param>
		public ViewOnlyAuthority(string From, string[] DeviceTokens, string[] ServiceTokens,
			string[] UserTokens, params string[] DataSourceIds)
			: base(From, DeviceTokens, ServiceTokens, UserTokens)
		{
			this.dataSourceIds = DataSourceIds;
		}

		/// <summary>
		/// If the origin has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the origin has the corresponding privilege.</returns>
		public override bool HasPrivilege(string Privilege)
		{
			string[] Parts = Privilege?.Split(':');
			if ((Parts?.Length ?? 0) < 2)
				return false;

			if (Parts[0] != "Source")
				return false;

			if (!(this.dataSourceIds is null) &&
				Array.IndexOf(this.dataSourceIds, Parts[1]) < 0)
			{
				return false;
			}

			switch (Parts.Length)
			{
				case 3:
					return Parts[2] == "View";

				case 4:
					return Parts[2] == "Node" && Parts[3] == "View";

				default:
					return false;
			}
		}
	}
}
