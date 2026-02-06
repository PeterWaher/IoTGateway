namespace Waher.Security
{
	/// <summary>
	/// Basic interface for a users with a Legal Identity.
	/// </summary>
	public interface ILegalIdentityUser
	{
		/// <summary>
		/// Legal Identity of the user.
		/// </summary>
		ILegalIdentity LegalIdentity { get; }
	}
}
