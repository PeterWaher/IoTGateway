using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base interface for constants that integrate into the script engine.
	/// </summary>
	public interface IConstant
	{
		/// <summary>
		/// Name of the constant
		/// </summary>
		string ConstantName
		{
			get;
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		string[] Aliases
		{
			get;
		}

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		IElement GetValueElement(Variables Variables);
	}
}
