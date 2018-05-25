using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Represents an ACME identifier.
	/// </summary>
	public class AcmeIdentifier : AcmeObject
	{
		private readonly string type = null;
		private readonly string value = null;

		/// <summary>
		/// Represents an ACME identifier.
		/// </summary>
		/// <param name="Client">ACME Client.</param>
		/// <param name="Type">Type of identifier.</param>
		/// <param name="Value">Identifier value.</param>
		public AcmeIdentifier(AcmeClient Client, string Type, string Value)
			: base(Client)
		{
			this.type = Type;
			this.value = Value;
		}

		internal AcmeIdentifier(AcmeClient Client, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client)
		{
			foreach (KeyValuePair<string, object> P in Obj)
			{
				switch (P.Key)
				{
					case "type":
						this.type = P.Value as string;
						break;

					case "value":
						this.value = P.Value as string;
						break;
				}
			}
		}

		/// <summary>
		/// Type of identifier.
		/// </summary>
		public string Type => this.type;

		/// <summary>
		/// Identifier value.
		/// </summary>
		public string Value => this.value;

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.type + ":" + this.value;
		}
	}
}
