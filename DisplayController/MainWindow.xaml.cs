using System.Windows;

namespace DisplayController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DisplayManager.SaveCurrentDisplaySettings();
		}

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            DisplayManager.SetDisplayResolution(1024, 768);
            DisplayManager.SetDisplayScaling(100);
        }
        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            DisplayManager.RevertDisplaySettings();
		}
	}
}