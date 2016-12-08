using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Searching
{
	internal interface IApplicableFilter
	{
		/// <summary>
		/// Checks if the filter applies to the object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Serializer">Corresponding object serializer.</param>
		/// <returns>If the filter can be applied.</returns>
		bool AppliesTo(object Object, IObjectSerializer Serializer);
	}
}
