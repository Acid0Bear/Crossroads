using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Cars
{
    public class GT500Weapony : Weapony
    {
        public float rotSpeed;

        public override void Minigun()
        {
            if (MiniGuntime <= 0 && MinigunOverHeat < TimeToOverHeat && !OverHeated)
            {
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("EnableEmit", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, 0);
                photonView.RPC("AudioMinigun", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, true);
                MinigunSpeed = 0.9f;
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
                    PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Saw"), Pos, Quaternion.identity);
                    photonView.RPC("LauchSaw", RpcTarget.All, targetFinder.Player.GetComponent<PhotonView>().ViewID,photonView.ViewID, PhotonRoom.photonroom.FindMyNum(PhotonNetwork.LocalPlayer.NickName));
                    photonView.RPC("AudioHeavy", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000);
                    AmmoBase[2] -= 1;
                    HeavyTime = 2f;
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
                var piece = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "GT-500","GT500Ability"), transform.position, Quaternion.identity);
                photonView.RPC("SetupSpecial", RpcTarget.All, photonView.ViewID, PhotonRoom.photonroom.FindMyNum(PhotonNetwork.LocalPlayer.NickName), PhotonView.Get(piece).ViewID);
                photonView.RPC("SpecialEmit", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, true);
                photonView.RPC("AudioSpecial", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000,true);
                SpecialDelay = SpecialBaseDelay;
            }
        }
        IEnumerator SpecialManage(GameObject Special, GameObject Mesh)
        {
            int previousWeap = 0;
            bool IsMine = false;
            if (Special.GetPhotonView().IsMine)
            {
                IsMine = true;
                previousWeap = Ammunition.ammunition.curWeap;
                Ammunition.ammunition.IsLocked = true;
                Ammunition.ammunition.curWeap = 4;
            }
            while (true)
            {
                Mesh.transform.localRotation = Quaternion.RotateTowards (Mesh.transform.localRotation, Quaternion.Euler(0, 0, Mesh.transform.localRotation.eulerAngles.z + 30), Time.deltaTime * rotSpeed);
                if (Special == null) break;
                yield return null;
            }
            if (IsMine)
            {      
                Ammunition.ammunition.curWeap = previousWeap;
                Ammunition.ammunition.IsLocked = false;
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("SpecialEmit", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, false);
                photonView.RPC("AudioSpecial", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber * 1000, false);
            }
        }
        
        [PunRPC]
        void EnableEmit(int AttackerID, int weap)
        {
            var Target = PhotonView.Find(AttackerID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            carc.Weap.WeaponPart[weap].EnEmit();
        }
        [PunRPC]
        void SpecialEmit(int AttackerID, bool State)
        {
            var Target = PhotonView.Find(AttackerID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            if(State)
                carc.Weap.WeaponPart[2].PlayAll();
            else
                carc.Weap.WeaponPart[2].StopAll();
        }

        [PunRPC]
        void LauchSaw(int PlayerId, int AttackerCar, int AttackerID)
        {
            var Target = PhotonView.Find(PlayerId).gameObject;
            var Attacker = PhotonView.Find(AttackerCar).gameObject;
            SawTargeting.St.Attacker = Attacker;
            SawTargeting.St.Target = Target;
            SawTargeting.St.Sender = AttackerID;
        }
        [PunRPC]
        void SetupSpecial(int AttackerCar, int AttackerID, int SpecialID)
        {
            var Attacker = PhotonView.Find(AttackerCar).gameObject;
            var carc = Attacker.GetComponent<CarController>();
            var SpecialPiece = PhotonView.Find(SpecialID).gameObject;
            StartCoroutine(SpecialManage(SpecialPiece, carc.Weap.Weapons[3]));
            var Script = SpecialPiece.GetComponent<GT500AbilityScript>();
            Script.source = Attacker;
            Script.SourceID = AttackerID;
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
        void AudioHeavy(int CarID)
        {
            var Target = PhotonView.Find(CarID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            carc.Weap.WeaponsAudioSrc[2].volume = PlayerPrefs.GetFloat("WeaponsVolume");
            carc.Weap.WeaponsAudioSrc[2].clip = carc.Weap.WeaponSounds[2].Sounds[0];
            carc.Weap.WeaponsAudioSrc[2].Play();
        }
        [PunRPC]
        void AudioSpecial(int CarID, bool state)
        {
            var Target = PhotonView.Find(CarID + 1).gameObject;
            var carc = Target.GetComponent<CarController>();
            carc.Weap.WeaponsAudioSrc[3].volume = PlayerPrefs.GetFloat("WeaponsVolume");
            carc.Weap.WeaponsAudioSrc[3].clip = carc.Weap.WeaponSounds[3].Sounds[0];
            if(state)
                carc.Weap.WeaponsAudioSrc[3].Play();
            else
                carc.Weap.WeaponsAudioSrc[3].Stop();
        }
        #endregion
    }
}
