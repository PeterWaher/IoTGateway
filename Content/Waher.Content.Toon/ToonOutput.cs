using System;
using System.Text;
using Waher.Script.Units.DerivedQuantities;

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
		private bool standardIndentation = true;
		private bool standardDelimiter = true;
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
	}
}
