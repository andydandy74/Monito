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
            List<String> groupsToKeep = new List<String>();
            List<String> nodesToKeep = new List<String>();
            List<String> textNotesToKeep = new List<String>();
            // Identify all groups to keep/ungroup
            if (ungroupAll)
            {
                List<String> groupIgnoreList = new List<String>();
                groupIgnoreList.Add("XXX");
                // var groupIgnoreList = ignoreGroupPrefixes.Split(new char[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
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
                                if (element.ToString() == "Dynamo.Graph.Notes.NoteModel") { textNotesToKeep.Add(element.GUID.ToString()); }
                                else { nodesToKeep.Add(element.GUID.ToString()); }
                            }
                        }
                        // Ungroup the rest
                        else if (!groupsToKeep.Contains(anno.GUID.ToString()))
                        {
                            anno.Select();
                            viewModel.UngroupAnnotationCommand.Execute(null);
                        }
                    }
                }
            }
            // Identify all text notes to keep/delete
            if (deleteTextNotes)
            {
                List<String> textNoteIgnoreList = new List<String>();
                textNoteIgnoreList.Add("XXX");
                // var textNoteIgnoreList = ignoreTextNotePrefixes.Split(new char[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (NoteModel note in viewModel.Model.CurrentWorkspace.Notes)
                {
                    foreach (string ignoreTerm in textNoteIgnoreList)
                    {
                        // Identify keepers
                        if (note.Text.StartsWith(ignoreTerm) && !textNotesToKeep.Contains(note.GUID.ToString()))
                        {
                            textNotesToKeep.Add(note.GUID.ToString());
                        }
                        // delete the rest
                        else if (!textNotesToKeep.Contains(note.GUID.ToString()))
                        {
                            MessageBox.Show(note.Text);
                            //viewModel.AddToSelectionCommand.Execute(note.GUID.ToString());
                            note.Select();
                            viewModel.DeleteCommand.Execute(null);
                        }
                    }
                }
            }
            // Selection
            // viewModel.SelectAllCommand.Execute(null);
            // ToDo: Replace this later with a better selection method that omits nodes from ignored groups
            // Node to code
            // ToDo: Provide a temporary fix for https://github.com/DynamoDS/Dynamo/issues/9117 here
            // by finding all string nodes that end on a single backslash
            // and escaping that backslash before calling node to code
            // viewModel.CurrentSpaceViewModel.NodeToCodeCommand.Execute(null);
            // Possible future improvement: eplace comments in code blocks with empty comments
            // This may also include deleting code blocks that only contain comments
            
            // Auto layout
            // viewModel.GraphAutoLayoutCommand.Execute(null);
        }
    }
}