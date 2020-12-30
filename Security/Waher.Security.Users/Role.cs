using System;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Security.Users
{
	/// <summary>
	/// Corresponds to a role in the system.
	/// </summary>
	[CollectionName("Roles")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Id")]
	[ArchivingTime]
	public class Role
	{
		private string objectId = null;
		private string id = string.Empty;
		private string description = string.Empty;
		private PrivilegePattern[] privileges = null;

		/// <summary>
		/// Corresponds to a role in the system.
		/// </summary>
		public Role()
		{
		}

		/// <summary>
		/// Object ID of role
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// Role ID
		/// </summary>
		public string Id
		{
			get => this.id;
			set => this.id = value;
		}

		/// <summary>
		/// Description of privilege.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Description
		{
			get => this.description;
			set => this.description = value;
		}

		/// <summary>
		/// Privileges
		/// </summary>
		[DefaultValueNull]
		public PrivilegePattern[] Privileges
		{
			get => this.privileges;
			set => this.privileges = value;
		}

		/// <summary>
		/// If the user has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the user has the corresponding privilege.</returns>
		public bool HasPrivilege(string Privilege)
		{
			PrivilegePattern[] Refs = this.privileges;
			int i, c = Refs?.Length ?? 0;
			bool? b;

			for (i = 0; i < c; i++)
			{
				b = Refs[i].IsIncluded(Privilege);
				if (b.HasValue)
					return b.Value;
			}

			return false;
		}

	}
}
