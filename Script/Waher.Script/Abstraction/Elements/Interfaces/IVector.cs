using System;
using System.Collections.Generic;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for vectors.
	/// </summary>
	public interface IVector : IElement
	{
		/// <summary>
		/// Dimension of vector.
		/// </summary>
		int Dimension
		{
			get;
		}

		/// <summary>
		/// An enumeration of vector elements.
		/// </summary>
		ICollection<IElement> VectorElements
		{
			get;
		}

        /// <summary>
        /// Gets an element of the vector.
        /// </summary>
        /// <param name="Index">Zero-based index into the vector.</param>
        /// <returns>Vector element.</returns>
        IElement GetElement(int Index);

        /// <summary>
        /// Sets an element in the vector.
        /// </summary>
        /// <param name="Index">Index.</param>
        /// <param name="Value">Element to set.</param>
        void SetElement(int Index, IElement Value);
	}
}
