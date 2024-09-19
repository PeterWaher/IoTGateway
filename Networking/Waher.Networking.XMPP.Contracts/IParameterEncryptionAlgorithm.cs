using Waher.Networking.XMPP.P2P.SymmetricCiphers;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Interface for parameter encryption algorithms.
	/// </summary>
	public interface IParameterEncryptionAlgorithm
	{
		/// <summary>
		/// Symmetric Cipher Algorithm used to encrypt parameters.
		/// </summary>
		SymmetricCipherAlgorithms Algorithm { get; }

		/// <summary>
		/// Key used for encryption
		/// </summary>
		byte[] Key { get; }

		/// <summary>
		/// Encrypts a parameter value.
		/// </summary>
		/// <param name="ParameterName">Name of parameter.</param>
		/// <param name="ParameterType">Type of parameter.</param>
		/// <param name="ParameterIndex">Zero-based parameter index in contract.</param>
		/// <param name="CreatorJid">Bare JID of creator of contract.</param>
		/// <param name="ContractNonce">Contract Nonce</param>
		/// <param name="ClearText">Clear-text string representation of value.</param>
		/// <returns>Cipher text.</returns>
		byte[] Encrypt(string ParameterName, string ParameterType, uint ParameterIndex, 
			string CreatorJid, string ContractNonce, string ClearText);

		/// <summary>
		/// Decrypts an encrypted parameter value.
		/// </summary>
		/// <param name="ParameterName">Name of parameter.</param>
		/// <param name="ParameterType">Type of parameter.</param>
		/// <param name="ParameterIndex">Zero-based parameter index in contract.</param>
		/// <param name="CreatorJid">Bare JID of creator of contract.</param>
		/// <param name="ContractNonce">Contract Nonce</param>
		/// <param name="CipherText">Cipher text.</param>
		/// <returns>Clear text string representation of value.</returns>
		string Decrypt(string ParameterName, string ParameterType, uint ParameterIndex, 
			string CreatorJid, string ContractNonce, byte[] CipherText);
	}
}
