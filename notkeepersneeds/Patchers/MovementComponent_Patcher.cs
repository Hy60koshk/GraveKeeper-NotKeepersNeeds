using System;
using Harmony;
using UnityEngine;

namespace NotKeepersNeeds {
	[HarmonyPatch(typeof(MovementComponent))]
	[HarmonyPatch("UpdateMovement")]
	internal class MovementComponent_UpdateMovement_Patch {
		[HarmonyPrefix]
		public static bool Prefix(MovementComponent __instance, Vector2 dir, ref float delta_time) {
			if (!__instance.wgo.is_player || __instance.player_controlled_by_script || __instance.wgo.is_dead) {
				return true;
			}
			Config.Options opts = Config.GetOptions();
			bool isSprintPressed = Input.GetKey(opts.SprintKey);
			if (opts.SprintToggle) {
				if (isSprintPressed && !opts._SprintStillPressed) {
					isSprintPressed = opts._SprintToggleOn = !opts._SprintToggleOn;
					opts._SprintStillPressed = true;
				}
				else {
					if (!isSprintPressed) {
						opts._SprintStillPressed = false;
					}
					isSprintPressed = opts._SprintToggleOn;
				}
			}

			float speed = __instance.wgo.data.GetParam("speed", 0.0f);
			if (speed > 0) {
				speed = 3.3f + __instance.wgo.data.GetParam("speed_buff", 0.0f);
				float energydt = delta_time * opts.EnergyForSprint;
				if (isSprintPressed && (MainGame.me.player.energy >= energydt)) {
					__instance.SetSpeed(speed * opts.SprintSpeed);
					if (energydt > 0) {
						MainGame.me.player.energy -= energydt;
					}
				}
				else {
					__instance.SetSpeed(speed * opts.DefaultSpeed);
				}
			}
			return true;
		}
	}
}