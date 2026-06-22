using System.Reflection;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about an argument in a method published via a JSON-RPC web service.
	/// </summary>
	public class JsonRpcArgumentInfo
	{
		/// <summary>
		/// Information about an argument in a method published via a JSON-RPC web service.
		/// </summary>
		/// <param name="Parameter">Parameter information.</param>
		/// <param name="IsSpecialArgument">If the argument represents a special argument.</param>
		/// <param name="HasDefaultValue">If the argument has a default value.</param>
		/// <param name="DefaultValue">Default value of argument, if defined.</param>
		public JsonRpcArgumentInfo(ParameterInfo Parameter, bool IsSpecialArgument, 
			bool HasDefaultValue, object? DefaultValue)
		{
			this.Parameter = Parameter;
			this.IsSpecialArgument = IsSpecialArgument;
			this.HasDefaultValue = HasDefaultValue;
			this.DefaultValue = DefaultValue;
		}

		/// <summary>
		/// Parameter information.
		/// </summary>
		public ParameterInfo Parameter { get; private set; }

		/// <summary>
		/// If the argument has a default value.
		/// </summary>
		public bool HasDefaultValue { get; private set; }

		/// <summary>
		/// Default value of argument, if defined.
		/// </summary>
		public object? DefaultValue { get; private set; }

		/// <summary>
		/// If the argument represents a special argument.
		/// </summary>
		public bool IsSpecialArgument { get; private set; }
	}
}
