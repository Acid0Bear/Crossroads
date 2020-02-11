using System.Collections;
using UnityEngine;

namespace Cars
{
    [RequireComponent(typeof (AudioSource))]
    public class WheelEffects : MonoBehaviour
    {
        public Transform SkidTrailPrefab;
        public static Transform skidTrailsDetachedParent;
        public ParticleSystem skidParticles;
        public bool skidding { get; private set; }
        public bool PlayingAudio { get; private set; }


        private AudioSource m_AudioSource;
        private Transform m_SkidTrail;
        private WheelCollider m_WheelCollider;


        private void Start()
        {
            //skidParticles = transform.root.GetComponentInChildren<ParticleSystem>();

            if (skidParticles == null)
            {
                Debug.LogWarning(" no particle system found on car to generate smoke particles", gameObject);
            }
            else
            {
                skidParticles.Stop();
            }

            m_WheelCollider = GetComponent<WheelCollider>();
            m_AudioSource = GetComponent<AudioSource>();
            if (PlayerPrefs.HasKey("CarsVolume"))
                m_AudioSource.volume = PlayerPrefs.GetFloat("CarsVolume")/2;
            else
            {
                m_AudioSource.volume = 0.6f;
                PlayerPrefs.SetFloat("CarsVolume", 0.6f);
            }
            PlayingAudio = false;
            skidding = false;
            if (skidTrailsDetachedParent == null)
            {
                skidTrailsDetachedParent = new GameObject("Skid Trails - Detached").transform;
            }
        }


        public void EmitTyreSmoke()
        {
            skidParticles.transform.position = transform.position - transform.up*m_WheelCollider.radius;
            skidParticles.Emit(1);
            if (!skidding)
            {
                StartCoroutine(StartSkidTrail());
            }
        }


        public void PlayAudio()
        {
            float pitch = Random.Range(.90f, 1.15f);
            m_AudioSource.pitch = pitch;
            m_AudioSource.volume = PlayerPrefs.GetFloat("CarsVolume")/2;
            m_AudioSource.Play();
            PlayingAudio = true;
        }


        public void StopAudio()
        {
            m_AudioSource.Stop();
            PlayingAudio = false;
        }


        public IEnumerator StartSkidTrail()
        {
            skidding = true;
            m_SkidTrail = Instantiate(SkidTrailPrefab);
            while (m_SkidTrail == null)
            {
                yield return null;
            }
            m_SkidTrail.parent = transform;
            m_SkidTrail.localPosition = -Vector3.up*m_WheelCollider.radius;
        }


        public void EndSkidTrail()
        {
            if (!skidding)
            {
                return;
            }
            skidding = false;
            m_SkidTrail.parent = skidTrailsDetachedParent;
            Destroy(m_SkidTrail.gameObject, 10);
        }
    }
}
