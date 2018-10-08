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

namespace Monito
{
    class UnfancifyViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;
        private bool ungroupAll;
        private bool deleteTextNotes;
        private string ignoreGroupPrefixes;
        private string ignoreTextNotePrefixes;
        private string unfancifyMsg = "";
        public ICommand UnfancifyCurrentGraph { get; set; }

        public UnfancifyViewModel(ReadyParams p, DynamoViewModel vm, KeyValueConfigurationCollection ms)
        {
            readyParams = p;
            viewModel = vm;
            ungroupAll = ms.GetLoadedSettingAsBoolean("UnfancifyUngroupAll");
            deleteTextNotes = ms.GetLoadedSettingAsBoolean("UnfancifyDeleteTextNotes");
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
            set
            {
                ungroupAll = value;
            }
        }

        /// <summary>
        /// Delete all text notes?
        /// </summary>
        public bool DeleteTextNotes
        {
            get { return deleteTextNotes; }
            set
            {
                deleteTextNotes = value;
            }
        }

        /// <summary>
        /// Group prefixes that should be ignored
        /// </summary>
        public string IgnoreGroupPrefixes
        {
            get { return ignoreGroupPrefixes; }
            set
            {
                ignoreGroupPrefixes = value;
            }
        }

        /// <summary>
        /// Text note prefixes that should be ignored
        /// </summary>
        public string IgnoreTextNotePrefixes
        {
            get { return ignoreTextNotePrefixes; }
            set
            {
                ignoreTextNotePrefixes = value;
            }
        }

        public string UnfancifyMsg
        {
            get { return unfancifyMsg; }
        }

        public void OnUnfancifyCurrentClicked(object obj)
        {
            UnfancifyGraph();
            unfancifyMsg = "Current graph successfully unfancified!";
            RaisePropertyChanged("UnfancifyMsg");
        }

        public void OnBatchUnfancifyClicked(string directoryPath)
        {
            // Read directory contents
            var graphs = System.IO.Directory.EnumerateFiles(directoryPath);
            int graphCount = 0;
            foreach (var graph in graphs)
            {
                var ext = System.IO.Path.GetExtension(graph);
                if (ext == ".dyn")
                {
                    unfancifyMsg += "Unfancifying " + graph + "\n";
                    RaisePropertyChanged("UnfancifyMsg");
                    viewModel.OpenCommand.Execute(graph);
                    viewModel.CurrentSpaceViewModel.RunSettingsViewModel.Model.RunType = RunType.Manual;
                    UnfancifyGraph();
                    viewModel.SaveAsCommand.Execute(graph);
                    viewModel.CloseHomeWorkspaceCommand.Execute(null);
                    graphCount += 1;
                }
            }
            unfancifyMsg += "Unfancified " + graphCount.ToString() + " graphs...";
            RaisePropertyChanged("UnfancifyMsg");
        }

        public void UnfancifyGraph()
        {
            // Create a list for storing guids of groups, nodes and text notes that we want to keep
            List<System.String> stuffToKeep = new List<System.String>();
            // Identify all groups to keep/ungroup
            if (ungroupAll)
            {
                var groupIgnoreList = ignoreGroupPrefixes.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (AnnotationModel anno in viewModel.Model.CurrentWorkspace.Annotations)
                {
                    foreach (string ignoreTerm in groupIgnoreList)
                    {
                        // Identify keepers
                        if (anno.AnnotationText.StartsWith(ignoreTerm) && !stuffToKeep.Contains(anno.GUID.ToString()))
                        {
                            stuffToKeep.Add(anno.GUID.ToString());
                            // Identify all nodes and text notes within those groups
                            foreach (var element in anno.SelectedModels) { stuffToKeep.Add(element.GUID.ToString()); }
                        }
                    }
                    // Add all obsolete groups to selection
                    if (!stuffToKeep.Contains(anno.GUID.ToString())) { viewModel.AddToSelectionCommand.Execute(anno); }
                }
                // Ungroup all obsolete groups
                viewModel.UngroupAnnotationCommand.Execute(null);
            }
            // Identify all text notes to keep/delete
            if (deleteTextNotes)
            {
                var textNoteIgnoreList = ignoreTextNotePrefixes.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (NoteModel note in viewModel.Model.CurrentWorkspace.Notes)
                {
                    foreach (string ignoreTerm in textNoteIgnoreList)
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
            // Select all obsolete nodes and pre-process string nodes
            foreach (NodeModel node in viewModel.Model.CurrentWorkspace.Nodes)
            {
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
            // Auto layout          
            GeneralUtils.ClearSelection();
            viewModel.GraphAutoLayoutCommand.Execute(null);
        }
    }
}