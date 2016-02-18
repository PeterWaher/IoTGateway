using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Basic interface for vectors.
	/// </summary>
	public interface IVector
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
