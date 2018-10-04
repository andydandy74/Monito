using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using System.Collections.ObjectModel;
using Dynamo.ViewModels;
using System.Linq;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Notes;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Input;
using Dynamo.UI.Commands;
using Dynamo.Models;
using System.Windows;
using Dynamo.Graph;

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
        public ICommand UnfancifyGraph { get; set; }

        public UnfancifyViewModel(ReadyParams p, DynamoViewModel vm, KeyValueConfigurationCollection ms)
        {
            readyParams = p;
            viewModel = vm;
            ungroupAll = ms.GetLoadedSettingAsBoolean("UnfancifyUngroupAll");
            deleteTextNotes = ms.GetLoadedSettingAsBoolean("UnfancifyDeleteTextNotes");
            ignoreGroupPrefixes = ms["UnfancifyIgnoreGroupPrefixes"].Value.Replace(";", Environment.NewLine);
            ignoreTextNotePrefixes = ms["UnfancifyIgnoreTextNotePrefixes"].Value.Replace(";", Environment.NewLine);
            UnfancifyGraph = new DelegateCommand(OnUnfancifyClicked);
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
        /// Delete all text notes
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

        public void OnUnfancifyClicked(object obj)
        {
            // Create three lists for storing guids of groups, nodes and text notes that we want to keep
            List<String> groupsToKeep = new List<String>();
            List<String> nodesToKeep = new List<String>();
            List<String> textNotesToKeep = new List<String>();
            // Identify all groups to keep/ungroup
            if (ungroupAll)
            {
                var groupIgnoreList = ignoreGroupPrefixes.Split(new [] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (AnnotationModel anno in viewModel.Model.CurrentWorkspace.Annotations)
                {
                    foreach (string ignoreTerm in groupIgnoreList)
                    {
                        // Identify keepers
                        if (anno.AnnotationText.StartsWith(ignoreTerm) && !groupsToKeep.Contains(anno.GUID.ToString()))
                        {
                            groupsToKeep.Add(anno.GUID.ToString());
                            // Identify all nodes and text notes within those groups
                            foreach (var element in anno.SelectedModels)
                            {
                                if (element.GetType() == typeof(NoteModel)) { textNotesToKeep.Add(element.GUID.ToString()); }
                                else { nodesToKeep.Add(element.GUID.ToString()); }
                            }
                        }
                    }
                    // Add all obsolete groups to selection
                    if (!groupsToKeep.Contains(anno.GUID.ToString())) { viewModel.AddToSelectionCommand.Execute(anno); }
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
                        if (note.Text.StartsWith(ignoreTerm) && !textNotesToKeep.Contains(note.GUID.ToString()))
                        {
                            textNotesToKeep.Add(note.GUID.ToString());
                        }
                    }
                    // Add all obsolete text notes to selection
                    if (!textNotesToKeep.Contains(note.GUID.ToString())) { viewModel.AddToSelectionCommand.Execute(note); }
                }
                // Delete all obsolete text notes
                viewModel.DeleteCommand.Execute(null);
            }
            // Select all obsolete nodes and pre-process string nodes
            foreach (NodeModel node in viewModel.Model.CurrentWorkspace.Nodes)
            {
                if (!nodesToKeep.Contains(node.GUID.ToString()))
                {
                    // Pre-Processing
                    // Temporary fix for https://github.com/DynamoDS/Dynamo/issues/9117 (Escape backslashes in string nodes)
                    // Temporary fix for https://github.com/DynamoDS/Dynamo/issues/9120 (Escape double quotes in string nodes)
                    if (node.GetType() == typeof(CoreNodeModels.Input.StringInput))
                    {
                        // StringInput inputNode = (StringInput)node;
                        // string nodeval = inputNode.Value;
                        string nodeVal = node.PrintExpression().ToString();
                        nodeVal = nodeVal.Remove(nodeVal.Length - 1).Substring(1);
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
            viewModel.SelectAllCommand.Execute(null);
            // ToDo: Better clear the selection entirely
            viewModel.GraphAutoLayoutCommand.Execute(null);
        }
    }
}