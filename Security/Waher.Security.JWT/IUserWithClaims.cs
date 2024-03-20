using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Security.JWT
{
	/// <summary>
	/// A User that can participate in distributed operations, where the user is identified using a JWT token.
	/// </summary>
	public interface IUserWithClaims : IUser
	{
		/// <summary>
		/// Creates a set of claims identifying the user.
		/// </summary>
		/// <param name="Encrypted">If communication is encrypted.</param>
		/// <returns>Set of claims.</returns>
		Task<IEnumerable<KeyValuePair<string, object>>> CreateClaims(bool Encrypted);

		/// <summary>
		/// Creates a JWT Token referencing the user object.
		/// </summary>
		/// <param name="Factory">JWT Factory.</param>
		/// <param name="Encrypted">If communication is encrypted.</param>
		/// <returns>Token, if able to create a token, null otherwise.</returns>
		Task<string> CreateToken(JwtFactory Factory, bool Encrypted);
	}
}
