using System;
using System.Threading.Tasks;
using Waher.Persistence;

namespace Waher.Networking.SASL
{
    /// <summary>
    /// Interface for SMTP user accounts.
    /// </summary>
    public interface IAccount
    {
        /// <summary>
        /// When account was created.
        /// </summary>
        DateTime Created
        {
            get;
        }

		/// <summary>
		/// User Name
		/// </summary>
		CaseInsensitiveString UserName
        {
            get;
        }

        /// <summary>
        /// Password
        /// </summary>
        string Password
        {
            get;
        }

        /// <summary>
        /// If the account is enabled.
        /// </summary>
        bool Enabled
        {
            get;
        }

		/// <summary>
		/// If the account has a given privilege.
		/// </summary>
		/// <param name="PrivilegeID">Privilege ID</param>
		/// <returns>If the account has that privilege.</returns>
		bool HasPrivilege(string PrivilegeID);

        /// <summary>
        /// Records a successful log in.
        /// </summary>
        /// <param name="RemoteEndPoint">Identifier of remote endpoint performing the login.</param>
        Task LoggedIn(string RemoteEndPoint);
	}
}
