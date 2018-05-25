using System.Collections.Generic;

namespace CSP.Data
{
	public class MrvResult
	{
		public Variable Variable { get; set; }
		public List<Domain> LegalValues { get; set; }

		public override string ToString()
		{
			return Variable.Name + " (" + LegalValues?.Count + ")";
		}
	}
}
