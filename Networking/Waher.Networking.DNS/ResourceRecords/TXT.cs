using System;
using System.Collections.Generic;
using System.IO;
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
		/// <param name="Data">RR-specific binary data.</param>
		/// <param name="EndPos">End position of record.</param>
		public TXT(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data, long EndPos)
			: base(Name, Type, Class, Ttl)
		{
			List<string> Text = new List<string>();

			while (Data.Position < EndPos)
				Text.Add(DnsResolver.ReadString(Data));

			this.text = Text.ToArray();
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
