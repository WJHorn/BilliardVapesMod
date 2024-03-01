using BepInEx.Logging;
using GameNetcodeStuff;
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
        public PlayerControllerB previousPlayerHeldBy;
        public Boolean isRipping = false;

        public override void OnNetworkSpawn()
        {
            this.insertedBattery.charge = 1;
            base.OnNetworkSpawn();
        }
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            previousPlayerHeldBy = playerHeldBy;

            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            smokePrefab = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "smoke"));
            isLoaded = true;

            if (smokePrefab == null)
            {
                Logger.LogError("Failed to load custom assets.");
                return;
            }

            if (base.IsOwner)
            {
                isRipping = true;
                StartCoroutine(doAnimation());

                if (this.insertedBattery.charge > 0)
                {
                    this.insertedBattery.charge -= 0.1f;

                    smoke = smokePrefab.LoadAsset<GameObject>("Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 1.prefab");
                    LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(smoke);
                    Utilities.FixMixerGroups(smoke);

                    //example code for playing audio
                    //int num = UnityEngine.Random.Range(0, ripClips.Length);
                    //audioSource.PlayOneShot(ripClips[num]);

                    //WalkieTalkie.TransmitOneShotAudio(audioSource, ripClips[num], 1f);
                    //RoundManager.Instance.PlayAudibleNoise(base.transform.position, 20, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

                    RipVapeServerRpc();

                    // damage player 
                    //playerHeldBy.DamagePlayer(50, causeOfDeath: CauseOfDeath.Unknown, deathAnimation: 1);
                }
                
                StartCoroutine(undoAnimation());
                
                
            }

            AssetBundle.UnloadAllAssetBundles(false);
        }

        public override void DiscardItem()
        {
            if (!isRipping)
            {
                base.DiscardItem();
            }
            else
            {
                previousPlayerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);
                previousPlayerHeldBy.activatingItem = false;
                this.itemProperties.rotationOffset.y = 0;
                this.itemProperties.positionOffset.z = 0;
            }
        }
        public override void OnPlaceObject()
        {
            if (!isRipping)
            {
                base.OnPlaceObject();
            }
            else
            {
                previousPlayerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);
                previousPlayerHeldBy.activatingItem = false;
                this.itemProperties.rotationOffset.y = 0;
                this.itemProperties.positionOffset.z = 0;
            }
        }

        [ServerRpc]
        private void RipVapeServerRpc()
        {
            RipVapeClientRpc();
        }

        [ClientRpc]
        private void RipVapeClientRpc()
        {
            Instantiate(smoke, parentObject.position, Quaternion.identity);
            ps = smoke.GetComponent<ParticleSystem>();
            ps.Play();
        }

        public IEnumerator doAnimation()
        {
            if (base.IsOwner)
            {
                yield return new WaitForSeconds(0);
                playerHeldBy.activatingItem = true;
                playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", true);
                this.itemProperties.rotationOffset.y = 45;
                this.itemProperties.positionOffset.z = (float)(0.2);
            }
        }

        public IEnumerator undoAnimation()
        {
            yield return new WaitForSeconds(1);
            if (base.IsOwner)
            {
                if (playerHeldBy != null)
                {
                    //undo the animation
                    playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);
                    playerHeldBy.activatingItem = false;
                    this.itemProperties.rotationOffset.y = 0;
                    this.itemProperties.positionOffset.z = 0;
                    isRipping = false;
                }
            }
        }
    }
}
