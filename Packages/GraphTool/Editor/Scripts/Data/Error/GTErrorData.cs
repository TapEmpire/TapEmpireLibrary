using UnityEngine;

namespace TEL.GraphTool.Data.Error
{
    public class GTErrorData
    {
        public Color Color { get; set; }

        public GTErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32(
                (byte) Random.Range(65, 256),
                (byte) Random.Range(50, 176),
                (byte) Random.Range(50, 176),
                255
            );
        }
    }
}