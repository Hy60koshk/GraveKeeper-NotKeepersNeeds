using System.IO;
using Harmony;
using System.Reflection;
using UnityEngine;

namespace NotKeepersNeeds {
	public class MainPatcher {
		public static void Patch() {
			HarmonyInstance harmony = HarmonyInstance.Create("com.koschk.notkeepersneeds.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	public class Config {
		private static Options options_ = null;

		public class Options {
			public float DmgMult = 1;
			public float GlobalDmgMult = 1;
			public float RegenMult = 1;
			public float SprintSpeed = 2;
			public float DefaultSpeed = 1;
			public float EnergyDrainMult = 1;
			public float EnergyReplenMult = 1;
			public float EnergyForSprint = 0;
			public float CraftingSpeed = 1;
			public float InteractionSpeed = 1;
			public float TimeMult = 1;
			public float SleepTimeMult = 1;
			public float OrbsMult = 1;
			public bool _OrbsHasConst = false;
			public bool OrbsConstAddIfZero = false;
			public bool SprintToggle = false;
			public bool RoundDown = false;
			public bool DullInventoryMusic = false;
			public bool UnconditionalSleep = false;
			public float InflationAmount = 1;
			public int[] OrbsConstant = new int[] { 0, 0, 0 };
			public KeyCode SprintKey = KeyCode.LeftShift;
			//public KeyCode ConfigReloadKey = KeyCode.Semicolon;

			public bool HealthRegen = false;
			public bool HealIfTired = false;
			public float HealthRegenPerSecond = 0.5f;

			public bool _SprintToggleOn = false;
			public bool _SprintStillPressed = false;

			public int GetOrbCount(int orig, int idx) {
				if (_OrbsHasConst && (OrbsConstAddIfZero || orig > 0)) {
					orig += OrbsConstant[idx];
				}
				if (OrbsMult == 1) {
					return orig > 0 ? orig : 0;
				}
				if (orig == 0) {
					return 0;
				}
				float tmp = orig * OrbsMult;
				if (tmp < 0) {
					return 0;
				}
				if (RoundDown) {
					return (int)(tmp - (tmp % 1));
				}
				else {
					return (int)(tmp + (1 - tmp % 1));
				}
			}
		}

		public static void Log(string line) {
			File.AppendAllText(@"./QMods/NotKeepersNeeds/log.txt", line);
		}

		private static bool parseBool(string raw) {
			return raw == "1" || raw.ToLower() == "true";
		}
		private static float parseFloat(string raw, float _default) {
			float value = 0;
			if (float.TryParse(raw, out value)) {
				return value;
			}
			return _default;
		}
		private static float parseFloat(string raw, float _default, float threshold) {
			float value = parseFloat(raw, _default);
			if (value > threshold) {
				return value;
			}
			return _default;
		}
		private static float parsePositive(string raw, float _default) {
			return parseFloat(raw, _default, 0);
		}
		private static float parseNonNegative(string raw, float _default) {
			float value = parseFloat(raw, _default);
			return value < 0 ? 0 : value;
		}

		public static Options GetOptions() {
			return GetOptions(false);
		}
		public static Options GetOptions(bool forceReload) {
			if (options_ != null && !forceReload) {
				return options_;
			}
			options_ = new Options();

			string cfgPath = @"./QMods/NotKeepersNeeds/config.txt";
			if (File.Exists(cfgPath)) {
				string[] lines = File.ReadAllLines(cfgPath);
				foreach (string line in lines) {
					if (line.Length < 3 || line[0] == '#') {
						continue;
					}
					string[] pair = line.Split('=');
					if (pair.Length > 1) {
						string key = pair[0];
						string rawVal = pair[1];
						switch (key) {
							case "DmgMult":
								options_.DmgMult = parseFloat(rawVal, options_.DmgMult, 0.04f);
								break;
							case "GlobalDmgMult":
								options_.GlobalDmgMult = parseFloat(rawVal, options_.GlobalDmgMult, 0.04f);
								break;
							case "RegenMult":
								options_.RegenMult = parsePositive(rawVal, options_.RegenMult);
								break;
							case "HealthRegenPerSecond":
								options_.HealthRegenPerSecond = parsePositive(rawVal, options_.HealthRegenPerSecond);
								break;
							case "SprintSpeed":
								options_.SprintSpeed = parsePositive(rawVal, options_.SprintSpeed);
								break;
							case "DefaultSpeed":
								options_.DefaultSpeed = parsePositive(rawVal, options_.DefaultSpeed);
								break;
							case "TimeMult":
								options_.TimeMult = parseFloat(rawVal, options_.TimeMult, 0.0009f);
								break;
							case "SleepTimeMult":
								options_.SleepTimeMult = parseFloat(rawVal, options_.SleepTimeMult, 0.09f);
								break;
							case "EnergyDrainMult":
								options_.EnergyDrainMult = parseNonNegative(rawVal, options_.EnergyDrainMult);
								break;
							case "EnergyReplenMult":
								options_.EnergyReplenMult = parsePositive(rawVal, options_.EnergyReplenMult);
								break;
							case "EnergyForSprint":
								options_.EnergyForSprint = parseNonNegative(rawVal, options_.EnergyForSprint);
								break;
							case "CraftingSpeed":
								options_.CraftingSpeed = parsePositive(rawVal, options_.CraftingSpeed);
								break;
							case "InteractionSpeed":
								options_.InteractionSpeed = parsePositive(rawVal, options_.InteractionSpeed);
								break;
							case "InflationAmount":
								options_.InflationAmount = parseFloat(rawVal, options_.InflationAmount);
								break;
							case "OrbsMult":
								options_.OrbsMult = parseNonNegative(rawVal, options_.OrbsMult);
								break;
							case "SprintKey":
								try {
									KeyCode code = Enum<KeyCode>.Parse(pair[1]);
									options_.SprintKey = code;
								}
								catch { }
								break;
							case "OrbsConstant": {
									string[] ocValues = pair[1].Split(':');
									options_._OrbsHasConst = true;
									int ocVal = 0;
									for (int i = 0; (i < ocValues.Length) && (i < options_.OrbsConstant.Length); i++) {
										if (int.TryParse(ocValues[i], out ocVal)) {
											options_.OrbsConstant[i] = ocVal;
										}
									}
								}
								break;
							case "SprintToggle":
								options_.SprintToggle = parseBool(rawVal);
								break;
							case "RoundDown":
								options_.RoundDown = parseBool(rawVal);
								break;
							case "OrbsConstAddIfZero":
								options_.OrbsConstAddIfZero = parseBool(rawVal);
								break;
							case "DullInventoryMusic":
								options_.DullInventoryMusic = parseBool(rawVal);
								break;
							case "HealthRegen":
								options_.HealthRegen = parseBool(rawVal);
								break;
							case "HealIfTired":
								options_.HealIfTired = parseBool(rawVal);
								break;
							case "UnconditionalSleep":
								options_.UnconditionalSleep = parseBool(rawVal);
								break;
						}					
					}
				}
				if (options_.EnergyDrainMult == 0) {
					options_.UnconditionalSleep = true;
				}
			}
			return options_;
		}
	}
}