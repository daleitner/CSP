using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CSP.Data;

namespace CSP.Calculation
{
	public static class CSPSolver
	{
		public static CSPContainer Solve(List<Variable> variables, List<Domain> domains, List<Constraint> constraints, bool isPairwiseDisjunct, BackgroundWorker worker)
		{
			var notMatched = ConstraintManager.GetNotMatchedConstraints(constraints, isPairwiseDisjunct);
			foreach (var constraint in notMatched)
			{
				constraints.Remove(constraint);
			}

			var nodes = constraints.Select(x => x.X).ToList();
			nodes.AddRange(constraints.Select(x => x.Y));
			nodes = nodes.Distinct().ToList();
			nodes = SortNodesByConstraints(nodes, constraints);
			int level = 2;
			worker.ReportProgress(0);
			do
			{
				level++;
				var circles = ConstraintManager.GetCircles(nodes, constraints, level);
				worker.ReportProgress(level*100/nodes.Count);
				var toRemoveConstraints = new List<Constraint>();
				foreach (var circle in circles)
				{
					if (circle.Any(c => toRemoveConstraints.Contains(c)))
						continue;
					var inconsistentConstraint = ConstraintManager.GetInconsistendConstraint(circle, constraints);
					toRemoveConstraints.Add(inconsistentConstraint);
					constraints.Remove(inconsistentConstraint);
				}

				notMatched.AddRange(toRemoveConstraints.Distinct());
			} while (level <= nodes.Count);

			worker.ReportProgress(0);
			var str = "";
			notMatched.ForEach(x => str += x + "\n");
			/*var redundandConstraints = ConstraintManager.GetRedundandConstraints(constraints, isPairwiseDisjunct);
			foreach (var redundandConstraint in redundandConstraints)
			{
				constraints.Remove(redundandConstraint);
			}*/

			SetVariablesWithNoConstraints(variables, domains, constraints, isPairwiseDisjunct, worker);
			var dict = new Dictionary<Variable, List<Domain>>();
			foreach (var variable in variables.Where(x => x.Value == null))
			{
				dict.Add(variable, new List<Domain>(domains));
			}

			if (isPairwiseDisjunct)
				SolvePairwiseDisjunctCSP(variables.Where(x => x.Value == null).ToList(), domains, variables.Count, constraints, worker);
			else if (!SolveCSP(dict, variables.Count, constraints, isPairwiseDisjunct, worker))
				return new CSPContainer {Assignments = new List<Variable>(), NotMatchedConstraints = new List<Constraint>()};
			
			foreach (var constraint in constraints)
			{
				if(!IsSatisfied(constraint))
					notMatched.Add(constraint);
			}

			for (var index = 0; index < notMatched.Count; index++)
			{
				var constraint = notMatched[index];
				if (IsSatisfied(constraint))
				{
					notMatched.Remove(constraint);
					index--;
				}
			}

			return new CSPContainer {Assignments = variables, NotMatchedConstraints = notMatched};
		}

		private static List<Variable> SortNodesByConstraints(List<Variable> nodes, List<Constraint> constraints)
		{
			var result = new List<Variable>();
			var tmp = new List<Variable>(nodes);
			while (tmp.Any())
			{
				var max = 0;
				Variable maxNode = null;
				foreach (var variable in tmp)
				{
					var cnt = constraints.Count(x => x.X == variable || x.Y == variable);
					if (cnt > max)
					{
						max = cnt;
						maxNode = variable;
					}
				}
				result.Add(maxNode);
				tmp.Remove(maxNode);
			}
			return result;
		}

		private static void SetVariablesWithNoConstraints(List<Variable> variables, List<Domain> domains, List<Constraint> constraints, bool isPairwiseDisjunct, BackgroundWorker worker)
		{
			foreach (var variable in variables)
			{
				if (constraints.Count(constraint => constraint.X == variable || constraint.Y == variable) > 0)
					continue;

				var maxDomain = domains.First();
				foreach (var domain in domains)
				{
					if (domain.Value > maxDomain.Value)
						maxDomain = domain;
				}

				variable.Value = maxDomain;
				if (isPairwiseDisjunct)
					domains.Remove(maxDomain);
				worker.ReportProgress(variables.Count(x => x.Value != null) * 100 / variables.Count);
			}
		}

		private static bool SolveCSP(Dictionary<Variable, List<Domain>> variables, int totalVariables, List<Constraint> constraints, bool isPairwiseDisjunct, BackgroundWorker worker)
		{
			if (variables.Count == 0)
				return true;
			UpdateRemainingValues(variables, constraints, isPairwiseDisjunct);
			var next = MinimumRemainingValues(variables, constraints);
			if (!next.LegalValues.Any())
				return false;

			var domainOrder = LeastConstrainingValueOrder(next, variables, constraints, isPairwiseDisjunct);
			foreach (var domain in domainOrder)
			{
				next.Variable.Value = domain;
				if (isPairwiseDisjunct)
					variables.Keys.ToList().ForEach(x => variables[x].Remove(domain));
				var variableDomains = variables[next.Variable];
				variables.Remove(next.Variable);

				worker.ReportProgress((totalVariables - variables.Count)*100/totalVariables);
				if (IsConsistent(constraints))
				{
					var tmpVariables = new Dictionary<Variable, List<Domain>>();
					foreach (var variable in variables.Keys)
					{
						tmpVariables.Add(variable, new List<Domain>(variables[variable]));
					}
					var isSuccess = SolveCSP(tmpVariables, totalVariables, constraints, isPairwiseDisjunct, worker);
					if (isSuccess)
						return true;
				}
				variables.Add(next.Variable, variableDomains);
				if (isPairwiseDisjunct)
					variables.Keys.ToList().ForEach(x => variables[x].Add(domain));
				next.Variable.Value = null;
			}
			return false;
		}

		private static bool SolvePairwiseDisjunctCSP(List<Variable> variables, List<Domain> domains, int totalVariables,
			List<Constraint> constraints, BackgroundWorker worker)
		{
			var sortedVariables = GetSortedVariables(variables, constraints, totalVariables, worker);
			var sortedDomains = domains.OrderBy(x => x.Value).ToList();
			for (var i = 0; i < sortedVariables.Count; i++)
			{
				sortedVariables[i].Value = sortedDomains[i];
			}
			return true;
		}

		private static List<Variable> GetSortedVariables(List<Variable> variables, List<Constraint> constraints, int totalVariables, BackgroundWorker worker)
		{
			var sortedVariables = new List<Variable> { constraints.First().X };
			var variablesDict = new Dictionary<Variable, bool> { { constraints.First().X, false} };
			var stack = new List<Variable> { constraints.First().X };
			while (stack.Any())
			{
				worker.ReportProgress(variablesDict.Values.Count(x => x)/totalVariables);
				var currentVariable = stack.First();
				var currentConstraints = constraints.Where(x => x.X == currentVariable || x.Y == currentVariable);
				foreach (var constraint in currentConstraints)
				{
					var neighbour = constraint.X == currentVariable ? constraint.Y : constraint.X;
					if (!sortedVariables.Contains(neighbour))
					{
						var indexToInsert = sortedVariables.IndexOf(currentVariable);
						if (neighbour == constraint.X && constraint.Comparator == CompareEnum.Greater ||
						    neighbour == constraint.Y && constraint.Comparator == CompareEnum.Smaller)
							indexToInsert++;
						sortedVariables.Insert(indexToInsert, neighbour);
						variablesDict.Add(neighbour, false);
						stack.Add(neighbour);
					}
					else
					{
						if (neighbour == constraint.X && constraint.Comparator == CompareEnum.Greater)
						{
							if (sortedVariables.IndexOf(neighbour) > sortedVariables.IndexOf(currentVariable))
								continue;
							sortedVariables.Remove(neighbour);
							sortedVariables.Insert(sortedVariables.IndexOf(currentVariable)+1, neighbour);
							if(variablesDict[neighbour])
								stack.Insert(1, neighbour);
						}
						else if (neighbour == constraint.Y && constraint.Comparator == CompareEnum.Smaller)
						{
							if (sortedVariables.IndexOf(currentVariable) < sortedVariables.IndexOf(neighbour))
								continue;
							sortedVariables.Remove(neighbour);
							sortedVariables.Insert(sortedVariables.IndexOf(currentVariable) + 1, neighbour);
							if (variablesDict[neighbour])
								stack.Insert(1, neighbour);
						}
						else if (neighbour == constraint.X && constraint.Comparator == CompareEnum.Smaller)
						{
							if (sortedVariables.IndexOf(neighbour) < sortedVariables.IndexOf(currentVariable))
								continue;
							sortedVariables.Remove(neighbour);
							sortedVariables.Insert(sortedVariables.IndexOf(currentVariable), neighbour);
							if (variablesDict[neighbour])
								stack.Insert(1, neighbour);
						} 
						else if(neighbour == constraint.Y && constraint.Comparator == CompareEnum.Greater)
						{
							if(sortedVariables.IndexOf(currentVariable) > sortedVariables.IndexOf(neighbour))
								continue;
							sortedVariables.Remove(neighbour);
							sortedVariables.Insert(sortedVariables.IndexOf(currentVariable), neighbour);
							if (variablesDict[neighbour])
								stack.Insert(1, neighbour);
						}
					}
				}

				variablesDict[currentVariable] = true;
				stack.RemoveAt(0);
			}
			return sortedVariables;
		}

		private static void UpdateRemainingValues(Dictionary<Variable, List<Domain>> variables, List<Constraint> constraints, bool isPairwiseDisjunct)
		{
			foreach (var variable in variables.Keys)
			{
				for (var index = 0; index < variables[variable].Count; index++)
				{
					var domain = variables[variable][index];
					variable.Value = domain;
					if (!IsConsistent(constraints))
					{
						variables[variable].Remove(domain);
						index--;
					}
					variable.Value = null;
				}
			}
		}

		private static MrvResult MinimumRemainingValues(Dictionary<Variable, List<Domain>> unassignedVariables, List<Constraint> constraints)
		{
			var mrv = new MrvResult {Variable = unassignedVariables.Keys.First(), LegalValues = unassignedVariables[unassignedVariables.Keys.First()]};
			var min = unassignedVariables[unassignedVariables.Keys.First()].Count;
			var mrvVariables = new List<Variable>();
			foreach(var unassignedVariable in unassignedVariables.Keys)
			{
				var legalValues = unassignedVariables[unassignedVariable];
				if (legalValues.Count < min)
				{
					min = legalValues.Count;
					mrv.Variable = unassignedVariable;
					mrv.LegalValues = legalValues;
					mrvVariables = new List<Variable> {unassignedVariable};
				}
				else if (legalValues.Count == min)
					mrvVariables.Add(unassignedVariable);
			}

			mrv.Variable = GetVariableWithMostConstraints(mrvVariables, constraints);
			mrv.LegalValues = unassignedVariables[mrv.Variable];

			return mrv;
		}

		private static List<Domain> GetLegalValues(Variable variable, List<Domain> domains, List<Constraint> constraints)
		{
			var legalValues = new List<Domain>();
			var subset = constraints.Where(x => x.X == variable || x.Y == variable).ToList();
			foreach (var domain in domains)
			{
				variable.Value = domain;
				if (IsConsistent(subset))
					legalValues.Add(domain);
			}
			variable.Value = null;
			return legalValues;
		}

		private static bool IsConsistent(List<Constraint> constraints)
		{
			return constraints.All(IsSatisfied);
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

		private static Variable GetVariableWithMostConstraints(List<Variable> unassigned, List<Constraint> constraints)
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

		private static List<Domain> LeastConstrainingValueOrder(MrvResult next, Dictionary<Variable, List<Domain>> variables, List<Constraint> constraints, bool isPairwiseDisjunct)
		{
			Variable act = next.Variable;
			var neighbournodes = GetNeighbours(act, constraints).Where(x => x.Value == null).ToList();
			var neighbours = new Dictionary<Variable, List<Domain>>();
			var constrainingValues = new Dictionary<Variable, List<Domain>>();
			foreach (var node in neighbournodes)
			{
				neighbours.Add(node, new List<Domain>(variables[node]));
				constrainingValues.Add(node, new List<Domain>());
			}
			List<Domain> result = new List<Domain>();

			var tmp = new List<Domain>(next.LegalValues);
			while (tmp.Count > 0)
			{
				Domain best = tmp.FirstOrDefault();
				constrainingValues.Keys.ToList().ForEach(x => constrainingValues[x] = new List<Domain>());
				foreach (var domain in tmp)
				{
					act.Value = domain;
					if (isPairwiseDisjunct) 
						neighbours.Keys.ToList().ForEach(x => neighbours[x].Remove(domain));
					var cnt = CountConstrainingValues(neighbours, constraints);
					if (IsBetter(cnt, constrainingValues))
					{
						constrainingValues = cnt;
						best = domain;
					}
					if(isPairwiseDisjunct)
						neighbours.Keys.ToList().ForEach(x => neighbours[x].Add(domain));
				}

				if (NeighboursHaveValuesLeft(act, constrainingValues, constraints, isPairwiseDisjunct))
				{
					result.Add(best);
				}
				tmp.Remove(best);
			}

			act.Value = null;
			return result;
		}

		private static bool NeighboursHaveValuesLeft(Variable variable, Dictionary<Variable, List<Domain>> constrainingValues, List<Constraint> constraints, bool isPairwiseDisjunct)
		{
			if (constrainingValues.Values.Select(x => x.Count).Contains(0))
				return false;
			if (!isPairwiseDisjunct)
				return true;

			var higherNeighbours = constraints.Where(x => x.X == variable && x.Comparator == CompareEnum.Greater).Select(x => x.Y).ToList();
			higherNeighbours.AddRange(constraints.Where(x => x.Y == variable && x.Comparator == CompareEnum.Smaller).Select(x => x.X));
			higherNeighbours = higherNeighbours.Where(x => constrainingValues.Keys.Contains(x)).Distinct().ToList();

			var allLeftDomains = new List<Domain>();
			foreach (var neighbour in higherNeighbours)
			{
				allLeftDomains.AddRange(constrainingValues[neighbour]);
			}
			allLeftDomains = allLeftDomains.Distinct().ToList();
			if (allLeftDomains.Count < higherNeighbours.Count)
				return false;

			var lowerNeighbours = constraints.Where(x => x.X == variable && x.Comparator == CompareEnum.Smaller).Select(x => x.Y).ToList();
			lowerNeighbours.AddRange(constraints.Where(x => x.Y == variable && x.Comparator == CompareEnum.Greater).Select(x => x.X));
			lowerNeighbours = lowerNeighbours.Where(x => constrainingValues.Keys.Contains(x)).Distinct().ToList();

			allLeftDomains = new List<Domain>();
			foreach (var neighbour in lowerNeighbours)
			{
				allLeftDomains.AddRange(constrainingValues[neighbour]);
			}
			allLeftDomains = allLeftDomains.Distinct().ToList();
			if (allLeftDomains.Count < lowerNeighbours.Count)
				return false;
			return true;
		}

		private static bool IsBetter(Dictionary<Variable, List<Domain>> cnt, Dictionary<Variable, List<Domain>> constrainingValues)
		{
			var tmp = cnt.Values.Select(x => x.Count).ToList();
			tmp.Sort();
			var tmp2 = constrainingValues.Values.Select(x => x.Count).ToList();
			tmp2.Sort();
			for (var i = 0; i < tmp.Count; i++)
			{
				if (tmp[i] > tmp2[i])
					return true;
				if (tmp[i] < tmp2[i])
					return false;
			}
			return false;
		}

		private static Dictionary<Variable, List<Domain>> CountConstrainingValues(Dictionary<Variable, List<Domain>> neighbours, List<Constraint> constraints)
		{
			var cnt = new Dictionary<Variable, List<Domain>>();
			foreach (var neighbour in neighbours.Keys)
			{
				cnt.Add(neighbour, GetLegalValues(neighbour, neighbours[neighbour], constraints));
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
