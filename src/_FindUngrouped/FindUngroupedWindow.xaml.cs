using Dynamo.ViewModels;
using System.Windows;
using System.Windows.Forms;

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
            string guid = "" + ((System.Windows.Controls.Button)sender).Tag;
            DynamoViewModel dynVM = Owner.DataContext as DynamoViewModel;
            var VMU = new ViewModelUtils(dynVM, Owner);
            VMU.ZoomToObject(guid);
        }

		public void selectSource_Click(object sender, RoutedEventArgs e)
		{
			var openDialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = true
			};

			if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				FindUngroupedViewModel vm = (FindUngroupedViewModel)findUngroupedPanel.DataContext;
				vm.OnBatchFixUngroupedClicked(openDialog.SelectedPath);
			}
		}
	}
}