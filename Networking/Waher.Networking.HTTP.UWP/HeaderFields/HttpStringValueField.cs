
using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Base class for HTTP fields that take a single string value.
	/// </summary>
	public abstract class HttpStringValueField : HttpField
	{
		/// <summary>
		/// Base class for HTTP fields that take a single string value.
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpStringValueField(string Key, string Value)
			: base(Key, RemoveQuotes(Value))
		{
		}

		private static string RemoveQuotes(string Value)
		{
			int c = Value.Length;

			if (c < 2)
				return Value;
			else
			{
				char ch1 = Value[0];
				char ch2 = Value[c - 1];

				if ((ch1 == '"' && ch2 == '"') || (ch1 == '\'' && ch2 == '\''))
					return Value.Substring(1, c - 2);
				else
					return Value;
			}
		}
	}
}
