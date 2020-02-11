using Cars;
namespace Photon.Pun
{
    using UnityEngine;

    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CarController))]
    [AddComponentMenu("Photon Networking/Photon Rigidbody View")]
    public class PhotonWheelCol : MonoBehaviour, IPunObservable
    {
        private float m_Distance;
        private float m_Angle;

        private Rigidbody m_Body;

        private PhotonView m_PhotonView;

        private CarController m_CarController;
        private AxleInfo[] m_carAxis;

        private Vector3 m_NetworkPosition;

        private Quaternion m_NetworkRotation;

        private Vector3 m_NetworkAxle;

        private float  vertInput, horInput, drag;

        private Vector3 m_Velocity, m_AngularVelocity;

        public float m_DistToSmooth = 3.0f;

        public void Awake()
        {
            this.m_Body = GetComponent<Rigidbody>();
            this.m_PhotonView = GetComponent<PhotonView>();
            this.m_CarController = GetComponent<CarController>();
            this.m_carAxis = m_CarController.carAxis;
            m_CarController.rb = m_Body;
            m_Body.centerOfMass = m_CarController.centerOfMass.localPosition;

            this.m_NetworkPosition = new Vector3();
            this.m_NetworkRotation = new Quaternion();
        }

        public void FixedUpdate()
        {
            if (!this.m_PhotonView.IsMine)
            {
                this.m_Body.drag = drag;
                m_CarController.CheckOnGround();
                foreach (AxleInfo axle in m_carAxis)
                {
                    if (axle.steering)
                    {
                        axle.rightWheel.steerAngle = m_CarController.steerAngle * horInput;
                        axle.leftWheel.steerAngle = m_CarController.steerAngle * horInput;
                    }
                    if (axle.motor)
                    {
                        axle.rightWheel.motorTorque = m_CarController.carSpeed * vertInput;
                        axle.leftWheel.motorTorque = m_CarController.carSpeed * vertInput;
                    }
                }
                m_CarController.AddDownForce();
                m_CarController.SteerHelpAssist();
                if(horInput == 0)
                    this.m_Body.angularVelocity = m_AngularVelocity;
                if (vertInput == 0)
                    this.m_Body.velocity = m_Velocity;
                if (Vector3.Distance(this.m_Body.position, this.m_NetworkPosition) > this.m_DistToSmooth)
                {
                    this.m_Body.position = Vector3.MoveTowards(this.m_Body.position, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                    this.m_Body.rotation = Quaternion.RotateTowards(this.m_Body.rotation, this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(this.m_Body.position);
                stream.SendNext(this.m_Body.rotation);
                stream.SendNext(new Vector3(m_CarController.horInput, m_CarController.vertInput, this.m_Body.drag));
                stream.SendNext(this.m_Body.velocity);
                stream.SendNext(this.m_Body.angularVelocity);
            }
            else
            {
                this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();
                this.m_NetworkAxle = (Vector3)stream.ReceiveNext();
                vertInput = m_NetworkAxle.y;
                horInput = m_NetworkAxle.x;
                drag = m_NetworkAxle.z;
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                m_Velocity = (Vector3)stream.ReceiveNext();
                this.m_Distance = Vector3.Distance(this.m_Body.position, this.m_NetworkPosition);
                m_AngularVelocity = (Vector3)stream.ReceiveNext();
                this.m_NetworkRotation = Quaternion.Euler(m_AngularVelocity * lag) * this.m_NetworkRotation;
                this.m_Angle = Quaternion.Angle(this.m_Body.rotation, this.m_NetworkRotation);
                
            }
        }
    }
}
