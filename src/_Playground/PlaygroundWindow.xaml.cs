using Dynamo.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Monito
{
    /// <summary>
    /// Interaction logic for PackageUpdatesWindow.xaml
    /// </summary>
    public partial class PlaygroundWindow : Window
    {

        public PlaygroundWindow()
        {
            InitializeComponent();
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            clickedGUID.Text = "" + ((Button)sender).Tag;
        }
    }
}