using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// Executes script from a file.
	/// Corresponds to the SCRIPT meta-data tag in Markdown.
	/// </summary>
	public class ScriptFile : FunctionOneScalarVariable
	{
		/// <summary>
		/// Executes script from a file, if not executed before, or if file timestamp has changed.
		/// Corresponds to the INIT meta-data tag in Markdown.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ScriptFile(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "ScriptFile";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "FileName" }; }
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			string Source = this.Expression.Source;
			if (string.IsNullOrEmpty(Source))
				throw new ScriptRuntimeException("Script has no source.", this);

			Source = Path.Combine(Source, Argument);

			string Script = File.ReadAllText(Source);
			Expression Exp = new Expression(Script, Source);

			return Exp.Root.Evaluate(Variables);
		}
	}
}
