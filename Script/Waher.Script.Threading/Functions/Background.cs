using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Constants;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Threading.Functions
{
	/// <summary>
	/// Executes the provided script asynchronously in the background.
	/// </summary>
	public class Background : FunctionOneVariable
	{
		/// <summary>
		/// Executes the provided script asynchronously in the background.
		/// </summary>
		/// <param name="Script">Script to execute in the background.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Background(ScriptNode Script, int Start, int Length, Expression Expression)
			: base(Script, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Background);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Script" };

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			KeyValuePair<Guid, Task<IElement>> P = EvaluateInBackground(this.Argument, Variables, true);

			return new ObjectValue(P.Key);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return Argument;
		}

		/// <summary>
		/// Evaluates script in the background.
		/// </summary>
		/// <param name="Script">Script to evaluate.</param>
		/// <param name="Variables">Set of variables.</param>
		/// <param name="CatchAndLogExceptions">If exceptions should be caught and logged (true), or passed on to the awaiting task (false).</param>
		/// <returns>ID that can be used to abort process, as well as a Task object that can be awaited, for the result.</returns>
		public static KeyValuePair<Guid, Task<IElement>> EvaluateInBackground(Expression Script, Variables Variables, bool CatchAndLogExceptions)
		{
			return EvaluateInBackground(Script.Root, Variables, CatchAndLogExceptions);
		}

		/// <summary>
		/// Evaluates script in the background.
		/// </summary>
		/// <param name="Node">Script to evaluate.</param>
		/// <param name="Variables">Set of variables.</param>
		/// <param name="CatchAndLogExceptions">If exceptions should be caught and logged (true), or passed on to the awaiting task (false).</param>
		/// <returns>ID that can be used to abort process, as well as a Task object that can be awaited, for the result.</returns>
		public static KeyValuePair<Guid, Task<IElement>> EvaluateInBackground(ScriptNode Node, Variables Variables, bool CatchAndLogExceptions)
		{
			Variables v = new Variables();
			Variables.CopyTo(v);

			Guid Id = Guid.NewGuid();
			TaskCompletionSource<IElement> Result = new TaskCompletionSource<IElement>();

			lock (backgroundProcesses)
			{
				backgroundProcesses[Id] = new KeyValuePair<Variables, TaskCompletionSource<IElement>>(v, Result);
			}

			Task.Run(async () =>
			{
				try
				{
					IElement E;

					if (Node.IsAsynchronous)
						E = await Node.EvaluateAsync(v);
					else
						E = Node.Evaluate(v);

					Result.TrySetResult(E);
				}
				catch (ScriptReturnValueException ex)
				{
					Result.TrySetResult(ex.ReturnValue);
				}
				catch (Exception ex)
				{
					if (CatchAndLogExceptions)
					{
						Log.Critical(ex);
						Result.TrySetResult(ObjectValue.Null);
					}
					else
						Result.TrySetException(ex);
				}
				finally
				{
					lock (backgroundProcesses)
					{
						backgroundProcesses.Remove(Id);
					}
				}
			});

			return new KeyValuePair<Guid, Task<IElement>>(Id, Result.Task);
		}

		/// <summary>
		/// Aborts a background task, earlier started by calling EvaluateInBackground.
		/// </summary>
		/// <param name="TaskId">ID of background task</param>
		/// <returns>If task was found and aborted.</returns>
		public static bool AbortBackgroundTask(Guid TaskId)
		{
			KeyValuePair<Variables, TaskCompletionSource<IElement>> Rec;

			lock (backgroundProcesses)
			{
				if (!backgroundProcesses.TryGetValue(TaskId, out Rec))
					return false;

				backgroundProcesses.Remove(TaskId);
			}

			Rec.Key.Abort();

			return true;
		}

		/// <summary>
		/// Waits for all background tasks to terminate. Aborts tasks still executing after timeout elapses.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public static async Task TerminateTasks(int Timeout)
		{
			KeyValuePair<Variables, TaskCompletionSource<IElement>>[] Tasks;
			int i, c;

			lock (backgroundProcesses)
			{
				c = backgroundProcesses.Count;
				if (c == 0)
					return;

				Tasks = new KeyValuePair<Variables, TaskCompletionSource<IElement>>[c];
				backgroundProcesses.Values.CopyTo(Tasks, 0);
				backgroundProcesses.Clear();
			}

			Task[] ToWait = new Task[c];
			Variables[] Variables = new Variables[c];

			for (i = 0; i < c; i++)
			{
				ToWait[i] = Tasks[i].Value.Task;
				Variables[i] = Tasks[i].Key;
			}

			await Task.WhenAny(Task.WhenAll(ToWait), Task.Delay(Timeout));

			for (i = 0; i < c; i++)
				Variables[i].Abort();
		}

		private static readonly Dictionary<Guid, KeyValuePair<Variables, TaskCompletionSource<IElement>>> backgroundProcesses =
			new Dictionary<Guid, KeyValuePair<Variables, TaskCompletionSource<IElement>>>();

	}
}
