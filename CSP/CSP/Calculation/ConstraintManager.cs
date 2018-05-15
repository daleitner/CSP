using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSP.Data;

namespace CSP.Calculation
{
	public class ConstraintManager
	{
		public static List<Constraint> GetNotMatchedConstraints(List<Constraint> constraints, bool isPairwiseDisjunctive)
		{
			if (isPairwiseDisjunctive)
				return constraints.Where(c => c.Comparator == CompareEnum.Equals).ToList();
			return new List<Constraint>();
		}

		public static List<Constraint> GetRedundandConstraints(List<Constraint> constraints, bool isPairwiseDisjunctive)
		{
			var redundandConstraints = new List<Constraint>();
			for (var i = 0; i < constraints.Count; i++)
			{
				for (var j = i+1; j < constraints.Count; j++)
				{
					if(constraints[i].IsSame(constraints[j]))
						redundandConstraints.Add(constraints[j]);
				}
			}
			if (isPairwiseDisjunctive)
				redundandConstraints.AddRange(constraints.Where(c => c.Comparator == CompareEnum.NotEquals));
			return redundandConstraints.Distinct().ToList();
		}
	}
}
