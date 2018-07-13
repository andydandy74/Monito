using Dynamo.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Monito
{
    /// <summary>
    /// Interaction logic for FindUngrouped.xaml
    /// </summary>
    public partial class FindUngroupedWindow : Window
    {
        public FindUngroupedWindow()
        {
            InitializeComponent();
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            string guid = "" + ((Button)sender).Tag;
            DynamoViewModel dynVM = Owner.DataContext as DynamoViewModel;
            var VMU = new ViewModelUtils(dynVM, Owner);
            VMU.ZoomToObject(guid);
        }
    }
}