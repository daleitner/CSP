using System.ComponentModel.DataAnnotations;

namespace CSP.Data
{
	public enum CompareEnum
	{
		[Display(Description = " ")]
		Default,
		[Display(Description = "=")]
		Equals,
		[Display(Description = "!=")]
		NotEquals,
		[Display(Description = ">")]
		Greater,
		[Display(Description = "<")]
		Smaller,
		[Display(Description = ">=")]
		GreaterOrEquals,
		[Display(Description = "<=")]
		SmallerOrEquals
	}
}
