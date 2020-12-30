using System;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Security.Users
{
	/// <summary>
	/// Corresponds to a privilege in the system.
	/// </summary>
	[CollectionName("Privileges")]
	[TypeName(TypeNameSerialization.None)]
	[Index("LocalId", "ParentFullId")]
	[Index("FullId")]
	[ArchivingTime]
	public class Privilege
	{
		private string objectId = null;
		private string parentFullId = string.Empty;
		private string localId = string.Empty;
		private string fullId = string.Empty;
		private string description = string.Empty;
		private Privilege parent = null;

		/// <summary>
		/// Corresponds to a privilege in the system.
		/// </summary>
		public Privilege()
		{
		}

		/// <summary>
		/// Object ID of privilege
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// Full Privilege ID of parent privilege. If the empty string, privilege is a root privilege.
		/// </summary>
		public string ParentFullId
		{
			get => this.parentFullId;
			set => this.parentFullId = value;
		}

		/// <summary>
		/// Local Privilege ID, unique among the child privileges of the same parent.
		/// </summary>
		public string LocalId
		{
			get => this.localId;
			set => this.localId = value;
		}

		/// <summary>
		/// Full Privilege ID. Corresponds to the concatenation of ancestor IDs with the local ID, delimited with period characters.
		/// </summary>
		public string FullId
		{
			get => this.fullId;
			set => this.fullId = value;
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
		/// Parent privilege.
		/// </summary>
		[IgnoreMember]
		public Privilege Parent
		{
			get
			{
				if (this.parent is null && !string.IsNullOrEmpty(this.parentFullId))
					this.parent = Privileges.GetPrivilege(this.parentFullId).Result;
				
				return this.parent;
			}

			internal set => this.parent = value;
		}
	}
}
