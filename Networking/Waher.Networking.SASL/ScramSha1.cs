using Waher.Security;

namespace Waher.Networking.SASL
{
	/// <summary>
	/// Authentication done by SCRAM-SHA-1 defined in RFC 5802:
	/// https://tools.ietf.org/html/rfc5802
	/// </summary>
	public class ScramSha1 : ScramAuthenticationMechanism
	{
		/// <summary>
		/// Authentication done by SCRAM-SHA-1 defined in RFC 5802:
		/// https://tools.ietf.org/html/rfc5802
		/// </summary>
		public ScramSha1()
		{
		}

		/// <summary>
		/// Name of the mechanism.
		/// </summary>
		public override string Name
		{
			get { return "SCRAM-SHA-1"; }
		}

		/// <summary>
		/// Weight of mechanisms. The higher the value, the more preferred.
		/// </summary>
		public override int Weight => 900;

		/// <summary>
		/// Hash function
		/// </summary>
		/// <param name="Data">Data to hash.</param>
		/// <returns>Hash of data.</returns>
		public override byte[] H(byte[] Data)
		{
			return Hashes.ComputeSHA1Hash(Data);
		}

	}
}
