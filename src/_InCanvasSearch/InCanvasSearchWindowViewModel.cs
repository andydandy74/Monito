using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using System.Collections.ObjectModel;
using Dynamo.ViewModels;
using System.Windows;

namespace Monito
{
    class InCanvasSearchWindowViewModel : NotificationObject, IDisposable
    {
        // Variable for storing a reference to our loaded parameters
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;

        public InCanvasSearchWindowViewModel(ReadyParams p, DynamoViewModel vm)
        {
            // Save a reference to our loaded parameters which
            // is required in order to access the workspaces
            readyParams = p;
            viewModel = vm;
        }

        // Very important - unsubscribe from our events to prevent a memory leak
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
                searchInCanvas(searchTerm);
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
        public void searchInCanvas(string searchTerm)
        {
            searchResults.Clear();
            foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
            {
                // Basic search. We can expand on this later, e.g. add node descriptions & values, text note content & group titles
                if (node.NickName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                {
                    searchResults.Add(new SearchResult(node.NickName, node.GUID.ToString()));
                }
            }
        }

        private string zoomGUID;
        /// <summary>
        /// The search term. Changes in the search field will trigger searchInCanvas().
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
                MessageBox.Show("zoomGUID was set...");
                try
                {
                    viewModel.CurrentSpaceViewModel.ResetFitViewToggleCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
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