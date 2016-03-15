using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Waher.Script.Exceptions;
using System.Collections;

namespace Waher.Script
{
	/// <summary>
	/// Collection of variables.
	/// </summary>
	public class Variables : IEnumerable<Variable>
	{
		private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
        private Stack<Dictionary<string, Variable>> stack = null;
#if WINDOWS_UWP
		private TextWriter consoleOut = new DebugWriter();
#else
		private TextWriter consoleOut = Console.Out;
#endif
		private Mutex mutex = new Mutex();

        /// <summary>
        /// Collection of variables.
        /// </summary>
        public Variables(params Variable[] Variables)
		{
			foreach (Variable Variable in Variables)
				this.variables[Variable.Name] = Variable;
		}

		/// <summary>
		/// Tries to get a variable object, given its name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Variable">Variable, if found, or null otherwise.</param>
		/// <returns>If a variable with the corresponding name was found.</returns>
		public bool TryGetVariable(string Name, out Variable Variable)
		{
			lock (this.variables)
			{
				return this.variables.TryGetValue(Name, out Variable);
			}
		}

		/// <summary>
		/// If the collection contains a variable with a given name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <returns>If a variable with that name exists.</returns>
		public bool ContainsVariable(string Name)
		{
			lock(this.variables)
			{
				return this.variables.ContainsKey(Name);
			}
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
				Variable v;

				lock (this.variables)
				{
					if (this.variables.TryGetValue(Name, out v))
						return v.ValueObject;
					else
						return null;
				}
			}

			set
			{
				Variable v;

				lock (this.variables)
				{
					if (this.variables.TryGetValue(Name, out v))
						v.SetValue(value);
					else
						this.variables[Name] = new Variable(Name, value);
				}
			}
		}

        /// <summary>
        /// Removes a varaiable from the collection.
        /// </summary>
        /// <param name="VariableName">Name of variable.</param>
        /// <returns>If the variable was found and removed.</returns>
        public bool Remove(string VariableName)
        {
            lock(this.variables)
            {
                return this.variables.Remove(VariableName);
            }
        }

        /// <summary>
        /// Pushes the current set of variables to the stack. This state is restored by calling <see cref="Pop"/>.
        /// Each call to this method must be followed by exactly one call to <see cref="Pop"/>.
        /// </summary>
        public void Push()
        {
            if (this.stack == null)
                this.stack = new Stack<Dictionary<string, Variable>>();

            this.stack.Push(this.variables);

            Dictionary<string, Variable> Clone = new Dictionary<string, Variable>();
            foreach (KeyValuePair<string, Variable> P in this.variables)
                Clone[P.Key] = P.Value;

            this.variables = Clone;
        }

        /// <summary>
        /// Pops a previously stored set of variables from the stack. Variables are stored on the stack by calling <see cref="Push"/>.
        /// </summary>
        public void Pop()
        {
            if (this.stack == null)
                throw new ScriptException("Stack is empty.");

            this.variables = this.stack.Pop();
        }

        /// <summary>
        /// Console out interface. Can be used by functions and script to output data to the console.
        /// </summary>
        public TextWriter ConsoleOut
        {
            get { return this.consoleOut; }
            set { this.consoleOut = value; }
        }

		/// <summary>
		/// Locks the collection. The collection is by default thread safe. But if longer transactions require unique access,
		/// this method can be used to aquire such unique access. This works, as long as all callers that affect the corresponding
		/// state call this method also.
		/// 
		/// Each successful call to this method must be followed by exacty one call to <see cref="Release"/>.
		/// </summary>
		/// <exception cref="TimeoutException">If access to the collection was not granted in the alotted time</exception>
		public void Lock()
		{
			this.Lock(30000);
		}

		/// <summary>
		/// Locks the collection. The collection is by default thread safe. But if longer transactions require unique access,
		/// this method can be used to aquire such unique access. This works, as long as all callers that affect the corresponding
		/// state call this method also.
		/// 
		/// Each successful call to this method must be followed by exacty one call to <see cref="Release"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds. Default timeout is 30000 milliseconds (30 s).</param>
		/// <exception cref="TimeoutException">If access to the collection was not granted in the alotted time</exception>
		public void Lock(int Timeout)
		{
			if (!this.mutex.WaitOne(Timeout))
				throw new TimeoutException("Unique access to variables connection was not granted.");
		}

		/// <summary>
		/// Releases the collection, previously locked through a call to <see cref="Lock"/>.
		/// </summary>
		public void Release()
		{
			this.mutex.ReleaseMutex();
		}

		public Variable[] AvailableVariables
		{
			get
			{
				Variable[] Variables;

				lock (this.variables)
				{
					Variables = new Variable[this.variables.Count];
					this.variables.Values.CopyTo(Variables, 0);
				}

				return Variables;
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
			private Variable[] variables;
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
	}
}
