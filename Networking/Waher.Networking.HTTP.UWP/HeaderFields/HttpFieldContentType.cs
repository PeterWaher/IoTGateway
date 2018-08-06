using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-Type HTTP Field header. (RFC 2616, §14.17)
	/// </summary>
	public class HttpFieldContentType : HttpField
	{
		private readonly KeyValuePair<string, string>[] fields;
		private readonly string type;
		private readonly string charset;

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
			{
				this.type = Value;
				this.fields = new KeyValuePair<string, string>[0];
			}
			else
			{
				this.type = Value.Substring(0, i).Trim();
				this.fields = CommonTypes.ParseFieldValues(Value.Substring(i + 1).Trim());

				foreach (KeyValuePair<string, string> P in this.fields)
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

		/// <summary>
		/// Any content-type related fields and their corresponding values.
		/// </summary>
		public KeyValuePair<string, string>[] Fields
		{
			get { return this.fields; }
		}
	}
}
