using System.Windows;
using System.Windows.Controls;

namespace Monito
{
    /// <summary>
    /// Interaction logic for SearchInWorkspaceWindow.xaml
    /// </summary>
    public partial class SearchInWorkspaceWindow : Window
    {

        public SearchInWorkspaceWindow()
        {
            InitializeComponent();
            searchQuery.Focus();
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            clickedGUID.Text = "" + ((Button)sender).Tag;
        }
    }
}