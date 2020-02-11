using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cars
{
    public class Weapony : MonoBehaviour
    {
        public TargetFinder targetFinder;
        public float MiniGuntime, MinigunSpeed = 0.1f, MinigunOverHeat = 0f, TimeToOverHeat = 3;
        public bool OverHeated;
        public float SecondaryTime;
        public float HeavyTime;
        public float MinesDelay;
        public float SpecialDelay, SpecialBaseDelay;
        public Animator Animator;
        public List<ParticleHadler> WeaponPart = new List<ParticleHadler>();
        public List<WeaponSounds> WeaponSounds = new List<WeaponSounds>();
        public List<GameObject> Weapons = new List<GameObject>();
        public List<AudioSource> WeaponsAudioSrc = new List<AudioSource>();
        private void Start()
        {
            targetFinder = TargetFinder.targetFinder;
        }
        public virtual void Minigun() { }
        public virtual void Special(bool IsFireButtonPressed) { }
        public virtual void Secondary(List<int> AmmoBase) { }
        public virtual void HeavyWeap(List<int> AmmoBase) { }
        public virtual void Gadget(List<int> AmmoBase) { }

        public virtual void SelectWeapon(int weap)
        {
            Animator.SetInteger("CurWeap", weap);
        }
    }
}

[System.Serializable]
public class ParticleHadler
{
    public List<ParticleSystem> particles = new List<ParticleSystem>();
    public void EnEmit() { foreach (ParticleSystem ps in particles) ps.Emit(1); }
    public void PlayAll() { foreach (ParticleSystem ps in particles) ps.Play(); }
    public void StopAll() { foreach (ParticleSystem ps in particles) ps.Stop(); }
}
[System.Serializable]
public class WeaponSounds
{
    public List<AudioClip> Sounds = new List<AudioClip>();
}