using System;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Identity Review event arguments.
	/// </summary>
	public class IdentityReviewEventArgs : MessageEventArgs
	{
		private readonly string legalId;
		private InvalidClaim[] invalidClaims;
		private InvalidPhoto[] invalidPhotos;
		private ValidationError[] validationErrors;
		private ValidClaim[] validClaims;
		private ValidPhoto[] validPhotos;
		private string[] unvalidatedClaims;
		private string[] unvalidatedPhotos;

		/// <summary>
		/// Identity Review event arguments.
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="LegalId">Identifier of associated Legal ID.</param>
		public IdentityReviewEventArgs(MessageEventArgs e, string LegalId)
			: this(e, LegalId, null, null, null, null, null, null, null)
		{
		}

		/// <summary>
		/// Identity Review event arguments.
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="LegalId">Identifier of associated Legal ID.</param>
		/// <param name="InvalidClaims">Invalid claims.</param>
		/// <param name="InvalidPhotos">Invalid photos.</param>
		/// <param name="ValidationErrors">Validation errors.</param>
		/// <param name="ValidClaims">Valid claims.</param>
		/// <param name="ValidPhotos">Valid photos.</param>
		/// <param name="UnvalidatedClaims">Unvalidated claims.</param>
		/// <param name="UnvalidatedPhotos">Unvalidated photos.</param>
		public IdentityReviewEventArgs(MessageEventArgs e, string LegalId, 
			InvalidClaim[] InvalidClaims, InvalidPhoto[] InvalidPhotos, 
			ValidationError[] ValidationErrors, ValidClaim[] ValidClaims, 
			ValidPhoto[] ValidPhotos, string[] UnvalidatedClaims,
			string[] UnvalidatedPhotos)
			: base(e)
		{
			this.legalId = LegalId;
			this.invalidClaims = InvalidClaims ?? Array.Empty<InvalidClaim>();
			this.invalidPhotos = InvalidPhotos ?? Array.Empty<InvalidPhoto>();
			this.validationErrors = ValidationErrors ?? Array.Empty<ValidationError>();
			this.validClaims = ValidClaims ?? Array.Empty<ValidClaim>();
			this.validPhotos = ValidPhotos ?? Array.Empty<ValidPhoto>();
			this.unvalidatedClaims = UnvalidatedClaims ?? Array.Empty<string>();
			this.unvalidatedPhotos = UnvalidatedPhotos ?? Array.Empty<string>();
		}

		/// <summary>
		/// Identifier of associated Legal ID.
		/// </summary>
		public string LegalId => this.legalId;

		/// <summary>
		/// Invalid claims.
		/// </summary>
		public InvalidClaim[] InvalidClaims
		{
			get => this.invalidClaims;
			internal set => this.invalidClaims = value ?? Array.Empty<InvalidClaim>();
		}

		/// <summary>
		/// Invalid photos.
		/// </summary>
		public InvalidPhoto[] InvalidPhotos
		{
			get => this.invalidPhotos;
			internal set => this.invalidPhotos = value ?? Array.Empty<InvalidPhoto>();
		}

		/// <summary>
		/// Validation errors.
		/// </summary>
		public ValidationError[] ValidationErrors
		{
			get => this.validationErrors;
			internal set => this.validationErrors = value ?? Array.Empty<ValidationError>();
		}

		/// <summary>
		/// Valid claims.
		/// </summary>
		public ValidClaim[] ValidClaims
		{
			get => this.validClaims;
			internal set => this.validClaims = value ?? Array.Empty<ValidClaim>();
		}

		/// <summary>
		/// Valid photos.
		/// </summary>
		public ValidPhoto[] ValidPhotos
		{
			get => this.validPhotos;
			internal set => this.validPhotos = value ?? Array.Empty<ValidPhoto>();
		}

		/// <summary>
		/// Unvalidated claims.
		/// </summary>
		public string[] UnvalidatedClaims
		{
			get => this.unvalidatedClaims;
			internal set => this.unvalidatedClaims = value ?? Array.Empty<string>();
		}

		/// <summary>
		/// Unvalidated photos.
		/// </summary>
		public string[] UnvalidatedPhotos
		{
			get => this.unvalidatedPhotos;
			internal set => this.unvalidatedPhotos = value ?? Array.Empty<string>();
		}

		/// <summary>
		/// If the application has been validated (true), invalidated (false), or not yet
		/// validated (null).
		/// </summary>
		public bool? IsValid
		{
			get
			{
				if (this.invalidClaims.Length > 0 || this.invalidPhotos.Length > 0)
					return false;

				if (this.validationErrors.Length > 0)
				{
					foreach (ValidationError Error in this.validationErrors)
					{
						if (Error.ErrorType == ValidationErrorType.Client)
							return false;
					}
				}

				if (this.unvalidatedClaims.Length > 0 ||
					this.unvalidatedPhotos.Length > 0 ||
					this.validationErrors.Length > 0)
				{
					return null;
				}

				return true;
			}
		}

		/// <summary>
		/// If errors are reported.
		/// </summary>
		public bool HasErrors => this.validationErrors.Length > 0;

		/// <summary>
		/// Number of errors reported.
		/// </summary>
		public int NrErrors => this.validationErrors.Length;

		/// <summary>
		/// If the application has validated claims.
		/// </summary>
		public bool HasValidatedClaims => this.validClaims.Length > 0;

		/// <summary>
		/// If the application has validated photos.
		/// </summary>
		public bool HasValidatedPhotos => this.validPhotos.Length > 0;
	}
}
