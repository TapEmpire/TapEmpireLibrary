using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace TEL.GraphTool.Utilities
{
    using Elements;
    using UnityEngine;

    public static class GTElementUtility
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };

            return button;
        }

        public static Toggle CreateToggle(string text, bool value, EventCallback<ChangeEvent<bool>> onValueChanged = null)
        {
            var toggle = new Toggle()
            {
                text = text,
                value = value
            };

            if (onValueChanged != null)
            {
                toggle.RegisterValueChangedCallback(onValueChanged);
            }

            return toggle;
        }

        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }

        public static Port CreatePort(this GTNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));

            port.portName = portName;

            return port;
        }

        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }

        public static Label CreateLabel(string value = null)
        {
            Label label = new Label(value);

            return label;
        }

        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);

            textArea.multiline = true;

            return textArea;
        }

        public static VisualElement CreateVerticalLine(float height = 2.0f, float margin = 10.0f)
        {
            VisualElement line = new VisualElement();

            line.style.height = height;
            line.style.backgroundColor = Color.grey;
            line.style.marginTop = margin;
            line.style.marginBottom = margin;

            return line;
        }

        public static VisualElement CreateHorizontalContainer()
        {
            VisualElement horizontalContainer = new VisualElement();

            horizontalContainer.style.flexDirection = FlexDirection.Row;
            // horizontalContainer.style.justifyContent = Justify.SpaceBetween; // This spreads out the children
            // horizontalContainer.style.alignItems = Align.Center; // This centers children vertically
            // horizontalContainer.style.paddingLeft = 10;
            // horizontalContainer.style.paddingRight = 10;

            horizontalContainer.style.marginLeft = 0;
            horizontalContainer.style.marginRight = 0;

            horizontalContainer.style.flexGrow = 1;


            return horizontalContainer;
        }
    }
}