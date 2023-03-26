﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RuntimeNodeEditor
{
    public class NodeGraph : MonoBehaviour
    {
        public RectTransform        GraphContainer  => _graphContainer;

        //  scene references
        public RectTransform        contextMenuContainer;
        public RectTransform        nodeContainer;
        public RectTransform        background;
        public GraphPointerListener pointerListener;
        public BezierCurveDrawer    drawer;

        public List<Node>           nodes;
        public List<Connection>     connections;

        //  cache
        private SocketOutput        _currentDraggingSocket;
        private Vector2             _pointerOffset;
	    private Vector2             _localPointerPos;
	    private Vector2             _duplicateOffset;
        private Vector2             _zoomCenterPos;
        private float               _currentZoom;
        private float               _minZoom;
        private float               _maxZoom;
        private RectTransform       _nodeContainer;
	    private RectTransform       _graphContainer;

        private SignalSystem        _signalSystem;

        public void Init(SignalSystem signalSystem, float minZoom, float maxZoom)
        {
            _nodeContainer                              = nodeContainer;
            _graphContainer                             = this.GetComponent<RectTransform>();
	        _duplicateOffset                            = (Vector2.one * 10f);
            nodes                                       = new List<Node>();
	        connections                                 = new List<Connection>();
            _signalSystem                               = signalSystem;
            _currentZoom                                = 1f;
            _minZoom                                    = minZoom;
            _maxZoom                                    = maxZoom;

            _signalSystem.OnOutputSocketDragStartEvent    += OnOutputDragStarted;
            _signalSystem.OnOutputSocketDragDropEvent     += OnOutputDragDroppedTo;
            _signalSystem.OnInputSocketClickEvent         += OnInputSocketClicked;
            _signalSystem.OnOutputSocketClickEvent        += OnOutputSocketClicked;
            _signalSystem.OnNodePointerDownEvent          += OnNodePointerDown;
            _signalSystem.OnNodePointerDragEvent          += OnNodePointerDrag;
            _signalSystem.OnGraphPointerDragEvent         += OnGraphPointerDragged;
            _signalSystem.OnGraphPointerScrollEvent       += OnGraphPointerScrolled;

            pointerListener.Init(_signalSystem);
            drawer.Init(_signalSystem);
        }

        public void SetSize(Vector2 size)
        {
            _graphContainer.sizeDelta = size;
        }

        public void Create(string prefabPath)
        {
	        var mousePosition   = Utility.GetMousePosition();
	        var pos             = Utility.GetLocalPointIn(nodeContainer, mousePosition);
            
            Create(prefabPath, pos);
        }

        public void Create(string prefabPath, Vector2 pos)
        {
            var node            = Utility.CreateNodePrefab<Node>(prefabPath, nodeContainer);
            node.Init(_signalSystem, _signalSystem, pos, NewId(), prefabPath);
            node.Setup();
            nodes.Add(node);
            HandleSocketRegister(node);
        }

        public void Delete(Node node)
        {
            ClearConnectionsOf(node);
            Destroy(node.gameObject);
            nodes.Remove(node);
        }
        
	    public void Duplicate(Node node)
	    {
		    Serializer info = new Serializer();
		    node.OnSerialize(info);
		    Create(node.LoadPath, node.Position + _duplicateOffset);
		    var newNode = nodes.Last();
		    newNode.OnDeserialize(info);
	    }

        public void Connect(SocketInput input, SocketOutput output)
        {
            var connection = new Connection(NewId(), input, output);

            input.Connect(connection);
            output.Connect(connection);

            connections.Add(connection);
            input.OwnerNode.Connect(input, output);
            drawer.Add(connection.connId, output.handle, input.handle);

            _signalSystem.InvokeSocketConnection(input, output);
        }

        public void Disconnect(Connection conn)
        {
            var input   = conn.input;
            var output  = conn.output;

            drawer.Remove(conn.connId);
            input.OwnerNode.Disconnect(input, output);

            input.Disconnect(conn);
            output.Disconnect();

            connections.Remove(conn);
            _signalSystem.InvokeSocketDisconnection(input, output);
        }

        public void Disconnect(SocketInput input)
        {
            var dcList = new List<Connection>(input.Connections);
            foreach (var conn in dcList)
            {
                Disconnect(conn);   
            }
        }

        public void Disconnect(string id)
        {
            var connection = connections.FirstOrDefault<Connection>(c => c.connId == id);
            Disconnect(connection);
        }

        public void ClearConnectionsOf(Node node)
        {
            connections.Where(conn => conn.output.OwnerNode == node || conn.input.OwnerNode == node)
                .ToList()
                .ForEach(conn => Disconnect(conn));
        }

        public void Load(string file)
        {
            try
            {
                var graph = JsonUtility.FromJson<GraphData>(file);

                foreach (var data in graph.nodes)
                {
                    LoadNode(data);
                }

                foreach (var node in nodes)
                {
                    var nodeData = graph.nodes.FirstOrDefault(data => data.id == node.ID);

                    for (int i = 0; i < nodeData.inputSocketIds.Length; i++)
                    {
                        node.Inputs[i].socketId = nodeData.inputSocketIds[i];
                    }

                    for (int i = 0; i < nodeData.outputSocketIds.Length; i++)
                    {
                        node.Outputs[i].socketId = nodeData.outputSocketIds[i];
                    }
                }

                foreach (var data in graph.connections)
                {
                    LoadConn(data);
                }

                drawer.UpdateDraw();
            }
            catch
            {
                Clear();
            }
        }

        public string ExportJson()
        {
            return JsonUtility.ToJson(Export(), true);
        }

        public GraphData Export()
        {
            var graph       = new GraphData();
            var nodeDatas   = new List<NodeData>();
            var connDatas   = new List<ConnectionData>();

            foreach (var node in nodes)
            {
                var ser = new Serializer();
                var data = new NodeData();
                node.OnSerialize(ser);

                data.id = node.ID;
                data.values = ser.Serialize();
                data.posX = node.Position.x;
                data.posY = node.Position.y;
                data.path = node.LoadPath;

                var inputIds = new List<string>();
                foreach (var input in node.Inputs)
                {
                    inputIds.Add(input.socketId);
                }

                var outputIds = new List<string>();
                foreach (var output in node.Outputs)
                {
                    outputIds.Add(output.socketId);
                }

                data.inputSocketIds = inputIds.ToArray();
                data.outputSocketIds = outputIds.ToArray();

                nodeDatas.Add(data);
            }

            foreach (var conn in connections)
            {
                var data = new ConnectionData();
                data.id = conn.connId;
                data.outputSocketId = conn.output.socketId;
                data.inputSocketId = conn.input.socketId;

                connDatas.Add(data);
            }

            graph.nodes = nodeDatas.ToArray();
            graph.connections = connDatas.ToArray();

            return graph;
        }

        public void Clear()
        {
            var nodesToClear = new List<Node>(nodes);
            nodesToClear.ForEach(n => Delete(n));

            drawer.UpdateDraw();
        }

        //  event handlers
        protected virtual void OnInputSocketClicked(SocketInput input, PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                connections.Where(conn => conn.input == input)
                            .ToList()
                            .ForEach(conn => Disconnect(conn));
            }
        }

        protected virtual void OnOutputSocketClicked(SocketOutput output, PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                connections.Where(conn => conn.output == output)
                            .ToList()
                            .ForEach(conn => Disconnect(conn));
            }
        }

        protected virtual void OnOutputDragDroppedTo(SocketInput target)
        {
            if (_currentDraggingSocket == null || target == null)
            {
                _currentDraggingSocket = null;
                drawer.CancelDrag();

                return;
            }

            // check if output connected to this target input already 
            if (_currentDraggingSocket.HasConnection() && target.HasConnection())
            {
                if (target.Connections.Contains(_currentDraggingSocket.connection) )
                {
                    //  then do nothing
                    _currentDraggingSocket = null;
                    drawer.CancelDrag();

                    return;
                }
            }

            if (target.HasConnection())
            {
                //  check if input allows multiple connection
                if (target.connectionType == ConnectionType.Single)
                {
                    //  disconnect old connection
                    Disconnect(target);
                }
            }

            Connect(target, _currentDraggingSocket);
            drawer.UpdateDraw();

            _currentDraggingSocket = null;
            drawer.CancelDrag();
        }

        protected virtual void OnOutputDragStarted(SocketOutput socketOnDrag)
        {
            _currentDraggingSocket = socketOnDrag;
            drawer.StartDrag(_currentDraggingSocket);
            drawer.UpdateDraw();

            //  check socket connection type
            if (_currentDraggingSocket.HasConnection())
            {
                //  if single, disconnect
                if (_currentDraggingSocket.connectionType == ConnectionType.Single)
                {
                    Disconnect(_currentDraggingSocket.connection);
                }
            }
        }

        protected virtual void OnNodePointerDown(Node node, PointerEventData eventData)
        {
            node.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(node.PanelRect, eventData.position,
                                                                    eventData.pressEventCamera, out _pointerOffset);
            DragNode(node, eventData);
        }

        protected virtual void OnNodePointerDrag(Node node, PointerEventData eventData)
        {
            DragNode(node, eventData);
        }

        protected virtual void OnGraphPointerScrolled(PointerEventData eventData)
        {
            if (Mathf.Abs(eventData.scrollDelta.y) > float.Epsilon)
            {
                _currentZoom    *= 1f + eventData.scrollDelta.y / 10f;
	            _currentZoom    = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);
	            _zoomCenterPos  = Utility.GetMousePosition();

                Vector2 beforePointInContent;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_graphContainer, _zoomCenterPos, null, out beforePointInContent);

                Vector2 pivotPosition = new Vector3(_graphContainer.pivot.x * _graphContainer.rect.size.x, _graphContainer.pivot.y * _graphContainer.rect.size.y);
                Vector2 posFromBottomLeft = pivotPosition + beforePointInContent;
                SetPivot(_graphContainer, new Vector2(posFromBottomLeft.x / _graphContainer.rect.width, posFromBottomLeft.y / _graphContainer.rect.height));

                if (Mathf.Abs(_graphContainer.localScale.x - _currentZoom) > 0.001f)
                {
                    _graphContainer.localScale = Vector3.one * _currentZoom;
                }
            }
        }

        protected virtual void OnGraphPointerDragged(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Middle)
            {
                _graphContainer.localPosition += new Vector3(eventData.delta.x, eventData.delta.y);
            }
        }


        //  helper methods
        private void DragNode(Node node, PointerEventData eventData)
        {
            if (!node.CanMove())
            {
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Vector2 pointerPos = ClampToNodeContainer(eventData);
                var success = RectTransformUtility.ScreenPointToLocalPointInRectangle(_nodeContainer, pointerPos,
                                                                                eventData.pressEventCamera, out _localPointerPos);
                if (success)
                {
                    node.SetPosition(_localPointerPos - _pointerOffset);
                    drawer.UpdateDraw();
                }
            }
        }

        private Vector2 ClampToNodeContainer(PointerEventData eventData)
        {
            var rawPointerPos = eventData.position;
            var canvasCorners = new Vector3[4];
            _nodeContainer.GetWorldCorners(canvasCorners);

            var clampedX = Mathf.Clamp(rawPointerPos.x, canvasCorners[0].x, canvasCorners[2].x);
            var clampedY = Mathf.Clamp(rawPointerPos.y, canvasCorners[0].y, canvasCorners[2].y);

            var newPointerPos = new Vector2(clampedX, clampedY);
            return newPointerPos;
        }

        private void HandleSocketRegister(Node node)
        {
            foreach (var i in node.Inputs)
            {
                i.socketId = NewId();
            }

            foreach (var o in node.Outputs)
            {
                o.socketId = NewId();
            }
        }

        private void LoadNode(NodeData data)
        {
            var node = Utility.CreateNodePrefab<Node>(data.path, nodeContainer);
            var pos  = new Vector2(data.posX, data.posY);
            node.Init(_signalSystem, _signalSystem, pos, data.id, data.path);
            node.Setup();
            nodes.Add(node);

            var ser = new Serializer();
            ser.Deserialize(data.values);
            node.OnDeserialize(ser);
        }

        private void LoadConn(ConnectionData data)
        {
            var input = nodes.SelectMany(n => n.Inputs).FirstOrDefault(i => i.socketId == data.inputSocketId);
            var output = nodes.SelectMany(n => n.Outputs).FirstOrDefault(o => o.socketId == data.outputSocketId);

            if (input != null && output != null)
            {
                var connection = new Connection(data.id, input, output);

                input.Connect(connection);
                output.Connect(connection);

                connections.Add(connection);
                input.OwnerNode.Connect(input, output);

                drawer.Add(connection.connId, output.handle, input.handle);
            }
        }

        private void SetPivot(RectTransform rectTransform, Vector2 pivot)
        {
            Vector2 size = rectTransform.rect.size;
            Vector2 deltaPivot = rectTransform.pivot - pivot;
            Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y) * rectTransform.localScale.x;
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }
        private static string NewId() { return Guid.NewGuid().ToString(); }
    }
}