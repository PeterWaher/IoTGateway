namespace Waher.Security.Authorization
{
	/// <summary>
	/// Authorization based on a single privilege.
	/// </summary>
	public class SinglePrivilege<T> : IAuthorization<T>
	{
		private readonly string privilege;

		/// <summary>
		/// Authorization based on a single privilege.
		/// </summary>
		/// <param name="Privilege">Privilege required for authorization.</param>
		public SinglePrivilege(string Privilege)
		{
			this.privilege = Privilege;
		}

		/// <summary>
		/// Checks if an user or object is authorized to perform an action.
		/// </summary>
		/// <param name="Resource">Resource to authorize access to.</param>
		/// <param name="User">User or object with access privileges.</param>
		/// <returns>If authorized access.</returns>
		public bool IsAuthorized(T Resource, IHasPrivileges User)
		{
			return User.HasPrivilege(this.privilege);
		}
	}
}
