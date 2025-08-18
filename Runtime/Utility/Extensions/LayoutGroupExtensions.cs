using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Utility
{
    public static class LayoutGroupExtensions
    {
        public static float CalculateHeightAsGrid(this LayoutGroup self, int elementsCount)
        {
            var grid = self as GridLayoutGroup;
            RectOffset padding = grid.padding;
            float cellHeight = grid.cellSize.y;
            float spacingY = grid.spacing.y;
            int childCount = elementsCount; // grid.transform.childCount;

            int columns = 1;

            switch (grid.constraint)
            {
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    columns = grid.constraintCount;
                    break;
                case GridLayoutGroup.Constraint.FixedRowCount:
                    columns = Mathf.CeilToInt((float)childCount / grid.constraintCount);
                    break;
                case GridLayoutGroup.Constraint.Flexible:
                    Debug.LogWarning("GridLayoutGroup.Constraint.Flexible is not supported for manual size calculation.");
                    return 0;
            }

            int rows = Mathf.CeilToInt((float)childCount / columns);

            float totalHeight = padding.top + (cellHeight * rows) + (spacingY * (rows - 1)) + padding.bottom;
            return totalHeight;
        }

        public static float CalculateHeightAsVertical(this LayoutGroup self, IEnumerable<RectTransform> elements)
        {
            var layoutGroup = self as VerticalLayoutGroup;
            RectOffset padding = layoutGroup.padding;
            float spacing = layoutGroup.spacing;

            float totalHeight = padding.top + padding.bottom;
            int activeChildCount = 0;

            totalHeight += elements.Sum(element => element.rect.height);
            activeChildCount += elements.Count();

            // foreach (RectTransform child in layoutGroup.transform)
            // {
            //     if (!child.gameObject.activeInHierarchy)
            //         continue;

            //     LayoutElement le = child.GetComponent<LayoutElement>();
            //     float preferredHeight = le != null && le.ignoreLayout == false && le.preferredHeight >= 0
            //         ? le.preferredHeight
            //         : child.rect.height; // LayoutUtility.GetPreferredHeight(child);

            //     totalHeight += preferredHeight;
            //     activeChildCount++;
            // }

            if (activeChildCount > 1)
                totalHeight += spacing * (activeChildCount - 1);

            return totalHeight;
        }
    }
}