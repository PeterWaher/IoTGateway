using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Model
{
	/// <summary>
	/// Type of parameter used in a function definition or a lambda definition.
	/// </summary>
	public enum ArgumentType
	{
		/// <summary>
		/// Normal argument. Passed as-is.
		/// </summary>
		Normal,

		/// <summary>
		/// Scalar argument. If a non-scalar is passed to a scalar argument, the function is canonically extended by repeatedly 
		/// calling it for each scalar member.
		/// </summary>
		Scalar,

		/// <summary>
		/// Vector argument. If a scalar is passed as an argument, it is converted to a vector. If a matrix is passed, the function is 
		/// canonically extended by repeatedly calling it for each row vector of the matrix.
		/// </summary>
		Vector,

        /// <summary>
        /// Set argument. If a scalar is passed as an argument, it is converted to a set. If a matrix is passed, the function is 
        /// canonically extended by repeatedly calling it for each row vector of the matrix.
        /// </summary>
        Set,

        /// <summary>
        /// Matrix argument. If a scalar or a vector is passed as an argument, is is converted to a matrix first.
        /// </summary>
        Matrix
    }

	/// <summary>
	/// Base interface for functions that integrate into the script engine.
	/// </summary>
	public interface IFunction
	{
		/// <summary>
		/// Name of the function
		/// </summary>
		string FunctionName
		{
			get;
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		string[] Aliases
		{
			get;
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		string[] DefaultArgumentNames
		{
			get;
		}

	}
}
