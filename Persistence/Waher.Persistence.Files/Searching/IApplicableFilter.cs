using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Searching
{
	internal interface IApplicableFilter
	{
		/// <summary>
		/// Checks if the filter applies to the object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Serializer">Corresponding object serializer.</param>
		/// <param name="Provider">Files provider.</param>
		/// <returns>If the filter can be applied.</returns>
		bool AppliesTo(object Object, IObjectSerializer Serializer, FilesProvider Provider);

		/// <summary>
		/// Gets an array of constant fields. Can return null, if there are no constant fields.
		/// </summary>
		string[] ConstantFields
		{
			get;
		}
	}
}
