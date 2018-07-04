using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
        private MenuItem monitoPackageDirectoriesMenuItem;
        private MenuItem monitoMyGraphsMenuItem;
        private MenuItem monitoPlayerInputsMenuItem;
        private MenuItem monitoSearchInWorkspaceMenuItem;
        private MenuItem monitoAboutMenuItem;
        private ViewStartupParams startupParams;
        private KeyValueConfigurationCollection monitoSettings;
        private bool monitoSettingsLoaded = false;
        private List<string> myGraphsRaw = new List<string>();

        public void Dispose() { }

        public void Startup(ViewStartupParams p)
        {
            startupParams = p;
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

            #region MY_GRAPHS
            if (monitoSettingsLoaded && monitoSettings["EnableMyGraphs"] != null && monitoSettings["EnableMyGraphs"].Value == "1")
            {
                
                var topDirs = monitoSettings["MyGraphsDirectoryPaths"].Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (topDirs.Length > 0)
                {
                    monitoMyGraphsMenuItem = new MenuItem { Header = "My Graphs" };
                    monitoMyGraphsMenuItem.ToolTip = new ToolTip { Content = "Quick access to all your graphs..." };
                    if (topDirs.Length == 1)
                    {
                        monitoMyGraphsMenuItem = buildMyGraphsMenu(topDirs[0], monitoMyGraphsMenuItem, VM);
                    }
                    else
                    {
                        foreach(string topDir in topDirs)
                        {
                            string topDirName = Path.GetFileName(topDir);
                            MenuItem topDirMenuItem = new MenuItem { Header = topDirName };
                            topDirMenuItem.ToolTip = new ToolTip { Content = topDir };
                            topDirMenuItem = buildMyGraphsMenu(topDir, topDirMenuItem, VM);
                            monitoMyGraphsMenuItem.Items.Add(topDirMenuItem);
                        }
                    }
                    if (monitoMyGraphsMenuItem != null) { monitoMenuItem.Items.Add(monitoMyGraphsMenuItem); }
                }
                    
            }
            #endregion MY_GRAPHS

            #region PACKAGE_DIRECTORIES
            if (monitoSettingsLoaded && monitoSettings["EnablePackageDirectories"] != null && monitoSettings["EnablePackageDirectories"].Value == "1")
            {
                monitoPackageDirectoriesMenuItem = new MenuItem { Header = "Package Directories" };
                monitoPackageDirectoriesMenuItem.ToolTip = new ToolTip { Content = "Quick access to all your package directories..." };
                foreach (string packageDir in startupParams.Preferences.CustomPackageFolders)
                {
                    if (Directory.Exists(packageDir))
                    {
                        MenuItem monitoPackageDirMenuItem = new MenuItem { Header = packageDir };
                        monitoPackageDirMenuItem.ToolTip = new ToolTip { Content = "Show contents of " + packageDir + " ..." };
                        monitoPackageDirMenuItem.Click += (sender, args) =>
                        {
                            if (Directory.Exists(packageDir)) { Process.Start(@"" + packageDir); }
                            else { MessageBox.Show("Directory " + packageDir + " has been moved, renamed or deleted..."); }
                        };
                        monitoPackageDirectoriesMenuItem.Items.Add(monitoPackageDirMenuItem);
                    }
                }
                monitoMenuItem.Items.Add(monitoPackageDirectoriesMenuItem);
            }          
            #endregion PACKAGE_DIRECTORIES

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
            if (monitoMenuItem.Items.Count > 0) { monitoMenuItem.Items.Add(new Separator()); }
            monitoMenuItem.Items.Add(monitoAboutMenuItem);
            #endregion ABOUT

            p.dynamoMenu.Items.Add(monitoMenuItem);
        }

        public void Shutdown() { }

        public string UniqueId
        {
            get { return "d8fcfe56-81e0-4e95-84af-d945ebd6478b"; }
        }

        public string Name
        {
            get { return "DynaMonito"; }
        }

        public MenuItem buildMyGraphsMenu(string dir, MenuItem menuItem, DynamoViewModel vm)
        {
            try
            {
                List<MenuItem> tempMenuItems = new List<MenuItem>();
                foreach (string d in Directory.GetDirectories(dir))
                {
                    string dirName = Path.GetFileName(d);
                    if (dirName != "backup")
                    {
                        MenuItem dirMenu = new MenuItem { Header = dirName };
                        dirMenu.ToolTip = new ToolTip { Content = d };
                        dirMenu = buildMyGraphsMenu(d, dirMenu, vm);
                        if (dirMenu != null) { tempMenuItems.Add(dirMenu); }                       
                    }
                }
                var files = Directory.GetFiles(dir, "*.dyn");
                if (files.Length > 0)
                {
                    foreach (string f in files)
                    {
                        string graphName = Path.GetFileNameWithoutExtension(f);
                        MenuItem graphMenu = new MenuItem { Header = graphName };
                        graphMenu.ToolTip = new ToolTip { Content = f };
                        graphMenu.Click += (sender, args) =>
                        {
                            if (File.Exists(f))
                            {
                                vm.CloseHomeWorkspaceCommand.Execute(null);
                                vm.OpenCommand.Execute(f);
                            }
                            else { MessageBox.Show("Graph " + graphName + " has been moved, renamed or deleted..."); }
                        };
                        tempMenuItems.Add(new MenuItem { Header = graphMenu });
                    }
                }
                if (tempMenuItems.Count > 0)
                {
                    foreach (MenuItem tempMenuItem in tempMenuItems)
                    {
                        menuItem.Items.Add(tempMenuItem);
                    }
                    return menuItem;
                }
                else { return null; }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }
    }
}