using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.StreamErrors
{
	/// <summary>
	/// The server either (1) is closing the existing stream for this entity because a new stream has been initiated that conflicts with the
	/// existing stream, or (2) is refusing a new stream for this entity because allowing the new stream would conflict with an existing
	/// stream (e.g., because the server allows only a certain number of connections from the same IP address or allows only one server-to-
	/// server stream for a given domain pair as a way of helping to ensure in-order processing as described under Section 10.1).
	/// </summary>
	public class ConflictException : StreamException
	{
		private readonly string[] alternatives;

		/// <summary>
		/// The server either (1) is closing the existing stream for this entity because a new stream has been initiated that conflicts with the
		/// existing stream, or (2) is refusing a new stream for this entity because allowing the new stream would conflict with an existing
		/// stream (e.g., because the server allows only a certain number of connections from the same IP address or allows only one server-to-
		/// server stream for a given domain pair as a way of helping to ensure in-order processing as described under Section 10.1).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ConflictException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Conflict." : Message, Stanza)
		{
			this.alternatives = GetAlternatives(Stanza);
		}

		internal static string[] GetAlternatives(XmlElement Stanza)
		{
			List<string> Alternatives = null;

			foreach (XmlNode N in Stanza.ChildNodes)
			{
				if (N is XmlElement E &&
					E.LocalName == "alternatives" &&
					E.NamespaceURI == XmppClient.AlternativesNamespace)
				{
					foreach (XmlNode N2 in E.ChildNodes)
					{
						if (N2 is XmlElement E2 &&
							E2.LocalName == "alternative" &&
							E2.NamespaceURI == XmppClient.AlternativesNamespace)
						{
							if (Alternatives is null)
								Alternatives = new List<string>();

							Alternatives.Add(E2.InnerText);
						}
					}

					if (Alternatives is null)
						Alternatives = new List<string>();

				}
			}

			return Alternatives?.ToArray();
		}

		/// <summary>
		/// If response contains alternatives.
		/// </summary>
		public bool HasAlternatives => !(this.alternatives is null) && this.alternatives.Length > 0;

		/// <summary>
		/// Alternatives
		/// </summary>
		public string[] Alternatives => this.alternatives;
	}
}
