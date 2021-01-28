using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Threading;

namespace Waher.Security.Users
{
	/// <summary>
	/// Maintains the collection of all roles in the system.
	/// </summary>
	public static class Roles
	{
		private static readonly Dictionary<string, Role> roles = new Dictionary<string, Role>();
		private static readonly MultiReadSingleWriteObject synchObj = new MultiReadSingleWriteObject();

		/// <summary>
		/// Gets the <see cref="Role"/> object corresponding to a Role ID.
		/// </summary>
		/// <param name="RoleId">Role ID.</param>
		/// <returns>Role object.</returns>
		public static async Task<Role> GetRole(string RoleId)
		{
			await synchObj.BeginWrite();
			try
			{
				if (roles.TryGetValue(RoleId, out Role Role))
					return Role;

				Role = await Database.FindFirstDeleteRest<Role>(new FilterFieldEqualTo("Id", RoleId));
				if (Role is null)
				{
					Role = new Role()
					{
						Id = RoleId,
						Description = string.Empty,
						Privileges = new PrivilegePattern[0]
					};

					await Database.Insert(Role);
				}

				roles[RoleId] = Role;
		
				return Role;
			}
			finally
			{
				await synchObj.EndWrite();
			}
		}

		/// <summary>
		/// Loads all roles
		/// </summary>
		public static async Task LoadAll()
		{
			IEnumerable<Role> Roles;

			await synchObj.BeginWrite();
			try
			{
				Roles = await Database.Find<Role>();

				roles.Clear();
				foreach (Role Role in Roles)
					roles[Role.Id] = Role;
			}
			finally
			{
				await synchObj.EndWrite();
			}
		}

		/// <summary>
		/// Clears internal caches.
		/// </summary>
		public static void ClearCache()
		{
			lock (roles)
			{
				roles.Clear();
			}
		}

	}
}
