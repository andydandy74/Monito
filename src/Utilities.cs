using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Monito
{
    /// <summary>
    /// Shared utility functions for view models
    /// </summary>
    class ViewModelUtils
    {
        private DynamoViewModel viewModel;
        private Window dynWindow;
        
        public ViewModelUtils(DynamoViewModel vm, Window dw)
        {
            viewModel = vm;
            dynWindow = dw;
        }
        /// <summary>
        /// Zoom in on the object with the given GUID.
        /// </summary>
        public void ZoomToObject(string guid)
        {
            bool isNode = viewModel.Model.CurrentWorkspace.Nodes.Count(x => x.GUID.ToString() == guid) > 0;
            bool isNote = viewModel.Model.CurrentWorkspace.Notes.Count(x => x.GUID.ToString() == guid) > 0;
            bool isAnno = viewModel.CurrentSpaceViewModel.Annotations.Count(x => x.AnnotationModel.GUID.ToString() == guid) > 0;
            double objectCenterX = 0;
            double objectCenterY = 0;
            if (isNode)
            {
                var zoomNode = viewModel.Model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == guid);
                objectCenterX = zoomNode.CenterX;
                objectCenterY = zoomNode.CenterY;
            }
            else if (isNote)
            {
                var zoomNote = viewModel.Model.CurrentWorkspace.Notes.First(x => x.GUID.ToString() == guid);
                objectCenterX = zoomNote.CenterX;
                objectCenterY = zoomNote.CenterY;
            }
            else if (isAnno)
            {
                var zoomAnno = viewModel.CurrentSpaceViewModel.Annotations.First(x => x.AnnotationModel.GUID.ToString() == guid);
                objectCenterX = zoomAnno.AnnotationModel.CenterX;
                objectCenterY = zoomAnno.AnnotationModel.CenterY;
            }
            var maxZoom = 4d;
            var corrX = -objectCenterX * maxZoom + dynWindow.ActualWidth / 2.2;
            var corrY = -objectCenterY * maxZoom + dynWindow.ActualHeight / 2.2;
            viewModel.CurrentSpaceViewModel.Zoom = maxZoom;
            viewModel.CurrentSpaceViewModel.X = corrX;
            viewModel.CurrentSpaceViewModel.Y = corrY;
            if (objectCenterX != 0 || objectCenterY !=0)
            {
                viewModel.ZoomInCommand.Execute(null);
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

        public static bool GetLoadedSettingAsBoolean(this KeyValueConfigurationCollection loadedsettings, string key)
        {
            if (loadedsettings[key] != null && loadedsettings[key].Value == "1") { return true; }
            else { return false; }
        }

        public static void ClearSelection()
        {
            var dynamoSelection = typeof(DynamoModel).Assembly.GetType("Dynamo.Selection.DynamoSelection");
            var selectionInstance = dynamoSelection.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var clearMethod = dynamoSelection.GetMethod("ClearSelection", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            clearMethod.Invoke(selectionInstance.GetValue(null), null);
        }
    }

    /// <summary>
    /// Class for structured storage of objects in workspace (nodes, text notes, groups).
    /// </summary>
    class ObjectInWorkspace
    {
        private string objectName;
        private string objectGUID;
        private double objectScore;
        private string objectDetails;

        public ObjectInWorkspace(string name, string guid, double score = 0, string details = "")
        {
            objectName = name;
            objectGUID = guid;
            objectScore = score;
            objectDetails = details;
        }

        public string Name
        {
            get { return objectName; }
        }

        public string GUID
        {
            get { return objectGUID; }
        }

        public double Score
        {
            get { return objectScore; }
        }

        public string Details
        {
            get { return objectDetails; }
        }
    }
}
