using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Xml.Model;

namespace Waher.Script.Xml
{
	/// <summary>
	/// Parses an XML document
	/// </summary>
	public class XmlParser : IKeyWord
	{
		/// <summary>
		/// Parses an XML document
		/// </summary>
		public XmlParser()
		{
		}

		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		public string KeyWord => "<";

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		public string[] InternalKeywords => new string[0];

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		public bool TryParse(ScriptParser Parser, out ScriptNode Result)
		{
			Result = this.ParseDocument(Parser);
			return !(Result is null);
		}

		private XmlScriptDocument ParseDocument(ScriptParser Parser)
		{
			List<XmlScriptProcessingInstruction> ProcessingInstructions = null;
			int Start = Parser.Start;
			int Pos;
			char ch;

			while (Parser.PeekNextChar() == '?')
			{
				Parser.NextChar();
				Pos = Parser.Position;

				while ((ch = Parser.NextChar()) != '?' && ch != 0)
					;

				if (ch == 0)
					return null;

				if (ProcessingInstructions is null)
					ProcessingInstructions = new List<XmlScriptProcessingInstruction>();

				string Text = Parser.Expression.Script.Substring(Pos, Parser.Position - Pos - 1);

				if (Parser.NextChar() != '>')
					throw Parser.SyntaxError("> expected.");

				ProcessingInstructions.Add(new XmlScriptProcessingInstruction(Text,
					Start, Parser.Position - Start, Parser.Expression));

				Parser.SkipWhiteSpace();

				if (Parser.NextChar() != '<')
					return null;

				Start = Parser.Position;
			}

			Parser.UndoChar();
			XmlScriptElement Root = this.ParseElement(Parser);
			if (Root is null)
				return null;

			return new XmlScriptDocument(Root, ProcessingInstructions?.ToArray() ?? new XmlScriptProcessingInstruction[0],
				Start, Parser.Position - Start, Parser.Expression);
		}

		private XmlScriptElement ParseElement(ScriptParser Parser)
		{
			if (Parser.NextChar() != '<')
				throw Parser.SyntaxError("< expected.");

			List<XmlScriptAttribute> Attributes = null;
			IElement ElementValue;
			int ElementStart = Parser.Position;
			string ElementName = ParseName(Parser);
			string Value;
			char ch;

			Parser.SkipWhiteSpace();

			while ((ch = Parser.PeekNextChar()) != 0)
			{
				if (ch == '>')
				{
					Parser.NextChar();

					XmlScriptElement Element = new XmlScriptElement(ElementName, Attributes?.ToArray() ?? new XmlScriptAttribute[0],
						ElementStart, Parser.Position - ElementStart, Parser.Expression);

					while (true)
					{
						int Pos = Parser.Position;
						int i = Parser.Expression.Script.IndexOf('<', Pos);
						int Len;

						if (i < 0)
							throw Parser.SyntaxError("Open element.");

						if (i > Pos)
						{
							Len = i - Pos;

							Element.Add(new XmlScriptText(Parser.Expression.Script.Substring(Pos, Len),
								Pos, Len, Parser.Expression));

							Parser.SkipChars(Len);
						}

						switch (Parser.PeekNextChars(2))
						{
							case "</":
								Parser.NextChar();
								Parser.NextChar();

								if (ParseName(Parser) != ElementName)
									throw Parser.SyntaxError("Expected end of element " + ElementName);

								if (Parser.NextChar() != '>')
									throw Parser.SyntaxError("> expected.");

								return Element;

							case "<!":
								if (Parser.PeekNextChars(4) == "<!--")
								{
									Parser.SkipChars(4);

									Pos = Parser.Position;
									i = Parser.Expression.Script.IndexOf("-->", Pos);
									if (i < 0)
										throw Parser.SyntaxError("Unterminated comment.");

									Len = i - Pos;
									Element.Add(new XmlScriptComment(Parser.Expression.Script.Substring(Pos, Len),
										Pos, Len, Parser.Expression));

									Parser.SkipChars(Len + 3);
								}
								else if (Parser.PeekNextChars(9) == "<![CDATA[")
								{
									Parser.SkipChars(9);

									Pos = Parser.Position;
									i = Parser.Expression.Script.IndexOf("]]>", Pos);
									if (i < 0)
										throw Parser.SyntaxError("Unterminated CDATA construct.");

									Len = i - Pos;
									Element.Add(new XmlScriptCData(Parser.Expression.Script.Substring(Pos, Len),
										Pos, Len, Parser.Expression));

									Parser.SkipChars(Len + 3);
								}
								else
									throw Parser.SyntaxError("Expected <!-- or <![CDATA[");

								break;

							case "<[":
								Parser.SkipChars(2);

								ScriptNode Node = Parser.ParseSequence();
								Parser.SkipWhiteSpace();

								if (Parser.PeekNextChars(2) != "]>")
									throw Parser.SyntaxError("]> expected.");

								Parser.SkipChars(2);

								Element.Add(new XmlScriptValue(Node, Node.Start, Node.Length, Node.Expression));
								break;

							case "<(":
								Parser.SkipChars(2);

								Node = Parser.ParseSequence();
								Parser.SkipWhiteSpace();

								if (Parser.PeekNextChars(2) != ")>")
									throw Parser.SyntaxError(")> expected.");

								Parser.SkipChars(2);

								Element.Add(new XmlScriptValue(Node, Node.Start, Node.Length, Node.Expression));
								break;

							default:
								Element.Add(this.ParseElement(Parser));
								break;
						}
					}
				}
				else if (ch == '/')
				{
					Parser.NextChar();
					if (Parser.NextChar() != '>')
						throw Parser.SyntaxError("> expected.");

					return new XmlScriptElement(ElementName, Attributes?.ToArray() ?? new XmlScriptAttribute[0],
						ElementStart, Parser.Position - ElementStart, Parser.Expression);
				}
				else if (char.IsLetter(ch) || ch == '_' || ch == ':')
				{
					int AttributeStart = Parser.Position;
					string AttributeName = ParseName(Parser);

					Parser.SkipWhiteSpace();
					if (Parser.NextChar() != '=')
						throw Parser.SyntaxError("= expected.");

					bool Bak = Parser.CanSkipWhitespace;
					Parser.CanSkipWhitespace = false;
					ScriptNode Node = Parser.ParsePowers();
					Parser.CanSkipWhitespace = Bak;

					if (Attributes is null)
						Attributes = new List<XmlScriptAttribute>();

					if (Node is ConstantElement Constant)
					{
						ElementValue = Constant.Constant;

						if (ElementValue is StringValue S)
							Value = S.Value;
						else
							Value = Expression.ToString(ElementValue.AssociatedObjectValue);

						Attributes.Add(new XmlScriptAttributeString(AttributeName, Value,
							AttributeStart, Parser.Position - AttributeStart, Parser.Expression));
					}
					else
					{
						Attributes.Add(new XmlScriptAttributeScript(AttributeName, Node,
							AttributeStart, Parser.Position - AttributeStart, Parser.Expression));
					}
				}
				else if (char.IsWhiteSpace(ch))
					Parser.NextChar();
				else
					throw Parser.SyntaxError("Invalid XML Element.");
			}

			return null;
		}

		private static string ParseName(ScriptParser Parser)
		{
			int Pos = Parser.Position;
			char ch = Parser.PeekNextChar();

			if (!(char.IsLetter(ch) || ch == '_' || ch == ':'))
				throw Parser.SyntaxError("Name expected.");

			while (Parser.NextChar() != 0)
			{
				ch = Parser.PeekNextChar();

				if (char.IsLetterOrDigit(ch) || char.IsDigit(ch) ||
					ch == '.' || ch == '-' || ch == '_' || ch == ':')
				{
					continue;
				}

				break;
			}

			return Parser.Expression.Script.Substring(Pos, Parser.Position - Pos);
		}

	}
}
