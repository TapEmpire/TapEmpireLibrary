using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TEL.GraphTool.Elements
{
    using System.Linq;
    using Data;
    using Utilities;
    using Windows;

    public class GTMultipleLinkNode : GTNode
    {
        private VisualElement _separator = null;

        public override void Initialize(GTNodeInitializeData nodeData, GTGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(nodeData, dsGraphView, position);

            if (Links.Count == 0)
            {
                GTLinkData linkData = new GTLinkData()
                {
                    Text = "Next"
                };

                Links.Add(linkData);
            }
        }

        public override void Draw()
        {
            base.Draw();

            /* MAIN CONTAINER */

            Button addChoiceButton = GTElementUtility.CreateButton("Add Choice", () =>
            {
                GTLinkData choiceData = new GTLinkData()
                {
                    Text = "Next",
                    LinkType = GTLinkType.Regular,
                };

                Links.Add(choiceData);

                Port choicePort = CreateChoicePort(choiceData);

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            var addPreUnlockButton = GTElementUtility.CreateButton("Add PreUnlock", () =>
            {
                CreateSeparator();

                GTLinkData choiceData = new GTLinkData()
                {
                    Text = "PreUnlock",
                    LinkType = GTLinkType.PreUnlock,
                };

                Links.Add(choiceData);

                Port choicePort = CreateChoicePort(choiceData);

                inputContainer.Add(choicePort);
            });
            addPreUnlockButton.AddToClassList("ds-node__button");

            mainContainer.Insert(2, addPreUnlockButton);

            /* OUTPUT CONTAINER */

            var hasPreUnlockPorts = Links.Any(link => link.LinkType == GTLinkType.PreUnlock);

            if (hasPreUnlockPorts)
            {
                CreateSeparator();
            }

            foreach (GTLinkData choice in Links)
            {
                Port choicePort = CreateChoicePort(choice);

                var isPreUnlockPort = choice.LinkType == GTLinkType.PreUnlock;
                System.Action add = isPreUnlockPort ? () => inputContainer.Add(choicePort) : () => outputContainer.Add(choicePort);
                add.Invoke();
            }

            RefreshExpandedState();
        }

        private void CreateSeparator()
        {
            if (_separator == null)
            {
                _separator = GTElementUtility.CreateVerticalLine(2.0f, 0.0f);
                inputContainer.Add(_separator);
            }
        }

        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            GTLinkData linkData = (GTLinkData)userData;

            Button deleteChoiceButton = GTElementUtility.CreateButton("X", () =>
            {
                if (Links.Count == 1)
                {
                    return;
                }

                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                Links.Remove(linkData);

                graphView.RemoveElement(choicePort);

                if (linkData.LinkType == GTLinkType.PreUnlock && !Links.Any(link => link.LinkType == GTLinkType.PreUnlock))
                {
                    inputContainer.Remove(_separator);
                    _separator = null;
                }
            });

            deleteChoiceButton.AddToClassList("ds-node__button");

            Label linkLabel = GTElementUtility.CreateLabel(linkData.Text);

            linkLabel.AddClasses("ds-node__link-label-field");

            choicePort.Add(linkLabel);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
    }
}