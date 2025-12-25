using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Security.CallStack;

namespace Waher.Security.TOTP
{
	/// <summary>
	/// Specifies the type of external credential algorithm to use.
	/// </summary>
	public enum CredentialAlgorithm
	{
		/// <summary>
		/// Counter-based one-time password (HOTP) algorithm (RFC 4226).
		/// </summary>
		HOTP,

		/// <summary>
		/// Time-based one-time password (TOTP) algorithm (RFC 6238).
		/// </summary>
		TOTP,

		/// <summary>
		/// Credentials are static (not one-time passwords).
		/// </summary>
		Static
	}

	/// <summary>
	/// Contains OTP secret information for an OTP endpoint.
	/// </summary>
	[CollectionName("ExternalCredentials")]
	[TypeName(TypeNameSerialization.None)]
	[ArchivingTime]
	[Index("Type", "Endpoint")]
	public class ExternalCredential : IEncryptedProperties
	{
		private static ICallStackCheck[] approvedSources = null;

		private HashFunction? hashFunction;
		private byte[] secret;
		private string account;
		private string issuer;
		private string label;
		private string description;
		private long? counter;
		private int nrDigits = HotpCalculator.DefaultNrDigits;
		private int timeStepSeconds = TotpCalculator.DefaultTimeStepSeconds;

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Credential Algorithm type.
		/// </summary>
		public CredentialAlgorithm Type { get; set; }

		/// <summary>
		/// Endpoint
		/// </summary>
		public string Endpoint { get; set; }

		/// <summary>
		/// Optional name of the endpoint.
		/// </summary>
		[Encrypted(32)]
		public string Label
		{
			get
			{
				AssertAllowed();
				return this.label;
			}

			set
			{
				AssertAllowed();
				this.label = value;
			}
		}

		/// <summary>
		/// Optional description of the endpoint.
		/// </summary>
		[Encrypted(32)]
		public string Description
		{
			get
			{
				AssertAllowed();
				return this.description;
			}

			set
			{
				AssertAllowed();
				this.description = value;
			}
		}

		/// <summary>
		/// Endpoint secret.
		/// </summary>
		[Encrypted(32)]
		public byte[] Secret
		{
			get
			{
				AssertAllowed();
				return this.secret;
			}

			set
			{
				AssertAllowed();
				this.secret = value;
			}
		}

		/// <summary>
		/// Issuer of credential.
		/// </summary>
		[Encrypted(32)]
		public string Issuer
		{
			get
			{
				AssertAllowed();
				return this.issuer;
			}

			set
			{
				AssertAllowed();
				this.issuer = value;
			}
		}

		/// <summary>
		/// Endpoint secret.
		/// </summary>
		[Encrypted(32)]
		public string Account
		{
			get
			{
				AssertAllowed();
				return this.account;
			}

			set
			{
				AssertAllowed();
				this.account = value;
			}
		}

		/// <summary>
		/// Hash function used for the endpoint.
		/// </summary>
		[Encrypted(16)]
		public HashFunction? HashFunction
		{
			get
			{
				AssertAllowed();
				return this.hashFunction;
			}

			set
			{
				AssertAllowed();
				this.hashFunction = value;
			}
		}

		/// <summary>
		/// Number of digits.
		/// </summary>
		[Encrypted(16)]
		public int NrDigits
		{
			get
			{
				AssertAllowed();
				return this.nrDigits;
			}

			set
			{
				AssertAllowed();
				this.nrDigits = value;
			}
		}

		/// <summary>
		/// Counter
		/// </summary>
		[Encrypted(16)]
		public long? Counter
		{
			get
			{
				AssertAllowed();
				return this.counter;
			}

			set
			{
				AssertAllowed();
				this.counter = value;
			}
		}

		/// <summary>
		/// Time step in seconds.
		/// </summary>
		[Encrypted(16)]
		public int TimeStepSeconds
		{
			get
			{
				AssertAllowed();
				return this.timeStepSeconds;
			}

			set
			{
				AssertAllowed();
				this.timeStepSeconds = value;
			}
		}

		/// <summary>
		/// Array of properties that are encrypted.
		/// </summary>
		public string[] EncryptedProperties => new string[]
		{
			nameof(this.Label),
			nameof(this.Description),
			nameof(this.Secret),
			nameof(this.Secret),
			nameof(this.Issuer),
			nameof(this.Account),
			nameof(this.HashFunction),
			nameof(this.NrDigits),
			nameof(this.Counter),
			nameof(this.TimeStepSeconds)
		};

		/// <summary>
		/// If access to sensitive methods is only accessible from a set of approved sources.
		/// </summary>
		/// <param name="ApprovedSources">Approved sources.</param>
		/// <exception cref="NotSupportedException">If trying to change previously set sources.</exception>
		public static void SetAllowedSources(ICallStackCheck[] ApprovedSources)
		{
			if (!(approvedSources is null))
				throw new NotSupportedException("Changing approved sources not permitted.");

			approvedSources = ApprovedSources;
		}

		private static void AssertAllowed()
		{
			if (!(approvedSources is null))
				Assert.CallFromSource(approvedSources);
		}

		/// <summary>
		/// Gets a secret for an endpoint, if one exists.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="Type">OTP Algorithm type.</param>
		/// <returns>Secret, if exists, null otherwise.</returns>
		internal static async Task<ExternalCredential> GetSecret(string Endpoint, CredentialAlgorithm Type)
		{
			return await Database.FindFirstIgnoreRest<ExternalCredential>(new FilterAnd(
				new FilterFieldEqualTo(nameof(Endpoint), Endpoint),
				new FilterFieldEqualTo(nameof(Type), Type)));
		}

		/// <summary>
		/// Tries to parse an OTP Auth URI (otpauth://).
		/// </summary>
		/// <remarks>
		/// Reference:
		/// https://github.com/google/google-authenticator/wiki/Key-Uri-Format
		/// </remarks>
		/// <param name="OtpAuthUri">URI</param>
		/// <returns>Parsed URI, if available, null otherwise.</returns>
		public static ExternalCredential TryParse(string OtpAuthUri)
		{
			if (!Uri.TryCreate(OtpAuthUri, UriKind.Absolute, out Uri ParsedUri))
				return null;

			return TryParse(ParsedUri);
		}

		/// <summary>
		/// Tries to parse an OTP Auth URI (otpauth://).
		/// </summary>
		/// <remarks>
		/// Reference:
		/// https://github.com/google/google-authenticator/wiki/Key-Uri-Format
		/// </remarks>
		/// <param name="OtpAuthUri">URI</param>
		/// <returns>Parsed URI, if available, null otherwise.</returns>
		public static ExternalCredential TryParse(Uri OtpAuthUri)
		{
			if (!OtpAuthUri.Scheme.Equals("otpauth", StringComparison.OrdinalIgnoreCase))
				return null;

			CredentialAlgorithm Type;
			string Label = OtpAuthUri.AbsolutePath;
			HashFunction HashFunction = HotpCalculator.DefaultHashFunction;
			string Key, Value;
			string Issuer = null;
			string Account = null;
			byte[] Secret = null;
			int NrDigits = HotpCalculator.DefaultNrDigits;
			int TimeStepSeconds = TotpCalculator.DefaultTimeStepSeconds;
			long? Counter = null;
			int i;

			switch (OtpAuthUri.Authority.ToLower())
			{
				case "totp":
					Type = CredentialAlgorithm.TOTP;
					break;

				case "hotp":
					Type = CredentialAlgorithm.HOTP;
					break;

				default:
					return null;
			}

			if (Label.StartsWith('/'))
				Label = WebUtility.UrlDecode(Label[1..]);

			i = Label.IndexOf(':');
			if (i >= 0)
			{
				Issuer = WebUtility.UrlDecode(Label[..i]);
				Account = WebUtility.UrlDecode(Label[(i + 1)..]);
			}

			Key = OtpAuthUri.Query;
			if (Key.StartsWith('?'))
				Key = Key[1..];

			foreach (string Part in Key.Split('&'))
			{
				i = Part.IndexOf('=');
				if (i < 0)
				{
					Key = WebUtility.UrlDecode(Part);
					Value = string.Empty;
				}
				else
				{
					Key = WebUtility.UrlDecode(Part[..i]);
					Value = WebUtility.UrlDecode(Part[(i + 1)..]);
				}

				switch (Key.ToLower())
				{
					case "secret":
						try
						{
							Secret = Base32.Decode(Value);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
							return null;
						}
						break;

					case "issuer":
						Issuer = Value;
						break;

					case "algorithm":
						switch (Value.ToUpper())
						{
							case "SHA1":
							case "SHA-1":
								HashFunction = Security.HashFunction.SHA1;
								break;

							case "SHA256":
							case "SHA-256":
							case "SHA2-256":
								HashFunction = Security.HashFunction.SHA256;
								break;

							case "SHA384":
							case "SHA-384":
							case "SHA2-384":
								HashFunction = Security.HashFunction.SHA384;
								break;

							case "SHA512":
							case "SHA-512":
							case "SHA2-512":
								HashFunction = Security.HashFunction.SHA512;
								break;

							default:
								Log.Warning("Unsupported OTP Auth URI hash algorithm: " + Value,
									new KeyValuePair<string, object>("Algorithm", Value));
								return null;
						}
						break;

					case "digits":
						if (!int.TryParse(Value, out i) || i < 6 || i > 8)
							return null;

						NrDigits = i;
						break;

					case "counter":
						if (Type != CredentialAlgorithm.HOTP)
							return null;

						if (!long.TryParse(Value, out long l) || l < 0)
							return null;

						Counter = l;
						break;

					case "period":
						if (Type != CredentialAlgorithm.TOTP)
							return null;

						if (!int.TryParse(Value, out i) || i <= 0)
							return null;

						TimeStepSeconds = i;
						break;

					default:
						Log.Warning("Unsupported OTP Auth URI parameter: " + Key,
							new KeyValuePair<string, object>("Key", Key),
							new KeyValuePair<string, object>("Value", Value));
						return null;
				}
			}

			if (Secret is null)
				return null;

			if (Type == CredentialAlgorithm.HOTP && !Counter.HasValue)
				return null;

			return new ExternalCredential()
			{
				Endpoint = string.IsNullOrEmpty(Issuer) ? Label : Issuer,
				Type = Type,
				Label = Label,
				Issuer = Issuer,
				Account = Account,
				HashFunction = HashFunction,
				Secret = Secret,
				NrDigits = NrDigits,
				Counter = Counter,
				TimeStepSeconds = TimeStepSeconds
			};
		}
	}
}
