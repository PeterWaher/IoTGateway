using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Runtime.Collections;
using Waher.Runtime.Counters;
using Waher.Runtime.Inventory;
using Waher.Runtime.Profiling;

namespace Waher.Security.E2EE
{
	/// <summary>
	/// Abstract base class for End-to-End encryption schemes.
	/// </summary>
	public abstract class E2eEndpoint : IE2eEndpoint
	{
		private const string E2eCounterName = "E2EE.Counter";

		/// <summary>
		/// urn:ieee:iot:e2e:1.0
		/// </summary>
		public const string IoTHarmonizationE2EIeeeV1 = "urn:ieee:iot:e2e:1.0";

		/// <summary>
		/// urn:nf:iot:e2e:1.0
		/// </summary>
		public const string IoTHarmonizationE2ENeuroFoundationV1 = "urn:nf:iot:e2e:1.0";

		/// <summary>
		/// Current namespace for End-to-End encryption.
		/// </summary>
		public const string IoTHarmonizationE2ECurrent = IoTHarmonizationE2ENeuroFoundationV1;

		/// <summary>
		/// Namespaces supported for End-to-end encryption.
		/// </summary>
		public static readonly string[] NamespacesIoTHarmonizationE2E = new string[]
		{
			IoTHarmonizationE2ENeuroFoundationV1,
			IoTHarmonizationE2EIeeeV1
		};


		private static readonly Dictionary<string, IE2eSymmetricCipher> symmetricCiphers = new Dictionary<string, IE2eSymmetricCipher>();
		private static Dictionary<string, IE2eEndpoint> endpointTypes = new Dictionary<string, IE2eEndpoint>();
		private static Type[] e2eTypes = null;
		private static bool initialized = false;
		private static bool e2eTypesLocked = false;

		private IE2eSymmetricCipher defaultSymmetricCipher;
		private IE2eEndpoint prev = null;

		/// <summary>
		/// Abstract base class for End-to-End encryption schemes.
		/// </summary>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public E2eEndpoint(IE2eSymmetricCipher DefaultSymmetricCipher)
		{
			this.defaultSymmetricCipher = DefaultSymmetricCipher;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			this.defaultSymmetricCipher?.Dispose();
			this.defaultSymmetricCipher = null;
		}

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public abstract int SecurityStrength { get; }

		/// <summary>
		/// Previous keys.
		/// </summary>
		public IE2eEndpoint Previous
		{
			get => this.prev;
			set => this.prev = value;
		}

		/// <summary>
		/// Local name of the E2E encryption scheme
		/// </summary>
		public abstract string LocalName { get; }

		/// <summary>
		/// Namespace of the E2E encryption scheme
		/// </summary>
		public virtual string Namespace => IoTHarmonizationE2ECurrent;

		/// <summary>
		/// Remote public key.
		/// </summary>
		public abstract byte[] PublicKey { get; }

		/// <summary>
		/// Remote public key, as a Base64 string.
		/// </summary>
		public abstract string PublicKeyBase64 { get; }

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public abstract IE2eEndpoint Create(int SecurityStrength);

		/// <summary>
		/// Creates a new endpoint given a private key.
		/// </summary>
		/// <param name="Secret">Secret.</param>
		/// <returns>Endpoint object.</returns>
		public abstract IE2eEndpoint CreatePrivate(byte[] Secret);

		/// <summary>
		/// Creates a new endpoint given a public key.
		/// </summary>
		/// <param name="PublicKey">Remote public key.</param>
		/// <returns>Endpoint object.</returns>
		public abstract IE2eEndpoint CreatePublic(byte[] PublicKey);

		/// <summary>
		/// Parses endpoint information from an XML element.
		/// </summary>
		/// <param name="Xml">XML element.</param>
		/// <returns>Parsed key information, if possible, null if XML is not well-defined.</returns>
		public IE2eEndpoint Parse(XmlElement Xml)
		{
			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "pub":
						return this.CreatePublic(Convert.FromBase64String(Attr.Value));

					case "d":
						return this.CreatePrivate(Convert.FromBase64String(Attr.Value));
				}
			}

			return null;
		}


		/// <summary>
		/// Exports the public key information to XML.
		/// </summary>
		public string ToXml()
		{
			return this.ToXml(string.Empty);
		}

		/// <summary>
		/// Exports the public key information to XML.
		/// </summary>
		/// <param name="ParentNamespace">Namespace of parent element.</param>
		public string ToXml(string ParentNamespace)
		{
			StringBuilder Xml = new StringBuilder();
			this.ToXml(Xml, ParentNamespace);
			return Xml.ToString();
		}

		/// <summary>
		/// Exports the public key information to XML.
		/// </summary>
		/// <param name="Xml">XML output</param>
		/// <param name="ParentNamespace">Namespace of parent element.</param>
		public void ToXml(StringBuilder Xml, string ParentNamespace)
		{
			Xml.Append('<');
			Xml.Append(this.LocalName);
			Xml.Append(" pub=\"");
			Xml.Append(this.PublicKeyBase64);

			string ns = this.Namespace;
			if (ns != ParentNamespace)
			{
				Xml.Append("\" xmlns=\"");
				Xml.Append(ns);
			}

			Xml.Append("\"/>");
		}

		/// <summary>
		/// Gets a shared secret for encryption, and optionally a corresponding cipher text.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint</param>
		/// <param name="Cipher">Symmetric cipher to use for encryption.</param>
		/// <param name="CipherText">Optional cipher text required by the recipient to
		/// be able to generate the same shared secret.</param>
		/// <returns>Shared secret.</returns>
		public abstract byte[] GetSharedSecretForEncryption(IE2eEndpoint RemoteEndpoint,
			IE2eSymmetricCipher Cipher, out byte[] CipherText);

		/// <summary>
		/// Gets a shared secret for decryption.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint</param>
		/// <param name="CipherText">Optional cipher text required by the recipient to
		/// be able to generate the same shared secret.</param>
		/// <returns>Shared secret.</returns>
		public abstract byte[] GetSharedSecretForDecryption(IE2eEndpoint RemoteEndpoint,
			byte[] CipherText);

		/// <summary>
		/// If the recipient needs a cipher text to generate the same shared secret.
		/// </summary>
		public abstract bool SharedSecretUseCipherText
		{
			get;
		}

		/// <summary>
		/// If signatures are supported.
		/// </summary>
		public virtual bool SupportsSignatures => true;

		/// <summary>
		/// Signs binary data using the local private key.
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <returns>Digital signature.</returns>
		public abstract byte[] Sign(byte[] Data);

		/// <summary>
		/// Signs binary data using the local private key.
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <returns>Digital signature.</returns>
		public abstract byte[] Sign(Stream Data);

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public abstract bool Verify(byte[] Data, byte[] Signature);

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public abstract bool Verify(Stream Data, byte[] Signature);

		/// <summary>
		/// If endpoint is considered safe (i.e. there are no suspected backdoors)
		/// </summary>
		public virtual bool Safe => true;

		/// <summary>
		/// If implementation is slow, compared to other options.
		/// </summary>
		public virtual bool Slow => false;

		/// <summary>
		/// If post-quantum cryptography is used.
		/// </summary>
		public virtual bool PostQuantumCryptography => false;

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder Xml = new StringBuilder();
			this.ToXml(Xml, string.Empty);
			return Xml.ToString();
		}

		/// <inheritdoc/>
		public override abstract bool Equals(object obj);

		/// <inheritdoc/>
		public override abstract int GetHashCode();

		/// <summary>
		/// Provides a score for the endpoint. More features, higher score.
		/// </summary>
		public int Score
		{
			get
			{
				int Result = 0;

				if (!this.SharedSecretUseCipherText)
					Result++;

				if (this.SupportsSignatures)
					Result++;

				if (this.Safe)
					Result++;

				if (!this.Slow)
					Result++;

				if (this.PostQuantumCryptography)
					Result += 2;    // To overcome requirement of using cipher texts to derive shared secrets.

				return Result;
			}
		}

		/// <summary>
		/// Default symmetric cipher.
		/// </summary>
		public virtual IE2eSymmetricCipher DefaultSymmetricCipher
		{
			get => this.defaultSymmetricCipher;
			set => this.defaultSymmetricCipher = value ?? this.defaultSymmetricCipher;
		}

		/// <summary>
		/// Gets the next counter value.
		/// </summary>
		/// <returns>Counter value.</returns>
		public async Task<uint> GetNextCounter()
		{
			return (uint)await RuntimeCounters.IncrementCounter(E2eCounterName);
		}

		/// <summary>
		/// Creates a set of endpoints within a range of security strengths.
		/// </summary>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <returns>Array of local endpoint keys.</returns>
		public static IE2eEndpoint[] CreateEndpoints(int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength)
		{
			return CreateEndpoints(DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength, null);
		}

		/// <summary>
		/// Creates a set of endpoints within a range of security strengths.
		/// </summary>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <param name="OnlyIfDerivedFrom">Only return endpoints derived from these type s.</param>
		/// <returns>Array of local endpoint keys.</returns>
		public static IE2eEndpoint[] CreateEndpoints(int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			params Type[] OnlyIfDerivedFrom)
		{
			return CreateEndpoints(DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength, OnlyIfDerivedFrom, null);
		}

		/// <summary>
		/// Creates a set of endpoints within a range of security strengths.
		/// </summary>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <param name="OnlyIfDerivedFrom">Only return endpoints derived from these types.</param>
		/// <param name="Thread">Optional profiling thread.</param>
		/// <returns>Array of local endpoint keys.</returns>
		public static IE2eEndpoint[] CreateEndpoints(int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			Type[] OnlyIfDerivedFrom, ProfilerThread Thread)
		{
			Thread = Thread?.CreateSubThread("Endpoints", ProfilerThreadType.Sequential);
			try
			{
				Thread?.Start();
				Thread?.NewState("Init");

				int i, c = OnlyIfDerivedFrom?.Length ?? 0;
				if (c == 1 && OnlyIfDerivedFrom[0] is null)
					c = 0;

				TypeInfo[] OnlyIfDerivedFromType = c == 0 ? null : new TypeInfo[c];

				for (i = 0; i < c; i++)
					OnlyIfDerivedFromType[i] = OnlyIfDerivedFrom[i].GetTypeInfo();

				List<IE2eEndpoint> Result = new List<IE2eEndpoint>();
				IEnumerable<IE2eEndpoint> Templates;
				bool CheckHeritance = true;

				lock (endpointTypes)
				{
					if (initialized)
						Templates = endpointTypes.Values;
					else
					{
						Dictionary<string, IE2eEndpoint> E2eTypes = new Dictionary<string, IE2eEndpoint>();
						Dictionary<string, bool> TypeNames = new Dictionary<string, bool>();
						TypeInfo E2eTypeInfo = typeof(IE2eEndpoint).GetTypeInfo();

						foreach (KeyValuePair<string, IE2eEndpoint> P in endpointTypes)
						{
							E2eTypes[P.Key] = P.Value;
							TypeNames[P.Value.GetType().FullName] = true;
						}

						foreach (Type T in e2eTypes ?? Types.GetTypesImplementingInterface(typeof(IE2eEndpoint)))
						{
							if (TypeNames.ContainsKey(T.FullName))
								continue;

							TypeInfo TI = T.GetTypeInfo();
							if (!(e2eTypes is null) && !E2eTypeInfo.IsAssignableFrom(TI))
								continue;

							if (c > 0)
							{
								bool DerivedFrom = false;

								for (i = 0; i < c; i++)
								{
									if (OnlyIfDerivedFromType[i].IsAssignableFrom(TI))
									{
										DerivedFrom = true;
										break;
									}
								}

								if (!DerivedFrom)
									continue;
							}

							ConstructorInfo CI = Types.GetDefaultConstructor(T);
							if (CI is null)
								continue;

							try
							{
								IE2eEndpoint Endpoint = (IE2eEndpoint)CI.Invoke(Types.NoParameters);
								E2eTypes[Endpoint.Namespace + "#" + Endpoint.LocalName] = Endpoint;
							}
							catch (Exception ex)
							{
								Log.Exception(ex);
								continue;
							}
						}

						endpointTypes = E2eTypes;
						Templates = E2eTypes.Values;

						if (OnlyIfDerivedFromType is null)
						{
							foreach (IE2eSymmetricCipher Cipher in CreateSymmetricCiphers())
								symmetricCiphers[Cipher.Namespace + "#" + Cipher.LocalName] = Cipher;

							initialized = true;
						}
						else
							CheckHeritance = false;
					}
				}

				foreach (IE2eEndpoint Endpoint in Templates)
				{
					if (CheckHeritance && c > 0)
					{
						bool DerivedFrom = false;

						for (i = 0; i < c; i++)
						{
							if (OnlyIfDerivedFromType[i].IsAssignableFrom(Endpoint.GetType().GetTypeInfo()))
							{
								DerivedFrom = true;
								break;
							}
						}

						if (!DerivedFrom)
							continue;
					}

					Thread?.NewState(Endpoint.LocalName);

					IE2eEndpoint Endpoint2 = Endpoint.Create(DesiredSecurityStrength);
					i = Endpoint2.SecurityStrength;
					if (i >= MinSecurityStrength && i <= MaxSecurityStrength)
						Result.Add(Endpoint2);
					else
						Endpoint2.Dispose();
				}

				return Result.ToArray();
			}
			finally
			{
				Thread?.Stop();
			}
		}

		/// <summary>
		/// Creates an array of available symmetric ciphers.
		/// </summary>
		/// <returns>Available symmetric ciphers.</returns>
		public static IE2eSymmetricCipher[] CreateSymmetricCiphers()
		{
			ChunkedList<IE2eSymmetricCipher> Result = new ChunkedList<IE2eSymmetricCipher>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IE2eSymmetricCipher)))
			{
				try
				{
					if (T.IsAbstract)
						continue;

					ConstructorInfo CI = Types.GetDefaultConstructor(T);
					if (CI is null)
						continue;

					Result.Add((IE2eSymmetricCipher)CI.Invoke(Types.NoParameters));
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Sets allowed cipers in endpoint security.
		/// </summary>
		/// <param name="CipherTypes">Allowed cipher types. null=all types allowed.</param>
		/// <param name="Lock">If set of ciphers should be locked.</param>
		public static void SetCiphers(Type[] CipherTypes, bool Lock)
		{
			if (e2eTypesLocked)
				throw new InvalidOperationException("Ciphers locked.");

			e2eTypes = CipherTypes;
			e2eTypesLocked = Lock;
		}

		/// <summary>
		/// Tries to get an existing endpoint, given its qualified name.
		/// </summary>
		/// <param name="LocalName">Local name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Endpoint">Endpoint, or null if not found.</param>
		/// <returns>If an endpoint was found with the given name.</returns>
		public static bool TryGetEndpoint(string LocalName, string Namespace, out IE2eEndpoint Endpoint)
		{
			if (Namespace.StartsWith("urn:ieee:"))
				Namespace = Namespace.Replace("urn:ieee:", "urn:nf:");

			string Key = Namespace + "#" + LocalName;

			if (endpointTypes.TryGetValue(Key, out Endpoint))
				return true;
			else if (initialized || endpointTypes.Count > 0)
				return false;

			CreateEndpoints(128, 0, int.MaxValue);

			return endpointTypes.TryGetValue(Key, out Endpoint);
		}

		/// <summary>
		/// Tries to create a new endpoint, given its qualified name.
		/// </summary>
		/// <param name="LocalName">Local name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Endpoint">Created endpoint, or null if not found.</param>
		/// <returns>If an endpoint was found with the given name, and a new instance was created.</returns>
		public static bool TryCreateEndpoint(string LocalName, string Namespace, out IE2eEndpoint Endpoint)
		{
			if (TryGetEndpoint(LocalName, Namespace, out Endpoint))
			{
				Endpoint = Endpoint.Create(Endpoint.SecurityStrength);
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Tries to get a symmetric cipher, given its qualified name.
		/// </summary>
		/// <param name="LocalName">Local name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="SymmetricCipher">Symmetric Cipher, or null if not found.</param>
		/// <returns>If a symmetric cipher was found with the given name.</returns>
		public static bool TryGetSymmetricCipher(string LocalName, string Namespace, 
			out IE2eSymmetricCipher SymmetricCipher)
		{
			if (Namespace.StartsWith("urn:ieee:"))
				Namespace = Namespace.Replace("urn:ieee:", "urn:nf:");

			string Key = Namespace + "#" + LocalName;

			if (symmetricCiphers.TryGetValue(Key, out SymmetricCipher))
				return true;
			else if (initialized || symmetricCiphers.Count > 0)
				return false;

			CreateEndpoints(128, 0, int.MaxValue);

			return symmetricCiphers.TryGetValue(Key, out SymmetricCipher);
		}

		/// <summary>
		/// Tries to create a new symmetric cipher, given its qualified name.
		/// </summary>
		/// <param name="LocalName">Local name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Cipher">Created symmetric cipher, or null if not found.</param>
		/// <returns>If a symmetric cipher was found with the given name, and a new instance was created.</returns>
		public static bool TryCreateSymmetricCipher(string LocalName, string Namespace, out IE2eSymmetricCipher Cipher)
		{
			if (TryGetSymmetricCipher(LocalName, Namespace, out Cipher))
			{
				Cipher = Cipher.CreteNew();
				return true;
			}
			else
				return false;
		}

	}
}
