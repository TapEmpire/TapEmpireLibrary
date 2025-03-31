using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TEL.Attributes;
using TEL.GraphTool.Data;

namespace TEL.GraphTool
{
    using TapEmpire.Utility;
    using Utilities;

    public class GTComponentNode : MonoBehaviour
    {
        public Action<GTComponentNode, bool> OnStartUnlock = null;
        public Action<GTComponentNode> OnUnlocked = null;
        public Action<GTComponentNode> OnMadeUnlockable = null;
        public Action<GTComponentNode> OnUnlockAnimationFinished = null;

        [field: SerializeField]
        [field: ReadOnly]
        public string NodeID { get; protected set; } = GenerateID();

        [field: SerializeField] public int NumberOfLevels { get; protected set; } = 1;
        [field: SerializeField] public int CurrentLevel { get; protected set; } = 0;
        [field: SerializeField] public bool IsUnlockable { get; protected set; } = false;
        [field: SerializeField] public bool IsAutoUnlockable { get; set; } = false;

        public IUserData UserData = null;

        public string LeveledNodeId => $"{NodeID}_{CurrentLevel}";
        public string NextLeveledNodeId => $"{NodeID}_{CurrentLevel + 1}";

        public List<string> NodeIds =>
            Enumerable.Range(0, NumberOfLevels).Select(level => $"{NodeID}_{level}").ToList();

        public bool IsFullyUnlocked => CurrentLevel == NumberOfLevels;

        public bool IsUnlocked(int level)
        {
            return CurrentLevel >= level;
        }

        public virtual void SetInitialState()
        {
        }

        public virtual void MakeUnlockable(int level)
        {
            // Debug.Log($"Make unlockable {CurrentLevel} {level} {NumberOfLevels}");
            if (CurrentLevel + 1 == level && CurrentLevel < NumberOfLevels)
            {
                IsUnlockable = true;
                OnMadeUnlockable?.Invoke(this);

                if (IsAutoUnlockable)
                {
                    Utilities.Delay(0.0f, () => Unlock(false).Forget());
                }
            }
        }

        public virtual async UniTask Unlock(bool quick = false)
        {
            if (UnlockInternal())
            {
                OnUnlockAnimationFinished?.Invoke(this);
            }
        }

        public virtual async UniTask Unlock(int level, bool quick = false)
        {
            if (UnlockInternal(level))
            {
                OnUnlockAnimationFinished?.Invoke(this);
            }
        }

        public virtual async UniTask UnlockReset(int level, bool isUnlockable)
        {
            if (level <= NumberOfLevels)
            {
                CurrentLevel = level;
                IsUnlockable = isUnlockable;

                if (isUnlockable)
                {
                    OnMadeUnlockable?.Invoke(this);
                }

                if (level > 0)
                {
                    OnUnlocked?.Invoke(this);
                }
            }
        }

        public virtual async UniTask UnlockWithoutPreNodes(bool quick = false)
        {
            if (UnlockInternal())
            {
                OnUnlockAnimationFinished?.Invoke(this);
            }
        }

        public virtual async UniTask UnlockUnlockable(bool quick = false)
        {
        }

        public void SetUserData(UnlockUserData userData)
        {
            if (UserData == null)
            {
                UserData = new UnlockUserData();
            }

            var unlockData = (UnlockUserData)UserData;

            if (userData.Cost != null) unlockData.Cost = userData.Cost;
            if (userData.UnlockType != null) unlockData.UnlockType = userData.UnlockType;
            if (userData.Coins != null) unlockData.Coins = userData.Coins;
        }

        public void ClearUserData()
        {
            UserData = null;
        }

        protected bool UnlockInternal()
        {
            // Debug.Log($"Unlock {IsUnlockable} {CurrentLevel} {NumberOfLevels}");
            // if (IsUnlockable && CurrentLevel < NumberOfLevels)
            if (CurrentLevel < NumberOfLevels)
            {
                ++CurrentLevel;
                IsUnlockable = false;
                OnUnlocked?.Invoke(this);
                return true;
            }

            return false;
        }

        protected bool UnlockInternal(int level)
        {
            if (level <= NumberOfLevels && CurrentLevel < level)
            {
                CurrentLevel = level;
                IsUnlockable = false;
                OnUnlocked?.Invoke(this);
                return true;
            }

            return false;
        }

        private static string GenerateID()
        {
            return Guid.NewGuid().ToString();
        }

        protected virtual void OnValidate()
        {
            if (Event.current == null)
            {
                return;
            }

            if (Event.current.type == EventType.ExecuteCommand)
            {
                Debug.Log("Validate " + Event.current?.type + " " + Event.current?.commandName);
                var commandName = Event.current.commandName;

                if (commandName == "Duplicate" || commandName == "Paste")
                {
                    ResetNodeID();
                }

                return;
            }

            if (Event.current.type == EventType.DragPerform)
            {
                ResetNodeID();
                return;
            }
        }

        public void ResetNodeID()
        {
            NodeID = GenerateID();
            EditorTools.SetDirty(this);
        }
    }
}