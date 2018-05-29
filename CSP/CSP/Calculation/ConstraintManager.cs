using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		public static List<List<Constraint>> GetCircles(List<Constraint> constraints, int maxLevel)
		{
			var circles = new List<List<Constraint>>();
			var nodes = constraints.Select(x => x.X).ToList();
			nodes.AddRange(constraints.Select(x => x.Y));
			nodes = nodes.Distinct().ToList();
			var compareConstraints = constraints
				.Where(x => x.Comparator == CompareEnum.Greater || x.Comparator == CompareEnum.Smaller).ToList();
			var inspectedNodes = new List<Variable>();
			for (var index = 0; index < nodes.Count; index++)
			{
				var variable = nodes[index];
				var nodeCircles = GetCompareCirclesRecursive(variable, compareConstraints.Where(x => !inspectedNodes.Contains(x.X) && !inspectedNodes.Contains(x.Y)).ToList(), 
					new List<Constraint>(), new List<Variable> {variable}, nodes.GetRange(0, index), maxLevel, 1);
				circles.AddRange(nodeCircles);
				inspectedNodes.Add(variable);
			}

			for (var i = 0; i < circles.Count; i++)
			{
				for (var j = i + 1; j < circles.Count; j++)
				{
					if(circles[i].Count != circles[j].Count)
						continue;
					if (circles[i].All(edge => circles[j].Contains(edge)))
					{
						circles.RemoveAt(j);
						j--;
					}
				}
			}
			return circles;
		}

		private static List<List<Constraint>> GetCompareCirclesRecursive(Variable root, List<Constraint> constraints, List<Constraint> path, List<Variable> currentNodes, List<Variable> inspectedNodes, int maxLevel, int currentLevel)
		{
			if(currentLevel > maxLevel)
				return new List<List<Constraint>>();
			var circles = new List<List<Constraint>>();
			var edges = constraints.Where(x => (x.X == currentNodes.Last() || x.Y == currentNodes.Last()) && x != path.LastOrDefault());

			foreach (var edge in edges)
			{
				var targetNode = edge.X == currentNodes.Last() ? edge.Y : edge.X;
				if (path.Any())
				{
					if (edge.X == currentNodes.Last())
					{
						if (path.Last().X == edge.X)
						{
							if (edge.Comparator == path.Last().Comparator)
								continue;
						}
						else
						{
							if (edge.Comparator != path.Last().Comparator)
								continue;
						}
					}
					else
					{
						if (path.Last().X == edge.Y)
						{
							if (edge.Comparator != path.Last().Comparator)
								continue;
						}
						else
						{
							if (edge.Comparator == path.Last().Comparator)
								continue;
						}
					}
				}

				if (inspectedNodes.Contains(targetNode))
					continue;

				path.Add(edge);
				if (targetNode == root)
				{
					circles.Add(new List<Constraint>(path));
				}
				else if(!currentNodes.Contains(targetNode))
				{
					currentNodes.Add(targetNode);
					circles.AddRange(GetCompareCirclesRecursive(root, constraints, path, currentNodes, inspectedNodes, maxLevel, currentLevel+1));
					currentNodes.Remove(targetNode);
				}
				path.Remove(edge);
			}
			return circles;
		}

		private static CompareEnum Opposite(CompareEnum comparison)
		{
			switch (comparison)
			{
				case CompareEnum.Equals:
					return CompareEnum.NotEquals;
				case CompareEnum.NotEquals:
					return CompareEnum.Equals;
				case CompareEnum.Greater:
					return CompareEnum.Smaller;
				case CompareEnum.Smaller:
					return CompareEnum.Greater;
				default:
					return CompareEnum.Default;
			}
		}

		public static Constraint GetInconsistendConstraint(List<Constraint> circle, List<Constraint> constraints)
		{
			var maxContradictions = int.MinValue;
			var constraintWithMaxContradictions = circle.Last();
			foreach (var edge in circle)
			{
				var contradictions = CountContradictions(edge, constraints);
				if (contradictions >= maxContradictions)
				{
					maxContradictions = contradictions;
					constraintWithMaxContradictions = edge;
				}
			}
			return constraintWithMaxContradictions;
		}

		private static int CountContradictions(Constraint edge, List<Constraint> constraints)
		{
			var counter = 0;
			var nodes = GetRemainingNodes(edge, constraints);
			foreach (var node in nodes)
			{
				if (FoundContradiction(node, edge, constraints))
					counter++;
				if (FoundConfirmation(node, edge, constraints))
					counter--;
			}

			return counter;
		}

		private static bool FoundContradiction(Variable node, Constraint edge, List<Constraint> constraints)
		{
			if (constraints.Any(x => x.X == node && x.Y == edge.X && x.Comparator == edge.Comparator) &&
			    constraints.Any(x => x.X == node && x.Y == edge.Y && x.Comparator == Opposite(edge.Comparator)))
				return true;
			if (constraints.Any(x => x.X == node && x.Y == edge.X && x.Comparator == edge.Comparator) &&
			    constraints.Any(x => x.X == edge.Y && x.Y == node && x.Comparator == edge.Comparator))
				return true;
			if (constraints.Any(x => x.X == edge.X && x.Y == node && x.Comparator == Opposite(edge.Comparator)) &&
			    constraints.Any(x => x.X == node && x.Y == edge.Y && x.Comparator == Opposite(edge.Comparator)))
				return true;
			if (constraints.Any(x => x.X == edge.X && x.Y == node && x.Comparator == Opposite(edge.Comparator)) &&
			    constraints.Any(x => x.X == edge.Y && x.Y == node && x.Comparator == edge.Comparator))
				return true;
			return false;
		}

		private static bool FoundConfirmation(Variable node, Constraint edge, List<Constraint> constraints)
		{
			if (constraints.Any(x => x.X == node && x.Y == edge.X && x.Comparator == Opposite(edge.Comparator)) &&
			    constraints.Any(x => x.X == node && x.Y == edge.Y && x.Comparator == edge.Comparator))
				return true;
			if (constraints.Any(x => x.X == node && x.Y == edge.X && x.Comparator == Opposite(edge.Comparator)) &&
			    constraints.Any(x => x.X == edge.Y && x.Y == node && x.Comparator == Opposite(edge.Comparator)))
				return true;
			if (constraints.Any(x => x.X == edge.X && x.Y == node && x.Comparator == edge.Comparator) &&
			    constraints.Any(x => x.X == node && x.Y == edge.Y && x.Comparator == edge.Comparator))
				return true;
			if (constraints.Any(x => x.X == edge.X && x.Y == node && x.Comparator == edge.Comparator) &&
			    constraints.Any(x => x.X == edge.Y && x.Y == node && x.Comparator == Opposite(edge.Comparator)))
				return true;
			return false;
		}

		private static List<Variable> GetRemainingNodes(Constraint edge, List<Constraint> constraints)
		{
			var nodes = constraints.Select(x => x.X).ToList();
			nodes.AddRange(constraints.Select(x => x.Y));
			nodes = nodes.Distinct().ToList();
			nodes.Remove(edge.X);
			nodes.Remove(edge.Y);
			return nodes;
		}
	}
}
