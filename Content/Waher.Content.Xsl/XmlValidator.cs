using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using Waher.Events;

namespace Waher.Content.Xsl
{
	/// <summary>
	/// Class performing XML validation.
	/// </summary>
	internal class XmlValidator
	{
		private readonly string objectId = null;
		private XmlSchemaException exception;

		public XmlValidator(string ObjectID)
		{
			this.objectId = ObjectID;
			this.exception = null;
		}

		internal void ValidationCallback(object Sender, ValidationEventArgs e)
		{
			switch (e.Severity)
			{
				case XmlSeverityType.Error:
					this.exception = e.Exception;
					break;

				case XmlSeverityType.Warning:
					if (!string.IsNullOrEmpty(this.objectId))
						Log.Warning(e.Message, this.objectId);
					break;
			}
		}

		internal XmlSchemaException Exception { get { return this.exception; } }
		internal bool HasError { get { return this.exception != null; } }

		internal void AssertNoError()
		{
			if (this.exception != null)
				throw this.exception;
		}
	}

}
