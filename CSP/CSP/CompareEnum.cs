using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSP
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
