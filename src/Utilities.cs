using Dynamo.Extensions;
using Dynamo.ViewModels;
using System;
using System.Linq;
using System.Windows;

namespace Monito
{
    /// <summary>
    /// Shared utility functions for view models
    /// </summary>
    class ViewModelUtils
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;
        
        public ViewModelUtils(ReadyParams p, DynamoViewModel vm)
        {
            readyParams = p;
            viewModel = vm;
        }
        /// <summary>
        /// Zoom in on the object with the given GUID.
        /// </summary>
        public void ZoomToObject(string guid)
        {
            try
            {
                // Clear current selection and select our node
                foreach (var item in readyParams.CurrentWorkspaceModel.Nodes)
                {
                    item.Deselect();
                    item.IsSelected = false;
                }

                bool isNode = readyParams.CurrentWorkspaceModel.Nodes.Count(x => x.GUID.ToString() == guid) > 0;
                bool isNote = viewModel.Model.CurrentWorkspace.Notes.Count(x => x.GUID.ToString() == guid) > 0;
                bool isAnno = viewModel.Model.CurrentWorkspace.Annotations.Count(x => x.GUID.ToString() == guid) > 0;

                // Zoom in on our node and deselect it again
                // BUG: Apparently this does NOT remove the node from the selection again
                // so each time we click on another button we add one more node to our selection
                // which results in only the first zoom operation being successful
                viewModel.CurrentSpaceViewModel.ResetFitViewToggleCommand.Execute(null);
                if (isNode)
                {
                    var zoomNode = readyParams.CurrentWorkspaceModel.Nodes.First(x => x.GUID.ToString() == guid);
                    viewModel.AddToSelectionCommand.Execute(zoomNode);
                    viewModel.FitViewCommand.Execute(null);
                    zoomNode.Deselect();
                    zoomNode.IsSelected = false;
                }
                else if (isNote)
                {
                    var zoomNote = viewModel.Model.CurrentWorkspace.Notes.First(x => x.GUID.ToString() == guid);
                    viewModel.AddToSelectionCommand.Execute(zoomNote);
                    viewModel.FitViewCommand.Execute(null);
                    zoomNote.Deselect();
                    zoomNote.IsSelected = false;
                }
                else if (isAnno)
                {
                    var zoomAnno = viewModel.Model.CurrentWorkspace.Annotations.First(x => x.GUID.ToString() == guid);
                    viewModel.AddToSelectionCommand.Execute(zoomAnno);
                    viewModel.FitViewCommand.Execute(null);
                    zoomAnno.Deselect();
                    zoomAnno.IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Shared general utility functions
    /// </summary>
    static class GeneralUtils
    {
        public static string Abbreviate(this String str)
        {
            str = str.Replace(Environment.NewLine, " ");
            if (str.Length > 60)
            {
                str = str.Substring(0, 60) + "...";
            }
            return str;
        }
    }

    /// <summary>
    /// Class for structured storage of objects in workspace (nodes, text notes, groups).
    /// </summary>
    class ObjectInWorkspace
    {
        private string objectName;
        private string objectGUID;
        private int objectScore;
        private string objectDetails;

        public ObjectInWorkspace(string name, string guid, int score = 0, string details = "")
        {
            this.objectName = name;
            this.objectGUID = guid;
            this.objectScore = score;
            this.objectDetails = details;
        }

        public string Name
        {
            get { return objectName; }
        }

        public string GUID
        {
            get { return objectGUID; }
        }

        public int Score
        {
            get { return objectScore; }
        }

        public string Details
        {
            get { return objectDetails; }
        }
    }
}
