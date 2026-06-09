namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all funcions.
	/// </summary>
	public abstract class Function : ScriptNode, IFunction
	{
		/// <summary>
		/// Base class for all funcions.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Function(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public abstract string FunctionName
		{
			get;
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public virtual string[] Aliases => null;

		/// <summary>
		/// Default Argument names
		/// </summary>
		public abstract string[] DefaultArgumentNames
		{
			get;
		}

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		public virtual string DefaultVariableName
		{
			get
			{
				string[] A = this.DefaultArgumentNames;
				if (A.Length == 1)
					return A[0];
				else
					return null;
			}
		}

		/// <summary>
		/// If the function is specific to a given context, as apparent from the expression
		/// object. This is used to triage between context-specific functions if multiple
		/// are registered with the same name.
		/// </summary>
		/// <param name="Expression">Expression being parsed.</param>
		/// <returns>If the function is specific to a given context.</returns>
		public virtual bool ContextSpecific(Expression Expression)
		{
			return false;
		}
	}
}
