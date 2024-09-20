using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Script.Functions.Vectors;
using Waher.Security;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Implements parameter encryption using symmetric ciphers avaialble through <see cref="IE2eSymmetricCipher"/>
	/// in the P2P library.
	/// </summary>
	public class ParameterEncryptionAlgorithm : IParameterEncryptionAlgorithm
	{
		private readonly ContractsClient client;
		private readonly SymmetricCipherAlgorithms algorithm;
		private readonly IE2eSymmetricCipher instance;
		private readonly byte[] key;

		private ParameterEncryptionAlgorithm(SymmetricCipherAlgorithms Algorithm,
			IE2eSymmetricCipher Instance, byte[] Key, ContractsClient Client)
		{
			this.algorithm = Algorithm;
			this.instance = Instance;
			this.key = Key;
			this.client = Client;
		}

		/// <summary>
		/// Key used to encrypt parameters.
		/// </summary>
		public byte[] Key => this.key;

		/// <summary>
		/// Symmetric Cipher Algorithm used to encrypt parameters.
		/// </summary>
		public SymmetricCipherAlgorithms Algorithm => this.algorithm;

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
		/// Creates a object that implements parameter encryption using symmetric ciphers 
		/// avaialble through <see cref="IE2eSymmetricCipher"/> in the P2P library.
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

			return await Create(ContractId, DefaultAlgorithm, Client, CreatorJid, Key);
		}

		/// <summary>
		/// Creates a object that implements parameter encryption using symmetric ciphers 
		/// avaialble through <see cref="IE2eSymmetricCipher"/> in the P2P library.
		/// </summary>
		/// <param name="ContractId">Optional Contract ID. If not provided, a new key will be generated.</param>
		/// <param name="Algorithm">Algorithm to instantiate.</param>
		/// <param name="Client">Client managing keys.</param>
		/// <param name="CreatorJid">JID of creator of contract.</param>
		/// <param name="Key">Encryption key to use.</param>
		public static async Task<ParameterEncryptionAlgorithm> Create(string ContractId, SymmetricCipherAlgorithms Algorithm,
			ContractsClient Client, string CreatorJid, byte[] Key)
		{
			IE2eSymmetricCipher Instance = E2eSymmetricCipher.Create(Algorithm);

			if (Key is null)
			{
				Key = Instance.GenerateKey();

				if (!string.IsNullOrEmpty(ContractId))
					await Client.SaveContractSharedSecret(ContractId, CreatorJid, Key, Algorithm, false);
			}

			return new ParameterEncryptionAlgorithm(Algorithm, Instance, Key, Client);
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
		public byte[] Encrypt(string ParameterName, string ParameterType, uint ParameterIndex, string CreatorJid, byte[] ContractNonce,
			string ClearText)
		{
			byte[] Data;

			Data = new byte[this.key.Length + ContractNonce.Length + 4];
			int c = this.key.Length;
			int d = ContractNonce.Length;

			Array.Copy(this.key, 0, Data, 0, c);
			Array.Copy(ContractNonce, 0, Data, c, d);
			Array.Copy(BitConverter.GetBytes(ParameterIndex), 0, Data, c + d, 4);

			Data = Hashes.ComputeSHA256Hash(Data);

			int SuffixLength = Data[0];

			if (ClearText is null)
				Data = new byte[SuffixLength];
			else
			{
				int Prefix = -1;
				int i, j;

				for (i = 1, c = Data.Length; i < c; i++)
				{
					if ((j = Data[i]) != 0)
					{
						Prefix = j;
						break;
					}
				}

				if (Prefix < 0)
					Prefix = 1;

				byte[] Data2 = Encoding.UTF8.GetBytes(ClearText);
				c = Data2.Length;

				Data = new byte[1 + c + SuffixLength];

				Data[0] = (byte)Prefix;
				Array.Copy(Data2, 0, Data, 1, c);
			}

			byte[] IV = this.instance.GetIV(ParameterName, ParameterType, CreatorJid,
				Convert.ToBase64String(ContractNonce), ParameterIndex);
			byte[] AssociatedData = Encoding.UTF8.GetBytes(ParameterName);

			byte[] Result = this.instance.Encrypt(Data, this.key, IV, AssociatedData, E2eBufferFillAlgorithm.Zeroes);

			return Result;
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
		public string Decrypt(string ParameterName, string ParameterType, uint ParameterIndex, string CreatorJid, byte[] ContractNonce,
			byte[] CipherText)
		{
			byte[] IV = this.instance.GetIV(ParameterName, ParameterType, CreatorJid, Convert.ToBase64String(ContractNonce), ParameterIndex);
			byte[] AssociatedData = Encoding.UTF8.GetBytes(ParameterName);
			byte[] Data = this.instance.Decrypt(CipherText, this.key, IV, AssociatedData);

			if (Data.Length == 0 || Data[0] == 0)
				return null;

			int c = Data.Length - 1;

			while (c >= 0 && Data[c] == 0)
				c--;

			return Encoding.UTF8.GetString(Data, 1, c);
		}
	}
}
