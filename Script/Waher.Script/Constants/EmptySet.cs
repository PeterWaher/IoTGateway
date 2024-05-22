using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Constants
{
    /// <summary>
    /// The empty set.
    /// </summary>
    public class EmptySet : IConstant
	{
        /// <summary>
        /// The empty set.
        /// </summary>
        public EmptySet()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName => "∅";

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases => new string[] { nameof(EmptySet) };

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			return set;
		}

		private static readonly Objects.Sets.EmptySet set = new Objects.Sets.EmptySet();
	}
}
