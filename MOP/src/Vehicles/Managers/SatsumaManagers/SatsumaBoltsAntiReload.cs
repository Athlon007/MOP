// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

using UnityEngine;
using MSCLoader.Helper;

using MOP.Common;
using MOP.Vehicles.Cases;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    /// <summary>
    /// This scripts looksa for the BoltCheck or Use scripts and disables restting of the playmaker scripts.
    /// </summary>
    class SatsumaBoltsAntiReload : MonoBehaviour
    {
        PlayMakerFSM fsm;

        float breakForce, breakTorque;
        FixedJoint fixedJoint;
        HingeJoint hingeJoint;

        bool glued;

        public SatsumaBoltsAntiReload()
        {
            Satsuma.Instance.AddPart(this);

            try
            {
                string fsmName = gameObject.GetPlayMakerFSM("BoltCheck") ? "BoltCheck" : "Use";
                fsm = gameObject.GetPlayMakerFSM(fsmName);

                if (fsm == null)
                    return;

                fsm.Fsm.RestartOnEnable = false;

                fixedJoint = gameObject.GetComponent<FixedJoint>();
                hingeJoint = gameObject.GetComponent<HingeJoint>();

                if (fixedJoint)
                {
                    breakTorque = fixedJoint.breakTorque;
                    breakForce = fixedJoint.breakForce;
                }
                else if (hingeJoint)
                {
                    breakTorque = hingeJoint.breakTorque;
                    breakForce = hingeJoint.breakForce;
                }

                if (transform.root == Satsuma.Instance.transform)
                {
                    glued = true;
                }
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, $"BOLTS_ANTI_LOAD_SCRIPT_ERROR_{this.gameObject.Path()}");
            }
        }

        void Update()
        {
            if (!glued) return;

            if (fixedJoint)
            {
                GlueFixedJoint();
            }
            else if (hingeJoint)
            {
                GlueHingeJoint();
            }
        }

        void GlueFixedJoint()
        {
            fixedJoint.breakTorque = Mathf.Infinity;
            fixedJoint.breakForce = Mathf.Infinity;
        }

        void GlueHingeJoint()
        {          
            hingeJoint.breakTorque = Mathf.Infinity;
            hingeJoint.breakForce = Mathf.Infinity;
        }

        internal void Unglue()
        {
            glued = false;

            if (hingeJoint)
            {
                hingeJoint.breakForce = breakForce;
                hingeJoint.breakTorque = breakTorque;
            }
            else if (fixedJoint)
            {
                fixedJoint.breakForce = breakForce;
                fixedJoint.breakTorque = breakTorque;
            }

            // This script is no longer needed. Disable it.
            this.enabled = false;
        }
    }
}
