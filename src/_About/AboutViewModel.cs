using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using System.Collections.ObjectModel;
using Dynamo.ViewModels;
using System.Windows;
using System.Linq;

namespace Monito
{
    class SearchInWorkspaceViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;

        public SearchInWorkspaceViewModel(ReadyParams p, DynamoViewModel vm)
        {
            readyParams = p;
            viewModel = vm;
        }

        public void Dispose() { }

        private string searchTerm;
        /// <summary>
        /// The search term. Changes in the search field will trigger searchInCanvas().
        /// </summary>
        public string SearchTerm
        {
            get
            {
                return searchTerm;
            }
            set
            {
                searchTerm = value;
                searchInWorkspace(searchTerm);
                RaisePropertyChanged(nameof(SearchResults));
            }
        }

        private ObservableCollection<SearchResult> searchResults = new ObservableCollection<SearchResult>();
        /// <summary>
        /// The search results as a list representation
        /// </summary>
        public ObservableCollection<SearchResult> SearchResults
        {
            get
            {
                return searchResults;
            }
        }

        /// <summary>
        /// The actual search function. Will update search results.
        /// </summary>
       private void searchInWorkspace(string searchTerm)
        {
            searchResults.Clear();
            foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
            {
                // Basic search. We can expand on this later, e.g. add node descriptions & values, text note content & group titles
                // This is how we can get notes and groups:
                // viewModel.Model.CurrentWorkspace.Notes
                // viewModel.Model.CurrentWorkspace.Annotations
                if (node.NickName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                {
                    searchResults.Add(new SearchResult("[Node] " + node.NickName, node.GUID.ToString()));
                }
            }
        }

        private string zoomGUID;
        /// <summary>
        /// The GUID of the node that was selected from the search results. Triggered by button click.
        /// </summary>
        public string ZoomGUID
        {
            get
            {
                return zoomGUID;
            }
            set
            {
                zoomGUID = value;
                ZoomToNode(value);
            }
        }

        /// <summary>
        /// Zoom in on the node with the given GUID.
        /// </summary>
        private void ZoomToNode(string guid)
        {
            try
            {
                // Clear current selection and select our node
                foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
                {
                    item.Deselect();
                }
                var node = readyParams.CurrentWorkspaceModel.Nodes.First(x => x.GUID.ToString() == guid);
                node.Select();
                // Get the node center
                var nodeX = node.CenterX;
                var nodeY = node.CenterY;
                /*
                // How do we set the workspace center?
                viewModel.Model.CurrentWorkspace.X = nodeX;
                viewModel.Model.CurrentWorkspace.Y = nodeY;
                viewModel.Model.CurrentWorkspace.Zoom = 2;
                // Zoom in
                // How do we zoom in?
                // var currentZoom = viewModel.Model.CurrentWorkspace.Zoom;
                // viewModel.CurrentSpaceViewModel.SetZoomCommand.Execute(2);
                // Just for testing*/
                // viewModel.CurrentSpaceViewModel.ResetFitViewToggleCommand.Execute(null);*/
                MessageBox.Show(nodeX.ToString() + " - " + nodeY.ToString() + " - ");
                // Deselect node since somehow it can only be deselected programmatically
                node.Deselect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Class for structured storage of search results.
    /// </summary>
    class SearchResult
    {
        private string nodeName;
        private string nodeGUID;

        public SearchResult(string name, string guid)
        {
            this.nodeName = name;
            this.nodeGUID = guid;
        }

        public string NodeName
        {
            get { return nodeName; }
        }

        public string NodeGUID
        {
            get { return nodeGUID; }
        }
    }
}