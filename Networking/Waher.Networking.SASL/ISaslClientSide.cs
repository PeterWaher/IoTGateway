using System;
using System.Threading.Tasks;
using Waher.Persistence;

namespace Waher.Networking.SASL
{
	/// <summary>
	/// Interface for client-side client connections.
	/// </summary>
	public interface ISaslClientSide
	{
		/// <summary>
		/// Initiates authentication
		/// </summary>
		/// <param name="Mechanism">Mechanism</param>
		/// <param name="Parameters">Any parameters.</param>
		/// <returns>If initiation was successful, challenge is returned.</returns>
		Task<string> Initiate(IAuthenticationMechanism Mechanism, string Parameters);

		/// <summary>
		/// Sends a challenge response back to the server.
		/// </summary>
		/// <param name="Mechanism">Mechanism</param>
		/// <param name="Parameters">Any parameters.</param>
		/// <returns>If challenge response was successful, response is returned.</returns>
		Task<string> ChallengeResponse(IAuthenticationMechanism Mechanism, string Parameters);

		/// <summary>
		/// Sends a final response back to the server.
		/// </summary>
		/// <param name="Mechanism">Mechanism</param>
		/// <param name="Parameters">Any parameters.</param>
		/// <returns>If final response was successful, response is returned. Can be null, if underlying connection does not provide a response.</returns>
		Task<string> FinalResponse(IAuthenticationMechanism Mechanism, string Parameters);

		/// <summary>
		/// Domain
		/// </summary>
		string Domain
		{
			get;
		}
	}
}
