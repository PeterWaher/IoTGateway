using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Toon
{
	/// <summary>
	/// TOON Output builder
	/// </summary>
	public class ToonOutput
	{
		private readonly StringBuilder output = new StringBuilder();
		private char delimiterCharacter = ',';
		private char indentCharacter = '\t';
		private int indentCharacterCount = 1;
		private int keyFoldingDepth = int.MaxValue;
		private bool standardIndentation = true;
		private bool standardDelimiter = true;
		private bool keyFolding = true;
		private bool empty = true;

		/// <summary>
		/// TOON Output builder
		/// </summary>
		public ToonOutput()
			: this(new StringBuilder())
		{
		}

		/// <summary>
		/// TOON Output builder
		/// </summary>
		/// <param name="Output">String builder where TOON output is appended.</param>
		public ToonOutput(StringBuilder Output)
		{
			this.output = Output;
		}

		/// <summary>
		/// Indentation character to use. Default is the TAB character.
		/// </summary>
		public char IndentCharacter
		{
			get => this.indentCharacter;
			set
			{
				this.indentCharacter = value;
				this.standardIndentation =
					this.indentCharacter == '\t' &&
					this.indentCharacterCount == 1;
			}
		}

		/// <summary>
		/// Indentation character to use. Default is the TAB character.
		/// </summary>
		public int IndentCharacterCount
		{
			get => this.indentCharacterCount;
			set
			{
				if (value <= 0)
				{
					throw new ArgumentException("Indent character count must be a positive integer.",
						nameof(this.IndentCharacterCount));
				}

				this.indentCharacterCount = value;
				this.standardIndentation =
					this.indentCharacter == '\t' &&
					this.indentCharacterCount == 1;
			}
		}

		/// <summary>
		/// If the indentation is standard (one TAB character per step).
		/// </summary>
		public bool StandardIndentation => this.standardIndentation;

		/// <summary>
		/// If the standard delimiter is used.
		/// </summary>
		public bool StandardDelimiter => this.standardDelimiter;

		/// <summary>
		/// String builder where TOON output is appended.
		/// </summary>
		public StringBuilder Output => this.output;

		/// <summary>
		/// If no output has been appended.
		/// </summary>
		public bool Empty => this.empty;

		/// <summary>
		/// Delimiter character to use. Default is the comma character.
		/// </summary>
		public char DelimiterCharacter
		{
			get => this.delimiterCharacter;
			set
			{
				this.delimiterCharacter = value;
				this.standardDelimiter = this.delimiterCharacter == ',';
			}
		}

		/// <summary>
		/// If key-folding is enabled (safe-mode). Default is true.
		/// </summary>
		public bool KeyFolding
		{
			get => this.keyFolding;
			set
			{
				this.keyFolding = value;

				if (value && this.keyFoldingDepth <= 1)
					this.keyFoldingDepth = 2;
			}
		}

		/// <summary>
		/// Maximum depth for key folding. Default is <see cref="int.MaxValue"/>, meaning
		/// that all keys will be folded, if key folding is enabled.
		/// </summary>
		public int KeyFoldingDepth
		{
			get => this.keyFoldingDepth;
			set
			{
				if (value < 2)
					this.keyFolding = false;

				this.keyFoldingDepth = value;
			}
		}

		/// <summary>
		/// Appends an indentation to the output, according to the level specified.
		/// </summary>
		/// <param name="Level">Indentation level</param>
		public void Indent(int Level)
		{
			this.empty = false;

			if (this.standardIndentation)
				JSON.Indent(this.output, Level);
			else
			{
				Level *= this.indentCharacterCount;

				while (Level-- > 0)
					this.output.Append(this.indentCharacter);
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.output.ToString();
		}

		/// <summary>
		/// Appends a character to the output.
		/// </summary>
		/// <param name="Value">Character to append.</param>
		public void Append(char Value)
		{
			this.empty = false;
			this.output.Append(Value);
		}

		/// <summary>
		/// Appends a string to the output.
		/// </summary>
		/// <param name="Value">String to append.</param>
		public void Append(string Value)
		{
			this.empty = false;
			this.output.Append(Value);
		}

		/// <summary>
		/// Appends the delimiter character to the output.
		/// </summary>
		public void AppendDelimiter()
		{
			this.empty = false;
			this.output.Append(this.delimiterCharacter);
		}

		/// <summary>
		/// Appends a new-line character to the output.
		/// </summary>
		public void AppendLine()
		{
			this.empty = false;
			this.output.Append('\n');
		}

		/// <summary>
		/// Encodes a string, if necessary, and appends it to the output.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <param name="InObject">If the string is part of an object construct.</param>
		public void AppendEncoded(string s, bool InObject)
		{
			this.Append(this.Encode(s, InObject));
		}

		/// <summary>
		/// Encodes a string for inclusion in TOON.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <param name="InObject">If the string is part of an object construct.</param>
		/// <returns>Encoded string.</returns>
		public string Encode(string s, bool InObject)
		{
			switch (s)
			{
				case null:
				case "":
					return "\"\"";

				case "true":
					return "\"true\"";

				case "false":
					return "\"false\"";

				case "null":
					return "\"null\"";

				case "-":
					return "\"-\"";

				default:
					if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
						return "\"" + s + "\"";

					if (JSON.ContainsEscapeCharacters(s))
						return "\"" + JSON.Encode(s) + "\"";

					if (s.StartsWith("[") ||
						s.StartsWith("{") ||
						s.IndexOf(this.delimiterCharacter) >= 0)
					{
						return "\"" + s + "\"";
					}

					if (InObject)
					{
						if (s.IndexOfAny(objectKeyCharacters) >= 0)
							return "\"" + s + "\"";
					}
					else
					{
						if (s.IndexOfAny(keyCharacters) >= 0)
							return "\"" + s + "\"";
					}

					return s;
			}
		}

		private static readonly char[] objectKeyCharacters = new char[] { ':', '-', ' ' };
		private static readonly char[] keyCharacters = new char[] { ':', '-' };

		/// <summary>
		/// Appends an object as a TOON object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		///	<param name="MemberExists">Checks if a member exists</param>
		public void AppendAsObject(IEnumerable<KeyValuePair<string, object>> Object,
			int? Indent, Func<string, bool> MemberExists)
		{
			bool MultiRow = Indent.HasValue;
			bool First = true;

			if (MultiRow)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, object> Member in Object)
				{
					if (!MultiRow)
					{
						if (!MultiRow)
						{
							if (First)
								First = false;
							else
								this.AppendDelimiter();
						}
					}

					this.AppendMember(Member.Key, Member.Value, Indent, MemberExists);
				}
			}

			if (MultiRow)
			{
				Indent--;

				if (!First && Indent.Value > 0)
				{
					this.AppendLine();
					this.Indent(Indent.Value);
				}
			}
		}

		/// <summary>
		/// Appends an object as a TOON object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <param name="MemberExists">Checks if a member exists</param>
		public void AppendAsObject(IEnumerable<KeyValuePair<string, IElement>> Object,
			int? Indent, Func<string, bool> MemberExists)
		{
			bool MultiRow = Indent.HasValue;
			bool First = true;

			if (MultiRow)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, IElement> Member in Object)
				{
					if (!MultiRow)
					{
						if (First)
							First = false;
						else
							this.AppendDelimiter();
					}

					this.AppendMember(Member.Key, Member.Value.AssociatedObjectValue, Indent,
						MemberExists);
				}
			}

			if (MultiRow)
			{
				Indent--;

				if (!First && Indent.Value > 0)
				{
					this.AppendLine();
					this.Indent(Indent.Value);
				}
			}
		}

		/// <summary>
		/// Appends a member of an object as TOON.
		/// </summary>
		/// <param name="Name">Name of member.</param>
		/// <param name="Value">Value of member.</param>
		/// <param name="Indent">Optional indentation.</param>
		/// <param name="MemberExists">Checks if a member exists</param>
		public void AppendMember(string Name, object Value, int? Indent,
			Func<string, bool> MemberExists)
		{
			bool AppendSpaces = Indent.HasValue;

			if (AppendSpaces && (Indent.Value > 0 || (Indent.Value == 0 && !this.Empty)))
			{
				this.AppendLine();
				this.Indent(Indent.Value);
			}

			if (Value is null)
			{
				this.AppendEncoded(Name, true);

				if (AppendSpaces)
					this.Append(": ");
				else
					this.Append(':');

				this.Append("null");
				return;
			}

			IToonEncoder Encoder = TOON.GetEncoder(Value);

			if (this.keyFolding &&
				Encoder.EncodesAsObject(Value) &&
				this.Encode(Name, true) == Name)
			{
				string OriginalName = Name;
				object OriginalValue = Value;
				int Depth = 1;

				while (Depth < this.keyFoldingDepth &&
					Encoder.CanFold(Value, out string FoldedName, out object FoldedValue) &&
					this.Encode(FoldedName, true) == FoldedName)
				{
					Name += "." + FoldedName;
					Value = FoldedValue;
					Encoder = TOON.GetEncoder(Value);
					Depth++;

					if (!Encoder.EncodesAsObject(Value))
						break;
				}

				if (Depth > 1 && MemberExists(Name))
				{
					Name = OriginalName;
					Value = OriginalValue;
				}
			}

			this.AppendEncoded(Name, true);

			if (Encoder.EncodesAsVector(Value))
			{
				Encoder.Encode(Value, Indent, this, BracketsMode.Count);
				return;
			}
			else if (Encoder.EncodesMultipleRows || !AppendSpaces)
				this.Append(':');
			else
				this.Append(": ");

			Encoder.Encode(Value, Indent, this);
		}

	}
}
