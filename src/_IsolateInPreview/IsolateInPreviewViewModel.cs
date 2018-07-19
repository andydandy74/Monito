using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.UI.Commands;
using System.Windows.Input;

namespace Monito
{
    class IsolateInPreviewViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;
        private Window dynWindow;
        public ICommand ResetAll { get; set; }
        public ICommand AddSelected { get; set; }
        public ICommand RemoveSelected { get; set; }
        public ICommand IsolateSelected { get; set; }

        public IsolateInPreviewViewModel(ReadyParams p, DynamoViewModel vm, Window dw)
        {
            readyParams = p;
            viewModel = vm;
            dynWindow = dw;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModel_NodesChanged;
            p.CurrentWorkspaceModel.NodeRemoved += CurrentWorkspaceModel_NodesChanged;
            ResetAll = new DelegateCommand(OnResetAllClicked);
            AddSelected = new DelegateCommand(OnAddSelectedClicked);
            RemoveSelected = new DelegateCommand(OnRemoveSelectedClicked);
            IsolateSelected = new DelegateCommand(OnIsolateSelectedClicked);
        }

        public void Dispose()
        {
            readyParams.CurrentWorkspaceModel.NodeAdded -= CurrentWorkspaceModel_NodesChanged;
            readyParams.CurrentWorkspaceModel.NodeRemoved -= CurrentWorkspaceModel_NodesChanged;
        }

        private string currentPreviewsMsg;
        public string CurrentPreviewsMsg
        {
            get
            {
                if (currentPreviews.Count > 0) { currentPreviewsMsg = "All nodes with active geometry preview in current workspace:"; }
                else { currentPreviewsMsg = "No nodes with active geometry preview in current workspace..."; }
                return currentPreviewsMsg;
            }
        }

        private ObservableCollection<ObjectInWorkspace> currentPreviews = new ObservableCollection<ObjectInWorkspace>();
        /// <summary>
        /// The search results as a list representation
        /// </summary>
        public ObservableCollection<ObjectInWorkspace> CurrentPreviews
        {
            get
            {
                List<ObjectInWorkspace> unorderedInputs = new List<ObjectInWorkspace>();
                foreach (NodeViewModel node in viewModel.CurrentSpaceViewModel.Nodes)
                {
                    if (node.NodeModel.IsVisible)
                    {
                        unorderedInputs.Add(new ObjectInWorkspace(node.NickName, node.NodeModel.GUID.ToString()));
                    }
                }
                currentPreviews.Clear();
                foreach (ObjectInWorkspace item in unorderedInputs.OrderBy(x => x.Name))
                {
                    currentPreviews.Add(item);
                }
                RaisePropertyChanged(nameof(CurrentPreviewsMsg));
                return currentPreviews;
            }
        }

        public void OnResetAllClicked(object obj)
        {
            foreach (NodeViewModel node in viewModel.CurrentSpaceViewModel.Nodes)
            {
                if (!node.IsVisible) { node.ToggleIsVisibleCommand.Execute(null); }
            }
            RaisePropertyChanged(nameof(CurrentPreviews));
        }

        public void OnAddSelectedClicked(object obj)
        {
            List<string> selectedGUIDs = new List<string>();
            foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
            {
                selectedGUIDs.Add(item.GUID.ToString());
            }
            foreach (NodeViewModel node in viewModel.CurrentSpaceViewModel.Nodes)
            {
                if (selectedGUIDs.Contains(node.NodeModel.GUID.ToString()))
                {
                    if (!node.IsVisible)
                    {
                        node.ToggleIsVisibleCommand.Execute(null);
                    }
                }
            }
            RaisePropertyChanged(nameof(CurrentPreviews));
        }

        public void OnRemoveSelectedClicked(object obj)
        {
            List<string> selectedGUIDs = new List<string>();
            foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
            {
                selectedGUIDs.Add(item.GUID.ToString());
            }
            foreach (NodeViewModel node in viewModel.CurrentSpaceViewModel.Nodes)
            {
                if (selectedGUIDs.Contains(node.NodeModel.GUID.ToString()))
                {
                    if (node.IsVisible)
                    {
                        node.ToggleIsVisibleCommand.Execute(null);
                    }
                }
            }
            RaisePropertyChanged(nameof(CurrentPreviews));
        }

        public void OnIsolateSelectedClicked(object obj)
        {
            List<string> selectedGUIDs = new List<string>();
            foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
            {
                selectedGUIDs.Add(item.GUID.ToString());
            }
            foreach (NodeViewModel node in viewModel.CurrentSpaceViewModel.Nodes)
            {
                if (selectedGUIDs.Contains(node.NodeModel.GUID.ToString()))
                {
                    if (!node.IsVisible)
                    {
                        node.ToggleIsVisibleCommand.Execute(null);
                    }
                }
                else
                {
                    if (node.IsVisible)
                    {
                        node.ToggleIsVisibleCommand.Execute(null);
                    }
                }
                
            }
            RaisePropertyChanged(nameof(CurrentPreviews));
        }

        private void CurrentWorkspaceModel_NodesChanged(NodeModel obj)
        {
            RaisePropertyChanged(nameof(CurrentPreviews));
        }
    }
}