namespace CSP
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			var viewModel = new MainViewModel();
			DataContext = viewModel;
		}
	}
}
