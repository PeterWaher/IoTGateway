using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Controls.Questions
{
	public class CanReadQuestion : NodeQuestion
	{
		private string[] fieldNames = null;
		private string[] availableFieldNames = null;
		private FieldType categories = FieldType.All;

		public CanReadQuestion()
			: base()
		{
		}

		[DefaultValueNull]
		public string[] FieldNames
		{
			get { return this.fieldNames; }
			set { this.fieldNames = value; }
		}

		[DefaultValueNull]
		public string[] AvailableFieldNames
		{
			get { return this.availableFieldNames; }
			set { this.availableFieldNames = value; }
		}

		[DefaultValue(FieldType.All)]
		public FieldType Categories
		{
			get { return this.categories; }
			set { this.categories = value; }
		}

		public override string QuestionString => "Can read?";
	}
}
