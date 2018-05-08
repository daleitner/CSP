namespace CSP.Data
{
	public class Domain
	{
		public Domain(string name, int value)
		{
			Name = name;
			Value = value;
		}
		public string Name { get; set; }
		public int Value { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}
