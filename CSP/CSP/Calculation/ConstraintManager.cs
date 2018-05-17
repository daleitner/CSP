using System;
using System.Collections.Generic;
using System.Globalization;
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
			var redundandConstraints = GetDuplicates(constraints);

			if (isPairwiseDisjunctive)
				redundandConstraints.AddRange(constraints.Where(c => c.Comparator == CompareEnum.NotEquals));
			redundandConstraints = redundandConstraints.Distinct().ToList();
			redundandConstraints.AddRange(GetTransitiveConstraints(constraints.Where(x => !redundandConstraints.Contains(x)).ToList()));
			return redundandConstraints.Distinct().ToList();
		}

		private static List<Constraint> GetDuplicates(List<Constraint> constraints)
		{
			var redundandConstraints = new List<Constraint>();
			for (var i = 0; i < constraints.Count; i++)
			{
				for (var j = i + 1; j < constraints.Count; j++)
				{
					if (constraints[i].IsSame(constraints[j]))
						redundandConstraints.Add(constraints[j]);
				}
			}
			return redundandConstraints;
		}

		private static List<Constraint> GetTransitiveConstraints(List<Constraint> constraints)
		{
			var transitives = new List<Constraint>();
			bool foundtransitive;
			do
			{
				foundtransitive = false;
				var remainingConstraints = constraints.Where(x => !transitives.Contains(x)).ToList();
				for (var i = 0; i < remainingConstraints.Count && !foundtransitive; i++)
				{
					if (remainingConstraints[i].Comparator == CompareEnum.Equals)
					{
						var chainConstraints = remainingConstraints.Where(x => x.Comparator == CompareEnum.Equals && x.X == remainingConstraints[i].Y).ToList();
						for (var j = 0; j < chainConstraints.Count && !foundtransitive; j++)
						{
							var transitiveConstraint = remainingConstraints.FirstOrDefault(x => x.Comparator == CompareEnum.Equals &&
								(x.X == remainingConstraints[i].X && x.Y == chainConstraints[j].Y ||
								x.Y == remainingConstraints[i].X && x.X == chainConstraints[j].Y));
							if (transitiveConstraint == null)
								continue;

							transitives.Add(transitiveConstraint);
							foundtransitive = true;
						}
					}
				}

			} while (foundtransitive);
			return transitives;
		}
	}
}
