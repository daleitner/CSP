using System;

namespace CSP.Data
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

		public bool IsSame(Constraint constraint)
		{
			if (constraint.X == X && constraint.Y == Y && constraint.Comparator == Comparator)
				return true;
			if (constraint.X != Y || constraint.Y != X)
				return false;

			return constraint.Comparator == CompareEnum.Equals && Comparator == CompareEnum.Equals ||
			       constraint.Comparator == CompareEnum.NotEquals && Comparator == CompareEnum.NotEquals ||
			       constraint.Comparator == CompareEnum.Greater && Comparator == CompareEnum.Smaller ||
			       constraint.Comparator == CompareEnum.Smaller && Comparator == CompareEnum.Greater ||
			       constraint.Comparator == CompareEnum.GreaterOrEquals && Comparator == CompareEnum.SmallerOrEquals ||
			       constraint.Comparator == CompareEnum.SmallerOrEquals && Comparator == CompareEnum.GreaterOrEquals;
		}

		public override string ToString()
		{
			return X.Name + " " + ComparatorToString() + " " + Y.Name;
		}

		private string ComparatorToString()
		{
			switch (Comparator)
			{
				case CompareEnum.Default:
					return "_";
				case CompareEnum.Equals:
					return "=";
				case CompareEnum.NotEquals:
					return "!=";
				case CompareEnum.Greater:
					return ">";
				case CompareEnum.Smaller:
					return "<";
				case CompareEnum.GreaterOrEquals:
					return ">=";
				case CompareEnum.SmallerOrEquals:
					return "<=";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
