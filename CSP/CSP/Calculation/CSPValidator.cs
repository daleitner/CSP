using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSP.Data;

namespace CSP.Calculation
{
	public class CSPValidator
	{
		public static bool Validate(List<Variable> variables, List<Domain> domains, List<Constraint> constraints, bool isPairwiseDisjunctive)
		{
			if (isPairwiseDisjunctive && variables.Count > domains.Count)
				throw new Exception("CSP is invalid, because there are more Variables than Domains. Add some additional Domains or remove the pairwise disjunctive flag");

			foreach (var constraint in constraints)
			{
				if(constraint.X == null || constraint.Y == null)
					throw new Exception("Error Constraint [" + constraint.Index + "]: Constraints must have set two variables. Value null is not allowed!");
				if(constraint.Comparator == CompareEnum.Default)
					throw new Exception("Error Constraint [" + constraint.Index + "]: Comparator is not set!");
				if (constraint.X == constraint.Y)
					throw new Exception("Error Constraint [" + constraint.Index + "]: Constraint to same variable is not allowed!");
			}
			return true;
		}
	}
}
