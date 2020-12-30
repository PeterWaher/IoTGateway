using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Security.Users
{
	/// <summary>
	/// Corresponds to a user in the system.
	/// </summary>
	[CollectionName("Users")]
	[TypeName(TypeNameSerialization.None)]
	[Index("UserName")]
	[ArchivingTime]
	public class User : IUser
	{
		private readonly Dictionary<string, bool> privileges = new Dictionary<string, bool>();
		private string objectId = null;
		private string userName = string.Empty;
		private string passwordHash = string.Empty;
		private string[] roleIds = null;
		private Role[] roles = null;
		private UserMetaData[] metaData = null;

		/// <summary>
		/// Corresponds to a user in the system.
		/// </summary>
		public User()
		{
		}

		/// <summary>
		/// Object ID of user
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// User Name
		/// </summary>
		public string UserName
		{
			get => this.userName;
			set => this.userName = value;
		}

		/// <summary>
		/// Role IDs
		/// </summary>
		[DefaultValueNull]
		public string[] RoleIds
		{
			get => this.roleIds;
			set
			{
				this.roleIds = value;

				lock (this.privileges)
				{
					this.roles = null;
					this.privileges.Clear();
				}
			}
		}

		/// <summary>
		/// Meta-data information about user.
		/// </summary>
		[DefaultValueNull]
		public UserMetaData[] MetaData
		{
			get => this.metaData;
			set => this.metaData = value;
		}

		/// <summary>
		/// Password Hash
		/// </summary>
		public string PasswordHash
		{
			get => this.passwordHash;
			set => this.passwordHash = value;
		}

		/// <summary>
		/// Type of password hash. The empty stream means a clear-text password.
		/// </summary>
		public string PasswordHashType => Users.HashMethodTypeName;

		/// <summary>
		/// Load role objects.
		/// </summary>
		/// <returns>Array of roles</returns>
		public async Task<Role[]> LoadRoles()
		{
			Role[] Roles = this.roles;

			if (Roles is null)
			{
				string[] Ids = this.roleIds;
				int i, c = Ids?.Length ?? 0;
				
				Roles = new Role[c];
				for (i = 0; i < c; i++)
					Roles[i] = await Security.Users.Roles.GetRole(Ids[i]);

				this.roles = Roles;
			}

			return Roles;
		}

		/// <summary>
		/// If the user has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the user has the corresponding privilege.</returns>
		public bool HasPrivilege(string Privilege)
		{
			lock (this.privileges)
			{
				if (this.privileges.TryGetValue(Privilege, out bool Result))
					return Result;
			}

			Role[] Roles = this.roles;
			if (Roles is null)
				Roles = this.LoadRoles().Result;

			bool HasPrivilege = false;

			foreach (Role Role in Roles)
			{
				if (Role.HasPrivilege(Privilege))
				{
					HasPrivilege = true;
					break;
				}
			}

			lock (this.privileges)
			{
				this.privileges[Privilege] = HasPrivilege;
			}

			Task.Run(() => Privileges.GetPrivilege(Privilege));

			return HasPrivilege;
		}
	}
}
