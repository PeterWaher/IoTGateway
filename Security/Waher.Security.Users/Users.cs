﻿using System;
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
		private static readonly MultiReadSingleWriteObject synchObj = new MultiReadSingleWriteObject(typeof(Users));
		private static readonly IUserSource source = new Users();
		private static LoginAuditor loginAuditor = null;
		private static HashComputationMethod hashMethod = null;
		private static string domain = string.Empty;
		private static string hashMethodTypeName = "Internal";
		private static bool hashMethodLocked = false;

		/// <summary>
		/// User source.
		/// </summary>
		public static IUserSource Source => source;

		/// <summary>
		/// Any Login Auditor registered, if any.
		/// </summary>
		public static LoginAuditor LoginAuditor => loginAuditor;

		/// <summary>
		/// Domain name, if provided.
		/// </summary>
		public static string Domain => domain;

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
						RoleIds = Array.Empty<string>()
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
			Register(HashComputationMethod, HashMethodTypeName, LoginAuditor, string.Empty, Lock);
		}

		/// <summary>
		/// Registers a Hash Digest Computation Method.
		/// </summary>
		/// <param name="HashComputationMethod">Hash Digest Computation Method.</param>
		/// <param name="HashMethodTypeName">Hash Digest Computation Method Type Name.</param>
		/// <param name="LoginAuditor">Auditor of login attempts.</param>
		/// <param name="Domain">Domain name</param>
		/// <param name="Lock">If the registration should be locked.</param>
		public static void Register(HashComputationMethod HashComputationMethod, string HashMethodTypeName, LoginAuditor LoginAuditor, 
			string Domain, bool Lock)
		{
			if (hashMethodLocked)
				throw new InvalidOperationException("Hash method already registered, and locked.");

			domain = Domain;
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
		/// If the Hash Method has been registered and locked.
		/// </summary>
		public static bool HashMethodLocked => hashMethodLocked;

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
		[Obsolete("Use the Passord Hash & Nonce overload to avoid sending passwords in clear text over the network.")]
		public static async Task<LoginResult> Login(string UserName, string Password, string RemoteEndPoint, string Protocol)
		{
			if (string.IsNullOrEmpty(Password))
				return new LoginResult();

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

		/// <summary>
		/// Attempts to login in the system.
		/// </summary>
		/// <param name="UserName">User name</param>
		/// <param name="PasswordHash">A Password Hash computed by the client.</param>
		/// <param name="Nonce">Nonce used in passord hash computation.</param>
		/// <param name="RemoteEndPoint">Remote endpoint of user attempting to login.</param>
		/// <param name="Protocol">Protocol used for login.</param>
		/// <returns>Login result.</returns>
		public static async Task<LoginResult> Login(string UserName, string PasswordHash, 
			string Nonce, string RemoteEndPoint, string Protocol)
		{
			if (string.IsNullOrEmpty(PasswordHash))
				return new LoginResult();

			if (!(loginAuditor is null))
			{
				DateTime? Next = await loginAuditor.GetEarliestLoginOpportunity(RemoteEndPoint, Protocol);
				if (Next.HasValue)
					return new LoginResult(Next.Value);
			}

			User User = await GetUser(UserName, false);

			if (!(User is null))
			{
				string UserPasswordHash = Convert.ToBase64String(Hashes.ComputeHMACSHA256Hash(
					Encoding.UTF8.GetBytes(Nonce),
					Convert.FromBase64String(User.PasswordHash)));

				if (UserPasswordHash != PasswordHash)
					User = null;
			}

			if (User is null)
				LoginAuditor.Fail("Invalid login.", UserName, RemoteEndPoint, Protocol);
			else
				LoginAuditor.Success("Successful login.", UserName, RemoteEndPoint, Protocol);

			return new LoginResult(User);
		}

		/// <summary>
		/// Clears internal caches.
		/// </summary>
		public static void ClearCache()
		{
			lock (users)
			{
				users.Clear();
			}
		}
	}
}
