using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Interface for classes implementing parameter options.
	/// </summary>
	public interface IParameterOptions
	{
		/// <summary>
		/// Gets the options for the parameter.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Array of options.</returns>
		Task<KeyValuePair<string, string>[]> GetOptions(Variables Variables);
	}
}
