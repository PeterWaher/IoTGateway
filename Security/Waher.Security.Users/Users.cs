using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Threading;
using Waher.Security.LoginMonitor;

namespace Waher.Security.Users
{
	/// <summary>
	/// Delegate for hash digest computation methods.
	/// </summary>
	/// <param name="UserName">User name</param>
	/// <param name="Password">Password</param>
	/// <returns>Hash digest.</returns>
	public delegate byte[] HashComputationMethod(string UserName, string Password);

	/// <summary>
	/// Maintains the collection of all users in the system.
	/// </summary>
	public class Users : IUserSource
	{
		private static readonly Dictionary<string, User> users = new Dictionary<string, User>();
		private static readonly MultiReadSingleWriteObject synchObj = new MultiReadSingleWriteObject();
		private static readonly IUserSource source = new Users();
		private static LoginAuditor loginAuditor = null;
		private static HashComputationMethod hashMethod = null;
		private static string hashMethodTypeName = "Internal";
		private static bool hashMethodLocked = false;

		/// <summary>
		/// User source.
		/// </summary>
		public static IUserSource Source => source;

		/// <summary>
		/// Tries to get a user with a given user name.
		/// </summary>
		/// <param name="UserName">User Name.</param>
		/// <returns>User, if found, null otherwise.</returns>
		public async Task<IUser> TryGetUser(string UserName)
		{
			return await GetUser(UserName, false);
		}

		/// <summary>
		/// Gets the <see cref="User"/> object corresponding to a User Name.
		/// </summary>
		/// <param name="UserName">User Name.</param>
		/// <param name="CreateIfNew">If user should be created, if it does not exist.</param>
		/// <returns>User object, or null if not found and not created.</returns>
		public static async Task<User> GetUser(string UserName, bool CreateIfNew)
		{
			User User;
			bool Load;

			await synchObj.BeginWrite();
			try
			{
				if (users.TryGetValue(UserName, out User))
					return User;

				User = await Database.FindFirstDeleteRest<User>(new FilterFieldEqualTo("UserName", UserName));
				if (User is null)
				{
					if (!CreateIfNew)
						return null;

					User = new User()
					{
						UserName = UserName,
						PasswordHash = string.Empty,
						RoleIds = new string[0]
					};

					await Database.Insert(User);

					Load = false;
				}
				else
					Load = true;
			
				users[UserName] = User;
			}
			finally
			{
				await synchObj.EndWrite();
			}

			if (Load)
				await User.LoadRoles();

			return User;
		}

		/// <summary>
		/// Registers a Hash Digest Computation Method.
		/// </summary>
		/// <param name="HashComputationMethod">Hash Digest Computation Method.</param>
		/// <param name="HashMethodTypeName">Hash Digest Computation Method Type Name.</param>
		/// <param name="LoginAuditor">Auditor of login attempts.</param>
		/// <param name="Lock">If the registration should be locked.</param>
		public static void Register(HashComputationMethod HashComputationMethod, string HashMethodTypeName, LoginAuditor LoginAuditor, bool Lock)
		{
			if (hashMethodLocked)
				throw new InvalidOperationException("Hash method already registered, and locked.");

			loginAuditor = LoginAuditor;
			hashMethod = HashComputationMethod;
			hashMethodTypeName = HashMethodTypeName;
			hashMethodLocked = Lock;
		}

		/// <summary>
		/// Hash Digest Computation Method Type Name.
		/// </summary>
		public static string HashMethodTypeName => hashMethodTypeName;

		/// <summary>
		/// Computes a hash of a password.
		/// </summary>
		/// <param name="UserName">User name.</param>
		/// <param name="Password">Password.</param>
		public static byte[] ComputeHash(string UserName, string Password)
		{
			HashComputationMethod h = hashMethod;
			if (h is null)
				return Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(UserName + ":" + Password));
			else
				return h(UserName, Password);
		}

		/// <summary>
		/// Attempts to login in the system.
		/// </summary>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <param name="RemoteEndPoint">Remote endpoint of user attempting to login.</param>
		/// <param name="Protocol">Protocol used for login.</param>
		/// <returns>Login result.</returns>
		public static async Task<LoginResult> Login(string UserName, string Password, string RemoteEndPoint, string Protocol)
		{
			if (!(loginAuditor is null))
			{
				DateTime? Next = await loginAuditor.GetEarliestLoginOpportunity(RemoteEndPoint, Protocol);
				if (Next.HasValue)
					return new LoginResult(Next.Value);
			}

			User User = await GetUser(UserName, false);

			if (!(User is null) && User.PasswordHash != Convert.ToBase64String(ComputeHash(UserName, Password)))
				User = null;

			if (User is null)
				LoginAuditor.Fail("Invalid login.", UserName, RemoteEndPoint, Protocol);
			else
				LoginAuditor.Success("Successful login.", UserName, RemoteEndPoint, Protocol);

			return new LoginResult(User);
		}
	}
}
