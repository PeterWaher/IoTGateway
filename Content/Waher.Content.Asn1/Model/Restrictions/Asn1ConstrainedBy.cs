using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// CONSTRAINED BY
	/// </summary>
	public class Asn1ConstrainedBy : Asn1ValueRestriction
	{
		private readonly string comment;

		/// <summary>
		/// CONSTRAINED BY
		/// </summary>
		/// <param name="Comment">Comment for user-defined constrained.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ConstrainedBy(string Comment, Asn1Document Document) 
			: base(new Asn1StringValue(Comment, Document), Document)
		{
			this.comment = Comment;
		}

		/// <summary>
		/// Gets checks to perform.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="VariableName">Variable being checked.</param>
		/// <param name="Type">Data type.</param>
		public override Task RenderChecks(StringBuilder Output,
			CSharpExportState State, string VariableName, Asn1Type Type)
		{
			Output.Append("/* ");
			Output.Append(this.comment);
			Output.Append(" */");

			return Task.CompletedTask;
		}
	}
}
