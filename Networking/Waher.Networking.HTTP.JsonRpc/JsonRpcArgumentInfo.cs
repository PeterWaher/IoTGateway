using System.Reflection;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about an argument in a method published via a JSON-RPC web service.
	/// </summary>
	internal class JsonRpcArgumentInfo
	{
		public ParameterInfo Parameter;
		public bool HasDefaultValue;
		public bool IsSpecialArgument;
		public object? DefaultValue;

		public JsonRpcArgumentInfo(ParameterInfo Parameter, bool IsSpecialArgument, 
			bool HasDefaultValue, object? DefaultValue)
		{
			this.Parameter = Parameter;
			this.IsSpecialArgument = IsSpecialArgument;
			this.HasDefaultValue = HasDefaultValue;
			this.DefaultValue = DefaultValue;
		}
	}
}
