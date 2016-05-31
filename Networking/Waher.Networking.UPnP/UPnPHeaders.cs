using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Direction of message
	/// </summary>
	public enum HttpDirection
	{
		/// <summary>
		/// Message is a request message.
		/// </summary>
		Request,

		/// <summary>
		/// Message is a response message.
		/// </summary>
		Response,

		/// <summary>
		/// Unknown HTTP message format.
		/// </summary>
		Unknown
	}

	/// <summary>
	/// Class managing any HTTP headers in a UPnP UDP request/response.
	/// </summary>
	public class UPnPHeaders : IEnumerable<KeyValuePair<string, string>>
	{
		private Dictionary<string, string> headers = new Dictionary<string, string>();
		private HttpDirection direction = HttpDirection.Unknown;
		private string[] rows;
		private string searchTarget = string.Empty;
		private string server = string.Empty;
		private string location = string.Empty;
		private string uniqueServiceName = string.Empty;
		private string verb = string.Empty;
		private string parameter = string.Empty;
		private string responseMessage = string.Empty;
		private string host = string.Empty;
		private string cacheControl = string.Empty;
		private string notificationType = string.Empty;
		private string notificationSubType = string.Empty;
		private double httpVersion = 0;
		private int responseCode = 0;

		internal UPnPHeaders(string Header)
		{
			string s, Key, Value;
			int i, j, c;

			this.rows = Header.Split(CRLF, StringSplitOptions.RemoveEmptyEntries);
			c = this.rows.Length;

			if (c > 0)
			{
				string[] P = this.rows[0].Split(' ');
				if (P.Length == 3 && P[2].StartsWith("HTTP/"))
				{
					this.direction = HttpDirection.Request;
					this.verb = P[0];
					this.parameter = P[1];

					if (!double.TryParse(P[2].Substring(5).Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out this.httpVersion))
						this.httpVersion = 0;
				}
				else if (P.Length >= 3 && P[0].StartsWith("HTTP/"))
				{
					this.direction = HttpDirection.Response;

					if (!double.TryParse(P[0].Substring(5).Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out this.httpVersion))
						this.httpVersion = 0;

					if (!int.TryParse(P[1], out this.responseCode))
						this.responseCode = 0;

					this.responseMessage = null;
					for (i = 2; i < P.Length; i++)
					{
						if (this.responseMessage == null)
							this.responseMessage = P[i];
						else
							this.responseMessage += " " + P[i];
					}
				}
			}

			for (i = 1; i < c; i++)
			{
				s = rows[i];
				j = s.IndexOf(':');

				Key = s.Substring(0, j).ToUpper();
				Value = s.Substring(j + 1).TrimStart();

				this.headers[Key] = Value;

				switch (Key)
				{
					case "ST":
						this.searchTarget = Value;
						break;

					case "SERVER":
						this.server = Value;
						break;

					case "LOCATION":
						this.location = Value;
						break;

					case "USN":
						this.uniqueServiceName = Value;
						break;

					case "HOST":
						this.host = Value;
						break;

					case "CACHE-CONTROL":
						this.cacheControl = Value;
						break;

					case "NT":
						this.notificationType = Value;
						break;

					case "NTS":
						this.notificationSubType = Value;
						break;
				}
			}
		}

		/// <summary>
		/// CR or LF characters.
		/// </summary>
		internal static readonly char[] CRLF = new char[] { '\r', '\n' };
		
		/// <summary>
		/// Gets the value of the corresponding key. If the key is not found, the empty string is returned.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <returns>Value</returns>
		public string this[string Key]
		{
			get
			{
				string Value;

				if (this.headers.TryGetValue(Key, out Value))
					return Value;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Message direction.
		/// </summary>
		public HttpDirection Direction { get { return this.direction; } }

		/// <summary>
		/// HTTP Verb
		/// </summary>
		public string Verb { get { return this.verb; } }

		/// <summary>
		/// HTTP Parameter
		/// </summary>
		public string Parameter { get { return this.parameter; } }

		/// <summary>
		/// HTTP Version
		/// </summary>
		public double HttpVersion { get { return this.httpVersion; } }

		/// <summary>
		/// Search Target header
		/// </summary>
		public string SearchTarget { get { return this.searchTarget; } }

		/// <summary>
		/// Server header
		/// </summary>
		public string Server { get { return this.server; } }

		/// <summary>
		/// Location header
		/// </summary>
		public string Location { get { return this.location; } }

		/// <summary>
		/// Unique Service Name (USN) header
		/// </summary>
		public string UniqueServiceName { get { return this.uniqueServiceName; } }

		/// <summary>
		/// Response message
		/// </summary>
		public string ResponseMessage { get { return this.responseMessage; } }

		/// <summary>
		/// Host
		/// </summary>
		public string Host { get { return this.host; } }

		/// <summary>
		/// Cache Control
		/// </summary>
		public string CacheControl { get { return this.cacheControl; } }

		/// <summary>
		/// Notification Type
		/// </summary>
		public string NotificationType { get { return this.notificationType; } }

		/// <summary>
		/// Notification Sub-type
		/// </summary>
		public string NotificationSubType { get { return this.notificationSubType; } }

		/// <summary>
		/// Response Code
		/// </summary>
		public int ResponseCode { get { return this.responseCode; } }

		/// <summary>
		/// Gets an enumerator, enumerating all headers.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return this.headers.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator, enumerating all headers.
		/// </summary>
		/// <returns>Enumerator</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.headers.GetEnumerator();
		}
	}
}
