using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Zip;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Runtime.Threading;
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
		/// Credentials are static UTF-8 encoded passwords.
		/// </summary>
		UTF8
	}

	/// <summary>
	/// Contains OTP secret information for an OTP endpoint.
	/// </summary>
	[CollectionName("ExternalCredentials")]
	[TypeName(TypeNameSerialization.None)]
	[ArchivingTime]
	[Index("Type", "Endpoint")]
	[Index("Endpoint", "Type")]
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
		private int? nrDigits = null;
		private int? timeStepSeconds = null;

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
				if (!string.IsNullOrEmpty(this.label))
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
				if (!string.IsNullOrEmpty(this.description))
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
				if (!(this.secret is null))
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
				if (!string.IsNullOrEmpty(this.issuer))
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
				if (!string.IsNullOrEmpty(this.account))
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
				if (this.hashFunction.HasValue)
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
				return this.nrDigits ?? HotpCalculator.DefaultNrDigits;
			}

			set
			{
				if (this.nrDigits.HasValue)
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
				if (this.counter.HasValue)
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
				return this.timeStepSeconds ?? TotpCalculator.DefaultTimeStepSeconds;
			}

			set
			{
				if (this.timeStepSeconds.HasValue)
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

		/// <summary>
		/// Gets stored credentials.
		/// </summary>
		/// <returns>Enumerated set of credentials.</returns>
		public static async Task<IEnumerable<ExternalCredential>> GetCredentials()
		{
			AssertAllowed();
			return await Database.Find<ExternalCredential>("Endpoint");
		}

		/// <summary>
		/// Gets the next password for the endpoint.
		/// </summary>
		public string Current
		{
			get
			{
				AssertAllowed();

				switch (this.Type)
				{
					case CredentialAlgorithm.HOTP:
						if (!this.Counter.HasValue)
							throw new InvalidOperationException("Counter not set for HOTP credential.");

						int Code = HotpCalculator.Compute(this.NrDigits, this.secret,
							this.hashFunction ?? HotpCalculator.DefaultHashFunction, this.counter.Value);

						return Code.ToString("D" + this.nrDigits.ToString());

					case CredentialAlgorithm.TOTP:
						Code = TotpCalculator.Compute(this.NrDigits, this.Secret,
							this.hashFunction ?? HotpCalculator.DefaultHashFunction,
							this.TimeStepSeconds, DateTime.UtcNow, TotpCalculator.DefaultT0);

						return Code.ToString("D" + this.nrDigits.ToString());

					case CredentialAlgorithm.UTF8:
						return Encoding.UTF8.GetString(this.Secret);

					default:
						throw new InvalidOperationException("Unknown credential algorithm type.");
				}
			}
		}

		/// <summary>
		/// When current pass code changes.
		/// </summary>
		public TimeSpan? Next
		{
			get
			{
				switch (this.Type)
				{
					case CredentialAlgorithm.TOTP:
						if (!this.timeStepSeconds.HasValue || !this.timeStepSeconds.HasValue)
							return null;

						DateTime Now = DateTime.UtcNow;
						long Counter = TotpCalculator.CalcCounter(Now, this.timeStepSeconds.Value, TotpCalculator.DefaultT0);
						DateTime Next = TotpCalculator.UnixEpoch.AddSeconds(((Counter + 1) * this.timeStepSeconds.Value) + TotpCalculator.DefaultT0);

						return Next.Subtract(Now);

					default:
						return null;
				}
			}
		}

		/// <summary>
		/// Label for when current pass code changes.
		/// </summary>
		public string NextLabel
		{
			get
			{
				TimeSpan? Next = this.Next;
				if (Next.HasValue)
					return Math.Ceiling(Next.Value.TotalSeconds).ToString() + " s";
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Creates a credential.
		/// </summary>
		/// <returns>Credential object.</returns>
		public static Task<ExternalCredential> CreateAsync(string OtpAuthUri)
		{
			ExternalCredential Credential = TryParse(OtpAuthUri)
				?? throw new ArgumentException("Invalid OTP Auth URI.", nameof(OtpAuthUri));

			return CreateAsync(Credential, Credential.Type, Credential.Endpoint,
				Credential.HashFunction, Credential.Secret, Credential.Issuer,
				Credential.Account, Credential.Label, Credential.Description,
				Credential.Counter, Credential.NrDigits, Credential.TimeStepSeconds);
		}

		/// <summary>
		/// Creates a credential.
		/// </summary>
		/// <param name="Parsed">Parsed object.</param>
		/// <param name="Type">Type of credential</param>
		/// <param name="EndPoint">Name of endpoint</param>
		/// <param name="HashFunction">Hash algorithm to use (if any).</param>
		/// <param name="Secret">Secret</param>
		/// <param name="Issuer">Issuer</param>
		/// <param name="Account">Account</param>
		/// <param name="Label">Label</param>
		/// <param name="Description">Description</param>
		/// <param name="Counter">Counter, if any.</param>
		/// <param name="NrDigits">Number of digits, if any.</param>
		/// <param name="TimeStepSeconds">Time step, in seconds, if any.</param>
		/// <returns>Credential object.</returns>
		public static async Task<ExternalCredential> CreateAsync(ExternalCredential Parsed,
			CredentialAlgorithm Type, string EndPoint, HashFunction? HashFunction,
			byte[] Secret, string Issuer, string Account, string Label, string Description,
			long? Counter, int? NrDigits, int? TimeStepSeconds)
		{
			int i;

			if (string.IsNullOrEmpty(Label))
				Label = Issuer + ":" + Account;
			else if (string.IsNullOrEmpty(Issuer) && string.IsNullOrEmpty(Account))
			{
				i = Label.IndexOf(':');
				if (i > 0)
				{
					Issuer = Label[..i];
					Account = Label[(i + 1)..];
				}
				else
					Issuer = Label;
			}

			if (string.IsNullOrEmpty(EndPoint))
				EndPoint = Label;

			using Semaphore CredentialLock = await Semaphores.BeginWrite("External Credentials");

			string Suffix = string.Empty;
			i = 1;
			ExternalCredential Result = await Database.FindFirstIgnoreRest<ExternalCredential>(new FilterAnd(
				new FilterFieldEqualTo(nameof(Endpoint), EndPoint + Suffix),
				new FilterFieldEqualTo(nameof(Type), Type)));

			while (!(Result is null))
			{
				i++;
				Suffix = " (" + i.ToString() + ")";

				Result = await Database.FindFirstIgnoreRest<ExternalCredential>(new FilterAnd(
					new FilterFieldEqualTo(nameof(Endpoint), EndPoint + Suffix),
					new FilterFieldEqualTo(nameof(Type), Type)));
			}

			if (Parsed is null)
			{
				Result = new ExternalCredential()
				{
					Endpoint = EndPoint + Suffix,
					Type = Type,
					HashFunction = HashFunction,
					Secret = Secret,
					Issuer = Issuer,
					Account = Account,
					Label = Label,
					Description = Description,
					Counter = Counter
				};
			}
			else
			{
				Result = Parsed;

				Result.Endpoint = EndPoint + Suffix;
				Result.Type = Type;
				Result.hashFunction = HashFunction;
				Result.secret = Secret;
				Result.issuer = Issuer;
				Result.account = Account;
				Result.label = Label;
				Result.description = Description;
				Result.counter = Counter;
			}

			if (NrDigits.HasValue)
				Result.nrDigits = NrDigits.Value;

			if (TimeStepSeconds.HasValue)
				Result.timeStepSeconds = TimeStepSeconds.Value;

			await Database.Insert(Result);

			return Result;
		}

		/// <summary>
		/// Exports credentials to a password-protected ZIP file.
		/// </summary>
		/// <param name="Password">Password to use to protect file.</param>
		public static Task<ZipFile> ExportAsync(string Password)
		{
			return ExportAsync(null, Password);
		}

		/// <summary>
		/// Exports credentials to a password-protected ZIP file.
		/// </summary>
		/// <param name="CredentialFileName">File name of the credentials file inside the ZIP file.</param>
		/// <param name="Password">Password to use to protect file.</param>
		public static async Task<ZipFile> ExportAsync(string CredentialFileName, 
			string Password)
		{
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings Settings = XML.WriterSettings(true, false);
			Settings.Encoding = Encoding.UTF8;

			using XmlWriter w = XmlWriter.Create(sb, Settings);

			w.WriteStartDocument();
			w.WriteStartElement("Credentials", "http://waher.se/schema/Credentials.xsd");

			foreach (ExternalCredential Credential in await GetCredentials())
			{
				w.WriteStartElement("Credential");
				w.WriteAttributeString("type", Credential.Type.ToString());
				w.WriteAttributeString("endpoint", Credential.Endpoint);
				w.WriteAttributeString("label", Credential.Label);
				w.WriteAttributeString("description", Credential.Description);
				w.WriteAttributeString("issuer", Credential.Issuer);
				w.WriteAttributeString("account", Credential.Account);

				if (Credential.hashFunction.HasValue)
					w.WriteAttributeString("hashFunction", Credential.hashFunction.Value.ToString());

				if (Credential.nrDigits.HasValue)
					w.WriteAttributeString("nrDigits", Credential.nrDigits.Value.ToString());

				if (Credential.counter.HasValue)
					w.WriteAttributeString("counter", Credential.counter.Value.ToString());

				if (Credential.timeStepSeconds.HasValue)
					w.WriteAttributeString("timeStepSeconds", Credential.timeStepSeconds.Value.ToString());

				w.WriteAttributeString("secret", Convert.ToBase64String(Credential.Secret));
				w.WriteEndElement();
			}

			w.WriteEndElement();
			w.WriteEndDocument();
			w.Flush();

			if (string.IsNullOrEmpty(CredentialFileName))
				CredentialFileName = "Credentials.xml";

			string Xml = sb.ToString();
			byte[] Bin = Encoding.UTF8.GetBytes(Xml);
			byte[] Archive = await Zip.CreateZipFile(CredentialFileName, Bin, Password,
				ZipEncryption.Aes256Ae2);

			return new ZipFile(Archive);
		}
	}
}