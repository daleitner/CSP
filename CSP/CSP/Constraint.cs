using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSP
{
	public class Constraint
	{
		public Constraint(int index, Variable x, CompareEnum comparator, Variable y)
		{
			Index = index;
			X = x;
			Y = y;
			Comparator = comparator;
		}
		public int Index { get; }
		public Variable X { get; set; }
		public Variable Y { get; set; }
		public CompareEnum Comparator { get; set; }
	}
}
