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
            // Selection
            viewModel.SelectAllCommand.Execute(null);
            // Replace this later with a better selection method that omits nodes from ignored groups
            // Node to code
            viewModel.CurrentSpaceViewModel.NodeToCodeCommand.Execute(null);
            // Ungroup all (except ignored groups)
            // Delete text notes (except ignored text notes)
            // Auto layout
            viewModel.GraphAutoLayoutCommand.Execute(null);
        }
    }
}