using System;
using System.Collections.Generic;
using System.Threading;
using CircuitSharp.Components;
using CircuitSharp.Components.Base;

namespace CircuitSharp.Core
{
    public class Circuit
    {
        #region Properties

        public bool Converged;

        #endregion

        #region Fields

        private double time;
        private double timeStep;
        //Microseconds
        private double tickTimeInterval;

        private readonly List<ICircuitElement> elements;
        private readonly List<List<long>> nodeMesh;

        private bool needAnalyze;

        private readonly List<long> nodeList;
        private ICircuitElement[] voltageSources;

        private double[][] circuitMatrix;
        private double[] circuitRightSide;
        private double[][] origMatrix;
        private double[] origRightSide;
        private RowInfo[] circuitRowInfo;
        private int[] circuitPermute;

        private int circuitMatrixSize;
        private int circuitMatrixFullSize;

        private bool circuitNonLinear;
        private bool circuitNeedsMap;

        private readonly IdGenerator idGenerator;

        private Thread simulationThread;
        private Action afterSimulationTick;
        private readonly Action<Error> onError;

        private bool isSimulating;

        #endregion

        #region Constructor

        public Circuit(Action<Error> onError)
        {
            SetTimeStep(5E-6);
            needAnalyze = true;
            isSimulating = false;
            elements = new List<ICircuitElement>();
            nodeMesh = new List<List<long>>();
            nodeList = new List<long>();
            idGenerator = new IdGenerator();
            this.onError = onError;
        }

        #endregion

        #region Public Methods

        #region Simulation Thread

        public void StartSimulation(Action afterSimulationTickAction)
        {
            if (simulationThread != null)
            {
                simulationThread.Abort();
                simulationThread.Join();
            }

            simulationThread = new Thread(SimulationLoop);
            afterSimulationTick = afterSimulationTickAction;
            simulationThread.Start();
        }

        public void StopSimulation()
        {
            if (isSimulating && simulationThread != null)
            {
                isSimulating = false;
                needAnalyze = true;
                simulationThread.Abort();
                simulationThread.Join();
                simulationThread = null;
            }
        }

        #endregion

        #region Get/Set Methods

        public double GetTickTimeInterval()
        {
            return tickTimeInterval;
        }

        public int GetNodesCount()
        {
            return nodeList.Count;
        }

        public int GetElementsCount()
        {
            return elements.Count;
        }

        public void ResetTime()
        {
            time = 0;
        }

        public double GetTime()
        {
            return time;
        }

        public double GetTimeStep()
        {
            return timeStep;
        }

        public void SetTimeStep(double newTimeStep)
        {
            timeStep = newTimeStep;
            tickTimeInterval = newTimeStep * 1E+6;

            needAnalyze = true;
        }

        public void NeedAnalyze()
        {
            needAnalyze = true;
        }

        #endregion

        #region Circuit Methods

        public T Create<T>(params object[] args) where T : class, ICircuitElement
        {
            T element = Activator.CreateInstance(typeof(T), args) as T;
            element.Circuit = this;
            AddElement(element);
            return element;
        }

        public void Connect(Lead left, Lead right)
        {
            var leftLeadIndex = left.Index;
            var rightLeadIndex = right.Index;

            var leftIndex = elements.IndexOf(left.Element);
            var rightIndex = elements.IndexOf(right.Element);

            var leftLeads = nodeMesh[leftIndex];
            var rightLeads = nodeMesh[rightIndex];

            var leftConnection = leftLeads[leftLeadIndex];
            var rightConnection = rightLeads[rightLeadIndex];

            // If both leads are unconnected, we need a new node
            var empty = leftConnection == -1 && rightConnection == -1;
            if (empty)
            {
                var id = idGenerator.NextId();
                leftLeads[leftLeadIndex] = id;
                rightLeads[rightLeadIndex] = id;
                return;
            }

            // If the left lead is unconnected, attach to right
            if (leftConnection == -1)
                leftLeads[leftLeadIndex] = rightLeads[rightLeadIndex];

            // If the right lead is unconnected, attach to left node
            // If the right lead is _connected_, replace with left node
            rightLeads[rightLeadIndex] = leftLeads[leftLeadIndex];
            needAnalyze = true;
        }

        public void UpdateVoltageSource(int n1, int n2, int vs, double v)
        {
            var vn = nodeList.Count + vs;
            StampRightSide(vn, v);
        }

        public void StampCurrentSource(int n1, int n2, double i)
        {
            StampRightSide(n1, -i);
            StampRightSide(n2, i);
        }

        // stamp independent voltage source #vs, from n1 to n2, amount v
        public void StampVoltageSource(int n1, int n2, int vs, double v)
        {
            var vn = nodeList.Count + vs;
            StampMatrix(vn, n1, -1);
            StampMatrix(vn, n2, 1);
            StampRightSide(vn, v);
            StampMatrix(n1, vn, 1);
            StampMatrix(n2, vn, -1);
        }

        // use this if the amount of voltage is going to be updated in doStep()
        public void StampVoltageSource(int n1, int n2, int vs)
        {
            var vn = nodeList.Count + vs;
            StampMatrix(vn, n1, -1);
            StampMatrix(vn, n2, 1);
            StampRightSide(vn);
            StampMatrix(n1, vn, 1);
            StampMatrix(n2, vn, -1);
        }

        public void StampResistor(int n1, int n2, double r)
        {
            double r0 = 1 / r;
            StampMatrix(n1, n1, r0);
            StampMatrix(n2, n2, r0);
            StampMatrix(n1, n2, -r0);
            StampMatrix(n2, n1, -r0);
        }

        public void StampConductance(int n1, int n2, double r0)
        {
            StampMatrix(n1, n1, r0);
            StampMatrix(n2, n2, r0);
            StampMatrix(n1, n2, -r0);
            StampMatrix(n2, n1, -r0);
        }

        // Voltage-controlled voltage source.
        // Control voltage source vs with voltage from n1 to n2 
        // (must also call StampVoltageSource())
        public void StampVCVS(int n1, int n2, double coef, int vs)
        {
            var vn = nodeList.Count + vs;
            StampMatrix(vn, n1, coef);
            StampMatrix(vn, n2, -coef);
        }

        // Voltage-controlled current source.
        // Current from cn1 to cn2 is equal to voltage from vn1 to vn2, divided by g 
        public void StampVCCS(int cn1, int cn2, int vn1, int vn2, double g)
        {
            StampMatrix(cn1, vn1, g);
            StampMatrix(cn2, vn2, g);
            StampMatrix(cn1, vn2, -g);
            StampMatrix(cn2, vn1, -g);
        }

        // Current-controlled current source.
        // Stamp a current source from n1 to n2 depending on current through vs 
        public void StampCCCS(int n1, int n2, int vs, double gain)
        {
            var vn = nodeList.Count + vs;
            StampMatrix(n1, vn, gain);
            StampMatrix(n2, vn, -gain);
        }

        // indicate that the value on the right side of row i changes in doStep()
        public void StampRightSide(int i)
        {
            if (i > 0)
                circuitRowInfo[i - 1].RightSideChanges = true;
        }

        // indicate that the values on the left side of row i change in doStep()
        public void StampNonLinear(int i)
        {
            if (i > 0)
                circuitRowInfo[i - 1].LeftSideChanges = true;
        }

        public ICircuitElement GetElement(int index)
        {
            return index < elements.Count ? elements[index] : null;
        }

        #endregion

        #endregion

        #region Private Methods

        #region Simulation Thread

        private void SimulationLoop()
        {
            isSimulating = true;

            if (elements.Count == 0)
            {
                StopSimulation();
                return;
            }

            if (needAnalyze)
                Analyze();

            if (needAnalyze)
            {
                StopSimulation();
                return;
            }

            while (isSimulating)
            {
                try
                {
                    if (needAnalyze)
                        Analyze();

                    Tick();
                    afterSimulationTick();
                }
                catch (CircuitException exception)
                {
                    onError(exception.Error);
                    StopSimulation();
                    break;
                }
            }
        }

        #endregion

        #region Circuit Methods

        private void AddElement(ICircuitElement element)
        {
            if (!elements.Contains(element))
            {
                elements.Add(element);

                var mesh = new List<long>();
                nodeMesh.Add(mesh);
                for (var x = 0; x < element.GetLeadCount(); x++)
                    mesh.Add(-1);

                var elementsCount = elements.Count - 1;
                var meshCount = nodeMesh.Count - 1;
                if (elementsCount != meshCount)
                    throw new CircuitException(new Error(Error.ErrorCode.E8, element));
            }
        }

        private long GetNodeId(int index)
        {
            return index < nodeList.Count ? nodeList[index] : 0;
        }

        private void Tick()
        {
            // Execute beginStep() on all elements
            for (var i = 0; i != elements.Count; i++)
                elements[i].BeginStep(this);

            int subIteration;
            const int subIterationCount = 5000;
            for (subIteration = 0; subIteration != subIterationCount; subIteration++)
            {
                Converged = true;

                // Copy origRightSide to circuitRightSide
                for (var i = 0; i != circuitMatrixSize; i++)
                    circuitRightSide[i] = origRightSide[i];

                // If the circuit is non linear, copy
                // origMatrix to circuitMatrix
                if (circuitNonLinear)
                    for (var i = 0; i != circuitMatrixSize; i++)
                    for (var j = 0; j != circuitMatrixSize; j++)
                        circuitMatrix[i][j] = origMatrix[i][j];

                // Execute step() on all elements
                for (var i = 0; i != elements.Count; i++)
                    elements[i].Step(this);

                // Can't have any values in the matrix be NaN or Inf
                for (var j = 0; j != circuitMatrixSize; j++)
                for (var i = 0; i != circuitMatrixSize; i++)
                {
                    var x = circuitMatrix[i][j];
                    if (double.IsNaN(x) || double.IsInfinity(x))
                        LogError(new Error(Error.ErrorCode.E1, null));
                }

                // If the circuit is non-Linear, factor it now,
                // if it's linear, it was factored in Analyze()
                if (circuitNonLinear)
                {
                    // Break if the circuit has converged.
                    if (Converged && subIteration > 0)
                        break;

                    if (!LuFactor(circuitMatrix, circuitMatrixSize, circuitPermute))
                        LogError(new Error(Error.ErrorCode.E2, null));
                }

                // Solve the factorized matrix
                LuSolve(circuitMatrix, circuitMatrixSize, circuitPermute, circuitRightSide);

                for (var j = 0; j != circuitMatrixFullSize; j++)
                {
                    var rowInfo = circuitRowInfo[j];
                    var res = rowInfo.Type == RowInfo.RowType.RowConst ? rowInfo.Value : circuitRightSide[rowInfo.MapCol];

                    // If any result is NaN, break
                    if (double.IsNaN(res))
                    {
                        Converged = false;
                        break;
                    }

                    if (j < nodeList.Count - 1)
                    {
                        // For each node in the mesh
                        for (var k = 0; k != nodeMesh.Count; k++)
                        {
                            // Get the leads connected to the node
                            var index = nodeMesh[k].IndexOf(GetNodeId(j + 1));
                            if (index != -1)
                                elements[k].SetLeadVoltage(index, res);
                        }
                    }
                    else
                    {
                        var ji = j - (nodeList.Count - 1);
                        voltageSources[ji].SetCurrent(ji, res);
                    }
                }

                // if the matrix is linear, we don't
                // need to do any more iterations
                if (!circuitNonLinear)
                    break;
            }

            if (subIteration == subIterationCount)
                LogError(new Error(Error.ErrorCode.E3, null));

            // Round to 12 digits
            time = Math.Round(time + timeStep, 12);
        }

        private void Analyze()
        {
            if (elements.Count == 0)
                return;

            nodeList.Clear();
            var internalList = new List<bool>();
            Action<long, bool> pushNode = (id, isInternal) =>
            {
                if (!nodeList.Contains(id))
                {
                    nodeList.Add(id);
                    internalList.Add(isInternal);
                }
            };

            // Search the circuit for a Ground, or Voltage source
            ICircuitElement voltageElm = null;
            var gotGround = false;
            var gotRail = false;
            for (var i = 0; i != elements.Count; i++)
            {
                var element = elements[i];
                if (element is Ground)
                {
                    gotGround = true;
                    break;
                }

                if (element is VoltageInput)
                    gotRail = true;

                if (element is Voltage && voltageElm == null)
                    voltageElm = element;

            }

            // If no ground and no rails, then the voltage elm's first terminal is ground.
            if (!gotGround && !gotRail && voltageElm != null)
            {
                var elementIndex = elements.IndexOf(voltageElm);
                var nodes = nodeMesh[elementIndex];
                pushNode(nodes[0], false);
            }
            else
            {
                // If the circuit contains a ground, rail, or voltage
                // element, push a temporary node to the node list.
                pushNode(idGenerator.NextId(), false);
            }

            // At this point, there is 1 node in the list, the special global ground node.
            var voltageSourceCount = 0; // Number of voltage sources
            for (var i = 0; i != elements.Count; i++)
            {
                var element = elements[i];
                var leads = element.GetLeadCount();

                // For each lead in the element
                for (var leadX = 0; leadX != leads; leadX++)
                {
                    // Id of the node leadX is connected too
                    var leadNode = nodeMesh[i][leadX];
                    // Index of the leadNode in the nodeList
                    var nodeIndex = nodeList.IndexOf(leadNode);
                    if (nodeIndex == -1)
                    {
                        // If the nodeList doesn't contain the node, push it
                        // onto the list and assign it's new index to the lead.
                        element.SetLeadNode(leadX, nodeList.Count);
                        pushNode(leadNode, false);
                    }
                    else
                    {
                        // Otherwise, assign the lead the index of 
                        // the node in the nodeList.
                        element.SetLeadNode(leadX, nodeIndex);

                        if (leadNode == 0)
                        {
                            // if it's the ground node, make sure the 
                            // node voltage is 0, cause it may not get set later
                            element.SetLeadVoltage(leadX, 0);
                        }
                    }
                }

                // Push an internal node onto the list for
                // each internal lead on the element.
                var internalLeads = element.GetInternalLeadCount();
                for (var x = 0; x != internalLeads; x++)
                {
                    element.SetLeadNode(leads + x, nodeList.Count);
                    pushNode(idGenerator.NextId(), true);
                }

                voltageSourceCount += element.GetVoltageSourceCount();
            }

            // Create the voltageSources array.
            // Also determine if circuit is nonlinear.

            voltageSources = new ICircuitElement[voltageSourceCount];
            voltageSourceCount = 0;

            circuitNonLinear = false;
            foreach (var element in elements)
            {
                if (element.NonLinear())
                    circuitNonLinear = true;

                // Assign each voltage source in the element a globally unique id,
                // (the index of the next open slot in voltageSources)
                for (var leadX = 0; leadX != element.GetVoltageSourceCount(); leadX++)
                {
                    voltageSources[voltageSourceCount] = element;
                    element.SetVoltageSource(leadX, voltageSourceCount++);
                }
            }

            var matrixSize = nodeList.Count - 1 + voltageSourceCount;

            // setup circuitMatrix
            circuitMatrix = new double[matrixSize][];
            for (var z = 0; z < matrixSize; z++)
                circuitMatrix[z] = new double[matrixSize];

            circuitRightSide = new double[matrixSize];

            // setup origMatrix
            origMatrix = new double[matrixSize][];
            for (var z = 0; z < matrixSize; z++)
                origMatrix[z] = new double[matrixSize];

            origRightSide = new double[matrixSize];

            // setup circuitRowInfo
            circuitRowInfo = new RowInfo[matrixSize];
            for (var i = 0; i != matrixSize; i++)
                circuitRowInfo[i] = new RowInfo();

            circuitPermute = new int[matrixSize];
            circuitMatrixSize = circuitMatrixFullSize = matrixSize;
            circuitNeedsMap = false;

            // Stamp linear circuit elements.
            for (var i = 0; i != elements.Count; i++)
                elements[i].Stamp(this);

            var closure = new bool[nodeList.Count];
            var changed = true;
            closure[0] = true;
            while (changed)
            {
                changed = false;
                for (var i = 0; i != elements.Count; i++)
                {
                    var element = elements[i];
                    // loop through all ce's nodes to see if they are connected
                    // to other nodes not in closure
                    for (var leadX = 0; leadX < element.GetLeadCount(); leadX++)
                    {
                        if (!closure[element.GetLeadNode(leadX)])
                        {
                            if (element.LeadIsGround(leadX))
                                closure[element.GetLeadNode(leadX)] = changed = true;
                            continue;
                        }

                        for (var k = 0; k != element.GetLeadCount(); k++)
                        {
                            if (leadX == k)
                                continue;
                            var leadNode = element.GetLeadNode(k);
                            if (element.LeadsAreConnected(leadX, k) && !closure[leadNode])
                            {
                                closure[leadNode] = true;
                                changed = true;
                            }
                        }
                    }
                }

                if (changed)
                    continue;

                // connect unconnected nodes
                for (var i = 0; i != nodeList.Count; i++)
                {
                    if (!closure[i] && !internalList[i])
                    {
                        StampResistor(0, i, 1E8);
                        closure[i] = true;
                        changed = true;
                        break;
                    }
                }
            }

            for (var i = 0; i != elements.Count; i++)
            {
                var element = elements[i];

                // look for inductors with no current path
                if (element is Inductor)
                {
                    var findPathInfo = new FindPathInfo(this, FindPathInfo.PathType.Induct, element,
                        element.GetLeadNode(1));
                    // first try FindPath with maximum depth of 5, to avoid slowdowns
                    if (!findPathInfo.FindPath(element.GetLeadNode(0), 5) &&
                        !findPathInfo.FindPath(element.GetLeadNode(0)))
                        element.Reset();
                }

                // look for current sources with no current path
                if (element is CurrentSource)
                {
                    var findPathInfo = new FindPathInfo(this, FindPathInfo.PathType.Induct, element,
                        element.GetLeadNode(1));
                    if (!findPathInfo.FindPath(element.GetLeadNode(0)))
                        LogError(new Error(Error.ErrorCode.E4, element));
                }

                // look for voltage source loops
                if (element is Voltage && element.GetLeadCount() == 2 || element is Wire)
                {
                    var findPathInfo = new FindPathInfo(this, FindPathInfo.PathType.Voltage, element,
                        element.GetLeadNode(1));
                    if (findPathInfo.FindPath(element.GetLeadNode(0)))
                        LogError(new Error(Error.ErrorCode.E5, element));
                }

                // look for shorted caps, or caps w/ voltage but no R
                if (element is Capacitor)
                {
                    var findPathInfo = new FindPathInfo(this, FindPathInfo.PathType.Short, element,
                        element.GetLeadNode(1));
                    if (findPathInfo.FindPath(element.GetLeadNode(0)))
                    {
                        element.Reset();
                    }
                    else
                    {
                        findPathInfo = new FindPathInfo(this, FindPathInfo.PathType.CapV, element,
                            element.GetLeadNode(1));
                        if (findPathInfo.FindPath(element.GetLeadNode(0)))
                            LogError(new Error(Error.ErrorCode.E6, element));
                    }
                }
            }

            for (var i = 0; i != matrixSize; i++)
            {
                int qm = -1, qp = -1;
                double qv = 0;
                var rowInfo = circuitRowInfo[i];

                if (rowInfo.LeftSideChanges || rowInfo.DropRow || rowInfo.RightSideChanges)
                    continue;

                double rsAdd = 0;

                // look for rows that can be removed
                var leadX = 0;
                for (; leadX != matrixSize; leadX++)
                {
                    var q = circuitMatrix[i][leadX];
                    if (circuitRowInfo[leadX].Type == RowInfo.RowType.RowConst)
                    {
                        // keep a running total of const values that have been removed already
                        rsAdd -= circuitRowInfo[leadX].Value * q;
                        continue;
                    }

                    if (q == 0)
                        continue;

                    if (qp == -1)
                    {
                        qp = leadX;
                        qv = q;
                        continue;
                    }

                    if (qm == -1 && q == -qv)
                    {
                        qm = leadX;
                        continue;
                    }

                    break;
                }

                if (leadX == matrixSize)
                {
                    if (qp == -1)
                        LogError(new Error(Error.ErrorCode.E7, null));

                    var info = circuitRowInfo[qp];
                    if (qm == -1)
                    {
                        // we found a row with only one nonzero entry;
                        // that value is a constant
                        for (var k = 0; info.Type == RowInfo.RowType.RowEqual && k < 100; k++)
                        {
                            // follow the chain
                            qp = info.NodeEq;
                            info = circuitRowInfo[qp];
                        }

                        if (info.Type == RowInfo.RowType.RowEqual)
                        {
                            // break equal chains
                            info.Type = RowInfo.RowType.RowNormal;
                            continue;
                        }

                        if (info.Type != RowInfo.RowType.RowNormal)
                            continue;

                        info.Type = RowInfo.RowType.RowConst;
                        info.Value = (circuitRightSide[i] + rsAdd) / qv;
                        circuitRowInfo[i].DropRow = true;
                        // start over from scratch
                        i = -1;
                    }
                    else if (circuitRightSide[i] + rsAdd == 0)
                    {
                        // we found a row with only two nonzero entries, and one
                        // is the negative of the other; the values are equal
                        if (info.Type != RowInfo.RowType.RowNormal)
                        {
                            (qm, qp) = (qp, qm);
                            info = circuitRowInfo[qp];
                            if (info.Type != RowInfo.RowType.RowNormal)
                            {
                                // we should follow the chain here, but this hardly
                                // ever happens so it's not worth worrying about
                                //System.out.println("swap failed");
                                continue;
                            }
                        }

                        info.Type = RowInfo.RowType.RowEqual;
                        info.NodeEq = qm;
                        circuitRowInfo[i].DropRow = true;
                    }
                }
            }

            var nn = 0;
            for (var i = 0; i != matrixSize; i++)
            {
                var rowInfo = circuitRowInfo[i];
                if (rowInfo.Type == RowInfo.RowType.RowNormal)
                {
                    rowInfo.MapCol = nn++;
                    continue;
                }

                if (rowInfo.Type == RowInfo.RowType.RowEqual)
                {
                    // resolve chains of equality; 100 max steps to avoid loops
                    for (var leadX = 0; leadX != 100; leadX++)
                    {
                        var e2 = circuitRowInfo[rowInfo.NodeEq];
                        if (e2.Type != RowInfo.RowType.RowEqual)
                            break;

                        if (i == e2.NodeEq)
                            break;

                        rowInfo.NodeEq = e2.NodeEq;
                    }
                }

                if (rowInfo.Type == RowInfo.RowType.RowConst)
                    rowInfo.MapCol = -1;
            }

            for (var i = 0; i != matrixSize; i++)
            {
                var rowInfo = circuitRowInfo[i];
                if (rowInfo.Type == RowInfo.RowType.RowEqual)
                {
                    var e2 = circuitRowInfo[rowInfo.NodeEq];
                    if (e2.Type == RowInfo.RowType.RowConst)
                    {
                        // if something is equal to a const, it's a const
                        rowInfo.Type = e2.Type;
                        rowInfo.Value = e2.Value;
                        rowInfo.MapCol = -1;
                    }
                    else
                    {
                        rowInfo.MapCol = e2.MapCol;
                    }
                }
            }

            var newSize = nn;
            var newMatrix = new double[newSize][];
            for (var z = 0; z < newSize; z++)
                newMatrix[z] = new double[newSize];

            var rightSide = new double[newSize];
            var ii = 0;
            for (var i = 0; i != matrixSize; i++)
            {
                var rri = circuitRowInfo[i];
                if (rri.DropRow)
                {
                    rri.MapRow = -1;
                    continue;
                }

                rightSide[ii] = circuitRightSide[i];
                rri.MapRow = ii;

                for (var leadX = 0; leadX != matrixSize; leadX++)
                {
                    var ri = circuitRowInfo[leadX];
                    if (ri.Type == RowInfo.RowType.RowConst)
                    {
                        rightSide[ii] -= ri.Value * circuitMatrix[i][leadX];
                    }
                    else
                    {
                        newMatrix[ii][ri.MapCol] += circuitMatrix[i][leadX];
                    }
                }

                ii++;
            }

            circuitMatrix = newMatrix;
            circuitRightSide = rightSide;
            matrixSize = circuitMatrixSize = newSize;

            // copy rightSide to origRightSide
            for (var i = 0; i != matrixSize; i++)
                origRightSide[i] = circuitRightSide[i];

            // copy matrix to origMatrix
            for (var i = 0; i != matrixSize; i++)
            for (var leadX = 0; leadX != matrixSize; leadX++)
                origMatrix[i][leadX] = circuitMatrix[i][leadX];

            circuitNeedsMap = true;
            needAnalyze = false;

            // If the matrix is linear, we can do the LuFactor 
            // here instead of needing to do it every frame.
            if (!circuitNonLinear)
                if (!LuFactor(circuitMatrix, circuitMatrixSize, circuitPermute))
                    LogError(new Error(Error.ErrorCode.E2, null));
        }

        private void LogError(Error error)
        {
            circuitMatrix = null;
            needAnalyze = true;
            throw new CircuitException(error);
        }

        // stamp value x in row i, column j, meaning that a voltage change
        // of dv in node j will increase the current into node i by x dv
        // (Unless i or j is a voltage source node.)
        private void StampMatrix(int i, int j, double x)
        {
            if (i > 0 && j > 0)
            {
                if (circuitNeedsMap)
                {
                    i = circuitRowInfo[i - 1].MapRow;
                    var ri = circuitRowInfo[j - 1];
                    if (ri.Type == RowInfo.RowType.RowConst)
                    {
                        circuitRightSide[i] -= x * ri.Value;
                        return;
                    }

                    j = ri.MapCol;
                }
                else
                {
                    i--;
                    j--;
                }

                circuitMatrix[i][j] += x;
            }
        }

        // stamp value x on the right side of row i, representing an
        // independent current source flowing into node i
        private void StampRightSide(int i, double x)
        {
            if (i > 0)
            {
                i = (circuitNeedsMap) ? circuitRowInfo[i - 1].MapRow : i - 1;
                circuitRightSide[i] += x;
            }
        }

        // Factors a matrix into upper and lower triangular matrices by
        // gaussian elimination. On entry, a[0..n-1][0..n-1] is the
        // matrix to be factored. ipvt[] returns an integer vector of pivot
        // indices, used in the LuSolve() routine.
        // http://en.wikipedia.org/wiki/Crout_matrix_decomposition
        private bool LuFactor(double[][] a, int n, int[] ipvt)
        {
            int i, j;
            var scaleFactors = new double[n];

            // divide each row by its largest element, keeping track of the
            // scaling factors
            for (i = 0; i != n; i++)
            {
                double largest = 0;
                for (j = 0; j != n; j++)
                {
                    var x = Math.Abs(a[i][j]);
                    if (x > largest)
                        largest = x;
                }

                // if all zeros, it's a singular matrix
                if (largest == 0)
                    return false;
                scaleFactors[i] = 1.0 / largest;
            }

            // use Crout's method; loop through the columns
            for (j = 0; j != n; j++)
            {
                // calculate upper triangular elements for this column
                int k;
                for (i = 0; i != j; i++)
                {
                    var q = a[i][j];
                    for (k = 0; k != i; k++)
                        q -= a[i][k] * a[k][j];
                    a[i][j] = q;
                }

                // calculate lower triangular elements for this column
                double largest = 0;
                var largestRow = -1;
                for (i = j; i != n; i++)
                {
                    var q = a[i][j];
                    for (k = 0; k != j; k++)
                        q -= a[i][k] * a[k][j];
                    a[i][j] = q;
                    var x = Math.Abs(q);
                    if (x >= largest)
                    {
                        largest = x;
                        largestRow = i;
                    }
                }

                // pivoting
                if (j != largestRow)
                {
                    for (k = 0; k != n; k++)
                        (a[largestRow][k], a[j][k]) = (a[j][k], a[largestRow][k]);
                    scaleFactors[largestRow] = scaleFactors[j];
                }

                // keep track of row interchanges
                ipvt[j] = largestRow;

                // avoid zeros
                if (a[j][j] == 0.0)
                    a[j][j] = 1e-18;

                if (j != n - 1)
                {
                    var mult = 1.0 / a[j][j];
                    for (i = j + 1; i != n; i++)
                        a[i][j] *= mult;
                }
            }

            return true;
        }

        // Solves the set of n linear equations using a LU factorization
        // previously performed by LuFactor. On input, b[0..n-1] is the right
        // hand side of the equations, and on output, contains the solution.
        private void LuSolve(double[][] a, int n, int[] ipvt, double[] b)
        {
            // find first nonzero b element
            int i;
            for (i = 0; i != n; i++)
            {
                var row = ipvt[i];
                var swap = b[row];
                b[row] = b[i];
                b[i] = swap;
                if (swap != 0)
                    break;
            }

            var bi = i++;
            for (; i < n; i++)
            {
                var row = ipvt[i];
                var tot = b[row];

                b[row] = b[i];
                // forward substitution using the lower triangular matrix
                for (var j = bi; j < i; j++)
                    tot -= a[i][j] * b[j];

                b[i] = tot;
            }

            for (i = n - 1; i >= 0; i--)
            {
                var tot = b[i];

                // back-substitution using the upper triangular matrix
                for (var j = i + 1; j != n; j++)
                    tot -= a[i][j] * b[j];

                b[i] = tot / a[i][i];
            }
        }

        #endregion

        #endregion
    }
}