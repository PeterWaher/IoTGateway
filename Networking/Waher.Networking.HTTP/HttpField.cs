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

		/// <summary>
		/// Parses a set of comma-separated field values, optionaly delimited by ' or " characters.
		/// </summary>
		/// <param name="Value">Field Value</param>
		/// <returns>Parsed set of field values.</returns>
		public static KeyValuePair<string, string>[] ParseFieldValues(string Value)
		{
			List<KeyValuePair<string, string>> Result = new List<KeyValuePair<string, string>>();
			StringBuilder sb = new StringBuilder();
			string Key = null;
			int State = 0;

			foreach (char ch in Value)
			{
				switch (State)
				{
					case 0:	// Waiting for Parameter Name.
						if (ch <= 32)
							break;
						else if (ch == '=')
						{
							State = 2;
							Key = string.Empty;
						}
						else
						{
							State++;
							sb.Append(ch);
						}
						break;

					case 1:	// Parameter
						if (ch == '=')
						{
							Key = sb.ToString().TrimEnd();
							sb.Clear();
							State = 2;
						}
						else
							sb.Append(ch);
						break;

					case 2:	// First character in Value
						if (ch == '"')
							State += 2;
						else if (ch == '\'')
							State += 4;
						else
						{
							sb.Append(ch);
							State++;
						}
						break;

					case 3:	// Normal value
						if (ch == ',' || ch == ';')
						{
							Value = sb.ToString().Trim();
							Result.Add(new KeyValuePair<string, string>(Key, Value));
							sb.Clear();
							State = 0;
						}
						else
							sb.Append(ch);
						break;

					case 4:	// "Value"
						if (ch == '"')
							State--;
						else if (ch == '\\')
							State++;
						else
							sb.Append(ch);
						break;

					case 5:	// Escape
						sb.Append(ch);
						State--;
						break;

					case 6:	// 'Value'
						if (ch == '"')
							State = 3;
						else if (ch == '\\')
							State++;
						else
							sb.Append(ch);
						break;

					case 7:	// Escape
						sb.Append(ch);
						State--;
						break;
				}
			}

			if (State == 3)
			{
				Value = sb.ToString().Trim();
				Result.Add(new KeyValuePair<string, string>(Key, Value));
			}

			return Result.ToArray();
		}
	}
}
