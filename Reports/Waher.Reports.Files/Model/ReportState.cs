using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things;
using Waher.Things.Queries;

namespace Waher.Reports.Files.Model
{
	/// <summary>
	/// Class containing the state of a report.
	/// </summary>
	public class ReportState
	{
		/// <summary>
		/// Class containing the state of a report.
		/// </summary>
		/// <param name="Query">Current query object reference.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="ReportFilesNamespace">Language namespace reference for report files.</param>
		/// <param name="Origin">Origin of request to generate report.</param>
		public ReportState(Query Query, Language Language, Variables Variables,
			Namespace ReportFilesNamespace, RequestOrigin Origin)
		{
			this.Query = Query;
			this.Language = Language;
			this.Variables = Variables;
			this.ReportFilesNamespace = ReportFilesNamespace;
			this.Origin = Origin;
			this.Namespace = null;
		}

		/// <summary>
		/// Current query object reference.
		/// </summary>
		public Query Query { get; }

		/// <summary>
		/// Preferred language.
		/// </summary>
		public Language Language { get; }

		/// <summary>
		/// Language namespace reference, if defined in the report.
		/// </summary>
		public Namespace Namespace { get; internal set; }

		/// <summary>
		/// Language namespace reference for report files.
		/// </summary>
		public Namespace ReportFilesNamespace { get; }

		/// <summary>
		/// Current set of variables.
		/// </summary>
		public Variables Variables { get; }

		/// <summary>
		/// Origin of request to generate report.
		/// </summary>
		public RequestOrigin Origin { get; }
	}
}
