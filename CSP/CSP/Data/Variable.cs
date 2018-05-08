namespace CSP.Data
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

		public override string ToString()
		{
			return Name;
		}
	}
}
