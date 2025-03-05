using System.Collections.Generic;
using System.Text;
using Waher.Events;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
	/// </summary>
	public class MethodNotAllowedException : HttpException, IEventTags
	{
		/// <summary>
		/// 405
		/// </summary>
		public const int Code = 405;

		/// <summary>
		/// Method Not Allowed
		/// </summary>
		public const string StatusMessage = "Method Not Allowed";

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="AllowedMethods">Allowed methods.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MethodNotAllowedException(string[] AllowedMethods, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields.Join(new KeyValuePair<string, string>("Allow", Join(AllowedMethods))))
		{
		}

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="AllowedMethods">Allowed methods.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MethodNotAllowedException(string[] AllowedMethods, object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields.Join(new KeyValuePair<string, string>("Allow", Join(AllowedMethods))))
		{
		}

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="AllowedMethods">Allowed methods.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MethodNotAllowedException(string[] AllowedMethods, byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields.Join(new KeyValuePair<string, string>("Allow", Join(AllowedMethods))))
		{
		}

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="AllowedMethods">Allowed methods.</param>
		/// <param name="Request">Request object.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MethodNotAllowedException(string[] AllowedMethods, HttpRequest Request, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Request, HeaderFields.Join(new KeyValuePair<string, string>("Allow", Join(AllowedMethods))))
		{
		}

		/// <summary>
		/// Method
		/// </summary>
		public string Method => this.Request.Header.Method;

		/// <summary>
		/// Tags related to the object.
		/// </summary>
		public KeyValuePair<string, object>[] Tags => new KeyValuePair<string, object>[]
		{
			new KeyValuePair<string, object>("Method", this.Method)
		};

		private static string Join(string[] Methods)
		{
			StringBuilder Output = null;

			foreach (string Method in Methods)
			{
				if (Output is null)
					Output = new StringBuilder();
				else
					Output.Append(", ");

				Output.Append(Method);
			}

			if (Output is null)
				return string.Empty;
			else
				return Output.ToString();
		}
	}
}
