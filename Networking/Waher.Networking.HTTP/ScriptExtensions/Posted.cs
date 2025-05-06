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
		public string ConstantName => nameof(Posted);

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
			if (!(Variables is SessionVariables SessionVariables))
			{
				if (Variables.ContextVariables is SessionVariables SessionVariables2)
					SessionVariables = SessionVariables2;
				else
					return ObjectValue.Null;
			}

			PostedInformation PostedInfo;

			if ((PostedInfo = SessionVariables.LastPost) is null)
				return ObjectValue.Null;

			if (SessionVariables.CurrentRequest is null ||
				string.Compare(PostedInfo.Resource, SessionVariables.CurrentRequest.SubPath, true) != 0)
			{
				SessionVariables.LastPost = null;
				return ObjectValue.Null;
			}

			if (!PostedInfo.RequestId.HasValue)
				PostedInfo.RequestId = SessionVariables.CurrentRequest.RequestId;
			else if (PostedInfo.RequestId.Value != SessionVariables.CurrentRequest.RequestId)
			{
				SessionVariables.LastPost = null;
				return ObjectValue.Null;
			}

			return PostedInfo.DecodedContent;
		}

	}
}
