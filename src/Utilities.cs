namespace Monito
{
    /// <summary>
    /// Class for structured storage of objects in workspace (nodes, text notes, groups).
    /// </summary>
    class ObjectInWorkspace
    {
        private string objectName;
        private string objectGUID;
        private int objectScore;

        public ObjectInWorkspace(string name, string guid, int score = 0)
        {
            this.objectName = name;
            this.objectGUID = guid;
            this.objectScore = score;
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
    }

    // We'll want zooming and highlighting in here as well
}
