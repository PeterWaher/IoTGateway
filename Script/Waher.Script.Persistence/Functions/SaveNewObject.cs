using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Persistence;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Saves an object to the object database.
	/// </summary>
	public class SaveNewObject : FunctionOneScalarVariable
	{
		/// <summary>
		/// Saves an object to the object database.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public SaveNewObject(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "SaveNewObject";
			}
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Object" };
			}
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			Database.Insert(Argument.AssociatedObjectValue).Wait();
			return Argument;
		}

	}
}
