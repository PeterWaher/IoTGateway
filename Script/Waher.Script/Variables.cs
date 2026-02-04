using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

namespace Waher.Script
{
	/// <summary>
	/// Converts a value to a printable string.
	/// </summary>
	/// <param name="Value">Value to print.</param>
	/// <param name="Variables">Reference to current variables collection.</param>
	/// <returns>String to print.</returns>
	public delegate Task<string> ValuePrinter(object Value, Variables Variables);

	/// <summary>
	/// Collection of variables.
	/// </summary>
	public class Variables : IEnumerable<Variable>, IContextVariables
	{
		/// <summary>
		/// Internal set of variables.
		/// </summary>
		protected Dictionary<string, Variable> variables = new Dictionary<string, Variable>();

		private CancellationTokenSource cancellation = new CancellationTokenSource();
		private Stack<Dictionary<string, Variable>> stack = null;
		private TextWriter consoleOut = null;
		private IContextVariables contextVariables = null;
		private volatile bool active = true;
		private ValuePrinter printer = null;

		/// <summary>
		/// Collection of variables.
		/// </summary>
		public Variables(params Variable[] Variables)
		{
			if ((Variables?.Length ?? 0) > 0)
			{
				foreach (Variable Variable in Variables)
					this.variables[Variable.Name] = Variable;
			}
		}

		/// <summary>
		/// Tries to get a variable object, given its name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Variable">Variable, if found, or null otherwise.</param>
		/// <returns>If a variable with the corresponding name was found.</returns>
		public virtual bool TryGetVariable(string Name, out Variable Variable)
		{
			if (this.active)
			{
				lock (this.variables)
				{
					if (this.variables.TryGetValue(Name, out Variable))
						return true;
				}

				if (!(this.contextVariables is null))
					return this.contextVariables.TryGetVariable(Name, out Variable);

				return false;
			}
			else
				throw new ScriptAbortedException();
		}

		/// <summary>
		/// If the collection contains a variable with a given name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <returns>If a variable with that name exists.</returns>
		public virtual bool ContainsVariable(string Name)
		{
			if (this.active)
			{
				lock (this.variables)
				{
					if (this.variables.ContainsKey(Name))
						return true;
				}

				if (!(this.contextVariables is null))
					return this.contextVariables.ContainsVariable(Name);

				return false;
			}
			else
				throw new ScriptAbortedException();
		}

		/// <summary>
		/// Access to variable values through the use of their names.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <returns>Associated variable object value.</returns>
		public object this[string Name]
		{
			get
			{
				if (this.TryGetVariable(Name, out Variable v))
					return v.ValueObject;
				else
					return null;
			}

			set
			{
				this.Add(Name, value);
			}
		}

		/// <summary>
		/// Adds a variable to the collection.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Value">Associated variable object value.</param>
		/// <returns>Reference to variable that was added.</returns>
		public virtual Variable Add(string Name, object Value)
		{
			Variable Result;

			if (this.active)
			{
				lock (this.variables)
				{
					if (this.variables.TryGetValue(Name, out Result))
						Result.SetValue(Value);
					else
						this.variables[Name] = Result = new Variable(Name, Value);
				}
			}
			else
				throw new ScriptAbortedException();

			return Result;
		}

		/// <summary>
		/// Removes a varaiable from the collection.
		/// </summary>
		/// <param name="VariableName">Name of variable.</param>
		/// <returns>If the variable was found and removed.</returns>
		public virtual bool Remove(string VariableName)
		{
			if (this.active)
			{
				lock (this.variables)
				{
					return this.variables.Remove(VariableName);
				}
			}
			else
				throw new ScriptAbortedException();
		}

		/// <summary>
		/// Removes all variables from the collection.
		/// </summary>
		public virtual void Clear()
		{
			if (this.active)
			{
				lock (this.variables)
				{
					this.variables.Clear();
				}
			}
			else
				throw new ScriptAbortedException();
		}

		/// <summary>
		/// Pushes the current set of variables to the stack. This state is restored by calling <see cref="Pop"/>.
		/// Each call to this method must be followed by exactly one call to <see cref="Pop"/>.
		/// </summary>
		public virtual void Push()
		{
			if (this.active)
			{
				if (this.stack is null)
					this.stack = new Stack<Dictionary<string, Variable>>();

				this.stack.Push(this.variables);

				Dictionary<string, Variable> Clone = new Dictionary<string, Variable>();
				foreach (KeyValuePair<string, Variable> P in this.variables)
					Clone[P.Key] = new Variable(P.Key, P.Value.ValueElement);

				this.variables = Clone;
			}
			else
				throw new ScriptAbortedException();
		}

		/// <summary>
		/// Pops a previously stored set of variables from the stack. Variables are stored on the stack by calling <see cref="Push"/>.
		/// </summary>
		public virtual void Pop()
		{
			if (this.active)
			{
				if (this.stack is null)
					throw new ScriptException("Stack is empty.");

				this.variables = this.stack.Pop();
			}
			else
				throw new ScriptAbortedException();
		}

		/// <summary>
		/// Console out interface. Can be used by functions and script to output data to the console.
		/// </summary>
		public TextWriter ConsoleOut
		{
			get => this.consoleOut;
			set => this.consoleOut = value;
		}

		/// <summary>
		/// Variables available during the current context.
		/// </summary>
		public IContextVariables ContextVariables
		{
			get => this.contextVariables;
			set => this.contextVariables = value;
		}

		/// <summary>
		/// Delegate that converts values to strings for (implicit) printing. Default is null, which will call the
		/// <see cref="Object.ToString()"/> method to convert the value to a string.
		/// </summary>
		public ValuePrinter Printer
		{
			get => this.printer;
			set => this.printer = value;
		}

		/// <summary>
		/// Returns an array of available variables.
		/// </summary>
		public Variable[] AvailableVariables
		{
			get
			{
				if (this.active)
				{
					Variable[] Variables;

					lock (this.variables)
					{
						Variables = new Variable[this.variables.Count];
						this.variables.Values.CopyTo(Variables, 0);
					}

					return Variables;
				}
				else
					throw new ScriptAbortedException();
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<Variable> GetEnumerator()
		{
			return new VariableEnumerator(this.AvailableVariables);
		}

		private class VariableEnumerator : IEnumerator<Variable>
		{
			private readonly Variable[] variables;
			private int pos = -1;

			internal VariableEnumerator(Variable[] Variables)
			{
				this.variables = Variables;
			}

			public Variable Current
			{
				get
				{
					return this.variables[this.pos];
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return this.variables[this.pos];
				}
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return ++this.pos < this.variables.Length;
			}

			public void Reset()
			{
				this.pos = -1;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.AvailableVariables.GetEnumerator();
		}

		/// <summary>
		/// Copies available variables to another variable collection.
		/// </summary>
		/// <param name="Variables">Variable collection to copy variables to.</param>
		public void CopyTo(Variables Variables)
		{
			Variable[] VariablesToCopy = this.AvailableVariables;
			Dictionary<string, Variable> Recipient = Variables.variables;

			lock (Recipient)
			{
				foreach (Variable Variable in VariablesToCopy)
					Recipient[Variable.Name] = Variable;
			}
		}

		/// <summary>
		/// Aborts the execution of script using this collection of variables.
		/// </summary>
		public void Abort()
		{
			if (this.active)
			{
				this.active = false;
				this.cancellation.Cancel();
			}
		}

		/// <summary>
		/// Allows new script to be evaluated using this collection of variables.
		/// </summary>
		public void CancelAbort()
		{
			if (!this.active)
			{
				this.active = true;
				this.cancellation = new CancellationTokenSource();
			}
		}

		/// <summary>
		/// If the script has been aborted.
		/// </summary>
		public bool Aborted => !this.active;

		/// <summary>
		/// If script is still active (i.e. has not been aborted).
		/// </summary>
		public bool Active => this.active;

		/// <summary>
		/// Event raised when there is a new value to preview.
		/// </summary>
		public event EventHandlerAsync<PreviewEventArgs> OnPreview;

		/// <summary>
		/// If previews are desired.
		/// </summary>
		public bool HandlesPreview
		{
			get
			{
				if (!(this.OnPreview is null))
					return true;

				if (this.contextVariables is Variables v)
					return v.HandlesPreview;
				else
					return false;
			}
		}

		/// <summary>
		/// Reports a preview of the final result.
		/// </summary>
		/// <param name="Expression">Expression being executed.</param>
		/// <param name="Result">Preview</param>
		public async Task Preview(Expression Expression, IElement Result)
		{
			EventHandlerAsync<PreviewEventArgs> h = this.OnPreview;

			if (!(h is null))
				await h.Raise(this, new PreviewEventArgs(Expression, this, Result));

			if (this.contextVariables is Variables v)
				await v.Preview(Expression, Result);
		}


		/// <summary>
		/// Reports current status of execution.
		/// </summary>
		/// <param name="Expression">Expression.</param>
		/// <param name="Result">Status Message</param>
		public async Task Status(Expression Expression, string Result)
		{
			EventHandlerAsync<StatusEventArgs> h = this.OnStatus;
			if (!(h is null))
				await h(this, new StatusEventArgs(Expression, this, Result));
		}

		/// <summary>
		/// If status messages are desired.
		/// </summary>
		public bool HandlesStatus => !(this.OnStatus is null);

		/// <summary>
		/// Event raised when a status message has been reported.
		/// </summary>
		public event EventHandlerAsync<StatusEventArgs> OnStatus = null;

		/// <summary>
		/// Cancellation token, that can be used to monitor for script abortion.
		/// </summary>
		public CancellationToken CancellationToken => this.cancellation.Token;
	}
}
