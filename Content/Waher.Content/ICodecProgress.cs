using System.Collections.Generic;

namespace Waher.Content
{
	/// <summary>
	/// Interface for reporting progress about an encoding or decoding.
	/// </summary>
	public interface ICodecProgress
	{
		/// <summary>
		/// Reports an early hint of a resource the recipient may need to
		/// process, in order to be able to process the current content item
		/// being processed.
		/// 
		/// For more information:
		/// https://datatracker.ietf.org/doc/html/rfc8297
		/// https://datatracker.ietf.org/doc/html/rfc8288
		/// https://www.iana.org/assignments/link-relations/link-relations.xhtml
		/// https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes/rel
		/// </summary>
		/// <param name="Resource">Referenced resource.</param>
		/// <param name="Relation">Resource relation to the current content item
		/// being processed.</param>
		/// <param name="AdditionalParameters">Additional parameters that might
		/// be of interest. Array may be null.</param>
		/// <returns></returns>
		string EarlyHint(string Resource, string Relation, 
			params KeyValuePair<string, string>[] AdditionalParameters);
	}
}
