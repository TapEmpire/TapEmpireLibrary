using System.Linq;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class MeshHelper
    {
        public static void UpdateMeshRendererInChildren(SkinnedMeshRenderer sourceRenderer, SkinnedMeshRenderer targetRenderer)
        {
            AssignBones(sourceRenderer, targetRenderer, sourceRenderer.rootBone.GetComponentsInChildren<Transform>(true));
        }
        
        public static void UpdateMeshRenderer(SkinnedMeshRenderer sourceRenderer, SkinnedMeshRenderer targetRenderer)
        {
            AssignBones(sourceRenderer, targetRenderer, sourceRenderer.bones);
        }

        private static void AssignBones(SkinnedMeshRenderer sourceRenderer, SkinnedMeshRenderer targetRenderer, Transform[] children)
        {
            sourceRenderer.sharedMesh = targetRenderer.sharedMesh;

            var bones = new Transform[targetRenderer.bones.Length];
            for (int i = 0; i < targetRenderer.bones.Length; i++)
            {
                bones[i] = children.First(x => x.name == targetRenderer.bones[i].name);
            }

            sourceRenderer.bones = bones;
        }
    }
}