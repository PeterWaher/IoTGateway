using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Represents a localization step, as defined in XEP-323:
	/// http://xmpp.org/extensions/xep-0323.html#localization
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	public class LocalizationStep
	{
		private int stringId;
		private string module;
		private string seed;

		/// <summary>
		/// Represents a localization step, as defined in XEP-323:
		/// http://xmpp.org/extensions/xep-0323.html#localization
		/// </summary>
		/// <param name="StringId">String ID</param>
		public LocalizationStep()
		{
			this.stringId = 0;
			this.module = null;
			this.seed = null;
		}

		/// <summary>
		/// Represents a localization step, as defined in XEP-323:
		/// http://xmpp.org/extensions/xep-0323.html#localization
		/// </summary>
		/// <param name="StringId">String ID</param>
		public LocalizationStep(int StringId)
		{
			this.stringId = StringId;
			this.module = null;
			this.seed = null;
		}

		/// <summary>
		/// Represents a localization step, as defined in XEP-323:
		/// http://xmpp.org/extensions/xep-0323.html#localization
		/// </summary>
		/// <param name="StringId">String ID</param>
		/// <param name="Module">Optional Language Module</param>
		public LocalizationStep(int StringId, string Module)
		{
			this.stringId = StringId;
			this.module = Module;
			this.seed = null;
		}

		/// <summary>
		/// Represents a localization step, as defined in XEP-323:
		/// http://xmpp.org/extensions/xep-0323.html#localization
		/// </summary>
		/// <param name="StringId">String ID</param>
		/// <param name="Module">Optional Language Module</param>
		/// <param name="Seed">Optional Seed value.</param>
		public LocalizationStep(int StringId, string Module, string Seed)
		{
			this.stringId = StringId;
			this.module = Module;
			this.seed = Seed;
		}

		/// <summary>
		/// String ID
		/// </summary>
		[ShortName("i")]
		public int StringId
		{
			get { return this.stringId; }
			set { this.stringId = value; }
		}

		/// <summary>
		/// Optional language module, if different from the base module.
		/// </summary>
		[DefaultValueNull]
		[ShortName("m")]
		public string Module
		{
			get { return this.module; }
			set { this.module = value; }
		}

		/// <summary>
		/// Optional localization seed.
		/// </summary>
		[DefaultValueNull]
		[ShortName("s")]
		public string Seed
		{
			get { return this.seed; }
			set { this.seed = value; }
		}

	}
}
