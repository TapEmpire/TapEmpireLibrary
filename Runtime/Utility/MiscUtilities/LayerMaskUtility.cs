using UnityEngine;

namespace TapEmpire.Utility
{
    public static class LayerMaskUtility
    {
        // public static void SetLayerMaskRecursively(GameObject obj, LayerMask layerMask)
        // {
        //     var layerInt = LayerMaskToIntLayer(layerMask);
        //     obj.layer = layerInt;
        //     foreach (Transform child in obj.transform)
        //     {
        //         SetLayerIntRecursively(child.gameObject, layerInt);
        //     }
        // }
        //
        // private static void SetLayerIntRecursively(GameObject obj, int layer)
        // {
        //     obj.layer = layer;
        //     foreach (Transform child in obj.transform)
        //     {
        //         SetLayerIntRecursively(child.gameObject, layer);
        //     }
        // }

        public static void SetLayerMask(this GameObject obj, LayerMask layerMask)
        {
            obj.layer = LayerMaskToIntLayer(layerMask);
        } 

        public static int LayerMaskToIntLayer(LayerMask layerMask)
        {
            var layerNumber = 0;
            var layer = layerMask.value;
            while (layer > 0)
            {
                layer = layer >> 1;
                layerNumber++;
            }
            return layerNumber - 1; // Because layer numbers start from zero
        }
    }
}