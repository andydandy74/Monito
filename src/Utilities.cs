namespace Monito
{
    /// <summary>
    /// Class for structured storage of objects in workspace (nodes, text notes, groups).
    /// </summary>
    class ObjectInWorkspace
    {
        private string objectName;
        private string objectGUID;

        public ObjectInWorkspace(string name, string guid)
        {
            this.objectName = name;
            this.objectGUID = guid;
        }

        public string Name
        {
            get { return objectName; }
        }

        public string GUID
        {
            get { return objectGUID; }
        }
    }

    // We'll want zooming and highlighting in here as well
}
