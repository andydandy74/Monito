using Dynamo.ViewModels;
using System;
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
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            clickedGUID.Text = "" + ((Button)sender).Tag;
        }
    }
}