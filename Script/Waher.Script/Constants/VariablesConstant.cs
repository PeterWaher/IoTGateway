using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Constants
{
    /// <summary>
    /// The empty set.
    /// </summary>
    public class VariablesConstant : IConstant
	{
        /// <summary>
        /// The empty set.
        /// </summary>
        public VariablesConstant()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "Variables"; }
		}

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
			return new ObjectValue(Variables);
		}
	}
}
