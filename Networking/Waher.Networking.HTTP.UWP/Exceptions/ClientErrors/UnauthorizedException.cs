using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Similar to 403 Forbidden, but specifically for use when authentication is required and has failed or has not yet been provided. The response must include a WWW-Authenticate header field containing a challenge applicable to the requested resource. See Basic access authentication and Digest access authentication.[31] 401 semantically means "unauthorised",[32] the user does not have valid authentication credentials for the target resource.
	/// </summary>
	public class UnauthorizedException : HttpException
	{
		/// <summary>
		/// 401
		/// </summary>
		public const int Code = 401;

		/// <summary>
		/// Unauthorized
		/// </summary>
		public const string StatusMessage = "Unauthorized";

		/// <summary>
		/// Similar to 403 Forbidden, but specifically for use when authentication is required and has failed or has not yet been provided. The response must include a WWW-Authenticate header field containing a challenge applicable to the requested resource. See Basic access authentication and Digest access authentication.[31] 401 semantically means "unauthorised",[32] the user does not have valid authentication credentials for the target resource.
		/// </summary>
		/// <param name="Challenges">Challenges to send to client.</param>
		public UnauthorizedException(string[] Challenges)
			: base(Code, StatusMessage, CreateChallengeHeaders(Challenges))
		{
		}

		/// <summary>
		/// Similar to 403 Forbidden, but specifically for use when authentication is required and has failed or has not yet been provided. The response must include a WWW-Authenticate header field containing a challenge applicable to the requested resource. See Basic access authentication and Digest access authentication.[31] 401 semantically means "unauthorised",[32] the user does not have valid authentication credentials for the target resource.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="Challenges">Challenges to send to client.</param>
		public UnauthorizedException(object ContentObject, string[] Challenges)
			: base(Code, StatusMessage, ContentObject, CreateChallengeHeaders(Challenges))
		{
		}

		/// <summary>
		/// Similar to 403 Forbidden, but specifically for use when authentication is required and has failed or has not yet been provided. The response must include a WWW-Authenticate header field containing a challenge applicable to the requested resource. See Basic access authentication and Digest access authentication.[31] 401 semantically means "unauthorised",[32] the user does not have valid authentication credentials for the target resource.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="Challenges">Challenges to send to client.</param>
		public UnauthorizedException(byte[] Content, string ContentType, string[] Challenges)
			: base(Code, StatusMessage, Content, ContentType, CreateChallengeHeaders(Challenges))
		{
		}

		private static KeyValuePair<string, string>[] CreateChallengeHeaders(string[] Challenges)
		{
			int i, c = Challenges.Length;
			KeyValuePair<string, string>[] Headers = new KeyValuePair<string, string>[c];

			for (i = 0; i < c; i++)
				Headers[i] = new KeyValuePair<string, string>("WWW-Authenticate", Challenges[i]);

			return Headers;
		}
	}
}
