using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Security.CallStack;

namespace Waher.Security.TOTP
{
	/// <summary>
	/// Contains OTP secret information for an OTP endpoint.
	/// </summary>
	[CollectionName("OtpSecrets")]
	[TypeName(TypeNameSerialization.None)]
	[ArchivingTime]
	[Index("Endpoint")]
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
		/// <returns>Secret, if exists, null otherwise.</returns>
		internal static async Task<byte[]> GetSecret(string Endpoint)
		{
			OtpSecret Secret = await Database.FindFirstIgnoreRest<OtpSecret>(
				new FilterFieldEqualTo(nameof(Endpoint), Endpoint));

			return Secret?.Secret;
		}
	}
}
