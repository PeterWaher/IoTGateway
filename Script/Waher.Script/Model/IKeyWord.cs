using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Model
{
	/// <summary>
	/// Interface for keywords with custom parsing.
	/// </summary>
	public interface IKeyWord
	{
		/// <summary>
		/// Keyword associated with custom parser.
		/// </summary>
		string KeyWord
		{
			get;
		}

		/// <summary>
		/// Keyword aliases, if available, null if none.
		/// </summary>
		string[] Aliases
		{
			get;
		}

		/// <summary>
		/// Any keywords used internally by the custom parser.
		/// </summary>
		string[] InternalKeywords
		{
			get;
		}

		/// <summary>
		/// Tries to parse a script node.
		/// </summary>
		/// <param name="Parser">Custom parser.</param>
		/// <param name="Result">Parsed Script Node.</param>
		/// <returns>If successful in parsing a script node.</returns>
		bool TryParse(ScriptParser Parser, out ScriptNode Result);
	}
}
