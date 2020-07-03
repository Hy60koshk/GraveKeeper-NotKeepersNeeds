using System;
using System.IO;
using Harmony;
using UnityEngine;

namespace NotKeepersNeeds {
	[HarmonyPatch(typeof(EnvironmentEngine))]
	[HarmonyPatch("Update")]
	class EnvironmentEngine_Update_Patch {
		[HarmonyPrefix]
		static bool Prefix(EnvironmentEngine __instance) {
			if (MainGame.game_starting || MainGame.paused || !MainGame.game_started || __instance.IsTimeStopped()) {
				return true;
			}
			Config.Options opts = Config.GetOptions();
			float mult = Time.timeScale == 10f ? opts.SleepTimeMult : opts.TimeMult;
			if (mult != 1) {
				float accountableDelta = Time.deltaTime / 225f; // since this._cur_time += deltaTime / 225f
				float adjDelta = accountableDelta * mult - accountableDelta;
				EnvironmentEngine.SetTime(__instance.time_of_day.time_of_day + adjDelta);
			}
			/*if (true) { // opts.AllowSaveEverywhere
				if (Input.GetKey(KeyCode.F5)) {
					PlatformSpecific.SaveGame(MainGame.me.save_slot, MainGame.me.save, (PlatformSpecific.OnSaveCompleteDelegate)(slot => {
						EffectBubblesManager.ShowImmediately(MainGame.me.player.pos3, "Saved succesfully");
					}));
				}
			}*/
			return true;
		}
	}
}