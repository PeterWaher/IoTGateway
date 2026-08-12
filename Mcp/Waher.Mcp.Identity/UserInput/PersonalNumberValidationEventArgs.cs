using System;

namespace Waher.Mcp.Identity.UserInput
{
	/// <summary>
	/// Event arguments for personal number validation events.
	/// </summary>
	public class PersonalNumberValidationEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for personal number validation events.
		/// </summary>
		/// <param name="PersonalNumber">Personal number provided.</param>
		/// <param name="CountryCode">Country of residence.</param>
		public PersonalNumberValidationEventArgs(string PersonalNumber, string CountryCode)
		{
			this.PersonalNumber = PersonalNumber;
			this.CountryCode = CountryCode;
		}

		/// <summary>
		/// Personal number provided.
		/// </summary>
		public string PersonalNumber { get; }

		/// <summary>
		/// Country of residence.
		/// </summary>
		public string CountryCode { get; }

		/// <summary>
		/// Normalized version of the personal number, if valid. Null otherwise.
		/// </summary>
		public string? NormalizedPersonalNumber { get; set; } = null;

		/// <summary>
		/// If the personal number is valid, in accordance with registered personal 
		/// number schemes.
		/// </summary>
		public bool? IsValid { get; set; } = null;
	}
}
