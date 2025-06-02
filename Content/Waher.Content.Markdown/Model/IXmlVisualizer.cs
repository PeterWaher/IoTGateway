using System.Threading.Tasks;
using System.Xml;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Interface for all XML visalizers.
	/// </summary>
	public interface IXmlVisualizer : IProcessingSupport<XmlDocument>
	{
		/// <summary>
		/// Transforms the XML document before visualizing it.
		/// </summary>
		/// <param name="Xml">XML Document.</param>
		/// <param name="Variables">Current variables.</param>
		/// <returns>Transformed object.</returns>
		Task<object> TransformXml(XmlDocument Xml, Variables Variables);
	}
}
