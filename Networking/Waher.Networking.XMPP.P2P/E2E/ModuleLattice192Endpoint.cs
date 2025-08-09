using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.PQC;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Module Lattice endpoint with 192 bit security strength.
	/// </summary>
	public class ModuleLattice192Endpoint : ModuleLatticeEndpoint
	{
		/// <summary>
		/// Module Lattice endpoint with 192 bit security strength.
		/// </summary>
		public ModuleLattice192Endpoint()
			: this(new Aes256())
		{
		}

		/// <summary>
		/// Module Lattice endpoint with 192 bit security strength.
		/// </summary>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public ModuleLattice192Endpoint(IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(ML_KEM.ML_KEM_768, ML_KEM.ML_KEM_768.KeyGen(),
				  ML_DSA.ML_DSA_65, ML_DSA.ML_DSA_65.KeyGen(),
				  DefaultSymmetricCipher)
		{
		}

		/// <summary>
		/// Module Lattice endpoint with 192 bit security strength.
		/// </summary>
		/// <param name="PublicKey">Remote public key.</param>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public ModuleLattice192Endpoint(byte[] PublicKey, IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(PublicKey, ML_KEM.ML_KEM_768, ML_DSA.ML_DSA_65, DefaultSymmetricCipher)
		{
		}

		/// <summary>
		/// Module Lattice endpoint with 192 bit security strength.
		/// </summary>
		/// <param name="KeyEncapsulationMechanismKeys">Key Encapsulation Mechanism Keys.</param>
		/// <param name="SignatureAlgorithmKeys">Signature Algorithm Keys.</param>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public ModuleLattice192Endpoint(ML_KEM_Keys KeyEncapsulationMechanismKeys, 
			ML_DSA_Keys SignatureAlgorithmKeys, IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(ML_KEM.ML_KEM_768, KeyEncapsulationMechanismKeys,
				  ML_DSA.ML_DSA_65, SignatureAlgorithmKeys, DefaultSymmetricCipher)
		{
		}

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 192;

		/// <summary>
		/// Local name of the E2E encryption scheme
		/// </summary>
		public override string LocalName => "ml192";

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new ModuleLattice192Endpoint(this.DefaultSymmetricCipher.CreteNew());
		}

		/// <summary>
		/// Creates a new endpoint given a private key.
		/// </summary>
		/// <param name="Secret">Secret.</param>
		/// <returns>Endpoint object.</returns>
		public override IE2eEndpoint CreatePrivate(byte[] Secret)
		{
			this.GeneratePrivateKeys(Secret, out ML_KEM_Keys KemKeys, out ML_DSA_Keys DsaKeys);
			return new ModuleLattice192Endpoint(KemKeys, DsaKeys, this.DefaultSymmetricCipher.CreteNew());
		}

		/// <summary>
		/// Creates a new endpoint given a public key.
		/// </summary>
		/// <param name="PublicKey">Remote public key.</param>
		/// <returns>Endpoint object.</returns>
		public override IE2eEndpoint CreatePublic(byte[] PublicKey)
		{
			return new ModuleLattice192Endpoint(PublicKey, this.DefaultSymmetricCipher.CreteNew());
		}
	}
}
