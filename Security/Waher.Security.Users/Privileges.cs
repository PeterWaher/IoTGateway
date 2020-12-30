using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Threading;

namespace Waher.Security.Users
{
	/// <summary>
	/// Maintains the collection of all privileges in the system.
	/// </summary>
	public class Privileges
	{
		private static readonly Dictionary<string, Privilege> privileges = new Dictionary<string, Privilege>();
		private static readonly MultiReadSingleWriteObject synchObj = new MultiReadSingleWriteObject();

		/// <summary>
		/// Gets the <see cref="Privilege"/> object corresponding to a full Privilege ID.
		/// </summary>
		/// <param name="PrivilegeId">Full Privilege ID, consisting of the concatenation of the full parent privelege ID, 
		/// a period character and the local id of the privilege.</param>
		/// <returns>Privilege object.</returns>
		public static async Task<Privilege> GetPrivilege(string PrivilegeId)
		{
			await synchObj.BeginWrite();
			try
			{
				return await GetPrivilegeLocked(PrivilegeId);
			}
			finally
			{
				await synchObj.EndWrite();
			}
		}

		private static async Task<Privilege> GetPrivilegeLocked(string PrivilegeId)
		{
			if (privileges.TryGetValue(PrivilegeId, out Privilege Privilege))
				return Privilege;

			Privilege = await Database.FindFirstDeleteRest<Privilege>(new FilterFieldEqualTo("FullId", PrivilegeId));
			if (Privilege is null)
			{
				string ParentId;
				string LocalId;
				int i = PrivilegeId.LastIndexOf('.');

				if (i < 0)
				{
					ParentId = string.Empty;
					LocalId = PrivilegeId;
				}
				else
				{
					ParentId = PrivilegeId.Substring(0, i);
					LocalId = PrivilegeId.Substring(i + 1);
				}

				Privilege = new Privilege()
				{
					ParentFullId = ParentId,
					LocalId = LocalId,
					FullId = PrivilegeId,
					Description = string.Empty
				};

				await Database.Insert(Privilege);
			}

			if (!string.IsNullOrEmpty(Privilege.ParentFullId))
				Privilege.Parent = await GetPrivilegeLocked(Privilege.ParentFullId);

			privileges[PrivilegeId] = Privilege;

			return Privilege;
		}

		/// <summary>
		/// Loads all privileges
		/// </summary>
		public static async Task LoadAll()
		{
			await synchObj.BeginWrite();
			try
			{
				privileges.Clear();

				IEnumerable<Privilege> Privileges = await Database.Find<Privilege>();

				foreach (Privilege Privilege in Privileges)
					privileges[Privilege.FullId] = Privilege;

				foreach (Privilege Privilege in Privileges)
				{
					if (!string.IsNullOrEmpty(Privilege.ParentFullId))
						Privilege.Parent = await GetPrivilegeLocked(Privilege.ParentFullId);
				}
			}
			finally
			{
				await synchObj.EndWrite();
			}
		}
	}
}
