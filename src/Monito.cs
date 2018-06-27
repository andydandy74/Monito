using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace Monito
{
    /// <summary>
    /// The main class of this view extension.
    /// </summary>
    public class MonitoViewExtension : IViewExtension
    {
        private MenuItem monitoMenuItem;
        private MenuItem monitoPlayerInputsMenuItem;
        private MenuItem monitoSearchInWorkspaceMenuItem;
        private MenuItem monitoAboutMenuItem;
        private KeyValueConfigurationCollection monitoSettings;
        private bool monitoSettingsLoaded = false;

        public void Dispose() { }

        public void Startup(ViewStartupParams p)
        {
            string configPath = this.GetType().Assembly.Location;
            try
            {
                Configuration myDllConfig = ConfigurationManager.OpenExeConfiguration(configPath);
                AppSettingsSection myDllConfigAppSettings = (AppSettingsSection)myDllConfig.GetSection("appSettings");
                monitoSettings = myDllConfigAppSettings.Settings;
                monitoSettingsLoaded = true;
            }
            catch { MessageBox.Show("Couldn't find, load or read config file at " + configPath); }
        }

        public void Loaded(ViewLoadedParams p)
        {
            monitoMenuItem = new MenuItem { Header = "DynaMonito" };
            var VM = p.DynamoWindow.DataContext as DynamoViewModel;

            #region PLAYER_INPUTS
            if (monitoSettingsLoaded && monitoSettings["EnablePlayerInputs"] != null && monitoSettings["EnablePlayerInputs"].Value == "1")
            {
                monitoPlayerInputsMenuItem = new MenuItem { Header = "Manage Dynamo Player Inputs" };
                monitoPlayerInputsMenuItem.ToolTip = new ToolTip { Content = "Manage which input nodes should be displayed by Dynamo Player..." };
                monitoPlayerInputsMenuItem.Click += (sender, args) =>
                {
                    var viewModel = new PlayerInputsViewModel(p, VM, p.DynamoWindow);
                    var window = new PlayerInputsWindow
                    {
                        playerInputsPanel = { DataContext = viewModel },
                        Owner = p.DynamoWindow
                    };
                    window.Left = window.Owner.Left + 400;
                    window.Top = window.Owner.Top + 200;
                    window.Show();
                };
                monitoMenuItem.Items.Add(monitoPlayerInputsMenuItem);
            }
            #endregion PLAYER INPUTS

            #region SEARCH_IN_WORKSPACE
            if (monitoSettingsLoaded && monitoSettings["EnableSearchInWorkspace"] != null && monitoSettings["EnableSearchInWorkspace"].Value == "1")
            {
                monitoSearchInWorkspaceMenuItem = new MenuItem { Header = "Search in Workspace" };
                monitoSearchInWorkspaceMenuItem.ToolTip = new ToolTip { Content = "Search for nodes, notes and groups in the current workspace..." };
                monitoSearchInWorkspaceMenuItem.Click += (sender, args) =>
                {
                    var viewModel = new SearchInWorkspaceViewModel(p, VM, p.DynamoWindow);
                    var window = new SearchInWorkspaceWindow
                    {
                        searchPanel = { DataContext = viewModel },
                        Owner = p.DynamoWindow
                    };
                    window.Left = window.Owner.Left + 400;
                    window.Top = window.Owner.Top + 200;
                    window.Show();
                };
                monitoMenuItem.Items.Add(monitoSearchInWorkspaceMenuItem);
            }
            #endregion SEARCH_IN_WORKSPACE

            #region ABOUT
            monitoAboutMenuItem = new MenuItem { Header = "About DynaMonito" };
            monitoAboutMenuItem.Click += (sender, args) =>
            {
                var window = new AboutWindow
                {
                    aboutPanel = { DataContext = this },
                    Owner = p.DynamoWindow
                };
                window.Left = window.Owner.Left + 400;
                window.Top = window.Owner.Top + 200;
                window.Show();
            };
            if (monitoMenuItem.Items.Count > 0)
            {
                monitoMenuItem.Items.Add(new Separator());
            }
            monitoMenuItem.Items.Add(monitoAboutMenuItem);
            #endregion ABOUT

            p.dynamoMenu.Items.Add(monitoMenuItem);
        }

        public void Shutdown() { }

        public string UniqueId
        {
            get
            {
                return "d8fcfe56-81e0-4e95-84af-d945ebd6478b";
            }
        }

        public string Name
        {
            get
            {
                return "DynaMonito";
            }
        }

    }
}