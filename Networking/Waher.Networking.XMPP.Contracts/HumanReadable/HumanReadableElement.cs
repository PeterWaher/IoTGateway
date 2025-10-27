﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;
using Waher.Script.Functions.ComplexNumbers;

namespace Waher.Networking.XMPP.Contracts.HumanReadable
{
	/// <summary>
	/// Abstract base class for human readable elements.
	/// </summary>
	public abstract class HumanReadableElement
	{
		private HumanReadableElement parent = null;

		/// <summary>
		/// Parent element.
		/// </summary>
		public HumanReadableElement Parent => this.parent;

		/// <summary>
		/// Registers a parent for a set of elements.
		/// </summary>
		/// <param name="Elements">Elements</param>
		protected void RegisterParent(HumanReadableElement[] Elements)
		{
			if (!(Elements is null))
			{
				foreach (HumanReadableElement E in Elements)
					E.parent = this;
			}
		}

		/// <summary>
		/// Unregisters a parent from a set of elements.
		/// </summary>
		/// <param name="Elements">Elements</param>
		protected void UnregisterParent(HumanReadableElement[] Elements)
		{
			if (!(Elements is null))
			{
				foreach (HumanReadableElement E in Elements)
				{
					if (E.parent == this)
						E.parent = null;
				}
			}
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		/// <returns>Returns first failing element, if found.</returns>
		public abstract Task<HumanReadableElement> IsWellDefined();

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
			if (!(Elements is null))
			{
				foreach (HumanReadableElement E in Elements)
					E.Serialize(Xml);
			}
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
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public Task GenerateMarkdown(MarkdownOutput Markdown, MarkdownSettings Settings)
		{
			return this.GenerateMarkdown(Markdown, 1, 0, Settings);
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public abstract Task GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings);

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
			'*', '_', '~', '\\', '`', '{', '}', '[', ']', '(', ')', '<', '>', '&', '#', '^'
		};

		private static readonly string[] specialCharactersSimplifiedEncoded = new string[]
		{
			"\\*", "\\_", "\\~", "\\\\", "\\`", "\\{", "\\}", "\\[", "\\]", "\\(", "\\)", "\\<", "\\>", "\\&", "\\#", "\\^"
		};

		/// <summary>
		/// Parses XML for a set of block or inline elements (but not both).
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <returns>Array of inline elements</returns>
		public static KeyValuePair<InlineElement[], BlockElement[]> ParseBlockOrInlineChildren(XmlElement Xml)
		{
			List<InlineElement> InlineElements = null;
			List<BlockElement> BlockElements = null;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					InlineElement InlineElement = InlineElement.TryParse(E);
					if (InlineElement is null)
					{
						BlockElement BlockElement = BlockElement.TryParse(E);
						if (BlockElement is null)
							return new KeyValuePair<InlineElement[], BlockElement[]>(null, null);
						else
						{
							BlockElements ??= new List<BlockElement>();

							BlockElements.Add(BlockElement);
						}
					}
					else
					{
						InlineElements ??= new List<InlineElement>();
						InlineElements.Add(InlineElement);
					}
				}
			}

			if (BlockElements is null)
				return new KeyValuePair<InlineElement[], BlockElement[]>(InlineElements?.ToArray(), null);
			else
			{
				if (!(InlineElements is null))
				{
					BlockElements.Add(new Paragraph()
					{
						Elements = InlineElements.ToArray()
					});
				}

				return new KeyValuePair<InlineElement[], BlockElement[]>(null, BlockElements.ToArray());
			}
		}

		/// <summary>
		/// Gets the language of the current element.
		/// </summary>
		/// <param name="Contract">Contract reference.</param>
		/// <returns>Language code, if found.</returns>
		public string GetLanguage(Contract Contract)
		{
			HumanReadableElement Loop = this;

			while (!(Loop is null))
			{
				if (Loop is HumanReadableText Text && !string.IsNullOrEmpty(Text.Language))
					return Text.Language;

				Loop = Loop.parent;
			}

			return Contract?.DefaultLanguage ?? string.Empty;
		}
	}
}
