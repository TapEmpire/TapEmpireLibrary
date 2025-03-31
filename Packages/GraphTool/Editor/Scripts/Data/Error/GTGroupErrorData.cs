using System.Collections.Generic;

namespace TEL.GraphTool.Data.Error
{
    using Elements;

    public class GTGroupErrorData
    {
        public GTErrorData ErrorData { get; set; }
        public List<GTGroup> Groups { get; set; }

        public GTGroupErrorData()
        {
            ErrorData = new GTErrorData();
            Groups = new List<GTGroup>();
        }
    }
}