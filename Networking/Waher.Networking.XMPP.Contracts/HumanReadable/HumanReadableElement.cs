using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Markdown;

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

			if (!(Elements is null))
			{
				foreach (HumanReadableElement E in Elements)
				{
					if (First)
					{
						First = false;
						Xml.Append('>');
					}

					E.Serialize(Xml);
				}
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
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public abstract void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, MarkdownSettings Settings);

		/// <summary>
		/// Encodes text to Markdown.
		/// </summary>
		/// <param name="s">Text to encode.</param>
		/// <param name="SimpleEscape">If simplified escape procedures are to be used.</param>
		/// <returns>Encoded text.</returns>
		protected static string MarkdownEncode(string s, bool SimpleEscape)
		{
			if (SimpleEscape)
				return CommonTypes.Escape(s, specialCharactersSimplified, specialCharactersSimplifiedEncoded);
			else
				return MarkdownDocument.Encode(s);
		}

		private static readonly char[] specialCharactersSimplified = new char[]
		{
			'*', '_', '~', '\\', '`', '{', '}', '[', ']', '<', '>', '&', '#', '^'
		};

		private static readonly string[] specialCharactersSimplifiedEncoded = new string[]
		{
			"\\*", "\\_", "\\~", "\\\\", "\\`", "\\{", "\\}", "\\[", "\\]", "\\<", "\\>", "\\&", "\\#", "\\^"
		};

	}
}
