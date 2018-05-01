using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSP
{
	public class MrvResult
	{
		public Variable Variable { get; set; }
		public List<Domain> LegalValues { get; set; }
	}
}
