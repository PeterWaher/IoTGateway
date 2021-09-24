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
	/// Executes script from a file, if not executed before, or if file timestamp has changed.
	/// Corresponds to the INIT meta-data tag in Markdown.
	/// </summary>
	public class InitScriptFile : FunctionOneScalarVariable
	{
		private static readonly Dictionary<string, DateTime> lastExecuted = new Dictionary<string, DateTime>();

		/// <summary>
		/// Executes script from a file, if not executed before, or if file timestamp has changed.
		/// Corresponds to the INIT meta-data tag in Markdown.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public InitScriptFile(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "InitScriptFile";

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

			if (NeedsExecution(Source))
			{
				string Script = File.ReadAllText(Source);
				Expression Exp = new Expression(Script, Source);

				return Exp.Root.Evaluate(Variables);
			}
			else
				return ObjectValue.Null;
		}

		/// <summary>
		/// Checks if an init-file needs to be executed.
		/// </summary>
		/// <param name="FileName">File name</param>
		/// <returns>If script file needs to be executed.</returns>
		public static bool NeedsExecution(string FileName)
		{
			DateTime Timestamp = File.GetLastWriteTime(FileName);
			DateTime? LastExecuted;
			Type RuntimeSettings = null;
			MethodInfo Get;
			MethodInfo Set;

			lock (lastExecuted)
			{
				if (lastExecuted.TryGetValue(FileName, out DateTime TP))
					LastExecuted = TP;
				else
					LastExecuted = null;
			}

			if (LastExecuted.HasValue && LastExecuted.Value >= Timestamp)
				return false;

			if (!LastExecuted.HasValue &&
				!((RuntimeSettings = Types.GetType("Waher.Runtime.Settings.RuntimeSettings")) is null) &&
				!((Get = RuntimeSettings.GetRuntimeMethod("Get", new Type[] { typeof(string), typeof(DateTime) })) is null))
			{
				LastExecuted = (DateTime)Get.Invoke(null, new object[] { FileName, DateTime.MinValue });

				if (LastExecuted.HasValue && LastExecuted.Value >= Timestamp)
				{
					lock (lastExecuted)
					{
						lastExecuted[FileName] = LastExecuted.Value;
					}

					return false;
				}
			}

			lock (lastExecuted)
			{
				lastExecuted[FileName] = Timestamp;
			}

			if (RuntimeSettings is null)
				RuntimeSettings = Types.GetType("Waher.Runtime.Settings.RuntimeSettings");

			if (!(RuntimeSettings is null) &&
				!((Set = RuntimeSettings.GetRuntimeMethod("Set", new Type[] { typeof(string), typeof(DateTime) })) is null))
			{
				Set.Invoke(null, new object[] { FileName, Timestamp });
			}

			return true;
		}
	}
}
