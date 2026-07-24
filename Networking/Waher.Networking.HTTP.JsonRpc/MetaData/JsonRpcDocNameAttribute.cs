using System;

namespace Waher.Networking.HTTP.JsonRpc.MetaData
{
	/// <summary>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class JsonRpcDocNameAttribute : Attribute
	{
		/// <summary>
		/// Provides a name to be used in documentation.
		/// </summary>
		/// <param name="Name">Name to use in documentation.</param>
		public JsonRpcDocNameAttribute(string Name)
		{
			this.Name = Name;
		}

		/// <summary>
		/// Name to use in documentation.
		/// </summary>
		public string Name { get; }
	}
}