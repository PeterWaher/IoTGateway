using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script
{
	/// <summary>
	/// Collection of variables.
	/// </summary>
	public class Variables
	{
		private Dictionary<string, Variable> variables = new Dictionary<string, Variable>(Types.CaseInsensitiveComparer);

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
	}
}
