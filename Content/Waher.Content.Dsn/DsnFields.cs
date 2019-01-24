using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Dsn
{
	/// <summary>
	/// Base class for DSN field classes.
	/// </summary>
	public abstract class DsnFields
	{
		private readonly KeyValuePair<string, string>[] other;

		/// <summary>
		/// Base class for DSN field classes.
		/// </summary>
		/// <param name="Rows">Rows</param>
		public DsnFields(string[] Rows)
		{
			List<KeyValuePair<string, string>> Other = new List<KeyValuePair<string, string>>();

			foreach (string Row in Rows)
			{
				int i = Row.IndexOf(':');
				string Key;
				string Value;

				if (i < 0)
				{
					Key = Row.Trim();
					Value = string.Empty;
				}
				else
				{
					Key = Row.Substring(0, i).Trim();
					Value = Row.Substring(i + 1).Trim();
				}

				if (!this.Parse(Key, Value))
					Other.Add(new KeyValuePair<string, string>(Key, Value));
			}

			this.other = Other.ToArray();
		}

		/// <summary>
		/// Parses a field
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		/// <returns>If the key was recognized.</returns>
		protected abstract bool Parse(string Key, string Value);

		/// <summary>
		/// Parses a type value prefixed to the field value.
		/// </summary>
		/// <param name="Value">Field value</param>
		/// <param name="Type">Resulting type.</param>
		/// <returns></returns>
		protected bool ParseType(ref string Value, out string Type)
		{
			int i = Value.IndexOf(';');
			if (i < 0)
			{
				Type = string.Empty;
				return false;
			}
			else
			{
				Type = Value.Substring(0, i).TrimEnd();
				Value = Value.Substring(i + 1).TrimStart();
				return true;
			}
		}
		
		/// <summary>
		/// Other fields.
		/// </summary>
		public KeyValuePair<string, string>[] Other => this.other;
	}
}
