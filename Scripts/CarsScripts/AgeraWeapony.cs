using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Cars
{
    public class AgeraWeapony : Weapony
    {
        public float JumpPwr = 800000;

        public override void Minigun()
        {
            if (MiniGuntime <= 0 && MinigunOverHeat < TimeToOverHeat && !OverHeated)
            {
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("EnableEmit", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, 0);
                photonView.RPC("AudioMinigun", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, true);
                MinigunSpeed = 0.9f;
                /*if (MinigunSpeed < 0.6f)
                    MinigunSpeed += 0.1f;
                else if (MinigunSpeed == 0.6f)
                    MinigunSpeed += 0.3f;
                else if (MinigunSpeed > 0.6f)
                {
                    MinigunOverHeat += Time.deltaTime;
                    if (MinigunOverHeat >= 3)
                        OverHeated = true;
                }*/
                MiniGuntime = 1 - MinigunSpeed;
                var Rot = CarController.Carcontroller.Weap.Weapons[0].transform.localRotation;
                Rot = Quaternion.RotateTowards(Rot, Quaternion.Euler(Rot.eulerAngles.x, Rot.eulerAngles.y + 90, Rot.eulerAngles.z), 30 * MinigunSpeed);
                CarController.Carcontroller.Weap.Weapons[0].transform.localRotation = Rot;
            }
            else if (OverHeated)
            {
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("AudioMinigun", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, false);
                MinigunOverHeat -= Time.deltaTime;
                if (MinigunOverHeat <= 0)
                {
                    OverHeated = false;
                }
                var Rot = CarController.Carcontroller.Weap.Weapons[0].transform.localRotation;
                Rot = Quaternion.RotateTowards(Rot, Quaternion.Euler(Rot.eulerAngles.x, Rot.eulerAngles.y + 90, Rot.eulerAngles.z), 30 * MinigunSpeed);
                CarController.Carcontroller.Weap.Weapons[0].transform.localRotation = Rot;
            }
            if (MinigunSpeed > 0.6f && !OverHeated)
            {
                if (MinigunOverHeat < TimeToOverHeat)
                    MinigunOverHeat += Time.deltaTime;
                if (MinigunOverHeat >= TimeToOverHeat)
                    OverHeated = true;
            }
        }
        public override void Secondary(List<int> AmmoBase)
        {
            targetFinder = TargetFinder.targetFinder;
            if (SecondaryTime <= 0 && AmmoBase[1] > 0)
            {
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("AudioCannon", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000);
                photonView.RPC("EnableEmit", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, 1);
                AmmoBase[1] -= 1;
                SecondaryTime = 1;
            }
        }
        public override void HeavyWeap(List<int> AmmoBase)
        {
            targetFinder = TargetFinder.targetFinder;
            if (HeavyTime <= 0 && AmmoBase[2] > 0)
            {
                PhotonView photonView = PhotonView.Get(this);
                if (targetFinder.Player)
                {
                    var Pos = CarController.Carcontroller.Weap.Weapons[2].transform.position;
                    PhotonNetwork.Instantiate(Path.Combine("Prefabs", "RocketWarhead"), Pos, Quaternion.identity);
                    photonView.RPC("LauchRocket", RpcTarget.All, targetFinder.Player.GetComponent<PhotonView>().ViewID, PhotonRoom.photonroom.FindMyNum(PhotonNetwork.LocalPlayer.NickName));
                    photonView.RPC("AudioRocket", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000);
                    AmmoBase[2] -= 1;
                    HeavyTime = 4;
                }
            }
        }

        public override void Gadget(List<int> AmmoBase)
        {
            if (MinesDelay <= 0 && AmmoBase[3] > 0)
            {
                var Pos = CarController.Carcontroller.transform.position;
                var bomb = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Bomb"), Pos, Quaternion.identity);
                bomb.GetComponentInChildren<LandMines>().OwnerID = PhotonRoom.photonroom.FindMyNum(PhotonNetwork.LocalPlayer.NickName);
                AmmoBase[3] -= 1;
                MinesDelay = 2;
            }
        }

        public override void Special(bool IsFireButtonPressed)
        {
            if (SpecialDelay <= 0)
            {
                PhotonView photonView = PhotonView.Get(this);
                Vector3 forw = new Vector3(0, 0, 0);
                if (CarController.Carcontroller.horInput > 0)
                    forw = CarController.Carcontroller.Carpos.forward;
                else if (CarController.Carcontroller.horInput < 0)
                    forw = CarController.Carcontroller.Carpos.forward * -1;
                forw.y = 0;
                CarController.Carcontroller.rb.AddRelativeForce((Vector3.up + forw) * (JumpPwr*100000));
                photonView.RPC("AudioSpecial", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000);
                photonView.RPC("SpecialEmit", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, 2);
                SpecialDelay = SpecialBaseDelay;
            }
        }
   
        [PunRPC]
        void EnableEmit(int CarID, int weap)
        {
            var Target = PhotonView.Find(CarID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            carc.Weap.WeaponPart[weap].EnEmit();
        }
        [PunRPC]
        void SpecialEmit(int CarID, int weap)
        {
            var Target = PhotonView.Find(CarID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            carc.Weap.WeaponPart[weap].PlayAll();
        }

        [PunRPC]
        void LauchRocket(int PlayerId, int AttackerID)
        {
            var Target = PhotonView.Find(PlayerId).gameObject;
            RocketTargeting.Rt.Target = Target;
            RocketTargeting.Rt.Sender = AttackerID;
        }
        #region Audio
        [PunRPC]
        void AudioMinigun(int CarID, bool IsFiring)
        {
            var Target = PhotonView.Find(CarID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            carc.Weap.WeaponsAudioSrc[0].volume = PlayerPrefs.GetFloat("WeaponsVolume");
            if (!IsFiring && !carc.Weap.WeaponsAudioSrc[0].isPlaying)
            {
                carc.Weap.WeaponsAudioSrc[0].clip = carc.Weap.WeaponSounds[0].Sounds[0];
                carc.Weap.WeaponsAudioSrc[0].Play();
            }
            else if (IsFiring && !carc.Weap.WeaponsAudioSrc[0].isPlaying)
            {
                carc.Weap.WeaponsAudioSrc[0].clip = carc.Weap.WeaponSounds[0].Sounds[1];
                carc.Weap.WeaponsAudioSrc[0].Play();
            }
        }
        [PunRPC]
        void AudioCannon(int CarID)
        {
            var Target = PhotonView.Find(CarID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            carc.Weap.WeaponsAudioSrc[1].volume = PlayerPrefs.GetFloat("WeaponsVolume");
            carc.Weap.WeaponsAudioSrc[1].clip = carc.Weap.WeaponSounds[1].Sounds[0];
            carc.Weap.WeaponsAudioSrc[1].Play();
        }
        [PunRPC]
        void AudioRocket(int CarID)
        {
            var Target = PhotonView.Find(CarID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            carc.Weap.WeaponsAudioSrc[2].volume = PlayerPrefs.GetFloat("WeaponsVolume");
            carc.Weap.WeaponsAudioSrc[2].clip = carc.Weap.WeaponSounds[2].Sounds[0];
            carc.Weap.WeaponsAudioSrc[2].Play();
        }
        [PunRPC]
        void AudioSpecial(int CarID)
        {
            var Target = PhotonView.Find(CarID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            carc.Weap.WeaponsAudioSrc[3].volume = PlayerPrefs.GetFloat("WeaponsVolume");
            carc.Weap.WeaponsAudioSrc[3].clip = carc.Weap.WeaponSounds[3].Sounds[0];
            carc.Weap.WeaponsAudioSrc[3].Play();
        }
        #endregion
    }
}
