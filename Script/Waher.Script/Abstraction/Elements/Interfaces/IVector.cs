using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Abstraction.Elements.Interfaces
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
	}
}
