using System.Collections.Generic;
using Waher.Events;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated. 
	/// If the request method was not HEAD and the server wishes to make public why the request has not been fulfilled, it SHOULD describe the 
	/// reason for the refusal in the entity. If the server does not wish to make this information available to the client, the status code 404 
	/// (Not Found) can be used instead. 
	/// </summary>
	public class ForbiddenException : HttpException
	{
		/// <summary>
		/// 403
		/// </summary>
		public const int Code = 403;

		/// <summary>
		/// Forbidden
		/// </summary>
		public const string StatusMessage = "Forbidden";

		/// <summary>
		/// The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated. 
		/// If the request method was not HEAD and the server wishes to make public why the request has not been fulfilled, it SHOULD describe the 
		/// reason for the refusal in the entity. If the server does not wish to make this information available to the client, the status code 404 
		/// (Not Found) can be used instead. 
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public ForbiddenException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated. 
		/// If the request method was not HEAD and the server wishes to make public why the request has not been fulfilled, it SHOULD describe the 
		/// reason for the refusal in the entity. If the server does not wish to make this information available to the client, the status code 404 
		/// (Not Found) can be used instead. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public ForbiddenException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated. 
		/// If the request method was not HEAD and the server wishes to make public why the request has not been fulfilled, it SHOULD describe the 
		/// reason for the refusal in the entity. If the server does not wish to make this information available to the client, the status code 404 
		/// (Not Found) can be used instead. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public ForbiddenException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}

		/// <summary>
		/// Returns a <see cref="ForbiddenException"/> object, and logs a entry in the event log about the event.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="ActorId">Actor ID</param>
		/// <param name="MissingPrivilege">Missing Privilege.</param>
		/// <returns>Exception object.</returns>
		public static ForbiddenException AccessDenied(string ObjectId, string ActorId, string MissingPrivilege)
		{
			return AccessDenied("Access denied. Missing privileges.", ObjectId, ActorId, MissingPrivilege);
		}

		/// <summary>
		/// Returns a <see cref="ForbiddenException"/> object, and logs a entry in the event log about the event.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="ActorId">Actor ID</param>
		/// <param name="MissingPrivilege">Missing Privilege.</param>
		/// <returns>Exception object.</returns>
		public static ForbiddenException AccessDenied(string Message, string ObjectId, string ActorId, string MissingPrivilege)
		{
			Log.Notice(Message, ObjectId, ActorId, "AccessDenied",
				new KeyValuePair<string, object>("MissingPrivilege", MissingPrivilege));

			return new ForbiddenException("Access denied. Missing privileges.");
		}
	}
}
