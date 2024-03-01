using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
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

        public static AssetBundle vapeAssets;
        public Item[] Vapes;
        AudioClip drop;
        AudioClip pickup;
        //public Item Vape;
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
            vapeAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "vapes"));

            if (vapeAssets == null)
            {
                Logger.LogError("Failed to load custom assets."); // ManualLogSource for your plugin
                return;
            }

            drop = vapeAssets.LoadAsset<AudioClip>("Assets/Vape/Items/drop.mp3");
            pickup = vapeAssets.LoadAsset<AudioClip>("Assets/Vape/Items/pickup.mp3");

            int iRarity = 100;
            int random = Random.Range(0, 4);
            Vapes = vapeAssets.LoadAllAssets<Item>();
            foreach (Item Vape in Vapes)
            {
                RipVape script = Vape.spawnPrefab.AddComponent<RipVape>();
                Vape.dropSFX = drop;
                Vape.pocketSFX = pickup;
                Vape.grabSFX = pickup;
                script.grabbable = true;
                script.grabbableToEnemies = true;
                script.itemProperties = Vape;
                

                NetworkPrefabs.RegisterNetworkPrefab(Vape.spawnPrefab);
                Utilities.FixMixerGroups(Vape.spawnPrefab);
                Items.RegisterScrap(Vape, iRarity, Levels.LevelTypes.All);

                AssetBundle.UnloadAllAssetBundles(false);
            }

            Logger.LogInfo("Vape mod loaded");
        }
    }
}