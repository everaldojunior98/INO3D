using System;
using Assets.Scripts.Components.Base;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class Resistor : InoComponent
    {
        #region Overrides

        protected override void SetupPorts()
        {
            DefaultHeight = 0.18f;
            Pins.Add(Tuple.Create("A", new Vector3(0,0, 0.0808f)));
            Pins.Add(Tuple.Create("B", new Vector3(0, 0, -0.0779f)));
        }

        public override void Delete()
        {
            DisconnectAllPorts();
            Destroy(gameObject);
        }

        #endregion
    }
}