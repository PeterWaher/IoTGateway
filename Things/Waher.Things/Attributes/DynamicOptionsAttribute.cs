using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines an option to display when editing the parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class DynamicOptionsAttribute : Attribute
	{
		private readonly string methodName;

		/// <summary>
		/// Defines a method for retrieving a dynamic set of options to display when editing the parameter.
		/// </summary>
		/// <param name="MethodName">Name of method on object that can be used to retrieve a dynamic set of options.</param>
		public DynamicOptionsAttribute(string MethodName)
		{
			this.methodName = MethodName;
		}

		/// <summary>
		/// Name of method on object that can be used to retrieve a dynamic set of options.
		/// </summary>
		public string MethodName => this.methodName;
	}
}
