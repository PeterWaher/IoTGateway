using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

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
				return ObjectValue.Null;

			if (!(v.ValueObject is PostedInformation PostedInfo))
			{
				Variables.Remove(" LastPost ");
				return ObjectValue.Null;
			}

			if (!Variables.TryGetVariable("Request", out v) ||
				!(v.ValueObject is HttpRequest Request) ||
				string.Compare(PostedInfo.Resource, Request.SubPath, true) != 0)
			{
				Variables.Remove(" LastPost ");
				return ObjectValue.Null;
			}

			if (!PostedInfo.RequestId.HasValue)
				PostedInfo.RequestId = Request.RequestId;
			else if (PostedInfo.RequestId.Value != Request.RequestId)
			{
				Variables.Remove(" LastPost ");
				return ObjectValue.Null;
			}

			return PostedInfo.DecodedContent;
		}

	}
}
