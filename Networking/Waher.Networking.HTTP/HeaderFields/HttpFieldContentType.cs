using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-Type HTTP Field header. (RFC 2616, §14.17)
	/// </summary>
	public class HttpFieldContentType : HttpField
	{
		private string type;
		private string charset;

		/// <summary>
		/// Content-Type HTTP Field header. (RFC 2616, §14.17)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldContentType(string Key, string Value)
			: base(Key, Value)
		{
			this.charset = string.Empty;

			int i = Value.IndexOf(';');
			if (i < 0)
				this.type = Value;
			else
			{
				this.type = Value.Substring(0, i).Trim();
				
				foreach (KeyValuePair<string, string> P in ParseFieldValues(Value.Substring(i + 1).Trim()))
				{
					if (P.Key.ToLower() == "charset")
					{
						this.charset = P.Value;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Content Type
		/// </summary>
		public string Type
		{
			get { return this.type; }
		}

		/// <summary>
		/// Character set name.
		/// </summary>
		public string CharacterSet
		{
			get { return this.charset; }
		}

		/// <summary>
		/// Text encoding.
		/// </summary>
		public Encoding Encoding
		{
			get
			{
				if (string.IsNullOrEmpty(this.charset))
					return Encoding.UTF8;
				else
					return System.Text.Encoding.GetEncoding(this.charset);
			}
		}
	}
}
