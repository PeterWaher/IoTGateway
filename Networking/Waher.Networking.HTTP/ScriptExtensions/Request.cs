using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Networking.HTTP.ScriptExtensions
{
	/// <summary>
	/// Access to the current request.
	/// </summary>
	public class Request : IConstant
	{
		/// <summary>
		/// Access to the current request.
		/// </summary>
		public Request()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName => nameof(Request);

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			if (!(Variables is SessionVariables SessionVariables))
			{
				if (Variables.ContextVariables is SessionVariables SessionVariables2)
					SessionVariables = SessionVariables2;
				else
					return ObjectValue.Null;
			}

			return new ObjectValue(SessionVariables.CurrentRequest);
		}
	}
}
