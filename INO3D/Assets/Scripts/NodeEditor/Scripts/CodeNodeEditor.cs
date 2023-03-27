using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Managers;
using RuntimeNodeEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class CodeNodeEditor : RuntimeNodeEditor.NodeEditor
    {
        #region Fields

        private Dictionary<string, string> nodes;

        #endregion

        #region Overrides

        public override void StartEditor(NodeGraph graph)
        {
            nodes = new Dictionary<string, string>
            {
                {"PinMode", "Nodes/PinModeNode"},
                {"Number", "Nodes/NumberNode"},
                {"Text", "Nodes/TextNode"},
                {"Math", "Nodes/MathNode"},
                {"Digital/DigitalRead", "Nodes/DigitalReadNode"},
                {"Digital/DigitalWrite", "Nodes/DigitalWriteNode"},
                {"Analog/AnalogRead", "Nodes/AnalogReadNode"},
                {"Analog/AnalogWrite", "Nodes/AnalogWriteNode"},
                {"Delay", "Nodes/DelayNode"},
                {"Millis", "Nodes/MillisNode"},
                //{"Serial/SerialRead", "Nodes/SerialReadNode"},
                {"Serial/SerialWrite", "Nodes/SerialWriteNode"},
                {"Conditional", "Nodes/ConditionalNode"}
            };

            base.StartEditor(graph);

            Events.OnGraphPointerClickEvent += OnGraphPointerClick;
            Events.OnNodePointerClickEvent += OnNodePointerClick;
            Events.OnConnectionPointerClickEvent += OnNodeConnectionPointerClick;
            Events.OnSocketConnect += OnConnect;

            Graph.SetSize(Vector2.one * 20000);
        }

        #endregion

        #region Public Methods

        public string GenerateLoopCode()
        {
            var code = string.Empty;
            foreach (var node in Graph.nodes.Where(node => !(node is PinModeNode) && node.IsTerminal && node.PreviousNodeSocket != null && !node.PreviousNodeSocket.HasConnection()))
                code += node.Value;
            return code;
        }

        public string GenerateSetupCode()
        {
            var code = string.Empty;
            foreach (var node in Graph.nodes.Where(node => node is PinModeNode && node.IsTerminal))
                code += node.Value;
            return code;
        }

        public string SaveGraph()
        {
            CloseContextMenu();
            return Graph.ExportJson();
        }

        public void LoadGraph(string graph)
        {
            CloseContextMenu();
            Graph.Clear();
            Graph.Load(graph);
        }

        #endregion

        #region Private Methods

        private void OnConnect(SocketInput arg1, SocketOutput arg2)
        {
            Graph.drawer.SetConnectionColor(arg2.connection.connId, Color.white);
        }

        private void OnGraphPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Right:
                {
                    var ctx = new ContextMenuBuilder();
                    foreach (var node in nodes)
                    {
                        ctx.Add(LocalizationManager.Instance.Localize("Nodes") + "/" + node.Key, () =>
                        {
                            Graph.Create(node.Value);
                            CloseContextMenu();
                        });
                    }

                    SetContextMenu(ctx.Build());
                    DisplayContextMenu();
                }
                    break;
                case PointerEventData.InputButton.Left:
                    CloseContextMenu();
                    break;
            }
        }

        private void OnNodePointerClick(Node node, PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                var ctx = new ContextMenuBuilder()
                    .Add(LocalizationManager.Instance.Localize("Nodes.Duplicate"), () => DuplicateNode(node))
                    .Add(LocalizationManager.Instance.Localize("Nodes.MultiClear"), () => ClearConnections(node))
                    .Add(LocalizationManager.Instance.Localize("Nodes.Delete"), () => DeleteNode(node))
                    .Build();

                SetContextMenu(ctx);
                DisplayContextMenu();
            }
        }

        private void OnNodeConnectionPointerClick(string connId, PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                var ctx = new ContextMenuBuilder()
                    .Add(LocalizationManager.Instance.Localize("Nodes.Clear"), () => DisconnectConnection(connId))
                    .Build();

                SetContextMenu(ctx);
                DisplayContextMenu();
            }
        }

        #region ContextMenu

        private void DeleteNode(Node node)
        {
            Graph.Delete(node);
            CloseContextMenu();
        }

        private void DuplicateNode(Node node)
        {
            Graph.Duplicate(node);
            CloseContextMenu();
        }

        private void DisconnectConnection(string lineId)
        {
            Graph.Disconnect(lineId);
            CloseContextMenu();
        }

        private void ClearConnections(Node node)
        {
            Graph.ClearConnectionsOf(node);
            CloseContextMenu();
        }

        #endregion

        #endregion
    }
}