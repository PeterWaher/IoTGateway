namespace Waher.Security
{
	/// <summary>
	/// Interface for objects that have privileges.
	/// </summary>
	public interface IHasPrivileges
	{
		/// <summary>
		/// If the object has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the object has the corresponding privilege.</returns>
		bool HasPrivilege(string Privilege);
	}
}
