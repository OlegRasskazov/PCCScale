using PCCScale.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;

namespace PCCScale
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            
        }
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}