using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cars
{
    public class EngineSoundManager : MonoBehaviour
    {

        [Header("pitch parameter")]
        public float flatoutSpeed = 20.0f;
        [Range(0.0f, 3.0f)]
        public float minPitch = 0.7f;
        [Range(0.0f, 0.1f)]
        public float pitchSpeed = 0.05f;

        public AudioSource _source;
        public CarController _vehicle;

        void Start()
        {
            if (PlayerPrefs.HasKey("CarsVolume"))
                _source.volume = PlayerPrefs.GetFloat("CarsVolume");
            else
            {
                _source.volume = 0.6f;
                PlayerPrefs.SetFloat("CarsVolume", 0.6f);
            }
        }

        void Update()
        {
            _source.volume = PlayerPrefs.GetFloat("CarsVolume");
            if (!_source.isPlaying || _source.clip == null)
            {
                _source.clip = _vehicle.rolling;
                _source.Play();
            }
            if (_source.clip == _vehicle.rolling && _vehicle.vertInput != 0 && _vehicle.onGround)
            {
                _source.pitch = Mathf.Lerp(_source.pitch, minPitch + Mathf.Abs(_vehicle.speed) / flatoutSpeed, pitchSpeed);
            }
            else if (_source.clip == _vehicle.rolling && _vehicle.vertInput == 0 || !_vehicle.onGround)
                _source.pitch = Mathf.Lerp(_source.pitch, minPitch, pitchSpeed);
        }
    }
}

