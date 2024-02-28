using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using System.IO;
using System.Reflection;
using UnityEngine;
using VapeMod.Behaviors;



namespace VapeMod
{

    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class VapeMod : BaseUnityPlugin
    {

        public static AssetBundle vapeModAsset;
        private const string modGUID = "BilliardHorn.LCVapeMod";
        private const string modName = "Vape Mod";
        private const string modVersion = "1.0.0";

        public AudioSource audioSource;
        public AudioClip[] drinkClips;

        private static VapeMod Instance;
        bool IsOwner;

        void Awake()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            Instance = this;
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            vapeModAsset = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "vapemodassets"));

            if (vapeModAsset == null)
            {
                Logger.LogError("Failed to load custom assets."); // ManualLogSource for your plugin
                return;
            }

            int iRarity = 100;
            Item Vape = vapeModAsset.LoadAsset<Item>("Assets/Vape/Vape.asset");
            RipVape script = Vape.spawnPrefab.AddComponent<RipVape>();
            script.grabbable = true;
            script.grabbableToEnemies = true;
            script.itemProperties = Vape;


            NetworkPrefabs.RegisterNetworkPrefab(Vape.spawnPrefab);
            Utilities.FixMixerGroups(Vape.spawnPrefab);
            Items.RegisterScrap(Vape, iRarity, Levels.LevelTypes.All);

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "A giant blueberry flavored vape";
            Items.RegisterShopItem(Vape, null, null, node, 0);

            Logger.LogInfo("Vape mod loaded");

        }
    }
}