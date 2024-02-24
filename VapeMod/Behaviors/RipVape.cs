using BepInEx.Logging;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace VapeMod.Behaviors
{
    internal class RipVape : PhysicsProp
    {
        public AudioSource audioSource;
        public AudioClip[] ripClips;
        public AssetBundle smokePrefab;
        ManualLogSource Logger;
        Boolean isLoaded = false;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (!isLoaded)
            {
                base.ItemActivate(used, buttonDown);

                string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                smokePrefab = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "smoke"));
                isLoaded = true;
            }

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
                
                GameObject smoke = smokePrefab.LoadAsset<GameObject>("Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 1.prefab");
                NetworkPrefabs.RegisterNetworkPrefab(smoke);
                Utilities.FixMixerGroups(smoke);
                Instantiate(smoke, parentObject.position, Quaternion.identity);
                ParticleSystem ps = smoke.GetComponent<ParticleSystem>();
                //Logger.LogError(ps);
                ps.Play();

                //GameObject.Destroy(smoke);
                //UnityEngine.Object.Instantiate(smoke, playerHeldBy.currentlyHeldObject.propBody.position, Quaternion.identity, playerHeldBy.currentlyHeldObject.propBody.transform);
            }
            StartCoroutine(undoAnimation());

            
        }

        public IEnumerator undoAnimation()
        {
            yield return new WaitForSeconds(3);
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
