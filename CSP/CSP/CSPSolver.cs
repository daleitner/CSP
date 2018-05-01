using System;
using System.Collections.Generic;
using System.Linq;

namespace CSP
{
	public static class CSPSolver
	{
		public static CSPContainer Solve(List<Variable> variables, List<Domain> domains, List<Constraint> constraints)
		{
			SolveCSP(variables, domains, constraints);
			var notMatched = new List<Constraint>();
			foreach (var constraint in constraints)
			{
				if(!IsSatisfied(constraint))
					notMatched.Add(constraint);
			}
			return new CSPContainer {Assignments = variables, NotMatchedConstraints = notMatched};
		}

		private static bool SolveCSP(List<Variable> variables, List<Domain> domains, List<Constraint> constraints)
		{
			var unassigned = variables.Where(x => x.Value == null).ToList();
			if (unassigned.Count == 0)
				return true;

			var next = MinimumRemainingValues(unassigned, domains, constraints);
			var domainOrder = LeastConstrainingValueOrder(next, domains, constraints);
			foreach (var domain in domainOrder)
			{
				next.Variable.Value = domain;
				if (IsConsistent(constraints))
				{
					var isSuccess = SolveCSP(variables, domains, constraints);
					if (isSuccess)
						return true;
				}
				next.Variable.Value = null;
			}
			return false;
		}

		private static MrvResult MinimumRemainingValues(List<Variable> unassignedVariables, List<Domain> domains, List<Constraint> constraints)
		{
			var legalValues = new List<List<Domain>>();
			foreach (var variable in unassignedVariables)
			{
				var cnt = GetLegalValues(variable, domains, constraints);
				legalValues.Add(cnt);
			}

			var max = legalValues[0].Count;
			MrvResult mrv = new MrvResult {Variable = unassignedVariables[0], LegalValues = legalValues[0]};
			bool allEqual = true;
			for(var i = 1; i< unassignedVariables.Count; i++)
			{
				if (legalValues[i].Count < max)
				{
					max = legalValues[i].Count;
					mrv.Variable = unassignedVariables[i];
					mrv.LegalValues = legalValues[i];
					allEqual = false;
				}
				else if (legalValues[i].Count > max)
					allEqual = false;
			}

			if (allEqual)
			{
				mrv.Variable = MostConstraints(unassignedVariables, constraints);
				mrv.LegalValues = legalValues[unassignedVariables.IndexOf(mrv.Variable)];
			}

			return mrv;
		}

		private static List<Domain> GetLegalValues(Variable variable, List<Domain> domains, List<Constraint> constraints)
		{
			var legalValues = new List<Domain>();
			foreach (var domain in domains)
			{
				variable.Value = domain;
				if (IsLegal(variable, constraints))
					legalValues.Add(domain);
			}
			variable.Value = null;
			return legalValues;
		}

		private static bool IsLegal(Variable variable, List<Constraint> constraints)
		{
			var subset = constraints.Where(x => x.X == variable || x.Y == variable);
			foreach (var constraint in subset)
			{
				if (!IsSatisfied(constraint))
					return false;
			}

			return true;
		}

		private static bool IsConsistent(List<Constraint> constraints)
		{
			foreach (var constraint in constraints)
			{
				if (!IsSatisfied(constraint))
					return false;
			}
			return true;
		}

		private static bool IsSatisfied(Constraint constraint)
		{
			if (constraint.X.Value == null || constraint.Y.Value == null)
				return true;

			switch (constraint.Comparator)
			{
				case CompareEnum.Default:
					throw new ArgumentException("Comperator must not be default");
				case CompareEnum.Equals:
					return constraint.X.Value.Value == constraint.Y.Value.Value;
				case CompareEnum.NotEquals:
					return constraint.X.Value.Value != constraint.Y.Value.Value;
				case CompareEnum.Greater:
					return constraint.X.Value.Value > constraint.Y.Value.Value;
				case CompareEnum.Smaller:
					return constraint.X.Value.Value < constraint.Y.Value.Value;
				case CompareEnum.GreaterOrEquals:
					return constraint.X.Value.Value >= constraint.Y.Value.Value;
				case CompareEnum.SmallerOrEquals:
					return constraint.X.Value.Value <= constraint.Y.Value.Value;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static Variable MostConstraints(List<Variable> unassigned, List<Constraint> constraints)
		{
			var max = -1;
			Variable result = null;
			foreach (var variable in unassigned)
			{
				var count = 0;
				foreach (var constraint in constraints)
				{
					if (constraint.X == variable || constraint.Y == variable)
						count++;
				}

				if (count > max)
				{
					max = count;
					result = variable;
				}
			}

			return result;
		}

		private static List<Domain> LeastConstrainingValueOrder(MrvResult next, List<Domain> domains, List<Constraint> constraints)
		{
			Variable act = next.Variable;
			var neighbours = GetNeighbours(act, constraints).Where(x => x.Value == null).ToList();
			List<Domain> result = new List<Domain>();


			var tmp = new List<Domain>(next.LegalValues);
			while (tmp.Count > 0)
			{
				Domain best = null;
				Dictionary<Variable, int> constrainingValues = new Dictionary<Variable, int>();
				foreach (var neighbour in neighbours)
				{
					constrainingValues.Add(neighbour, 0);
				}
				
				foreach (var domain in tmp)
				{
					act.Value = domain;
					var cnt = CountConstrainingValues(neighbours, domains, constraints);
					if (IsBetter(cnt, constrainingValues))
					{
						constrainingValues = cnt;
						best = domain;
					}
				}

				if (!constrainingValues.ContainsValue(0))
				{
					result.Add(best);
				}
				tmp.Remove(best);
			}

			act.Value = null;
			return result;
		}

		private static bool IsBetter(Dictionary<Variable, int> cnt, Dictionary<Variable, int> constrainingValues)
		{
			foreach (var variable in cnt.Keys)
			{
				if (cnt[variable] < constrainingValues[variable])
					return false;
			}

			return true;
		}

		private static Dictionary<Variable, int> CountConstrainingValues(List<Variable> neighbours, List<Domain> domains, List<Constraint> constraints)
		{
			var cnt = new Dictionary<Variable, int>();
			foreach (var neighbour in neighbours)
			{
				cnt.Add(neighbour, GetLegalValues(neighbour, domains, constraints).Count);
			}

			return cnt;
		}

		private static List<Variable> GetNeighbours(Variable variable, List<Constraint> constraints)
		{
			var subset = constraints.Where(x => x.X == variable || x.Y == variable);
			var neighbours = new List<Variable>();
			foreach (var constraint in subset)
			{
				if (constraint.X == variable && !neighbours.Contains(constraint.Y))
					neighbours.Add(constraint.Y);
				else if (constraint.Y == variable && !neighbours.Contains(constraint.X))
					neighbours.Add(constraint.X);
			}
			return neighbours;
		}
	}
}
