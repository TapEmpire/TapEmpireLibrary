using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Level
{
    [Serializable]
    public class LevelReference<T> : ILevelReference<T>
    {
        [SerializeField, HideLabel]
        private T _value;

        public T Reference => _value;

        public LevelReference(T value) => _value = value;
    }

    public class IntReference : LevelReference<int>
    {
        public IntReference(int value) : base(value) { }
    }

    public class BoolReference : LevelReference<bool>
    {
        public BoolReference(bool value) : base(value) { }
    }

    public class Vector3Reference : LevelReference<Vector3>
    {
        public Vector3Reference(Vector3 value) : base(value) { }
    }

    public class TransformReference : LevelReference<Transform>
    {
        public TransformReference(Transform value) : base(value) { }
    }

    public class TransformListReference : LevelReference<List<Transform>>
    {
        public TransformListReference(List<Transform> value) : base(value) { }
    }

    public class RectTransformReference : LevelReference<RectTransform>
    {
        public RectTransformReference(RectTransform value) : base(value) { }
    }

    public class CanvasReference : LevelReference<Canvas>
    {
        public CanvasReference(Canvas value) : base(value) { }
    }

    public class CameraReference : LevelReference<Camera>
    {
        public CameraReference(Camera value) : base(value) { }
    }

    public class SpriteRendererReference : LevelReference<SpriteRenderer>
    {
        public SpriteRendererReference(SpriteRenderer value) : base(value) { }
    }
}