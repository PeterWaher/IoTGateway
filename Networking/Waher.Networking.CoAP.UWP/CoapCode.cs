using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP Codes, as defined in §12.1 of RFC 7252:
	/// https://tools.ietf.org/html/rfc7252#section-12.1
	/// </summary>
	public enum CoapCode
	{
		/// <summary>
		/// 0.00
		/// </summary>
		EmptyMessage = 0,

		/// <summary>
		/// 0.01
		/// </summary>
		GET = 1,

		/// <summary>
		/// 0.02
		/// </summary>
		POST = 2,

		/// <summary>
		/// 0.03
		/// </summary>
		PUT = 3,

		/// <summary>
		/// 0.04
		/// </summary>
		DELETE = 4,

		/// <summary>
		/// 0.05
		/// 
		/// Defined in: https://datatracker.ietf.org/doc/draft-ietf-core-etch/
		/// </summary>
		FETCH = 5,

		/// <summary>
		/// 0.06
		/// 
		/// Defined in: https://datatracker.ietf.org/doc/draft-ietf-core-etch/
		/// </summary>
		PATCH = 6,

		/// <summary>
		/// 0.07
		/// 
		/// Defined in: https://datatracker.ietf.org/doc/draft-ietf-core-etch/
		/// </summary>
		iPATCH = 7,

		/// <summary>
		/// 2.01
		/// </summary>
		Created = 64 + 1,

		/// <summary>
		/// 2.02
		/// </summary>
		Deleted = 64 + 2,

		/// <summary>
		/// 2.03
		/// </summary>
		Valid = 64 + 3,

		/// <summary>
		/// 2.04
		/// </summary>
		Changed = 64 + 4,

		/// <summary>
		/// 2.05
		/// </summary>
		Content = 64 + 5,

		/// <summary>
		/// 2.31
		/// 
		/// Defined in RFC 7959:
		/// https://tools.ietf.org/html/rfc7959
		/// </summary>
		Continue = 64 + 31,

		/// <summary>
		/// 4.00
		/// </summary>
		BadRequest = 128 + 0,

		/// <summary>
		/// 4.01
		/// </summary>
		Unauthorized = 128 + 1,

		/// <summary>
		/// 4.02
		/// </summary>
		BadOption = 128 + 2,

		/// <summary>
		/// 4.03
		/// </summary>
		Forbidden = 128 + 3,

		/// <summary>
		/// 4.04
		/// </summary>
		NotFound = 128 + 4,

		/// <summary>
		/// 4.05
		/// </summary>
		MethodNotAllowed = 128 + 5,

		/// <summary>
		/// 4.06
		/// </summary>
		NotAcceptable = 128 + 6,

		/// <summary>
		/// 4.08
		/// 
		/// Defined in RFC 7959:
		/// https://tools.ietf.org/html/rfc7959
		/// </summary>
		RequestEntityIncomplete = 128 + 8,

		/// <summary>
		/// 4.09
		/// 
		/// Defined in:
		/// https://datatracker.ietf.org/doc/draft-ietf-core-etch/
		/// </summary>
		Conflict = 128 + 9,

		/// <summary>
		/// 4.12
		/// </summary>
		PreconditionFailed = 128 + 12,

		/// <summary>
		/// 4.13
		/// </summary>
		RequestEntityTooLarge = 128 + 13,

		/// <summary>
		/// 4.15
		/// </summary>
		UnsupportedContentFormat = 128 + 15,

		/// <summary>
		/// 4.22
		/// 
		/// Defined in:
		/// https://datatracker.ietf.org/doc/draft-ietf-core-etch/
		/// </summary>
		UnprocessableEntity = 128 + 22,

		/// <summary>
		/// 5.00
		/// </summary>
		InternalServerError = 160 + 0,

		/// <summary>
		/// 5.01
		/// </summary>
		NotImplemented = 160 + 1,

		/// <summary>
		/// 5.02
		/// </summary>
		BadGateway = 160 + 2,

		/// <summary>
		/// 5.03
		/// </summary>
		ServiceUnavailable = 160 + 3,

		/// <summary>
		/// 5.04
		/// </summary>
		GatewayTimeout = 160 + 4,

		/// <summary>
		/// 5.05
		/// </summary>
		ProxyingNotSupported = 160 + 5
	}
}
