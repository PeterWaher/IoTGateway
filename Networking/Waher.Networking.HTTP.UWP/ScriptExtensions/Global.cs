using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions
{
	/// <summary>
	/// Access to global variables.
	/// </summary>
	public class Global : IConstant
	{
		/// <summary>
		/// Access to global variables.
		/// </summary>
		public Global()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName => nameof(Global);

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
			return SessionVariables.GlobalVariablesElement;
		}
	}
}
