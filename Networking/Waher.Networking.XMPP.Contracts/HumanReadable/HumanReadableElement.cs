using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable
{
	/// <summary>
	/// Abstract base class for human readable elements.
	/// </summary>
	public abstract class HumanReadableElement
	{
		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public abstract bool IsWellDefined
		{
			get;
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public abstract void Serialize(StringBuilder Xml);

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		/// <param name="Elements">Elements to serialize.</param>
		protected static void Serialize(StringBuilder Xml, IEnumerable<HumanReadableElement> Elements)
		{
			foreach (HumanReadableElement E in Elements)
				E.Serialize(Xml);
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		/// <param name="Elements">Elements to serialize.</param>
		/// <param name="EncapsulatingElementName">Encapsulating element name.</param>
		protected static void Serialize(StringBuilder Xml, IEnumerable<HumanReadableElement> Elements, string EncapsulatingElementName)
		{
			bool First = true;

			Xml.Append('<');
			Xml.Append(EncapsulatingElementName);

			foreach (HumanReadableElement E in Elements)
			{
				if (First)
				{
					First = false;
					Xml.Append('>');
				}

				E.Serialize(Xml);
			}

			if (First)
				Xml.Append("/>");
			else
			{
				Xml.Append("</");
				Xml.Append(EncapsulatingElementName);
				Xml.Append('>');
			}
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		public abstract void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, Contract Contract);
	}
}
