using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using System.Collections.ObjectModel;
using Dynamo.ViewModels;
using System.Windows;
using System.Linq;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Notes;

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
                searchResults.Clear();
                foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
                {
                    // Basic search. We can expand on this later (issue #5)
                    if (node.NickName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        searchResults.Add(new SearchResult("[Node] " + node.NickName, node.GUID.ToString()));
                    }
                }
               /* foreach (NoteModel note in viewModel.Model.CurrentWorkspace.Notes)
                {
                    if (note.Text.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        searchResults.Add(new SearchResult("[Text Note] " + note.Text, note.GUID.ToString()));
                    }
                }
                foreach (AnnotationModel anno in viewModel.Model.CurrentWorkspace.Annotations)
                {
                    if (anno.Text.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        searchResults.Add(new SearchResult("[Group] " + anno.Text, anno.GUID.ToString()));
                    }
                    searchResults.Add(new SearchResult("[Group] " + anno.Text, anno.GUID.ToString()));
                }*/
                return searchResults;
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
                
                foreach (var item in readyParams.CurrentWorkspaceModel.Nodes)
                {
                    item.Deselect();
                    item.IsSelected = false;
                }
                var node = readyParams.CurrentWorkspaceModel.Nodes.First(x => x.GUID.ToString() == guid);
                // Zoom in on our node and deselect it again
                viewModel.CurrentSpaceViewModel.ResetFitViewToggleCommand.Execute(null);
                viewModel.AddToSelectionCommand.Execute(node);
                viewModel.FitViewCommand.Execute(null);
                // BUG: Apparently this does NOT remove the node from the selection again
                // so each time we click on another button we add one more node to our selection
                // which results in only the first zoom operation being successful
                node.Deselect();
                node.IsSelected = false;                
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