using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Threading.Functions
{
	/// <summary>
	/// Executes tasks in parallel.
	/// </summary>
	public class Parallel : FunctionMultiVariate
	{
		/// <summary>
		/// Executes tasks in parallel.
		/// </summary>
		/// <param name="Tasks">Tasks to execute in parallel.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Parallel(ScriptNode Tasks, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Tasks }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Executes tasks in parallel.
		/// </summary>
		/// <param name="Tasks">Tasks to execute in parallel.</param>
		/// <param name="Tasks2">Tasks to execute in parallel.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Parallel(ScriptNode Tasks, ScriptNode Tasks2, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Tasks, Tasks2 }, argumentTypes2Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Executes tasks in parallel.
		/// </summary>
		/// <param name="Tasks">Tasks to execute in parallel.</param>
		/// <param name="Tasks2">Tasks to execute in parallel.</param>
		/// <param name="Tasks3">Tasks to execute in parallel.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Parallel(ScriptNode Tasks, ScriptNode Tasks2, ScriptNode Tasks3, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Tasks, Tasks2, Tasks3 }, argumentTypes3Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Executes tasks in parallel.
		/// </summary>
		/// <param name="Tasks">Tasks to execute in parallel.</param>
		/// <param name="Tasks2">Tasks to execute in parallel.</param>
		/// <param name="Tasks3">Tasks to execute in parallel.</param>
		/// <param name="Tasks4">Tasks to execute in parallel.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Parallel(ScriptNode Tasks, ScriptNode Tasks2, ScriptNode Tasks3, ScriptNode Tasks4, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Tasks, Tasks2, Tasks3, Tasks4 }, argumentTypes4Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Executes tasks in parallel.
		/// </summary>
		/// <param name="Tasks">Tasks to execute in parallel.</param>
		/// <param name="Tasks2">Tasks to execute in parallel.</param>
		/// <param name="Tasks3">Tasks to execute in parallel.</param>
		/// <param name="Tasks4">Tasks to execute in parallel.</param>
		/// <param name="Tasks5">Tasks to execute in parallel.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Parallel(ScriptNode Tasks, ScriptNode Tasks2, ScriptNode Tasks3, ScriptNode Tasks4, ScriptNode Tasks5,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Tasks, Tasks2, Tasks3, Tasks4, Tasks5 }, argumentTypes5Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Executes tasks in parallel.
		/// </summary>
		/// <param name="Tasks">Tasks to execute in parallel.</param>
		/// <param name="Tasks2">Tasks to execute in parallel.</param>
		/// <param name="Tasks3">Tasks to execute in parallel.</param>
		/// <param name="Tasks4">Tasks to execute in parallel.</param>
		/// <param name="Tasks5">Tasks to execute in parallel.</param>
		/// <param name="Tasks6">Tasks to execute in parallel.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Parallel(ScriptNode Tasks, ScriptNode Tasks2, ScriptNode Tasks3, ScriptNode Tasks4, ScriptNode Tasks5, ScriptNode Tasks6,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Tasks, Tasks2, Tasks3, Tasks4, Tasks5, Tasks6 }, argumentTypes6Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Executes tasks in parallel.
		/// </summary>
		/// <param name="Tasks">Tasks to execute in parallel.</param>
		/// <param name="Tasks2">Tasks to execute in parallel.</param>
		/// <param name="Tasks3">Tasks to execute in parallel.</param>
		/// <param name="Tasks4">Tasks to execute in parallel.</param>
		/// <param name="Tasks5">Tasks to execute in parallel.</param>
		/// <param name="Tasks6">Tasks to execute in parallel.</param>
		/// <param name="Tasks7">Tasks to execute in parallel.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Parallel(ScriptNode Tasks, ScriptNode Tasks2, ScriptNode Tasks3, ScriptNode Tasks4, ScriptNode Tasks5, ScriptNode Tasks6,
			ScriptNode Tasks7, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Tasks, Tasks2, Tasks3, Tasks4, Tasks5, Tasks6, Tasks7 }, argumentTypes7Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Executes tasks in parallel.
		/// </summary>
		/// <param name="Tasks">Tasks to execute in parallel.</param>
		/// <param name="Tasks2">Tasks to execute in parallel.</param>
		/// <param name="Tasks3">Tasks to execute in parallel.</param>
		/// <param name="Tasks4">Tasks to execute in parallel.</param>
		/// <param name="Tasks5">Tasks to execute in parallel.</param>
		/// <param name="Tasks6">Tasks to execute in parallel.</param>
		/// <param name="Tasks7">Tasks to execute in parallel.</param>
		/// <param name="Tasks8">Tasks to execute in parallel.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Parallel(ScriptNode Tasks, ScriptNode Tasks2, ScriptNode Tasks3, ScriptNode Tasks4, ScriptNode Tasks5, ScriptNode Tasks6,
			ScriptNode Tasks7, ScriptNode Tasks8, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Tasks, Tasks2, Tasks3, Tasks4, Tasks5, Tasks6, Tasks7, Tasks8 }, argumentTypes8Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Executes tasks in parallel.
		/// </summary>
		/// <param name="Tasks">Tasks to execute in parallel.</param>
		/// <param name="Tasks2">Tasks to execute in parallel.</param>
		/// <param name="Tasks3">Tasks to execute in parallel.</param>
		/// <param name="Tasks4">Tasks to execute in parallel.</param>
		/// <param name="Tasks5">Tasks to execute in parallel.</param>
		/// <param name="Tasks6">Tasks to execute in parallel.</param>
		/// <param name="Tasks7">Tasks to execute in parallel.</param>
		/// <param name="Tasks8">Tasks to execute in parallel.</param>
		/// <param name="Tasks9">Tasks to execute in parallel.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Parallel(ScriptNode Tasks, ScriptNode Tasks2, ScriptNode Tasks3, ScriptNode Tasks4, ScriptNode Tasks5, ScriptNode Tasks6,
			ScriptNode Tasks7, ScriptNode Tasks8, ScriptNode Tasks9, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Tasks, Tasks2, Tasks3, Tasks4, Tasks5, Tasks6, Tasks7, Tasks8, Tasks9 }, argumentTypes9Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Parallel);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Tasks" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			List<TaskRec> Records = new List<TaskRec>();
			int i, c = 0;

			foreach (ScriptNode Node in this.Arguments)
			{
				if (Node is VectorDefinition VectorDef)
				{
					foreach (ScriptNode Node2 in VectorDef.Elements)
					{
						Records.Add(new TaskRec()
						{
							Index = c++,
							Node = Node2
						});
					}
				}
				else
				{
					Records.Add(new TaskRec()
					{
						Index = c++,
						Node = Node
					});
				}
			}

			Task[] Tasks = new Task[c];
			IElement[] Results = new IElement[c];

			for (i = 0; i < c; i++)
			{
				TaskRec Rec = Records[i];
				Variables v2 = new Variables()
				{
					ContextVariables = Variables.ContextVariables,
					ConsoleOut = Variables.ConsoleOut
				};
				Variables.CopyTo(v2);
				Tasks[i] = this.EvaluateAsync(Rec.Node, v2, Results, i);
			}

			await Task.WhenAll(Tasks);

			return VectorDefinition.Encapsulate(Results, false, this);
		}

		private async Task EvaluateAsync(ScriptNode TaskNode, Variables Variables, IElement[] Results, int Index)
		{
			if (TaskNode.IsAsynchronous)
				Results[Index] = await Task.Run(() => TaskNode.EvaluateAsync(Variables));
			else
				Results[Index] = await Task.Run(() => TaskNode.Evaluate(Variables));
		}

		private class TaskRec
		{
			public int Index;
			public ScriptNode Node;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return VectorDefinition.Encapsulate(Arguments, false, this);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			return Task.FromResult<IElement>(VectorDefinition.Encapsulate(Arguments, false, this));
		}
	}
}
