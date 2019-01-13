using Dynamo.ViewModels;
using System.Windows;
using System.Windows.Forms;

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

        void button_Click(object sender, RoutedEventArgs e)
        {
            string guid = "" + ((System.Windows.Controls.Button)sender).Tag;
            DynamoViewModel dynVM = Owner.DataContext as DynamoViewModel;
            var VMU = new ViewModelUtils(dynVM, Owner);
            VMU.ZoomToObject(guid);
        }

		public void selectSourceInputs_Click(object sender, RoutedEventArgs e)
		{
			var openInputsDialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = true
			};

			if (openInputsDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				PlayerInputsViewModel vm = (PlayerInputsViewModel)playerInputsPanel.DataContext;
				vm.OnBatchResetInputsClicked(openInputsDialog.SelectedPath);
			}
		}

		public void selectSourceOutputs_Click(object sender, RoutedEventArgs e)
		{
			var openOutputsDialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = true
			};

			if (openOutputsDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				PlayerInputsViewModel vm = (PlayerInputsViewModel)playerInputsPanel.DataContext;
				vm.OnBatchResetOutputsClicked(openOutputsDialog.SelectedPath);
			}
		}
	}
}