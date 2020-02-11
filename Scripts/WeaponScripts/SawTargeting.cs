using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace Cars
{
    public class SawTargeting : MonoBehaviour
    {
        public static SawTargeting St;
        public GameObject Target;
        public GameObject Attacker;
        public GameObject Mesh;
        public AudioClip CollisionSound;
        public AudioSource MySrc;
        List<GameObject> insideMe = new List<GameObject>();
        public float Speed, TarDist, rotSpeed;
        public int Damage = -20;
        public int Sender;
        float Dist, Timer;
        public ParticleSystem sparks;
        bool Collided = false;
        int collisions = 0;
        GameObject LastCollision;
        private void Awake()
        {
            if (St == null)
                St = this;
        }
        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject == Attacker) return;
            Collided = true;
            sparks.Play();
            MySrc.volume = PlayerPrefs.GetFloat("WeaponsVolume");
            MySrc.clip = CollisionSound;
            MySrc.Play();
            if (collision.gameObject.tag == "Player" && LastCollision != Target)
            {
                LastCollision = collision.gameObject;
                PhotonView photonView = PhotonView.Get(this);
                if (photonView.IsMine)
                    photonView.RPC("ExplodeTarget", RpcTarget.All, Target.GetComponent<PhotonView>().ViewID);
                collisions++;
                var players = GameObject.FindGameObjectsWithTag("Player");
                if(collisions <5)
                for(int j = 0; j < players.Length; j++)
                {
                    if(Vector3.Distance(players[j].transform.position, this.transform.position) < TarDist && players[j] != Target && players[j] != Attacker)
                    {
                        Target = players[j];
                        Collided = false;
                        Damage += 5;
                        return;
                    }
                }
                transform.SetParent(Target.transform);
            }
            Timer = 5f;
            StartCoroutine(DestroyWarhead());
        }


        IEnumerator DestroyWarhead()
        {
            while (true)
            {
                if (sparks.particleCount == 0 && Timer <= 0 && !MySrc.isPlaying)
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
                if (St == this) St = null;
                if (!Collided)
                {
                    Mesh.transform.localRotation = Quaternion.Slerp(Mesh.transform.rotation, Quaternion.Euler(90, 0, Mesh.transform.rotation.eulerAngles.z + 30), Time.deltaTime * rotSpeed);
                    transform.position = Vector3.MoveTowards(transform.position, TargetPos, Time.deltaTime * Speed);
                    transform.LookAt(TargetPos);
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
