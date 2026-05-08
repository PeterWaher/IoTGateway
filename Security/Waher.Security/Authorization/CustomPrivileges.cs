using System;

namespace Waher.Security.Authorization
{
	/// <summary>
	/// Authorization based on a custom privileges.
	/// </summary>
	public class CustomPrivileges<T> : IAuthorization<T>
	{
		private readonly Func<T, string[]> calculatePrivileges;
		/// <summary>
		/// Authorization based on a custom privileges.
		/// </summary>
		/// <param name="CalculatePrivileges">Function to calculate the privileges required for 
		/// authorization of a resource.</param>
		public CustomPrivileges(Func<T, string[]> CalculatePrivileges)
		{
			if (CalculatePrivileges is null)
				throw new ArgumentNullException(nameof(CalculatePrivileges));

			this.calculatePrivileges = CalculatePrivileges;
		}

		/// <summary>
		/// Checks if an user or object is authorized to perform an action.
		/// </summary>
		/// <param name="Resource">Resource to authorize access to.</param>
		/// <param name="User">User or object with access privileges.</param>
		/// <returns>If authorized access.</returns>
		public bool IsAuthorized(T Resource, IHasPrivileges User)
		{
			string[] Privileges = this.calculatePrivileges(Resource);
			int i, c = Privileges.Length;

			for (i = 0; i < c; i++)
			{
				if (!User.HasPrivilege(Privileges[i]))
					return false;
			}

			return true;
		}
	}
}
