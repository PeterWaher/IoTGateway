using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.HTTP.HeaderFields;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Contains information about all fields in an HTTP header.
	/// </summary>
	public abstract class HttpHeader : ICollection<HttpField>
	{
		/// <summary>
		/// HTTP fields.
		/// </summary>
		protected Dictionary<string, HttpField> fields = new Dictionary<string, HttpField>();

		private HttpFieldContentEncoding contentEncoding = null;
		private HttpFieldContentLanguage contentLanguage = null;
		private HttpFieldContentLength contentLength = null;
		private HttpFieldContentLocation contentLocation = null;
		private HttpFieldContentMD5 contentMD5 = null;
		private HttpFieldContentRange contentRange = null;
		private HttpFieldContentType contentType = null;
		private HttpFieldTransferEncoding transferEncoding = null;
		private HttpFieldVia via = null;

		/// <summary>
		/// Contains information about all fields in an HTTP header.
		/// </summary>
		/// <param name="Header">HTTP Header.</param>
		public HttpHeader(string Header)
		{
			HttpField Field;
			string Key;
			string KeyLower;
			string Value;
			int i;
			bool First = true;

			foreach (string Row in Header.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
			{
				if (First)
				{
					First = false;
					this.ParseFirstRow(Row);
				}
				else
				{
					i = Row.IndexOf(':');
					if (i < 0)
						continue;

					Key = Row.Substring(0, i).Trim();
					Value = Row.Substring(i + 1).Trim();

					Field = this.ParseField(KeyLower = Key.ToLower(), Key, Value);
					this.fields[KeyLower] = Field;
				}
			}
		}

		/// <summary>
		/// Contains information about all fields in an HTTP header.
		/// </summary>
		/// <param name="FirstRow">First row.</param>
		///	<param name="Headers">Headers.</param>
		public HttpHeader(string FirstRow, params KeyValuePair<string, string>[] Headers)
		{
			HttpField Field;
			string KeyLower;

			this.ParseFirstRow(FirstRow);
			foreach (KeyValuePair<string, string> P in Headers)
			{
				Field = this.ParseField(KeyLower = P.Key.ToLower(), P.Key, P.Value);
				this.fields[KeyLower] = Field;
			}
		}

		/// <summary>
		/// Parses the first row of an HTTP header.
		/// </summary>
		/// <param name="Row">First row.</param>
		protected abstract void ParseFirstRow(string Row);

		/// <summary>
		/// Access to individual fields.
		/// </summary>
		/// <param name="Key">HTTP header field name.</param>
		/// <returns>Header value, if found, or the empty string, if not found.</returns>
		public string this[string Key]
		{
			get
			{
				if (this.fields.TryGetValue(Key.ToLower(), out HttpField Field))
					return Field.Value;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Parses a specific HTTP header field.
		/// </summary>
		/// <param name="KeyLower">Lower-case version of field name.</param>
		/// <param name="Key">Field name, as it appears in the header.</param>
		/// <param name="Value">Unparsed header field value</param>
		/// <returns>HTTP header field object, corresponding to the particular field.</returns>
		protected virtual HttpField ParseField(string KeyLower, string Key, string Value)
		{
			switch (KeyLower)
			{
				case "content-encoding": return this.contentEncoding = new HttpFieldContentEncoding(Key, Value);
				case "content-language": return this.contentLanguage = new HttpFieldContentLanguage(Key, Value);
				case "content-length": return this.contentLength = new HttpFieldContentLength(Key, Value);
				case "content-location": return this.contentLocation = new HttpFieldContentLocation(Key, Value);
				case "content-md5": return this.contentMD5 = new HttpFieldContentMD5(Key, Value);
				case "content-range": return this.contentRange = new HttpFieldContentRange(Key, Value);
				case "content-type": return this.contentType = new HttpFieldContentType(Key, Value);
				case "transfer-encoding": return this.transferEncoding = new HttpFieldTransferEncoding(Key, Value);
				case "via": return this.via = new HttpFieldVia(Key, Value);
				default: return new HttpField(Key, Value);
			}
		}

		#region ICollection<HttpField>

		/// <summary>
		/// Adds an HTTP field
		/// </summary>
		public void Add(HttpField item)
		{
			this.fields[item.Key.ToLower()] = item;
		}

		/// <summary>
		/// Clears the collection of HTTP fields.
		/// </summary>
		public void Clear()
		{
			this.fields.Clear();
		}

		/// <summary>
		/// Checks if the header contains a field having the same Key and Value properties as <paramref name="item"/>.
		/// </summary>
		/// <param name="item">HTTP header field.</param>
		/// <returns>If a similar HTTP header field exists in the collection.</returns>
		public bool Contains(HttpField item)
		{
			return (this.fields.TryGetValue(item.Key.ToLower(), out HttpField Field) && Field.Value == item.Value);
		}

		/// <summary>
		/// Copies the collection of HTTP header fields to an array.
		/// </summary>
		/// <param name="array">Array to copy to.</param>
		/// <param name="arrayIndex">Offset into array where first element is copied to.</param>
		public void CopyTo(HttpField[] array, int arrayIndex)
		{
			this.fields.Values.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Number of HTTP fields in the collection.
		/// </summary>
		public int Count
		{
			get { return this.fields.Count; }
		}

		/// <summary>
		/// If the collection is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Removes a field having the same Key and Value properties as <paramref name="item"/>.
		/// </summary>
		/// <param name="item">HTTP header field.</param>
		/// <returns>If such a field was found and removed.</returns>
		public bool Remove(HttpField item)
		{
			string Key;

			if (this.fields.TryGetValue(Key = item.Key.ToLower(), out HttpField Field) && Field.Value == item.Value)
				return this.fields.Remove(Key);
			else
				return false;
		}

		/// <summary>
		/// Gets an enumerator of available HTTP header fields.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<HttpField> GetEnumerator()
		{
			return this.fields.Values.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of available HTTP header fields.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.fields.Values.GetEnumerator();
		}

		#endregion

		/// <summary>
		/// Content-Encoding HTTP Field header. (RFC 2616, §14.11)
		/// </summary>
		public HttpFieldContentEncoding ContentEncoding { get { return this.contentEncoding; } }

		/// <summary>
		/// Content-Language HTTP Field header. (RFC 2616, §14.12)
		/// </summary>
		public HttpFieldContentLanguage ContentLanguage { get { return this.contentLanguage; } }

		/// <summary>
		/// Content-Length HTTP Field header. (RFC 2616, §14.13)
		/// </summary>
		public HttpFieldContentLength ContentLength { get { return this.contentLength; } }

		/// <summary>
		/// Content-Location HTTP Field header. (RFC 2616, §14.14)
		/// </summary>
		public HttpFieldContentLocation ContentLocation { get { return this.contentLocation; } }

		/// <summary>
		/// Content-MD5 HTTP Field header. (RFC 2616, §14.15)
		/// </summary>
		public HttpFieldContentMD5 ContentMD5 { get { return this.contentMD5; } }

		/// <summary>
		/// Content-Range HTTP Field header. (RFC 2616, §14.16)
		/// </summary>
		public HttpFieldContentRange ContentRange { get { return this.contentRange; } }

		/// <summary>
		/// Content-Type HTTP Field header. (RFC 2616, §14.17)
		/// </summary>
		public HttpFieldContentType ContentType { get { return this.contentType; } }

		/// <summary>
		/// Transfer-Encoding HTTP Field header. (RFC 2616, §14.41)
		/// </summary>
		public HttpFieldTransferEncoding TransferEncoding { get { return this.transferEncoding; } }

		/// <summary>
		/// Via HTTP Field header. (RFC 2616, §14.45)
		/// </summary>
		public HttpFieldVia Via { get { return this.via; } }

		/// <summary>
		/// If the message contains, apart from the header, a message body also.
		/// </summary>
		public abstract bool HasMessageBody
		{
			get;
		}

	}
}
