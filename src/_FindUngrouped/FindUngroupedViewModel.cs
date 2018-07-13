using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dynamo.UI.Commands;
using System.Windows;

namespace Monito
{
    class FindUngroupedViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;
        public ICommand FixUngrouped { get; set; }

        public FindUngroupedViewModel(ReadyParams p, DynamoViewModel vm)
        {
            readyParams = p;
            viewModel = vm;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModel_NodesChanged;
            p.CurrentWorkspaceModel.NodeRemoved += CurrentWorkspaceModel_NodesChanged;
            FixUngrouped = new DelegateCommand(OnFixUngroupedClicked);
        }

        public void Dispose()
        {
            readyParams.CurrentWorkspaceModel.NodeAdded -= CurrentWorkspaceModel_NodesChanged;
            readyParams.CurrentWorkspaceModel.NodeRemoved -= CurrentWorkspaceModel_NodesChanged;
        }

        private string currentUngroupedMsg;
        public string CurrentUngroupedMsg
        {
            get
            {
                if (currentUngrouped.Count > 0) { currentUngroupedMsg = "All ungrouped nodes and notes in current workspace:"; }
                else { currentUngroupedMsg = "No ungrouped nodes and notes in current workspace..."; }
                return currentUngroupedMsg;
            }
        }

        private ObservableCollection<ObjectInWorkspace> currentUngrouped = new ObservableCollection<ObjectInWorkspace>();
        /// <summary>
        /// The search results as a list representation
        /// </summary>
        public ObservableCollection<ObjectInWorkspace> CurrentUngrouped
        {
            get
            {
                List<string> allGroupedObjects = new List<string>();
                foreach (var anno in viewModel.CurrentSpaceViewModel.Annotations)
                {
                    foreach (var member in anno.AnnotationModel.SelectedModels)
                    {
                        allGroupedObjects.Add(member.GUID.ToString());
                    }
                }
                List<ObjectInWorkspace> unorderedUngrouped = new List<ObjectInWorkspace>();
                foreach (var node in viewModel.CurrentSpaceViewModel.Nodes)
                {
                    if (!allGroupedObjects.Contains(node.NodeModel.GUID.ToString()))
                    {
                        unorderedUngrouped.Add(new ObjectInWorkspace(node.NickName.Abbreviate() + " [Node]", node.NodeModel.GUID.ToString()));
                    }
                }
                foreach (var note in viewModel.CurrentSpaceViewModel.Notes)
                {
                    if (!allGroupedObjects.Contains(note.Model.GUID.ToString()))
                    {
                        unorderedUngrouped.Add(new ObjectInWorkspace(note.Text.Abbreviate() + " [Note]", note.Model.GUID.ToString()));
                    }
                }
                currentUngrouped.Clear();
                foreach (ObjectInWorkspace item in unorderedUngrouped.OrderBy(x => x.Name)) { currentUngrouped.Add(item); }
                RaisePropertyChanged(nameof(CurrentUngroupedMsg));
                return currentUngrouped;
            }
        }

        public void OnFixUngroupedClicked(object obj)
        {
            MessageBox.Show("Not yet available...");
            // RaisePropertyChanged(nameof(CurrentUngrouped));
        }

        private void CurrentWorkspaceModel_NodesChanged(NodeModel obj)
        {
            RaisePropertyChanged(nameof(CurrentUngrouped));
        }
    }
}