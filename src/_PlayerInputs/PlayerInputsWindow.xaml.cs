using System.Windows;

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
            InputAction.Text = "ResetAll";
        }

        void click_ResetSelected(object sender, RoutedEventArgs e)
        {
            InputAction.Text = "ResetSelected";
        }

        void click_SetSelectedAsInput(object sender, RoutedEventArgs e)
        {
            InputAction.Text = "SetSelectedAsInput";
        }
    }
}