using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Security.JWS
{
	/// <summary>
	/// Abstract base class for JWS algorithm.
	/// </summary>
	public interface IJwsAlgorithm : IDisposable
	{
		/// <summary>
		/// Short name for algorithm.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Signs data.
		/// </summary>
		/// <param name="Header">Properties to include in the header.</param>
		/// <param name="Payload">Properties to include in the payload.</param>
		/// <param name="HeaderString">Resulting encoded header string.</param>
		/// <param name="PayloadString">Resulting encoded payload string.</param>
		/// <param name="Signature">Generated signature.</param>
		void Sign(IEnumerable<KeyValuePair<string, object>> Header,
			IEnumerable<KeyValuePair<string, object>> Payload, out string HeaderString,
			out string PayloadString, out string Signature);

		/// <summary>
		/// Signs data.
		/// </summary>
		/// <param name="HeaderEncoded">Encoded properties to include in the header.</param>
		/// <param name="PayloadEncoded">Encoded properties to include in the payload.</param>
		/// <returns>Signature</returns>
		string Sign(string HeaderEncoded, string PayloadEncoded);

		/// <summary>
		/// Checks if a signature is valid.
		/// </summary>
		/// <param name="HeaderEncoded">Encoded properties to include in the header.</param>
		/// <param name="PayloadEncoded">Encoded properties to include in the payload.</param>
		/// <param name="SignatureEncoded">Encoded signature.</param>
		/// <returns>If the signature is valid.</returns>
		bool IsValid(string HeaderEncoded, string PayloadEncoded, string SignatureEncoded);
	}
}
