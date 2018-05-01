using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSP
{
	public class Variable
	{
		public Variable(string name, Domain value)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; set; }
		public Domain Value { get; set; }
	}
}
