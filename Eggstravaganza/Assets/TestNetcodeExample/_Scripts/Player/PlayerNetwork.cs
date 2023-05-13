using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float m_InterpolationTime = 0.1f;

    private readonly NetworkVariable<PlayerNetworkData> m_NetState = new(writePerm: NetworkVariableWritePermission.Owner);

    private Vector3 m_Velocity;
    private float m_RotationVelocity;

    private void Update()
    {
        if (IsOwner)
        {
            m_NetState.Value = new PlayerNetworkData
            {
                Position = transform.position,
                Rotation = transform.rotation.eulerAngles
            };
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, m_NetState.Value.Position, ref m_Velocity,
                m_InterpolationTime);
            transform.rotation = Quaternion.Euler(
                0f,
                Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, m_NetState.Value.Rotation.y,
                    ref m_RotationVelocity, m_InterpolationTime),
                0f);
        }
    }


    private struct PlayerNetworkData : INetworkSerializable
    {
        private float m_XPos;
        private float m_ZPos;
        private float m_YRot;

        internal Vector3 Position
        {
            get => new(m_XPos, 0f, m_ZPos);
            set
            {
                m_XPos = value.x;
                m_ZPos = value.z;
            }
        }

        internal Vector3 Rotation
        {
            get => new(0f, m_YRot, 0f);
            set => m_YRot = value.y;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_XPos);
            serializer.SerializeValue(ref m_ZPos);
            serializer.SerializeValue(ref m_YRot);
        }
    }
}
