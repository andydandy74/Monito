using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Notes;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Input;
using Dynamo.UI.Commands;
using Dynamo.Graph;
using Dynamo.Models;
using CoreNodeModels.Input;
using System.Windows;
using System.Windows.Threading;

namespace Monito
{
    class UnfancifyViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;
        private Window dynWindow;
        private bool ungroupAll;
        private bool deleteTextNotes;
        private bool deleteWatchNodes;
        private bool disableGeometryPreview;
        private bool disablePreviewBubbles;
        private string ignoreGroupPrefixes;
        private string ignoreTextNotePrefixes;
        private string unfancifyMsg = "";
        public ICommand UnfancifyCurrentGraph { get; set; }

        public UnfancifyViewModel(ReadyParams p, DynamoViewModel vm, KeyValueConfigurationCollection ms, Window dw)
        {
            readyParams = p;
            viewModel = vm;
            dynWindow = dw;
            ungroupAll = ms.GetLoadedSettingAsBoolean("UnfancifyUngroupAll");
            deleteTextNotes = ms.GetLoadedSettingAsBoolean("UnfancifyDeleteTextNotes");
            deleteWatchNodes = ms.GetLoadedSettingAsBoolean("UnfancifyDeleteWatchNodes");
            disableGeometryPreview = ms.GetLoadedSettingAsBoolean("UnfancifyDisableGeometryPreview");
            disablePreviewBubbles = ms.GetLoadedSettingAsBoolean("UnfancifyDisablePreviewBubbles");
            ignoreGroupPrefixes = ms["UnfancifyIgnoreGroupPrefixes"].Value.Replace(";", Environment.NewLine);
            ignoreTextNotePrefixes = ms["UnfancifyIgnoreTextNotePrefixes"].Value.Replace(";", Environment.NewLine);
            UnfancifyCurrentGraph = new DelegateCommand(OnUnfancifyCurrentClicked);
        }

        public void Dispose() { }
        
        /// <summary>
        /// Ungroup all groups?
        /// </summary>
        public bool UngroupAll
        {
            get { return ungroupAll; }
            set { ungroupAll = value; }
        }

        /// <summary>
        /// Delete all text notes?
        /// </summary>
        public bool DeleteTextNotes
        {
            get { return deleteTextNotes; }
            set { deleteTextNotes = value; }
        }

        /// <summary>
        /// Delete all watch nodes?
        /// </summary>
        public bool DeleteWatchNodes
        {
            get { return deleteWatchNodes; }
            set { deleteWatchNodes = value; }
        }

        /// <summary>
        /// Disable geometry preview for all nodes?
        /// </summary>
        public bool DisableGeometryPreview
        {
            get { return disableGeometryPreview; }
            set { disableGeometryPreview = value; }
        }

        /// <summary>
        /// Disable preview bubbles for all nodes?
        /// </summary>
        public bool DisablePreviewBubbles
        {
            get { return disablePreviewBubbles; }
            set { disablePreviewBubbles = value; }
        }

        /// <summary>
        /// Group prefixes that should be ignored
        /// </summary>
        public string IgnoreGroupPrefixes
        {
            get { return ignoreGroupPrefixes; }
            set { ignoreGroupPrefixes = value; }
        }

        /// <summary>
        /// Text note prefixes that should be ignored
        /// </summary>
        public string IgnoreTextNotePrefixes
        {
            get { return ignoreTextNotePrefixes; }
            set { ignoreTextNotePrefixes = value; }
        }

        public string UnfancifyMsg
        {
            get { return unfancifyMsg; }
            set
            {
                unfancifyMsg = value;
                RaisePropertyChanged("UnfancifyMsg");
            }
        }

        public void OnUnfancifyCurrentClicked(object obj)
        {
			UnfancifyMsg = "";
			UnfancifyGraph();
            UnfancifyMsg = "Current graph successfully unfancified!";
        }

        public void OnBatchUnfancifyClicked(string directoryPath)
        {
            UnfancifyMsg = "";
            // Read directory contents
            var graphs = System.IO.Directory.EnumerateFiles(directoryPath);
            int graphCount = 0;
            foreach (var graph in graphs)
            {
                var ext = System.IO.Path.GetExtension(graph);
                if (ext == ".dyn")
                {
                    viewModel.OpenCommand.Execute(graph);
                    viewModel.CurrentSpaceViewModel.RunSettingsViewModel.Model.RunType = RunType.Manual;
                    UnfancifyGraph();
                    viewModel.SaveAsCommand.Execute(graph);
                    viewModel.CloseHomeWorkspaceCommand.Execute(null);
                    graphCount += 1;
                    UnfancifyMsg += "Unfancified " + graph + "\n";
                }
            }
            UnfancifyMsg += "Unfancified " + graphCount.ToString() + " graphs...";
        }

        public void UnfancifyGraph()
        {
            // Create a list for storing guids of groups, nodes and text notes that we want to keep
            var stuffToKeep = new List<string>();
			GeneralUtils.ClearSelection();
			// Identify all groups to keep/ungroup
			if (ungroupAll)
            {
                var groupIgnoreList = ignoreGroupPrefixes.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var anno in viewModel.CurrentSpaceViewModel.Annotations)
                {
                    foreach (var ignoreTerm in groupIgnoreList)
                    {
                        // Identify keepers
                        if (anno.AnnotationText.StartsWith(ignoreTerm) && !stuffToKeep.Contains(anno.AnnotationModel.GUID.ToString()))
                        {
                            stuffToKeep.Add(anno.AnnotationModel.GUID.ToString());
                            // Identify all nodes and text notes within those groups
                            foreach (var element in anno.SelectedModels) { stuffToKeep.Add(element.GUID.ToString()); }
                        }
                    }
                    // Add all obsolete groups to selection
                    if (!stuffToKeep.Contains(anno.AnnotationModel.GUID.ToString())) { viewModel.AddToSelectionCommand.Execute(anno.AnnotationModel); }
                }
                // Ungroup all obsolete groups
                viewModel.UngroupAnnotationCommand.Execute(null);
            }
            // Identify all text notes to keep/delete
            if (deleteTextNotes)
            {
                var textNoteIgnoreList = ignoreTextNotePrefixes.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var note in viewModel.Model.CurrentWorkspace.Notes)
                {
                    foreach (var ignoreTerm in textNoteIgnoreList)
                    {
                        // Identify keepers
                        if (note.Text.StartsWith(ignoreTerm) && !stuffToKeep.Contains(note.GUID.ToString()))
                        {
                            stuffToKeep.Add(note.GUID.ToString());
                        }
                    }
                    // Add all obsolete text notes to selection
                    if (!stuffToKeep.Contains(note.GUID.ToString())) { viewModel.AddToSelectionCommand.Execute(note); }
                }
                // Delete all obsolete text notes
                viewModel.DeleteCommand.Execute(null);
            }
            // Process nodes
            foreach (var node in viewModel.Model.CurrentWorkspace.Nodes)
            {
                // Select all obsolete nodes and pre-process string nodes
                if (!stuffToKeep.Contains(node.GUID.ToString()))
                {
                    // Pre-Processing
                    // Temporary fix for https://github.com/DynamoDS/Dynamo/issues/9117 (Escape backslashes in string nodes)
                    // Temporary fix for https://github.com/DynamoDS/Dynamo/issues/9120 (Escape double quotes in string nodes)
                    if (node.GetType() == typeof(StringInput))
                    {
                        StringInput inputNode = (StringInput)node;
                        string nodeVal = inputNode.Value;
                        nodeVal = nodeVal.Replace("\\", "\\\\").Replace("\"", "\\\"");
                        var updateVal = new UpdateValueParams("Value", nodeVal);
                        node.UpdateValue(updateVal);
                    }
                    viewModel.AddToSelectionCommand.Execute(node);
                }
            }
            // Node to code
            viewModel.CurrentSpaceViewModel.NodeToCodeCommand.Execute(null);
			GeneralUtils.ClearSelection();
			// Process remaining nodes
			var nodesToDelete = new List<NodeModel>();
            if (disableGeometryPreview || disablePreviewBubbles || deleteWatchNodes)
            {
                foreach (var node in viewModel.CurrentSpaceViewModel.Nodes)
                {
                    // Turn off geometry preview
                    if (disableGeometryPreview)
                    {
                        if (node.IsVisible) { node.ToggleIsVisibleCommand.Execute(null); }
                    }
                    // Turn off preview bubbles (only works after file has been saved and re-opened)
                    if (disablePreviewBubbles)
                    {
                        if (node.PreviewPinned) { node.PreviewPinned = false; }
                    }
                    // Identify Watch nodes
                    if (deleteWatchNodes)
                    {
                        string nodeType = node.NodeModel.GetType().ToString();
                        if (nodeType == "CoreNodeModels.Watch" || nodeType == "Watch3DNodeModels.Watch3D" || nodeType == "CoreNodeModels.WatchImageCore")
                        {
							if (node.NodeModel.OutputNodes.Count == 0) { viewModel.AddToSelectionCommand.Execute(node.NodeModel); }
						}
                    }
                }
            }
            // Delete Watch nodes
            if (deleteWatchNodes) { viewModel.DeleteCommand.Execute(null); }
            // Auto layout          
            dynWindow.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                viewModel.CurrentSpaceViewModel.GraphAutoLayoutCommand.Execute(null);
            }));
        }
    }
}