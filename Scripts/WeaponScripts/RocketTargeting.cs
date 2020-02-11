using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace Cars
{
    public class RocketTargeting : MonoBehaviour
    {
        public static RocketTargeting Rt;
        public ParticleSystem Mine;
        public GameObject Target;
        public AudioClip ExplSound;
        public AudioSource MySrc;
        public float Speed, TarDist;
        public int Damage = -25;
        public int Sender;
        public GameObject Explosion;
        float Dist, Timer;
        ParticleSystem expl;
        GameObject ExplGO;
        bool IsSpawned = false;
        bool Collided = false;
        private void Awake()
        {
            if (Rt == null)
                Rt = this;
        }
        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.tag == "Player")
            {
                return;
            }
            Collided = true;
            var em = Mine.emission;
            em.enabled = false;
            ExplGO = Instantiate(Explosion, transform);
            ExplGO.transform.position = transform.position;
            expl = ExplGO.GetComponent<ParticleSystem>();
            expl.Play();
            GetComponent<MeshRenderer>().enabled = false;
            IsSpawned = true;
            Timer = 0.5f;
            MySrc.clip = ExplSound;
            MySrc.volume = PlayerPrefs.GetFloat("WeaponsVolume");
            MySrc.Play();
            StartCoroutine(DestroyWarhead());
        }
        IEnumerator DestroyWarhead()
        {
            while (true)
            {
                if (ExplGO != null && expl.particleCount == 0 && Timer <= 0 && !MySrc.isPlaying)
                {
                    //Destroy(ExplGO);
                    if (PhotonView.Find(this.GetComponent<PhotonView>().ViewID).IsMine)
                        PhotonNetwork.Destroy(this.gameObject);
                    break;
                }
                yield return null;
            }
        }
        private void Update()
        {
            if (Timer > 0)
                Timer -= Time.deltaTime;
            if (Target)
            {
                Vector3 TargetPos = Target.transform.position;
                TargetPos.y += 0.5f;
                if (Rt == this) Rt = null;
                if (!Collided)
                {
                    transform.position = Vector3.MoveTowards(transform.position, TargetPos, Time.deltaTime * Speed);
                    transform.LookAt(TargetPos);
                }
                Dist = Vector3.Distance(transform.position, TargetPos);
                if (Dist < TarDist)
                {
                    if (!IsSpawned)
                    {
                        var em = Mine.emission;
                        em.enabled = false;
                        ExplGO = Instantiate(Explosion, transform);
                        ExplGO.transform.position = transform.position;
                        expl = ExplGO.GetComponent<ParticleSystem>();
                        expl.Play();
                        MySrc.clip = ExplSound;
                        MySrc.volume = PlayerPrefs.GetFloat("WeaponsVolume");
                        MySrc.Play();
                        GetComponent<MeshRenderer>().enabled = false;
                        PhotonView photonView = PhotonView.Get(this);
                        if (photonView.IsMine)
                            photonView.RPC("ExplodeTarget", RpcTarget.All, Target.GetComponent<PhotonView>().ViewID);
                        IsSpawned = true;
                        Timer = 0.5f;
                        StartCoroutine(DestroyWarhead());
                    }
                }
            }
        }

        [PunRPC]
        void ExplodeTarget(int TarID)
        {
            var Target = PhotonView.Find(TarID).gameObject;
            GameLogic.gameLogic.statHandler.stats[Sender].DamageDealt += Damage * -1;
            Target.GetComponent<Health>().ManageHealth(Damage, Sender);
        }
    }
}
