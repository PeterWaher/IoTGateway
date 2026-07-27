namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// Type of OAuth token.
	/// </summary>
	public enum OAuthTokenType
	{
		/// <summary>
		/// Access token.
		/// </summary>
		AccessToken,

		/// <summary>
		/// Access token that has expired or been deprectated.
		/// </summary>
		ExpiredAccessToken,

		/// <summary>
		/// Access code, used to obtain an access token.
		/// </summary>
		AccessCode,

		/// <summary>
		/// Refresh token.
		/// </summary>
		RefreshToken,

		/// <summary>
		/// Expired refresh token.
		/// </summary>
		ExpiredRefreshToken
	}
}
