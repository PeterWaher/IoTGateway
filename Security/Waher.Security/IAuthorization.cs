namespace Waher.Security
{
	/// <summary>
	/// Basic authorization interface for objects of type <typeparamref name="T"/>.
	/// </summary>
	public interface IAuthorization<T>
	{
		/// <summary>
		/// Checks if an user or object is authorized to perform an action.
		/// </summary>
		/// <param name="Resource">Resource to authorize access to.</param>
		/// <param name="User">User or object with access privileges.</param>
		/// <returns>If authorized access.</returns>
		bool IsAuthorized(T Resource, IHasPrivileges User);
	}
}
