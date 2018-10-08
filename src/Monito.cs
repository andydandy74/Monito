using Dynamo.Models;
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
        private MenuItem monitoIsolateInPreviewMenuItem;
        private MenuItem monitoFindUngroupedMenuItem;
        private MenuItem monitoPackageDirectoriesMenuItem;
        private MenuItem monitoMyGraphsMenuItem;
        private MenuItem monitoMyTemplatesMenuItem;
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
            // Try loading the package config file
            // We need this to determine which tools to load (and also settings for some of the tools)
            string configPath = this.GetType().Assembly.Location + ".config";
            if (!File.Exists(configPath)) { configPath = configPath.Replace("bin\\Monito", "extra\\Monito"); }
            try
            {
                var map = new ExeConfigurationFileMap() { ExeConfigFilename = configPath };
                Configuration myDllConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                AppSettingsSection myDllConfigAppSettings = (AppSettingsSection)myDllConfig.GetSection("appSettings");
                monitoSettings = myDllConfigAppSettings.Settings;
                monitoSettingsLoaded = true; 
            }
            catch { MessageBox.Show("Couldn't find, load or read DynaMonito config file at " + configPath); }
        }

        public void Loaded(ViewLoadedParams p)
        {
            monitoMenuItem = new MenuItem { Header = "DynaMonito" };
            var VM = p.DynamoWindow.DataContext as DynamoViewModel;

            #region FIND_UNGROUPED
            if (monitoSettingsLoaded && monitoSettings.GetLoadedSettingAsBoolean("EnableFindUngrouped"))
            {
                monitoFindUngroupedMenuItem = new MenuItem { Header = "Find and Fix Ungrouped" };
                monitoFindUngroupedMenuItem.ToolTip = new ToolTip { Content = "Identify nodes and notes that don't belong to a group.." };
                monitoFindUngroupedMenuItem.Click += (sender, args) =>
                {
                    var viewModel = new FindUngroupedViewModel(p, VM);
                    var window = new FindUngroupedWindow
                    {
                        findUngroupedPanel = { DataContext = viewModel },
                        Owner = p.DynamoWindow
                    };
                    window.Left = window.Owner.Left + 400;
                    window.Top = window.Owner.Top + 200;
                    window.Show();
                };
                monitoMenuItem.Items.Add(monitoFindUngroupedMenuItem);
            }
            #endregion FIND_UNGROUPED

            #region ISOLATE_IN_GEOMETRY_PREVIEW
            if (monitoSettingsLoaded && monitoSettings.GetLoadedSettingAsBoolean("EnableIsolateInGeometryPreview"))
            {
                monitoIsolateInPreviewMenuItem = new MenuItem { Header = "Isolate in Geometry Preview" };
                monitoIsolateInPreviewMenuItem.ToolTip = new ToolTip { Content = "Quickly isolate the current selection in geometry preview..." };
                monitoIsolateInPreviewMenuItem.Click += (sender, args) =>
                {
                    var viewModel = new IsolateInPreviewViewModel(p, VM, p.DynamoWindow);
                    var window = new IsolateInPreviewWindow
                    {
                        isolatePreviewPanel = { DataContext = viewModel },
                        Owner = p.DynamoWindow
                    };
                    window.Left = window.Owner.Left + 400;
                    window.Top = window.Owner.Top + 200;
                    window.Show();
                };
                monitoMenuItem.Items.Add(monitoIsolateInPreviewMenuItem);
            }
            #endregion ISOLATE_IN_GEOMETRY_PREVIEW

            #region PLAYER_INPUTS
            if (monitoSettingsLoaded && monitoSettings.GetLoadedSettingAsBoolean("EnablePlayerInputs"))
            {
                monitoPlayerInputsMenuItem = new MenuItem { Header = "Manage Dynamo Player Inputs and Outputs" };
                monitoPlayerInputsMenuItem.ToolTip = new ToolTip { Content = "Manage which input and output nodes should be displayed by Dynamo Player..." };
                monitoPlayerInputsMenuItem.Click += (sender, args) =>
                {
                    var viewModel = new PlayerInputsViewModel(p, VM);
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
            if (monitoSettingsLoaded && monitoSettings.GetLoadedSettingAsBoolean("EnableMyGraphs"))
            {
                // Read list of graph directories from config
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

            #region MY_TEMPLATES
            if (monitoSettingsLoaded && monitoSettings.GetLoadedSettingAsBoolean("EnableMyTemplates"))
            {
                var tplDir = monitoSettings["MyTemplatesDirectoryPath"].Value;
                if (Directory.Exists(tplDir))
                {
                    // Create a menu item for each template
                    List<MenuItem> tempMenuItems = new List<MenuItem>();
                    var templates = Directory.GetFiles(tplDir, "*.dyn");
                    foreach (string t in templates)
                    {
                        string tplName = Path.GetFileNameWithoutExtension(t);
                        MenuItem tplMenu = new MenuItem { Header = tplName };
                        tplMenu.ToolTip = new ToolTip { Content = t };
                        tplMenu.Click += (sender, args) =>
                        {
                            if (File.Exists(t))
                            {
                                // Close current home workspace, open template and set to manual mode
                                VM.CloseHomeWorkspaceCommand.Execute(null);
                                VM.OpenCommand.Execute(t);
                                VM.CurrentSpaceViewModel.RunSettingsViewModel.Model.RunType = RunType.Manual;
                                // Select all nodes and notes as well as annotations and copy everything
                                VM.SelectAllCommand.Execute(null);
                                // Need to copy groups as well
                                foreach (var anno in VM.HomeSpaceViewModel.Annotations) { VM.AddToSelectionCommand.Execute(anno); }
                                VM.CopyCommand.Execute(null);
                                // Create new home workspace, set to manual mode and paste template content
                                VM.NewHomeWorkspaceCommand.Execute(null);
                                VM.CurrentSpaceViewModel.RunSettingsViewModel.Model.RunType = RunType.Manual;
                                VM.Model.Paste();
                                foreach (var anno in VM.HomeSpaceViewModel.Annotations) { anno.AnnotationModel.Deselect(); }
                            }
                            else { MessageBox.Show("Template " + tplName + " has been moved, renamed or deleted..."); }
                        };
                        tempMenuItems.Add(tplMenu);
                    }
                    // Only show the templates menu item if templates exist
                    if (tempMenuItems.Count > 0)
                    {
                        monitoMyTemplatesMenuItem = new MenuItem { Header = "New Workspace from Template" };
                        monitoMyTemplatesMenuItem.ToolTip = new ToolTip { Content = "Quick access to all your templates..." };
                        foreach (MenuItem tempMenuItem in tempMenuItems) { monitoMyTemplatesMenuItem.Items.Add(tempMenuItem); }
                        monitoMenuItem.Items.Add(monitoMyTemplatesMenuItem);
                    }
                }
            }
            #endregion MY_TEMPLATES

            #region PACKAGE_DIRECTORIES
            if (monitoSettingsLoaded && monitoSettings.GetLoadedSettingAsBoolean("EnablePackageDirectories"))
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
            if (monitoSettingsLoaded && monitoSettings.GetLoadedSettingAsBoolean("EnableSearchInWorkspace"))
            {
                monitoSearchInWorkspaceMenuItem = new MenuItem { Header = "Search in Workspace" };
                monitoSearchInWorkspaceMenuItem.ToolTip = new ToolTip { Content = "Search for nodes, notes and groups in the current workspace..." };
                monitoSearchInWorkspaceMenuItem.Click += (sender, args) =>
                {
                    var viewModel = new SearchInWorkspaceViewModel(p, VM, monitoSettings);
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

        /// <summary>
        /// Builds a menu structure by recursively parsing a given directory. 
        /// Returns null if the directory doesn't exist or is empty.
        /// </summary>
        public MenuItem buildMyGraphsMenu(string dir, MenuItem menuItem, DynamoViewModel vm)
        {
            if (!Directory.Exists(dir)) { return null; }
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
                tempMenuItems.Add(graphMenu);
            }
            if (tempMenuItems.Count > 0)
            {
                foreach (MenuItem tempMenuItem in tempMenuItems) { menuItem.Items.Add(tempMenuItem); }
                return menuItem;
            }
            else { return null; }
        }
    }
}