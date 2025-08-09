using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.PQC;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Module Lattice endpoint with 128 bit security strength.
	/// </summary>
	public class ModuleLattice128Endpoint : ModuleLatticeEndpoint
	{
		/// <summary>
		/// Module Lattice endpoint with 128 bit security strength.
		/// </summary>
		public ModuleLattice128Endpoint()
			: this(new Aes256())
		{
		}

		/// <summary>
		/// Module Lattice endpoint with 128 bit security strength.
		/// </summary>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public ModuleLattice128Endpoint(IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(ML_KEM.ML_KEM_512, ML_KEM.ML_KEM_512.KeyGen(),
				  ML_DSA.ML_DSA_44, ML_DSA.ML_DSA_44.KeyGen(),
				  DefaultSymmetricCipher)
		{
		}

		/// <summary>
		/// Module Lattice endpoint with 128 bit security strength.
		/// </summary>
		/// <param name="PublicKey">Remote public key.</param>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public ModuleLattice128Endpoint(byte[] PublicKey, IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(PublicKey, ML_KEM.ML_KEM_512, ML_DSA.ML_DSA_44, DefaultSymmetricCipher)
		{
		}

		/// <summary>
		/// Module Lattice endpoint with 128 bit security strength.
		/// </summary>
		/// <param name="KeyEncapsulationMechanismKeys">Key Encapsulation Mechanism Keys.</param>
		/// <param name="SignatureAlgorithmKeys">Signature Algorithm Keys.</param>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public ModuleLattice128Endpoint(ML_KEM_Keys KeyEncapsulationMechanismKeys, 
			ML_DSA_Keys SignatureAlgorithmKeys, IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(ML_KEM.ML_KEM_512, KeyEncapsulationMechanismKeys,
				  ML_DSA.ML_DSA_44, SignatureAlgorithmKeys, DefaultSymmetricCipher)
		{
		}

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 128;

		/// <summary>
		/// Local name of the E2E encryption scheme
		/// </summary>
		public override string LocalName => "ml128";

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new ModuleLattice128Endpoint(this.DefaultSymmetricCipher.CreteNew());
		}

		/// <summary>
		/// Creates a new endpoint given a private key.
		/// </summary>
		/// <param name="Secret">Secret.</param>
		/// <returns>Endpoint object.</returns>
		public override IE2eEndpoint CreatePrivate(byte[] Secret)
		{
			this.GeneratePrivateKeys(Secret, out ML_KEM_Keys KemKeys, out ML_DSA_Keys DsaKeys);
			return new ModuleLattice128Endpoint(KemKeys, DsaKeys, this.DefaultSymmetricCipher.CreteNew());
		}

		/// <summary>
		/// Creates a new endpoint given a public key.
		/// </summary>
		/// <param name="PublicKey">Remote public key.</param>
		/// <returns>Endpoint object.</returns>
		public override IE2eEndpoint CreatePublic(byte[] PublicKey)
		{
			return new ModuleLattice128Endpoint(PublicKey, this.DefaultSymmetricCipher.CreteNew());
		}
	}
}
