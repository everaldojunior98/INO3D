using System;
using CircuitSharp.Components;
using CircuitSharp.Components.Base;

namespace CircuitSharp.Core
{
    public class FindPathInfo
    {
        #region Fields

        public enum PathType
        {
            Induct,
            Voltage,
            Short,
            CapV,
        }

        private readonly Circuit circuit;
        private readonly bool[] used;
        private readonly int destiny;
        private readonly ICircuitElement firstElement;
        private readonly PathType type;

        #endregion

        #region Constructor

        public FindPathInfo(Circuit circuit, PathType type, ICircuitElement element, int destiny)
        {
            this.circuit = circuit;
            this.destiny = destiny;
            this.type = type;
            firstElement = element;
            used = new bool[this.circuit.GetNodesCount()];
        }

        #endregion

        #region Public Methods

        public bool FindPath(int n1, int depth = -1)
        {
            if (n1 == destiny)
                return true;

            if (depth-- == 0)
                return false;

            if (used[n1])
                return false;

            used[n1] = true;
            for (var i = 0; i != circuit.GetElementsCount(); i++)
            {
                var element = circuit.GetElement(i);
                if (element == firstElement)
                    continue;

                if (type == PathType.Induct)
                    if (element is CurrentSource)
                        continue;

                if (type == PathType.Voltage)
                    if (!(element.IsWire() || element is Voltage))
                        continue;

                if (type == PathType.Short && !element.IsWire())
                    continue;

                if (type == PathType.CapV)
                    if (!(element.IsWire() || element is Capacitor || element is Voltage))
                        continue;

                if (n1 == 0)
                {
                    // look for posts which have a ground connection;
                    // our path can go through ground
                    for (var z = 0; z != element.GetLeadCount(); z++)
                    {
                        if (element.LeadIsGround(z) && FindPath(element.GetLeadNode(z), depth))
                        {
                            used[n1] = false;
                            return true;
                        }
                    }
                }

                int j;
                for (j = 0; j != element.GetLeadCount(); j++)
                    if (element.GetLeadNode(j) == n1)
                        break;

                if (j == element.GetLeadCount())
                    continue;

                if (element.LeadIsGround(j) && FindPath(0, depth))
                {
                    used[n1] = false;
                    return true;
                }

                if (type == PathType.Induct && element is Inductor)
                {
                    var c = element.GetCurrent();
                    if (j == 0)
                        c = -c;

                    if (Math.Abs(c - firstElement.GetCurrent()) > 1e-10)
                        continue;
                }

                for (var k = 0; k != element.GetLeadCount(); k++)
                {
                    if (j == k)
                        continue;

                    if (element.LeadsAreConnected(j, k) && FindPath(element.GetLeadNode(k), depth))
                    {
                        used[n1] = false;
                        return true;
                    }
                }
            }

            used[n1] = false;
            return false;
        }

        #endregion
    }
}