using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Security.LoginMonitor;

namespace Waher.Networking.SASL
{
    /// <summary>
    /// Interface for XMPP Server persistence layers. The persistence layer should implement caching.
    /// </summary>
    public interface ISaslPersistenceLayer
    {
        /// <summary>
        /// Method to call to fetch account information.
        /// </summary>
        /// <param name="UserName">User Name</param>
        /// <returns>Account, if found, null otherwise.</returns>
        Task<IAccount> GetAccount(CaseInsensitiveString UserName);

        /// <summary>
        /// Successful login to account registered.
        /// </summary>
        /// <param name="UserName">User name of account.</param>
        /// <param name="RemoteEndpoint">Remote endpoint of client.</param>
        void AccountLogin(CaseInsensitiveString UserName, string RemoteEndpoint);

		/// <summary>
		/// Generates a set of random numbers.
		/// </summary>
		/// <param name="NrBytes">Number of bytes.</param>
		/// <returns>Set of random bytes.</returns>
		byte[] GetRandomNumbers(int NrBytes);

		/// <summary>
		/// Current domain.
		/// </summary>
		string Domain
		{
			get;
		}

        /// <summary>
        /// Login auditor.
        /// </summary>
        LoginAuditor Auditor
        {
            get;
        }

	}
}
