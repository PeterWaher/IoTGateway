namespace Waher.Security.Authorization
{
	/// <summary>
	/// Authorization based on multiple privileges.
	/// </summary>
	public class MultiplePrivileges<T> : IAuthorization<T>
	{
		private readonly string[] privileges;
		private readonly int count;

		/// <summary>
		/// Authorization based on multiple privileges.
		/// </summary>
		/// <param name="Privileges">Privileges required for authorization.</param>
		public MultiplePrivileges(string[] Privileges)
		{
			this.privileges = Privileges;
			this.count = Privileges?.Length ?? 0;
		}

		/// <summary>
		/// Checks if an user or object is authorized to perform an action.
		/// </summary>
		/// <param name="Resource">Resource to authorize access to.</param>
		/// <param name="User">User or object with access privileges.</param>
		/// <returns>If authorized access.</returns>
		public bool IsAuthorized(T Resource, IHasPrivileges User)
		{
			for (int i = 0; i < this.count; i++)
			{
				if (!User.HasPrivilege(this.privileges[i]))
					return false;
			}

			return true;
		}
	}
}
