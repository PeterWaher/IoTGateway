using System;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Defines a privilege that is required by the user that calls a method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class RequiredPrivilegeAttribute : Attribute
	{
		/// <summary>
		/// Defines a privilege that is required by the user that calls a method.
		/// </summary>
		/// <param name="Privilege">Required privilege.</param>
		public RequiredPrivilegeAttribute(string Privilege)
		{
			this.Privilege = Privilege;
		}

		/// <summary>
		/// Required privilege.
		/// </summary>
		public string Privilege { get; }
	}
}
