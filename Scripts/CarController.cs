using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cars
{
    public class CarController : MonoBehaviour
    {
        #region Init/Targ
        Joystick joystick;
        [Header("Initialization")]
        public Health health;
        public static CarController Carcontroller;
        public Transform Carpos, Camtarget;
        public Weapony Weap;
        public Material CarMat;
        public List<Texture2D> Patterns;
        [HideInInspector]
        public List<GameObject> insideMe = new List<GameObject>();
        #endregion
        #region CarProp
        [Header("Car Properites")]
        public AxleInfo[] carAxis = new AxleInfo[2];
        public WheelCollider[] wheelColliders;
        public float carSpeed;
        public float DownForce;
        public float steerAngle;
        public Transform centerOfMass;
        [Range(0, 1)]
        public float steerHelpValue = 0;
        #endregion
        #region Audio
        [Header("Car Audio")]
        public AudioClip rolling;
        public float absolute, check;
        public AudioClip suspension, hit;
        public AudioSource engine, grouding;
        #endregion
        #region Tires
        [Header("For Smoke From Tires")]
        [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];
        [SerializeField] private float m_SlipLimit = 1.25f;
        #endregion
        #region Misc
        [HideInInspector]
        public float horInput;
        [HideInInspector]
        public float vertInput;
        [HideInInspector]
        public Rigidbody rb;
        [HideInInspector]
        public bool onGround;
        [Header("Misc")]
        public Vector3 additionalWheelAngle;
        public MiscPaticles MiscsPart;
        public AudioClip NitroActivation, NitroRunning;
        public AudioSource NitroSrc;
        public float speed, RotSpeed;
        float lastYRotation;
        Vector3 LastPos;
        #endregion
        private void Awake()
        {
            //TurnOff();
            if (Carcontroller == null)
            {
                Carcontroller = this;
            }
        }
        void Start()
        {
            LastPos = transform.position;
        }

        #region TargetDetecting
        private void OnTriggerEnter(Collider e)
        {
            if (e.gameObject != this.gameObject && e.gameObject.tag == "Player")
            {
                if (!this.insideMe.Contains(e.gameObject))
                {
                    Debug.DrawRay(this.gameObject.transform.position, e.gameObject.transform.position, Color.red);
                    Debug.Log("Collider triggered");
                    this.insideMe.Add(e.gameObject);
                }

            }
        }

        private void OnTriggerExit(Collider e)
        {
            if (this.insideMe.Contains(e.gameObject))
            {
                this.insideMe.Remove(e.gameObject);
            }

        }
        #endregion

        private void OnCollisionEnter(Collision collision)
        {
            
            grouding.pitch = Random.Range(0.9f, 1.2f);
            grouding.clip = hit;
            check = collision.relativeVelocity.magnitude;
            grouding.volume = 0.01f * check;
            if (!grouding.isPlaying && check > absolute)
            grouding.Play();
        }
        void Update()
        {
            if (SceneManager.GetActiveScene().buildIndex == 1) return;
            if (PhotonView.Get(this).IsMine)
            {
                if (insideMe.Contains(null))
                    insideMe.Clear();
                if (health.isAlive && !health.IsStunned)
                {
                    rb.drag = 0.24f;
                    CheckOnGround();
                    Accelerate();
                    ManageSyspension();
                    SteerHelpAssist();
                    CheckForWheelSpin();
                }
                else if (!health.isAlive || health.IsStunned)
                {
                    rb.drag = 2;
                    horInput = 0;
                    vertInput = 0;
                    CheckOnGround();
                    Accelerate();
                    ManageSyspension();
                    SteerHelpAssist();
                }
            }
            else
            {
                foreach (AxleInfo axle in carAxis)
                {
                    VisualWheelsToColliders(axle.rightWheel, axle.visRightWheel);
                    VisualWheelsToColliders(axle.leftWheel, axle.visLeftWheel);
                }
                CheckForWheelSpin();
            }
            //Carpos = gameObject.transform;
            //horInput = Input.GetAxis("Horizontal");
            //vertInput = Input.GetAxis("Vertical");
            //ManageNitro();
            //ManageHardBreak();
            //EmitSmokeFromTires();  
        }
        IEnumerator WaitForGround()
        {
            int grounded = 0;
            while (grounded < 4)
            {
                foreach (WheelCollider wheelCol in wheelColliders)
                {
                    if (wheelCol.isGrounded)
                        grounded++;
                }
                if (grounded != 4) grounded = 0;
                yield return null;
            }
            grouding.pitch = Random.Range(0.9f, 1.2f);
            grouding.clip = suspension;
            grouding.volume = 0.15f;
            if (!grouding.isPlaying)
                grouding.Play();
        }
        private void CheckForWheelSpin()
        {
            // loop through all wheels
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelHit;
                wheelColliders[i].GetGroundHit(out wheelHit);
                // is the tire slipping above the given threshhold
                if (Mathf.Abs(wheelHit.forwardSlip) >= m_SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= (m_SlipLimit/1.5))
                {
                    m_WheelEffects[i].EmitTyreSmoke();
                    // avoiding all four tires screeching at the same time
                    // if they do it can lead to some strange audio artefacts
                    if (!AnySkidSoundPlaying())
                    {
                        m_WheelEffects[i].PlayAudio();
                    }
                    continue;
                }
                // if it wasnt slipping stop all the audio
                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();
                }
                // end the trail generation
                m_WheelEffects[i].EndSkidTrail();
            }
        }

        private bool AnySkidSoundPlaying()
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_WheelEffects[i].PlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }

        void Accelerate()
        {
            speed = Vector3.Distance(LastPos, transform.position);
            LastPos = transform.position;
            if (vertInput == 0)
                MiscsPart.GasReleased();
            else
                MiscsPart.GasPressed();
            foreach (AxleInfo axle in carAxis)
            {
                if (axle.steering)
                {
                    axle.rightWheel.steerAngle = steerAngle * horInput;
                    axle.leftWheel.steerAngle = steerAngle * horInput;
                }
                if (axle.motor)
                {
                    axle.rightWheel.motorTorque = carSpeed * vertInput;
                    axle.leftWheel.motorTorque = carSpeed * vertInput;
                }
                VisualWheelsToColliders(axle.rightWheel, axle.visRightWheel);
                VisualWheelsToColliders(axle.leftWheel, axle.visLeftWheel);
            }
            AddDownForce();
        }

        void VisualWheelsToColliders(WheelCollider col, Transform visWheel)
        {
            Vector3 position;
            Quaternion rotation;

            col.GetWorldPose(out position, out rotation);

            visWheel.position = position;
            visWheel.rotation = rotation * Quaternion.Euler(additionalWheelAngle);
        }

        void ManageSyspension()
        {
            float sysMax = 0.65f;
            if (rb.velocity.magnitude > 20)
                switch (horInput)
                {
                    case 0.5f:
                        foreach (AxleInfo axle in carAxis)
                        {
                            if (axle.leftWheel.suspensionDistance < sysMax)
                                axle.leftWheel.suspensionDistance += 0.01f;
                        }
                        break;
                    case 1f:
                        foreach (AxleInfo axle in carAxis)
                        {
                            if (axle.leftWheel.suspensionDistance < sysMax)
                                axle.leftWheel.suspensionDistance += 0.05f;
                        }
                        break;
                    case -0.5f:
                        foreach (AxleInfo axle in carAxis)
                        {
                            if (axle.rightWheel.suspensionDistance < sysMax)
                                axle.rightWheel.suspensionDistance += 0.01f;
                        }
                        break;
                    case -1f:
                        foreach (AxleInfo axle in carAxis)
                        {
                            if (axle.rightWheel.suspensionDistance < sysMax)
                                axle.rightWheel.suspensionDistance += 0.05f;
                        }
                        break;
                }
            if (horInput == 0)
                foreach (AxleInfo axle in carAxis)
                {
                    if (axle.rightWheel.suspensionDistance > 0.55f)
                        axle.rightWheel.suspensionDistance -= 0.05f;
                    if (axle.leftWheel.suspensionDistance > 0.55f)
                        axle.leftWheel.suspensionDistance -= 0.05f;
                }
        }

        public void SteerHelpAssist()
        {
            if (!onGround)
                return;

            if (Mathf.Abs(transform.rotation.eulerAngles.y - lastYRotation) < 10f)
            {
                float turnAdjust = (transform.rotation.eulerAngles.y - lastYRotation) * steerHelpValue;
                Quaternion rotateHelp = Quaternion.AngleAxis(turnAdjust, Vector3.up);
                rb.velocity = rotateHelp * rb.velocity;
            }
            lastYRotation = transform.rotation.eulerAngles.y;
        }

        public void CheckOnGround()
        {
            onGround = true;
            int notGrounded = 0;
            foreach (WheelCollider wheelCol in wheelColliders)
            {
                if (!wheelCol.isGrounded)
                {
                    onGround = false;
                    notGrounded++;
                }   
            }
            if (notGrounded == 2) StartCoroutine(WaitForGround());
        }

        public void RotateCenter()
        {
            if (horInput == -1)
            {
                centerOfMass.localRotation = Quaternion.RotateTowards(centerOfMass.localRotation, Quaternion.Euler(0, 30, 0), Time.deltaTime * RotSpeed);
            }
            else if (horInput == 1)
            {
                centerOfMass.localRotation = Quaternion.RotateTowards(centerOfMass.localRotation, Quaternion.Euler(0, -30, 0), Time.deltaTime * RotSpeed);
            }
            else if (horInput != 1 || horInput != -1)
            {
                centerOfMass.localRotation = Quaternion.RotateTowards(centerOfMass.localRotation, Quaternion.identity, Time.deltaTime * RotSpeed);
            }
        }
        public void AddDownForce()
        {
            rb.AddForce(-transform.up * DownForce * rb.velocity.magnitude);
        }

        public void TurnOn()
        {
            this.enabled = true;
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = centerOfMass.localPosition;
        }

        public void TurnOff()
        {
            this.enabled = false;
        }
    }
}
[System.Serializable]
public class AxleInfo
{
    public WheelCollider rightWheel;
    public WheelCollider leftWheel;

    public Transform visRightWheel;
    public Transform visLeftWheel;

    public bool steering;
    public bool motor;

}
[System.Serializable]
public class MiscPaticles
{
    public List<ParticleSystem> Exhaust = new List<ParticleSystem>();
    public List<ParticleSystem> Nitro = new List<ParticleSystem>();
    public void GasPressed()
    {
        foreach (ParticleSystem part in Exhaust)
        {
            var tmp = part.emission;
            tmp.rateOverTime = 30;
        }
    }
    public void GasReleased()
    {
        foreach (ParticleSystem part in Exhaust)
        {
            var tmp = part.emission;
            tmp.rateOverTime = 8;
        }
    }
    public void EnableNitro()
    {
        foreach (ParticleSystem part in Nitro)
        {
            part.Play();
        }
    }
    public void DisableNitro()
    {
        foreach (ParticleSystem part in Nitro)
        {
            part.Stop();
        }
    }
}
