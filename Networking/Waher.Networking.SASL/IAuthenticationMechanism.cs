using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.SASL
{
    /// <summary>
    /// Interface for authentication mechanisms.
    /// </summary>
    public interface IAuthenticationMechanism
    {
        /// <summary>
        /// Name of the mechanism.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Weight of mechanisms. The higher the value, the more preferred.
        /// </summary>
        int Weight
        {
            get;
        }

		/// <summary>
		/// Checks if a mechanism is allowed during the current conditions.
		/// </summary>
		/// <param name="SslStream">SSL stream, if available.</param>
		/// <returns>If mechanism is allowed.</returns>
		bool Allowed(SslStream SslStream);

		/// <summary>
		/// Performs intitialization of the mechanism. Can be used to set
		/// static properties that will be used through-out the runtime of the
		/// server.
		/// </summary>
		Task Initialize();

        /// <summary>
        /// Authentication request has been made.
        /// </summary>
        /// <param name="Data">Data in authentication request.</param>
        /// <param name="Connection">Connection performing the authentication.</param>
        /// <param name="PersistenceLayer">Persistence layer.</param>
        /// <returns>If authentication was successful (true). If false, mechanism must send the corresponding challenge.</returns>
        Task<bool?> AuthenticationRequest(string Data, ISaslServerSide Connection, ISaslPersistenceLayer PersistenceLayer);

        /// <summary>
        /// Response request has been made.
        /// </summary>
        /// <param name="Data">Data in response request.</param>
        /// <param name="Connection">Connection performing the authentication.</param>
        /// <param name="PersistenceLayer">Persistence layer.</param>
        /// <returns>If authentication was successful (true).</returns>
        Task<bool?> ResponseRequest(string Data, ISaslServerSide Connection, ISaslPersistenceLayer PersistenceLayer);

		/// <summary>
		/// Authenticates the user using the provided credentials.
		/// </summary>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Connection">Connection</param>
		/// <returns>If authentication was successful or not. If null is returned, the mechanism did not perform authentication.</returns>
		Task<bool?> Authenticate(string UserName, string Password, ISaslClientSide Connection);
	}
}
