using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions
{
	/// <summary>
	/// Decoded data posted to the resource
	/// </summary>
	public class Posted : IConstant
	{
		/// <summary>
		/// Decoded data posted to the resource
		/// </summary>
		public Posted()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "Posted"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			if (!Variables.TryGetVariable(" LastPost ", out Variable v))
				return null;

			IElement Result = v.ValueElement;

			if (!Variables.TryGetVariable(" LastPostResource ", out v) ||
				!(v.ValueObject is string SubPath) ||
				!Variables.TryGetVariable("Request", out v) ||
				!(v.ValueObject is HttpRequest Request) ||
				string.Compare(SubPath, Request.SubPath, true) != 0)
			{
				return null;
			}

			return Result;
		}

	}
}
