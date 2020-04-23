using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Security
{
	/// <summary>
	/// Interface for classes that monitor login events, and help applications determine malicious intent. 
	/// </summary>
	public interface ILoginAuditor
	{
		/// <summary>
		/// Processes a successful login attempt.
		/// 
		/// NOTE: Typically, logins are audited by listening on logged events.
		/// This method should only be called directly when such events are not logged.
		/// </summary>
		/// <param name="RemoteEndpoint">String-representation of remote endpoint.</param>
		/// <param name="Protocol">Protocol used to log in.</param>
		Task ProcessLoginSuccessful(string RemoteEndpoint, string Protocol);

		/// <summary>
		/// Processes a failed login attempt.
		/// 
		/// NOTE: Typically, logins are audited by listening on logged events.
		/// This method should only be called directly when such events are not logged.
		/// </summary>
		/// <param name="RemoteEndpoint">String-representation of remote endpoint.</param>
		/// <param name="Protocol">Protocol used to log in.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Reason">Reason for the failure. Will be logged with the state object, in case the remote endpoint
		/// gets blocked.</param>
		/// <returns>If the remote endpoint was or has been blocked as a result of the failure.</returns>
		Task<bool> ProcessLoginFailure(string RemoteEndpoint, string Protocol, DateTime Timestamp, string Reason);

		/// <summary>
		/// Checks when a remote endpoint can login.
		/// </summary>
		/// <param name="RemoteEndpoint">String-representation of remote endpoint.</param>
		/// <param name="Protocol">Protocol used to log in.</param>
		/// <returns>When the remote endpoint is allowed to login:
		/// 
		/// null = Remote endpoint can login now.
		/// <see cref="DateTime.MaxValue"/> = Remote endpoint has been blocked and cannot login.
		/// Other <see cref="DateTime"/> values indicate when, at the earliest, the remote endpoint
		/// is allowed to login again.
		/// </returns>
		Task<DateTime?> GetEarliestLoginOpportunity(string RemoteEndpoint, string Protocol);

	}
}
