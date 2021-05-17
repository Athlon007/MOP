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

using MOP.FSM;
using MOP.Vehicles.Cases;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaFreezeFix : MonoBehaviour
    {
        Vector3 satsumaLastVelocity; // Last known velocity of Satsuma
        int attempts; // Amount of attempts of resetting the Satsuma velocity.
        int lastAttempt = -1; // Last attempt number.
        const int MaxAttempts = 10; // Maximum amount of attempts that can be applied in a row. If that number gets exceeded, cooldown gets activated
        
        bool startCooldown; // Should cooldown be started?
        float cooldownTimer; // Timer of the cooldown.
        const int CooldownTime = 5; // Cooldown duration.

        ConfigurableJoint joint; // Satsuma Driver joint.
        float lastBreakForce; // Last break force of the player.

        bool impacted; // Has impact just occured

        void Start()
        {
            joint = Satsuma.Instance.transform.Find("DriverHeadPivot").gameObject.GetComponent<ConfigurableJoint>();
        }

        void FixedUpdate()
        {
            if (FsmManager.IsPlayerInSatsuma())
            {
                // Basically checks if the car suddenly stopped.
                // If the difference is too high, restore last known Satsuma velocity.
                // This fix can be only applied MaxAttempts amount of times.
                // This prevents this script for forcing car through stuff like walls.
                if (Satsuma.Instance.rb.velocity.magnitude < 5 && satsumaLastVelocity.magnitude > 10 && attempts < MaxAttempts)
                { 
                    Satsuma.Instance.rb.velocity = satsumaLastVelocity;
                    attempts++;
                    startCooldown = true;

                    // On impact, save last joint.BreakForce,
                    // and set break force to large value,
                    // this should prevent player from dying from driving over bump,
                    // while also making him possible to die from driving into wall.
                    if (!impacted)
                    {
                        impacted = true;
                        lastBreakForce = joint.breakForce;
                        joint.breakForce = 1000;
                        joint.breakTorque = 1000;
                    }
                }

                // Store the last known Satsuma velocity.
                satsumaLastVelocity = Satsuma.Instance.rb.velocity;

                // Do not execute cooldown part,
                // if the startCooldown hasn't been toggled
                // and the lastAttempt is not the same as Attempts.
                // The last check is so the cooldown is not executed,
                // while player is still crashing into things.
                if (!startCooldown && lastAttempt != attempts)
                {
                    lastAttempt = attempts;
                    return;
                }

                if (attempts > 0)
                {
                    cooldownTimer += Time.deltaTime;

                    // If cooldown ran out, reset everything as it was before.
                    if (cooldownTimer > CooldownTime)
                    {
                        attempts = 0;
                        cooldownTimer = 0;
                        joint.breakForce = lastBreakForce;
                        joint.breakTorque = lastBreakForce;
                        impacted = false;
                        startCooldown = false;
                    }
                }
            }
        }

    }
}
