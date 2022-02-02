using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using HarmonyLib;
using BepInEx.Configuration;


namespace CostChoiceNodeFix
{
	[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
	[BepInDependency(APIGUID, BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency(ZGUID, BepInDependency.DependencyFlags.SoftDependency)]

	public partial class Plugin : BaseUnityPlugin
	{
		public const string APIGUID = "cyantist.inscryption.api";
		public const string PluginGuid = "extraVoid.inscryption.CardNodeFix";
		public const string ZGUID = "extraVoid.inscryption.renderPatcher";
		public static bool RenderFixActive = false;
		private const string PluginName = "CardNodeFix";
		private const string PluginVersion = "1.0.0";

		public static string Directory;
		internal static ManualLogSource Log;

		internal static ConfigEntry<bool> configOn;


		private void Awake()
		{
			Log = base.Logger;
			Directory = this.Info.Location.Replace("CardNodeFix.dll", "");

			if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ZGUID))
			{
				RenderFixActive = true;
			}

			configOn = Config.Bind("On/Off", "On/Off", true, "Turn on or off if Energy and Mox cards should show up at cost choice nodes in act 1");


			if (configOn.Value)
			{
				Harmony harmony = new(PluginGuid);
				harmony.PatchAll();
			}
		}
	}
}