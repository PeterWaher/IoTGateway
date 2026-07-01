using System.Reflection;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about an argument in a protected method.
	/// </summary>
	public class ProtectedMethodArgumentInfo
	{
		/// <summary>
		/// Information about an argument in a protected method.
		/// </summary>
		/// <param name="Parameter">Parameter information.</param>
		/// <param name="IsSpecialArgument">If the argument represents a special argument.</param>
		/// <param name="HasDefaultValue">If the argument has a default value.</param>
		/// <param name="DefaultValue">Default value of argument, if defined.</param>
		/// <param name="IsMetaDataArgument">If the argument represents a meta-data argument.</param>
		public ProtectedMethodArgumentInfo(ParameterInfo Parameter, bool IsSpecialArgument, 
			bool HasDefaultValue, object? DefaultValue, bool IsMetaDataArgument)
		{
			this.Parameter = Parameter;
			this.IsSpecialArgument = IsSpecialArgument;
			this.HasDefaultValue = HasDefaultValue;
			this.DefaultValue = DefaultValue;
			this.IsMetaDataArgument = IsMetaDataArgument;
		}

		/// <summary>
		/// Parameter information.
		/// </summary>
		public ParameterInfo Parameter { get; }

		/// <summary>
		/// If the argument has a default value.
		/// </summary>
		public bool HasDefaultValue { get; }

		/// <summary>
		/// Default value of argument, if defined.
		/// </summary>
		public object? DefaultValue { get; }

		/// <summary>
		/// If the argument represents a special argument.
		/// </summary>
		public bool IsSpecialArgument { get; }

		/// <summary>
		/// If the argument represents a meta-data argument.
		/// </summary>
		public bool IsMetaDataArgument { get; }
	}
}
