using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Implements parameter encryption using symmetric ciphers avaialble through <see cref="IE2eSymmetricCipher"/>
	/// in the P2P library.
	/// </summary>
	public class ParameterEncryptionAlgorithm : IParameterEncryptionAlgorithm
	{
		private readonly SymmetricCipherAlgorithms algorithm;
		private readonly IE2eSymmetricCipher instance;
		private readonly byte[] key;

		private ParameterEncryptionAlgorithm(SymmetricCipherAlgorithms Algorithm, IE2eSymmetricCipher Instance, byte[] Key)
		{
			this.algorithm = Algorithm;
			this.instance = Instance;
			this.key = Key;
		}

		/// <summary>
		/// Key used to encrypt parameters.
		/// </summary>
		internal byte[] Key => this.key;

		/// <summary>
		/// Symmetric Cipher Algorithm used to encrypt parameters.
		/// </summary>
		internal SymmetricCipherAlgorithms Algorithm => this.algorithm;

		/// <summary>
		/// Implements parameter encryption using symmetric ciphers avaialble through <see cref="IE2eSymmetricCipher"/>
		/// in the P2P library.
		/// </summary>
		/// <param name="Algorithm">Algorithm to instantiate.</param>
		/// <param name="Client">Client managing keys.</param>
		public static Task<ParameterEncryptionAlgorithm> Create(SymmetricCipherAlgorithms Algorithm, ContractsClient Client)
		{
			return Create(null, Algorithm, Client);
		}

		/// <summary>
		/// Implements parameter encryption using symmetric ciphers avaialble through <see cref="IE2eSymmetricCipher"/>
		/// in the P2P library.
		/// </summary>
		/// <param name="ContractId">Optional Contract ID. If not provided, a new key will be generated.</param>
		/// <param name="DefaultAlgorithm">Default Algorithm to instantiate, if none is defined already.</param>
		/// <param name="Client">Client managing keys.</param>
		public static async Task<ParameterEncryptionAlgorithm> Create(string ContractId, SymmetricCipherAlgorithms DefaultAlgorithm,
			ContractsClient Client)
		{
			string CreatorJid = Client.Client.BareJID;
			byte[] Key = null;

			if (!string.IsNullOrEmpty(ContractId))
			{
				Tuple<SymmetricCipherAlgorithms, string, byte[]> T = await Client.TryLoadContractSharedSecret(ContractId);

				if (!(T.Item3 is null))
				{
					DefaultAlgorithm = T.Item1;
					CreatorJid = T.Item2;
					Key = T.Item3;
				}
			}

			IE2eSymmetricCipher Instance = E2eSymmetricCipher.Create(DefaultAlgorithm);

			if (Key is null)
			{
				Key = Instance.GenerateKey();

				if (!string.IsNullOrEmpty(ContractId))
					await Client.SaveContractSharedSecret(ContractId, CreatorJid, Key, DefaultAlgorithm, false);
			}

			return new ParameterEncryptionAlgorithm(DefaultAlgorithm, Instance, Key);
		}

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
		public byte[] Encrypt(string ParameterName, string ParameterType, uint ParameterIndex, string CreatorJid, string ContractNonce, 
			string ClearText)
		{
			byte[] Data = Encoding.UTF8.GetBytes(ClearText);
			byte[] IV = this.instance.GetIV(ParameterName, ParameterType, CreatorJid, ContractNonce, ParameterIndex);
			byte[] AssociatedData = Encoding.UTF8.GetBytes(ParameterName);

			return this.instance.Encrypt(Data, this.key, IV, AssociatedData);
		}

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
		public string Decrypt(string ParameterName, string ParameterType, uint ParameterIndex, string CreatorJid, string ContractNonce,
			byte[] CipherText)
		{
			byte[] IV = this.instance.GetIV(ParameterName, ParameterType, CreatorJid, ContractNonce, ParameterIndex);
			byte[] AssociatedData = Encoding.UTF8.GetBytes(ParameterName);
			byte[] Data = this.instance.Decrypt(CipherText, this.key, IV, AssociatedData);

			return Encoding.UTF8.GetString(Data);
		}
	}
}
