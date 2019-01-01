using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Descriptive text
	/// </summary>
	public class TXT : ResourceRecord
	{
		private readonly string[] text;

		/// <summary>
		/// Descriptive text
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Text">Descriptive text.</param>
		public TXT(string Name, TYPE Type, CLASS Class, uint Ttl, string[] Text)
			: base(Name, Type, Class, Ttl)
		{
			this.text = Text;
		}

		/// <summary>
		/// Descriptive text.
		/// </summary>
		public string[] Text => this.text;

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(base.ToString());

			foreach (string s in this.text)
			{
				sb.Append('\t');
				sb.Append(s);
			}

			return sb.ToString();
		}

	}
}
