using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Waher.Content;
using Waher.Networking.HTTP.OAuth.Interfaces;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// Manages the OAuth 2 environment.
	/// </summary>
	public class OAuth2Environment
	{
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		private OAuthAuthorizeResource? authorizeResource = null;
		private OAuthTokenResource? tokenResource = null;
		private OAuthRegistrationResource? registrationResource = null;
		private OAuthDeviceAuthorizationResource? deviceAuthorizationResource = null;
		private AuthorizationServerMetaData? serverMetaDataResource = null;
		private ProtectedResourceMetaData? resourceMetaData = null;
		private IUserSource? userSource = null;
		private IDynamicUserSource? dynamicUserSource = null;
		private IThingRegistryUserSource? thingRegistryUserSource = null;
		private JwtFactory? jwtFactory;
		private string? loginMasterFileName;
		private string? realm;
		private int minStrength;
		private bool encrypted;
		private bool locked = false;

		/// <summary>
		/// Manages the OAuth 2.0 environment.
		/// </summary>
		public OAuth2Environment()
		{
			this.Register(Security.Users.Users.Source); // Default users source
		}

		/// <summary>
		/// If the environment has a registered authorization resource
		/// </summary>
		public bool HasAuthorizeResource => !(this.authorizeResource is null);

		/// <summary>
		/// If the environment has a registered token resource
		/// </summary>
		public bool HasTokenResource => !(this.tokenResource is null);

		/// <summary>
		/// If the environment has a registered registration resource
		/// </summary>
		public bool HasRegistrationResource => !(this.registrationResource is null);

		/// <summary>
		/// If the environment has a registered device authorization resource
		/// </summary>
		public bool HasDeviceAuthorizationResource => !(this.deviceAuthorizationResource is null);

		/// <summary>
		/// If the environment has a registered server meta-data resource
		/// </summary>
		public bool HasServerMetaDataResource => !(this.serverMetaDataResource is null);

		/// <summary>
		/// If the environment has a registered resource meta-data resource
		/// </summary>
		public bool HasResourceMetaData => !(this.resourceMetaData is null);

		/// <summary>
		/// If the environment has a registered user source
		/// </summary>
		public bool HasUserSource => !(this.userSource is null);

		/// <summary>
		/// If the environment has a registered dynamic user source
		/// </summary>
		public bool HasDynamicUserSource => !(this.dynamicUserSource is null);

		/// <summary>
		/// If the environment has a registered thing registry user source
		/// </summary>
		public bool HasThingRegistryUserSource => !(this.thingRegistryUserSource is null);

		/// <summary>
		/// If a login master file name has been registered
		/// </summary>
		public bool HasLoginMasterFileName => !string.IsNullOrEmpty(this.loginMasterFileName);

		/// <summary>
		/// Registered authorization resource
		/// </summary>
		public OAuthAuthorizeResource AuthorizeResource
		{
			get
			{
				if (this.authorizeResource is null)
					throw new InvalidOperationException("No authorize resource has been registered.");

				return this.authorizeResource;
			}
		}

		/// <summary>
		/// Registered token resource
		/// </summary>
		public OAuthTokenResource TokenResource
		{
			get
			{
				if (this.tokenResource is null)
					throw new InvalidOperationException("No token resource has been registered.");

				return this.tokenResource;
			}
		}

		/// <summary>
		/// Registered registration resource
		/// </summary>
		public OAuthRegistrationResource RegistrationResource
		{
			get
			{
				if (this.registrationResource is null)
					throw new InvalidOperationException("No registration resource has been registered.");

				return this.registrationResource;
			}
		}

		/// <summary>
		/// Registered device authorization resource
		/// </summary>
		public OAuthDeviceAuthorizationResource DeviceAuthorizationResource
		{
			get
			{
				if (this.deviceAuthorizationResource is null)
					throw new InvalidOperationException("No device authorization resource has been registered.");

				return this.deviceAuthorizationResource;
			}
		}

		/// <summary>
		/// Registered server meta-data resource
		/// </summary>
		public AuthorizationServerMetaData ServerMetaDataResource
		{
			get
			{
				if (this.serverMetaDataResource is null)
					throw new InvalidOperationException("No server meta-data resource has been registered.");

				return this.serverMetaDataResource;
			}
		}

		/// <summary>
		/// Registered resource meta-data resource
		/// </summary>
		public ProtectedResourceMetaData ResourceMetaData
		{
			get
			{
				if (this.resourceMetaData is null)
					throw new InvalidOperationException("No resource meta-data resource has been registered.");

				return this.resourceMetaData;
			}
		}

		/// <summary>
		/// Registered user source
		/// </summary>
		public IUserSource UserSource
		{
			get
			{
				if (this.userSource is null)
					throw new InvalidOperationException("No user source has been registered.");

				return this.userSource;
			}
		}

		/// <summary>
		/// Registered dynamic user source
		/// </summary>
		public IDynamicUserSource DynamicUserSource
		{
			get
			{
				if (this.dynamicUserSource is null)
					throw new InvalidOperationException("No dynamic user source has been registered.");

				return this.dynamicUserSource;
			}
		}

		/// <summary>
		/// Registered thing registry user source
		/// </summary>
		public IThingRegistryUserSource ThingRegistryUserSource
		{
			get
			{
				if (this.thingRegistryUserSource is null)
					throw new InvalidOperationException("No thing registry user source has been registered.");

				return this.thingRegistryUserSource;
			}
		}

		/// <summary>
		/// Registered JWT factory
		/// </summary>
		public JwtFactory JwtFactory
		{
			get
			{
				if (this.jwtFactory is null)
				{
					if (Types.TryGetModuleParameter("JWT", out JwtFactory JwtFactory) &&
						!JwtFactory.Disposed)
					{
						this.jwtFactory = JwtFactory;
					}
					else
						this.jwtFactory = JwtFactory.CreateHmacSha256(this.realm);
				}

				return this.jwtFactory;
			}
		}

		/// <summary>
		/// File name to master file to use in generated login pages.
		/// </summary>
		public string? LoginMasterFileName
		{
			get => this.loginMasterFileName;
			set
			{
				this.AssertUnlocked();

				if (!string.IsNullOrEmpty(value) && !File.Exists(value))
					throw new FileNotFoundException("Login master file not found.", value);

				this.loginMasterFileName = value;
			}
		}

		/// <summary>
		/// If the environment has been locked.
		/// </summary>
		public bool Locked => this.locked;

		/// <summary>
		/// Registered realm name.
		/// </summary>
		public string? Realm
		{
			get
			{
				this.CheckDomainParameters();
				return this.realm;
			}
		}

		/// <summary>
		/// Minimum strength of ciphers used in encryption.
		/// </summary>
		public int MinStrength
		{
			get
			{
				this.CheckDomainParameters();
				return this.minStrength;
			}
		}

		/// <summary>
		/// If TLS-encryption is enabled.
		/// </summary>
		public bool Encrypted
		{
			get
			{
				this.CheckDomainParameters();
				return this.encrypted;
			}
		}

		private void AssertUnlocked()
		{
			if (this.locked)
				throw new UnauthorizedAccessException("OAUTH 2 environment is locked and cannot be modified.");
		}

		/// <summary>
		/// Locks the OAUTH 2 environment.
		/// </summary>
		public void Lock()
		{
			this.AssertUnlocked();
			this.locked = true;
		}

		/// <summary>
		/// Registers an authorization resource.
		/// </summary>
		/// <param name="AuthorizeResource">Authorization resource to register.</param>
		public void Register(OAuthAuthorizeResource? AuthorizeResource)
		{
			this.AssertUnlocked();
			this.authorizeResource = AuthorizeResource;
		}

		/// <summary>
		/// Registers a token resource.
		/// </summary>
		/// <param name="TokenResource">Token resource to register.</param>
		public void Register(OAuthTokenResource? TokenResource)
		{
			this.AssertUnlocked();
			this.tokenResource = TokenResource;
		}

		/// <summary>
		/// Registers a registration resource.
		/// </summary>
		/// <param name="RegistrationResource">Registration resource to register.</param>
		public void Register(OAuthRegistrationResource? RegistrationResource)
		{
			this.AssertUnlocked();
			this.registrationResource = RegistrationResource;
		}

		/// <summary>
		/// Registers a device authorization resource.
		/// </summary>
		/// <param name="DeviceAuthorizationResource">Device authorization resource to register.</param>
		public void Register(OAuthDeviceAuthorizationResource? DeviceAuthorizationResource)
		{
			this.AssertUnlocked();
			this.deviceAuthorizationResource = DeviceAuthorizationResource;
		}

		/// <summary>
		/// Registers a server meta-data resource.
		/// </summary>
		/// <param name="ServerMetaDataResource">Server meta-data resource to register.</param>
		public void Register(AuthorizationServerMetaData? ServerMetaDataResource)
		{
			this.AssertUnlocked();
			this.serverMetaDataResource = ServerMetaDataResource;
		}

		/// <summary>
		/// Registers a resource meta-data resource.
		/// </summary>
		/// <param name="ResourceMetaData">Resource meta-data resource to register.</param>
		public void Register(ProtectedResourceMetaData? ResourceMetaData)
		{
			this.AssertUnlocked();
			this.resourceMetaData = ResourceMetaData;
		}

		/// <summary>
		/// Registers a user source.
		/// </summary>
		/// <param name="UserSource">User source to register.</param>
		public void Register(IUserSource? UserSource)
		{
			this.AssertUnlocked();
			this.userSource = UserSource;
			this.dynamicUserSource = UserSource as IDynamicUserSource;
			this.thingRegistryUserSource = UserSource as IThingRegistryUserSource;
		}

		/// <summary>
		/// Registers a JWT factory.
		/// </summary>
		/// <param name="JwtFactory">JWT factory to register.</param>
		public void Register(JwtFactory JwtFactory)
		{
			this.AssertUnlocked();
			this.jwtFactory = JwtFactory;
		}

		/// <summary>
		/// Registers domain parameters such as realm, minimum strength and encryption.
		/// </summary>
		/// <param name="Realm">Realm name.</param>
		/// <param name="MinStrength">Minimum strength of ciphers used in encryption.</param>
		/// <param name="Encrypted">If TLS-encryption is enabled.</param>
		public void Register(string Realm, int MinStrength, bool Encrypted)
		{
			this.AssertUnlocked();
			this.realm = Realm;
			this.minStrength = MinStrength;
			this.encrypted = Encrypted;
		}

		private void CheckDomainParameters()
		{
			if (this.realm is null)
			{
				GetDomainParameters(out string? Domain, out int MinStrength, out bool Encrypted);
				this.realm = Domain;
				this.minStrength = MinStrength;
				this.encrypted = Encrypted;
			}
		}

		/// <summary>
		/// Gets domain parameters, based on module parameters defined in the system.
		/// </summary>
		/// <param name="Domain">Domain name</param>
		/// <param name="MinStrength">Minimum strength of ciphers used in encryption.</param>
		/// <param name="Encrypted">If TLS-encryption is enabled.</param>
		public static void GetDomainParameters(out string? Domain, out int MinStrength,
			out bool Encrypted)
		{
			if (!Types.TryGetModuleParameter("X509", out object Obj) ||
				!(Obj is X509Certificate Certificate))
			{
				if (Types.TryGetModuleParameter("Realm", out Obj) &&
					Obj is string Realm)
				{
					Domain = Realm;
				}
				else
					Domain = null;

				Encrypted = false;
				MinStrength = 0;
			}
			else
			{
				Encrypted = true;
				Domain = BinaryTcpClient.GetDomainFromSubject(Certificate.Subject);
				MinStrength = 128;
			}
		}

		/// <summary>
		/// Generates a random unique code.
		/// </summary>
		/// <param name="NrBytes">Number of bytes of random.</param>
		/// <returns>Random unique code.</returns>
		public string GenerateRandomCode(int NrBytes)
		{
			byte[] Bin = new byte[NrBytes];

			lock (rnd)
			{
				rnd.GetBytes(Bin);
			}

			return Base64Url.Encode(Bin);
		}

	}
}