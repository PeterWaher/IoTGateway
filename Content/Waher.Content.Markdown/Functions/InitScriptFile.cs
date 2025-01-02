using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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
	public class InitScriptFile : FunctionOneScalarStringVariable
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
		public override string FunctionName => nameof(InitScriptFile);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "FileName" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateScalarAsync(string Argument, Variables Variables)
		{
			string Source = this.Expression.Source;
			if (string.IsNullOrEmpty(Source))
				throw new ScriptRuntimeException("Script has no source.", this);

			Source = Path.Combine(Source, Argument);

			if (await NeedsExecution(Source))
			{
				string Script = await Resources.ReadAllTextAsync(Source);
				Expression Exp = new Expression(Script, Source);

				return await Exp.Root.EvaluateAsync(Variables);
			}
			else
				return ObjectValue.Null;
		}

		/// <summary>
		/// Checks if an init-file needs to be executed.
		/// </summary>
		/// <param name="FileName">File name</param>
		/// <returns>If script file needs to be executed.</returns>
		public static async Task<bool> NeedsExecution(string FileName)
		{
			DateTime Timestamp = File.GetLastWriteTimeUtc(FileName);
			DateTime? LastExecuted;
			Type RuntimeSettings = null;
			MethodInfo GetAsync;
			MethodInfo SetAsync;

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
				!((GetAsync = RuntimeSettings.GetRuntimeMethod("GetAsync", new Type[] { typeof(string), typeof(DateTime) })) is null) &&
				GetAsync.ReturnType == typeof(Task<DateTime>))
			{
				LastExecuted = await (Task<DateTime>)GetAsync.Invoke(null, new object[] { FileName, DateTime.MinValue });

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
				!((SetAsync = RuntimeSettings.GetRuntimeMethod("SetAsync", new Type[] { typeof(string), typeof(DateTime) })) is null) &&
				SetAsync.ReturnType == typeof(Task<bool>))
			{
				Task<bool> Result = (Task<bool>)SetAsync.Invoke(null, new object[] { FileName, Timestamp });
				await Result;
			}

			return true;
		}
	}
}
