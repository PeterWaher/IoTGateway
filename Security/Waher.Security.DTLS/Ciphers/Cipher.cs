using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Waher.Security.DTLS.Ciphers
{
	/// <summary>
	/// Base class for all ciphers used in (D)TLS.
	/// </summary>
	public abstract class Cipher : ICipher
	{
		private SHA256 hashFunction;

		/// <summary>
		/// Base class for all ciphers used in (D)TLS.
		/// </summary>
		public Cipher()
		{
			this.hashFunction = SHA256.Create();
		}

		/// <summary>
		/// Cipher name.
		/// </summary>
		public abstract string Name
		{
			get;
		}

		/// <summary>
		/// IANA cipher suite code:
		/// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-4
		/// </summary>
		public abstract ushort IanaCipherSuite
		{
			get;
		}

		/// <summary>
		/// Priority. The higher the number, the higher priority.
		/// </summary>
		public abstract int Priority
		{
			get;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.hashFunction != null)
			{
				this.hashFunction.Dispose();
				this.hashFunction = null;
			}
		}

		/// <summary>
		/// If the cipher can be used by the endpoint.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <returns>If the cipher can be used.</returns>
		public abstract bool CanBeUsed(DtlsEndpoint Endpoint);

		/// <summary>
		/// Sends the Client Key Exchange message.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		public abstract void SendClientKeyExchange(DtlsEndpoint Endpoint);

		/// <summary>
		/// Pseudo-random function for the cipher, as defined in §5 of RFC 5246:
		/// https://tools.ietf.org/html/rfc5246#section-5
		/// </summary>
		/// <param name="Secret">Secret</param>
		/// <param name="Label">Label</param>
		/// <param name="Seed">Seed</param>
		/// <param name="NrBytes">Number of bytes to generate.</param>
		public virtual byte[] PRF(byte[] Secret, string Label, byte[] Seed, uint NrBytes)
		{
			byte[] Result = new byte[NrBytes];
			byte[] A, P;
			int Pos = 0;
			uint BytesLeft = NrBytes;

			using (HMACSHA256 Hmac = new HMACSHA256(Secret))
			{
				A = Concat(Encoding.UTF8.GetBytes(Label), Seed);

				while (BytesLeft > 0)
				{
					A = Hmac.ComputeHash(A);
					P = Hmac.ComputeHash(Concat(A, Seed));

					if (BytesLeft < P.Length)
					{
						Array.Copy(P, 0, Result, Pos, (int)BytesLeft);
						Pos += (int)BytesLeft;
						BytesLeft = 0;
					}
					else
					{
						Array.Copy(P, 0, Result, Pos, P.Length);
						Pos += P.Length;
						BytesLeft -= (uint)P.Length;
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// Concatenates two binary arrays.
		/// </summary>
		/// <param name="A1">Array 1</param>
		/// <param name="A2">Array 2</param>
		/// <returns>A1+A2</returns>
		public static byte[] Concat(byte[] A1, byte[] A2)
		{
			int c1, c2;

			if (A1 == null || (c1 = A1.Length) == 0)
				return A2;
			else if (A2 == null || (c2 = A2.Length) == 0)
				return A1;

			byte[] Result = new byte[c1 + c2];

			Array.Copy(A1, 0, Result, 0, c1);
			Array.Copy(A2, 0, Result, c1, c2);

			return Result;
		}

		/// <summary>
		/// Finishes the handshake.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="Client">If the client acts as a client (true), or a server (false).</param>
		/// <param name="Handshake">Entire handshake communication.</param>
		public virtual void SendFinished(DtlsEndpoint Endpoint, bool Client, byte[] Handshake)
		{
			byte[] Hash = this.hashFunction.ComputeHash(Handshake);
			string Label = Client ? "client finished" : "server finished";
			byte[] VerifyData = this.PRF(Endpoint.MasterSecret, Label, Hash, 12);

			Endpoint.SendHandshake(HandshakeType.finished, VerifyData, false);
		}
	}
}
