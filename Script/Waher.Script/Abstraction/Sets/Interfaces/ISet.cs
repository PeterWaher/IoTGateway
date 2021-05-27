using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of sets.
	/// </summary>
	public interface ISet : IElement
	{
		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		bool Contains(IElement Element);

        /// <summary>
        /// Size of set, if finite and known, otherwise null is returned.
        /// </summary>
        int? Size
        {
            get;
        }
	}
}
