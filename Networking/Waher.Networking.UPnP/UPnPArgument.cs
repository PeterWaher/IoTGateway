using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Direction of action arguments.
	/// </summary>
	public enum ArgumentDirection
	{
		/// <summary>
		/// Input variable
		/// </summary>
		In,

		/// <summary>
		/// Output variable
		/// </summary>
		Out
	}

	/// <summary>
	/// Contains information about an argument.
	/// </summary>
	public class UPnPArgument
	{
		private readonly XmlElement xml;
		private readonly string name;
		private readonly ArgumentDirection direction;
		private readonly bool returnValue;
		private readonly string relatedStateVariable;

		internal UPnPArgument(XmlElement Xml)
		{
			this.xml = Xml;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "name":
						this.name = N.InnerText;
						break;

					case "direction":
						if (N.InnerText.ToLower() == "out")
							this.direction = ArgumentDirection.Out;
						else
							this.direction = ArgumentDirection.In;
						break;

					case "retval":
						this.returnValue = true;
						break;

					case "relatedStateVariable":
						this.relatedStateVariable = N.InnerText;
						break;
				}
			}
		}

		/// <summary>
		/// Underlying XML definition.
		/// </summary>
		public XmlElement Xml
		{
			get { return this.xml; }
		}

		/// <summary>
		/// Argument Name
		/// </summary>
		public string Name { get { return this.name; } }

		/// <summary>
		/// Argument Direction
		/// </summary>
		public ArgumentDirection Direction { get { return this.direction; } }

		/// <summary>
		/// If the argument is the return value
		/// </summary>
		public bool ReturnValue { get { return this.returnValue; } }

		/// <summary>
		/// Related State Variable
		/// </summary>
		public string RelatedStateVariable { get { return this.relatedStateVariable; } }

	}
}
