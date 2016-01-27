using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class for all HTTP fields.
	/// </summary>
	public class HttpField
	{
		private string key;
		private string value;

		/// <summary>
		/// Base class for all HTTP fields.
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpField(string Key, string Value)
		{
			this.key = Key;
			this.value = Value;
		}

		/// <summary>
		/// HTTP Field Name
		/// </summary>
		public string Key
		{
			get { return this.key; }
		}

		/// <summary>
		/// HTTP Field Value
		/// </summary>
		public string Value
		{
			get { return this.value; }
		}

	}
}
