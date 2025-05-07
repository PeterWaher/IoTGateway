namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Enumeration of different parameter validation reasons.
	/// </summary>
	public enum ParameterErrorReason
	{
		/// <summary>
		/// The parameter has a script expression for validation. The expression rejected the value.
		/// </summary>
		ScriptExpressionRejection,

		/// <summary>
		/// The parameter has a regular expression for validation. The expression rejected the value.
		/// </summary>
		RegularExpressionRejection,

		/// <summary>
		/// Parameter lacks a value.
		/// </summary>
		LacksValue,

		/// <summary>
		/// An exception was raised during validation. See the error text for an exception message.
		/// </summary>
		Exception,

		/// <summary>
		/// Reference is not valid
		/// </summary>
		InvalidReference,

		/// <summary>
		/// Unable to get referenced contract.
		/// </summary>
		UnableToGetContract,

		/// <summary>
		/// Reference contracts namespace is not valid.
		/// </summary>
		InvalidContractNamespace,

		/// <summary>
		/// Reference contracts template ID is not valid.
		/// </summary>
		InvalidTemplateId,

		/// <summary>
		/// Reference contracts provider is not valid.
		/// </summary>
		InvalidProvider,

		/// <summary>
		/// Referenced contract not valid. The error text contains the <see cref="ContractStatus"/> enumerated value specifying the error.
		/// </summary>
		ContractNotValid,

		/// <summary>
		/// Value below minimum value.
		/// </summary>
		BelowMin,

		/// <summary>
		/// Value above maximum value.
		/// </summary>
		AboveMax,

		/// <summary>
		/// Value is too short.
		/// </summary>
		TooShort,

		/// <summary>
		/// Value to too long.
		/// </summary>
		TooLong,

		/// <summary>
		/// Value outside of allowed range.
		/// </summary>
		Outside
	}
}
