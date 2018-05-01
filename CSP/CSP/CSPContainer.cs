using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSP
{
	public class CSPContainer
	{
		public List<Variable> Assignments { get; set; }
		public List<Constraint> NotMatchedConstraints { get; set; }
	}
}
