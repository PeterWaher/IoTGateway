using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines a method that returns a Content-Type string or <see cref="ContentTypeAttribute"/> of a multi-row property.
	/// Acts as a hint to clients, on how the text can be edited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class DynamicContentTypeAttribute : Attribute
	{
		private readonly string methodName;

		/// <summary>
		/// Defines a method that returns a Content-Type string or <see cref="ContentTypeAttribute"/> of a multi-row property.
		/// Acts as a hint to clients, on how the text can be edited.
		/// </summary>
		/// <param name="MethodName">Name of method on object that can be used to retrieve a dynamic set of options.</param>
		public DynamicContentTypeAttribute(string MethodName)
		{
			this.methodName = MethodName;
		}

		/// <summary>
		/// Name of method on object that can be used to retrieve a dynamic set of options.
		/// </summary>
		public string MethodName => this.methodName;
	}
}
