using Waher.Security;

namespace Waher.Networking.SASL
{
    /// <summary>
    /// Base class for all MD5-hashed authentication mechanisms.
    /// </summary>
    public abstract class Md5AuthenticationMechanism : HashedAuthenticationMechanism 
    {
        /// <summary>
        /// Base class for all MD5-hashed authentication mechanisms.
        /// </summary>
        public Md5AuthenticationMechanism()
        {
        }

        /// <summary>
        /// Hash function
        /// </summary>
        /// <param name="Data">Data to hash.</param>
        /// <returns>Hash of data.</returns>
        public override byte[] H(byte[] Data)
        {
            return Hashes.ComputeMD5Hash(Data);
        }
    }
}
