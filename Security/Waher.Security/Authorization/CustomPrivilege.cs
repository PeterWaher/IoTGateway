using System;

namespace Waher.Security.Authorization
{
	/// <summary>
	/// Authorization based on a custom privilege.
	/// </summary>
	public class CustomPrivilege<T> : IAuthorization<T>
	{
		private readonly Func<T, string> calculatePrivilege;
		/// <summary>
		/// Authorization based on a custom privilege.
		/// </summary>
		/// <param name="CalculatePrivilege">Function to calculate the privilege required for 
		/// authorization of a resource.</param>
		public CustomPrivilege(Func<T, string> CalculatePrivilege)
		{
			if (CalculatePrivilege is null)
				throw new ArgumentNullException(nameof(CalculatePrivilege));

			this.calculatePrivilege = CalculatePrivilege;
		}

		/// <summary>
		/// Checks if an user or object is authorized to perform an action.
		/// </summary>
		/// <param name="Resource">Resource to authorize access to.</param>
		/// <param name="User">User or object with access privileges.</param>
		/// <returns>If authorized access.</returns>
		public bool IsAuthorized(T Resource, IHasPrivileges User)
		{
			string Privilege = this.calculatePrivilege(Resource);
			return User.HasPrivilege(Privilege);
		}
	}
}
