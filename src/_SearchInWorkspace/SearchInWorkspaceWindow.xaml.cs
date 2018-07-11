using Dynamo.ViewModels;
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
            var dynWindow = this.Owner;
            var vm = dynWindow.DataContext as DynamoViewModel;
            string guid = "" + ((Button)sender).Tag;
            var VMU = new ViewModelUtils(vm, dynWindow);
            VMU.ZoomToObject(guid);
        }
    }
}