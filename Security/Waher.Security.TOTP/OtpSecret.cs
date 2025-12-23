using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Security.CallStack;

namespace Waher.Security.TOTP
{
	/// <summary>
	/// Specifies the type of one-time password (OTP) algorithm to use.
	/// </summary>
	public enum OtpType
	{
		/// <summary>
		/// Counter-based one-time password (HOTP) algorithm (RFC 4226).
		/// </summary>
		HOTP,

		/// <summary>
		/// Time-based one-time password (TOTP) algorithm (RFC 6238).
		/// </summary>
		TOTP
	}

	/// <summary>
	/// Contains OTP secret information for an OTP endpoint.
	/// </summary>
	[CollectionName("OtpSecrets")]
	[TypeName(TypeNameSerialization.None)]
	[ArchivingTime]
	[Index("Endpoint", "Type")]
	public class OtpSecret : IEncryptedProperties
	{
		private static ICallStackCheck[] approvedSources = null;

		private byte[] secret;

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Endpoint
		/// </summary>
		public string Endpoint { get; set; }

		/// <summary>
		/// Optional name of the endpoint.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Label { get; set; }

		/// <summary>
		/// Optional description of the endpoint.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Description { get; set; }

		/// <summary>
		/// OTP Algorithm type.
		/// </summary>
		public OtpType Type { get; set; }

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
		/// Hash function used for the endpoint.
		/// </summary>
		public HashFunction HashFunction { get; set; }

		/// <summary>
		/// Array of properties that are encrypted.
		/// </summary>
		public string[] EncryptedProperties => new string[] { nameof(this.Secret) };

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
		internal static async Task<OtpSecret> GetSecret(string Endpoint, OtpType Type)
		{
			return await Database.FindFirstIgnoreRest<OtpSecret>(new FilterAnd(
				new FilterFieldEqualTo(nameof(Endpoint), Endpoint),
				new FilterFieldEqualTo(nameof(Type), Type)));
		}
	}
}
