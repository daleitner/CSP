using System.Collections.Generic;

namespace CSP.Data
{
	public class CSPContainer
	{
		public List<Variable> Assignments { get; set; }
		public List<Constraint> NotMatchedConstraints { get; set; }
	}
}
