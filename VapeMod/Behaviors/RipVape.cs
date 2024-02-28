using BepInEx.Logging;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace VapeMod.Behaviors
{
    internal class RipVape : PhysicsProp
    {
        public ParticleSystem ps;
        public AudioSource audioSource;
        public AudioClip[] ripClips;
        public AssetBundle smokePrefab;
        ManualLogSource Logger;
        public Boolean isLoaded = false;
        public GameObject smoke;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            smokePrefab = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "smoke"));
            isLoaded = true;

            if (smokePrefab == null)
            {
                Logger.LogError("Failed to load custom assets."); // ManualLogSource for your plugin
                return;
            }

            //int num = UnityEngine.Random.Range(0, ripClips.Length);
            //audioSource.PlayOneShot(ripClips[num]);


            //WalkieTalkie.TransmitOneShotAudio(audioSource, ripClips[num], 1f);
            //RoundManager.Instance.PlayAudibleNoise(base.transform.position, 20, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

            if (base.IsOwner)
            {
            playerHeldBy.activatingItem = buttonDown;
            playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", buttonDown);
            this.itemProperties.rotationOffset.y = 45;
            this.itemProperties.positionOffset.z = (float)(0.1);

            smoke = smokePrefab.LoadAsset<GameObject>("Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 1.prefab");
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(smoke);
            Utilities.FixMixerGroups(smoke);
            

            
            RipVapeServerRpc();

            }

            StartCoroutine(undoAnimation());
            AssetBundle.UnloadAllAssetBundles(false);
        }

        [ServerRpc]
        private void RipVapeServerRpc()
        {
            RipVapeClientRpc();
        }

        [ClientRpc]
        private void RipVapeClientRpc()
        {

            //ps.useAutoRandomSeed = false;
            //Logger.LogError(ps
            //Instantiate(smoke, parentObject.position, Quaternion.identity);

            Instantiate(smoke, parentObject.position, Quaternion.identity);
            ps = smoke.GetComponent<ParticleSystem>();
            ps.Play();

            //GameObject.Destroy(smoke);
            //UnityEngine.Object.Instantiate(smoke, playerHeldBy.currentlyHeldObject.propBody.position, Quaternion.identity, playerHeldBy.currentlyHeldObject.propBody.transform);
        }

        public IEnumerator undoAnimation()
        {
            yield return new WaitForSeconds(1);
            if (base.IsOwner)
            {
                // damage player 
                //playerHeldBy.DamagePlayer(50, causeOfDeath: CauseOfDeath.Unknown, deathAnimation: 1);

                playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);
                playerHeldBy.activatingItem = false;
                this.itemProperties.rotationOffset.y = 0;
                this.itemProperties.positionOffset.z = 0;
                //smokePrefab.Unload(smokePrefab);
                
            }
        }
    }
}
