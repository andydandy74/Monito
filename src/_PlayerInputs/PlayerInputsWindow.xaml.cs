using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Monito
{
    /// <summary>
    /// Interaction logic for PlayerInputsWindow.xaml
    /// </summary>
    public partial class PlayerInputsWindow : Window
    {
        public PlayerInputsWindow()
        {
            InitializeComponent();
        }

        void click_ResetAll(object sender, RoutedEventArgs e)
        {
            //InputAction.Text = "ResetAll";
            DynamoViewModel dynVM = Owner.DataContext as DynamoViewModel;
            foreach (NodeModel node in dynVM.CurrentSpaceViewModel.Model.Nodes)
            {
                if (node.IsSetAsInput) { node.IsSetAsInput = false; }
            }
            // need to raise property changed event here
            var vm = DataContext as PlayerInputsViewModel;
            vm.ResetAll();
        }

        void click_ResetSelected(object sender, RoutedEventArgs e)
        {
            //InputAction.Text = "ResetSelected";
            DynamoViewModel dynVM = Owner.DataContext as DynamoViewModel;
            foreach (var item in dynVM.CurrentSpaceViewModel.Model.CurrentSelection)
            {
                if (item.IsSetAsInput) { item.IsSetAsInput = false; }
            }
            // need to raise property changed event here
        }

        void click_SetSelectedAsInput(object sender, RoutedEventArgs e)
        {
            //InputAction.Text = "SetSelectedAsInput";
            DynamoViewModel dynVM = Owner.DataContext as DynamoViewModel;
            foreach (var item in dynVM.CurrentSpaceViewModel.Model.CurrentSelection)
            {
                if (!item.IsSetAsInput) { item.IsSetAsInput = true; }
            }
            // need to raise property changed event here
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