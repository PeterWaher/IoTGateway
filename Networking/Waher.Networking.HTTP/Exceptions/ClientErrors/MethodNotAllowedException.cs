using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
	/// </summary>
	public class MethodNotAllowedException : HttpException
	{
		private const int Code = 405;
		private const string Msg = "Method Not Allowed";

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="AllowedMethods">Allowed methods.</param>
		public MethodNotAllowedException(string[] AllowedMethods)
			: base(Code, Msg, new KeyValuePair<string, string>("Allow", Join(AllowedMethods)))
		{
		}

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="AllowedMethods">Allowed methods.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public MethodNotAllowedException(string[] AllowedMethods, object ContentObject)
			: base(Code, Msg, ContentObject, new KeyValuePair<string, string>("Allow", Join(AllowedMethods)))
		{
		}

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="AllowedMethods">Allowed methods.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public MethodNotAllowedException(string[] AllowedMethods, byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType, new KeyValuePair<string, string>("Allow", Join(AllowedMethods)))
		{
		}

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
