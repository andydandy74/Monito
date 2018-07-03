using System.Windows;
using System.Windows.Controls;

namespace Monito
{
    /// <summary>
    /// Interaction logic for IsolateInPreviewWindow.xaml
    /// </summary>
    public partial class IsolateInPreviewWindow : Window
    {

        public IsolateInPreviewWindow()
        {
            InitializeComponent();
        }

        void click_PreviewAll(object sender, RoutedEventArgs e)
        {
            InputAction.Text = "PreviewAll";
        }

        void click_PreviewSelected(object sender, RoutedEventArgs e)
        {
            InputAction.Text = "PreviewSelected";
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            clickedGUID.Text = "" + ((Button)sender).Tag;
        }
    }
}