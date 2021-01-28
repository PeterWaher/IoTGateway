using System;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Constants
{
	/// <summary>
	/// Points to the language namespace object for the current page.
	/// </summary>
	public class Namespace : IConstant
	{
		/// <summary>
		/// Points to the language namespace object for the current page.
		/// </summary>
		public Namespace()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName => "Namespace";

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases => new string[0];

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			if (Variables.TryGetVariable("Namespace", out Variable v) &&
				v.ValueObject is Waher.Runtime.Language.Namespace Namespace)
			{
				if (Variables.TryGetVariable("Request", out v) &&
					v.ValueObject is HttpRequest Request &&
					Request.Resource.ResourceName == Namespace.Name)
				{
					return new ObjectValue(Namespace);
				}
				else
					Variables.Remove("Namespace");
			}

			Namespace = GetNamespaceAsync(Variables).Result;
			Variables["Namespace"] = Namespace;

			return new ObjectValue(Namespace);
		}

		/// <summary>
		/// Gets the current language for the session.
		/// </summary>
		/// <param name="Session">Session variables.</param>
		/// <returns>Language object</returns>
		public static async Task<Waher.Runtime.Language.Namespace> GetNamespaceAsync(Variables Session)
		{
			Waher.Runtime.Language.Language Language = await Constants.Language.GetLanguageAsync(Session);

			if (Session.TryGetVariable("Namespace", out Variable v))
			{
				if (v.ValueObject is Waher.Runtime.Language.Namespace Namespace)
					return Namespace;
				else if (v.ValueObject is string NamespaceCode)
					return await Language.GetNamespaceAsync(NamespaceCode);
			}

			if (Session.TryGetVariable("Request", out v) &&
				v.ValueObject is HttpRequest Request)
			{
				return await Language.GetNamespaceAsync(Request.Resource.ResourceName);
			}

			return null;
		}

	}
}
