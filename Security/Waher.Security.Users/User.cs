using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Persistence.Attributes;
using Waher.Security.JWT;
using Waher.Things;

namespace Waher.Security.Users
{
	/// <summary>
	/// Corresponds to a user in the system.
	/// </summary>
	[CollectionName("Users")]
	[TypeName(TypeNameSerialization.None)]
	[Index("UserName")]
	[Index("LegalId")]
	[Index("PersonalNumber", "Country")]
	[ArchivingTime]
	public class User : IUserWithClaims, IRequestOrigin
	{
		private readonly static RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		private readonly Dictionary<string, bool> privileges = new Dictionary<string, bool>();
		private string objectId = null;
		private string userName = string.Empty;
		private string passwordHash = string.Empty;
		private string[] roleIds = null;
		private string legalId = string.Empty;
		private string personalNumber = string.Empty;
		private string country = string.Empty;
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
		/// Legal ID associated with account.
		/// </summary>
		public string LegalId
		{
			get => this.legalId;
			set => this.legalId = value;
		}

		/// <summary>
		/// Personal Number associated with account.
		/// </summary>
		public string PersonalNumber
		{
			get => this.personalNumber;
			set => this.personalNumber = value;
		}

		/// <summary>
		/// Country associated with account.
		/// </summary>
		public string Country
		{
			get => this.country;
			set => this.country = value;
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
			if (string.IsNullOrEmpty(Privilege))
				return true;

			lock (this.privileges)
			{
				if (this.privileges.TryGetValue(Privilege, out bool Result))
					return Result;
			}

			Role[] Roles = this.roles ?? this.LoadRoles().Result;

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

		/// <summary>
		/// Origin of request.
		/// </summary>
		public Task<RequestOrigin> GetOrigin()
		{
			return Task.FromResult(new RequestOrigin(this.userName, null, null, null));
		}

		/// <summary>
		/// Creates a set of claims identifying the user.
		/// </summary>
		/// <param name="Encrypted">If communication is encrypted.</param>
		/// <returns>Set of claims.</returns>
		public async Task<IEnumerable<KeyValuePair<string, object>>> CreateClaims(bool Encrypted)
		{
			int IssuedAt = (int)Math.Round(DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalSeconds);
			int Expires = IssuedAt + 3600;
			byte[] RandomBytes = new byte[32];

			lock (rnd)
			{
				rnd.GetBytes(RandomBytes);
			}

			List<KeyValuePair<string, object>> Claims = new List<KeyValuePair<string, object>>()
			{
				new KeyValuePair<string, object>(JwtClaims.JwtId, Convert.ToBase64String(RandomBytes)),
				new KeyValuePair<string, object>(JwtClaims.Subject, this.userName),
				new KeyValuePair<string, object>(JwtClaims.IssueTime, IssuedAt),
				new KeyValuePair<string, object>(JwtClaims.ExpirationTime, Expires)
			};

			if (!string.IsNullOrEmpty(Users.Domain))
				Claims.Add(new KeyValuePair<string, object>(JwtClaims.Issuer, Users.Domain));

			if (Encrypted)
			{
				if (!(this.roleIds is null))
				{
					StringBuilder sb = null;

					foreach (string RoleId in this.roleIds)
						AppendValue(ref sb, "\"" + RoleId + "\"", ", ");

					Claims.Add(new KeyValuePair<string, object>(JwtClaims.Roles, sb?.ToString()));

					sb = null;

					foreach (Role Role in await this.LoadRoles())
					{
						foreach (PrivilegePattern Privilege in Role.Privileges)
						{
							if (Privilege.Include)
								AppendValue(ref sb, "+" + Privilege.Expression, Environment.NewLine);
							else
								AppendValue(ref sb, "-" + Privilege.Expression, Environment.NewLine);
						}
					}

					Claims.Add(new KeyValuePair<string, object>(JwtClaims.Entitlements, sb?.ToString()));
				}
			}

			return Claims;
		}

		/// <summary>
		/// Appends a value to a <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="Output">Value will be appended here. If created, the <paramref name="Delimiter"/> will be appended first, otherwise
		/// the <see cref="StringBuilder"/> will be created first.</param>
		/// <param name="Value">Value to append.</param>
		/// <param name="Delimiter">Delimiter, in case other values have been appended first.</param>
		public static void AppendValue(ref StringBuilder Output, object Value, string Delimiter)
		{
			if (Value is null)
				return;

			if (!(Value is string s))
				s = Value.ToString();

			if (!string.IsNullOrEmpty(s))
			{
				if (Output is null)
					Output = new StringBuilder();
				else
					Output.Append(Delimiter);

				Output.Append(s);
			}
		}

		/// <summary>
		/// Creates a JWT Token referencing the user object.
		/// </summary>
		/// <param name="Factory">JWT Factory.</param>
		/// <param name="Encrypted">If communication is encrypted.</param>
		/// <returns>Token, if able to create a token, null otherwise.</returns>
		public async Task<string> CreateToken(JwtFactory Factory, bool Encrypted)
		{
			return Factory.Create(await this.CreateClaims(Encrypted));
		}
	}
}
