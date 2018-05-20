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
			var nodes = constraints.Select(x => x.X).ToList();
			nodes.AddRange(constraints.Select(x => x.Y));
			nodes = nodes.Distinct().ToList();

			foreach (var variable in nodes)
			{
				transitives.AddRange(GetTransitiveConstraintsForNode(variable, constraints.Where(x => !transitives.Contains(x)).ToList(), null, new List<Variable> { variable}));
			}
			return transitives;
		}

		private static List<Constraint> GetTransitiveConstraintsForNode(Variable sourceNode, List<Constraint> constraints, Constraint currentConstraint, List<Variable> currentVariables)
		{
			var transitives = new List<Constraint>();
			bool transitivesFound;
			do
			{
				transitivesFound = false;
				var inspectedConstraints =
					constraints.Where(x => !transitives.Contains(x) && (x.X == currentVariables.Last() || x.Y == currentVariables.Last()) && x != currentConstraint);
				foreach (var constraint in inspectedConstraints)
				{
					if (transitivesFound)
						break;

					var targetNode = constraint.X == currentVariables.Last() ? constraint.Y : constraint.X;
					if (PossibleLastElementOfChain(constraint, currentConstraint, currentVariables.Last()))
					{
						if (targetNode == sourceNode)
							return new List<Constraint> {constraint};
					}

					if(!IsChain(constraint, currentConstraint, currentVariables.Last()) || currentVariables.Contains(targetNode))
						continue;

					currentVariables.Add(targetNode);
					var foundTransitives = GetTransitiveConstraintsForNode(sourceNode,
						constraints.Where(x => !transitives.Contains(x)).ToList(), constraint, currentVariables);
					currentVariables.Remove(targetNode);

					if (foundTransitives.Any())
					{
						transitives.AddRange(foundTransitives);
						transitivesFound = true;
					}
				}
			} while (transitivesFound);
			return transitives;
		}

		private static bool IsChain(Constraint constraint, Constraint currentConstraint, Variable currentVariable)
		{
			if (currentConstraint == null)
				return true;

			if (constraint.Comparator == CompareEnum.NotEquals)
				return false;

			if (constraint.Comparator == CompareEnum.Equals && currentConstraint.Comparator == CompareEnum.Equals)
				return true;

			var cIsInverted = constraint.Y == currentVariable;
			var ccIsInverted = currentConstraint.X == currentVariable;

			if (!ccIsInverted && !cIsInverted)
			{
				return constraint.Comparator == currentConstraint.Comparator;
			}

			if (!ccIsInverted || !cIsInverted)
			{
				return currentConstraint.Comparator == CompareEnum.Greater && constraint.Comparator == CompareEnum.Smaller ||
				       currentConstraint.Comparator == CompareEnum.Smaller && constraint.Comparator == CompareEnum.Greater;
			}

			return constraint.Comparator == currentConstraint.Comparator;
		}

		private static bool PossibleLastElementOfChain(Constraint constraint, Constraint currentConstraint, Variable currentVariable)
		{
			if (currentConstraint == null)
				return false;

			if (constraint.Comparator == CompareEnum.NotEquals)
				return false;

			if (constraint.Comparator == CompareEnum.Equals && currentConstraint.Comparator == CompareEnum.Equals)
				return true;

			var cIsInverted = constraint.Y == currentVariable;
			var ccIsInverted = currentConstraint.X == currentVariable;

			if (!ccIsInverted && !cIsInverted)
			{
				return currentConstraint.Comparator == CompareEnum.Greater && constraint.Comparator == CompareEnum.Smaller ||
				       currentConstraint.Comparator == CompareEnum.Smaller && constraint.Comparator == CompareEnum.Greater;
			}

			if (!ccIsInverted || !cIsInverted)
			{
				return currentConstraint.Comparator == CompareEnum.Smaller && constraint.Comparator == CompareEnum.Smaller ||
				       currentConstraint.Comparator == CompareEnum.Greater && constraint.Comparator == CompareEnum.Greater;
			}

			return constraint.Comparator != currentConstraint.Comparator;
		}
	}
}
