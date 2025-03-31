using System.Collections.Generic;

namespace TEL.GraphTool.Data.Error
{
    using Elements;

    public class GTNodeErrorData
    {
        public GTErrorData ErrorData { get; set; }
        public List<GTNode> Nodes { get; set; }

        public GTNodeErrorData()
        {
            ErrorData = new GTErrorData();
            Nodes = new List<GTNode>();
        }
    }
}