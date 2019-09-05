using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace App.Social
{
    public class OVRPhotonAvatarView : MonoBehaviour
    {
        #region Private Fields
        private PhotonView _photonView;

        [SerializeField]
        private OvrAvatar _ovrAvatar;

        private OvrAvatarRemoteDriver _remoteDriver;

        private List<byte[]> _packetData;

        private bool _isOVRReady;
        #endregion

        #region Monobehaviour Functions
        private void Awake()
        {
            _isOVRReady = false;
        }

        private IEnumerator Start()
        {
            Debug.Log("[OVRPhotonAvatarView] Start().");
            _photonView = GetComponent<PhotonView>();

            if (_photonView.isMine)
            {
                _ovrAvatar = GetComponent<OvrAvatar>();

                _packetData = new List<byte[]>();

                _ovrAvatar.RecordPackets = true;
                _ovrAvatar.PacketRecorded += OnLocalAvatarPacketRecorded;
            }
            else
            {
                _remoteDriver = GetComponent<OvrAvatarRemoteDriver>();
            }

            yield return null; // Fix ovrAvatarPacket_GetSize wait for plugin
            _isOVRReady = true;
        }

        private void OnEnable()
        {
            if (_ovrAvatar != null && _photonView != null && _photonView.isMine)
            {
                _ovrAvatar.RecordPackets = false;
                _ovrAvatar.PacketRecorded -= OnLocalAvatarPacketRecorded;
            }
        }

        private void OnDisable()
        {
            if (_ovrAvatar != null && _photonView.isMine)
            {
                _ovrAvatar.RecordPackets = false;
                _ovrAvatar.PacketRecorded -= OnLocalAvatarPacketRecorded;
            }
        }
        #endregion

        #region Private Functions
        private int localSequence;

        /// <summary>
        /// OnLocalAvatarPacketRecorded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnLocalAvatarPacketRecorded(object sender, OvrAvatar.PacketEventArgs args)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(outputStream);

                uint size = Oculus.Avatar.CAPI.ovrAvatarPacket_GetSize(args.Packet.ovrNativePacket);
                byte[] data = new byte[size];
                Oculus.Avatar.CAPI.ovrAvatarPacket_Write(args.Packet.ovrNativePacket, size, data);

                writer.Write(localSequence++);
                writer.Write(size);
                writer.Write(data);

                _packetData.Add(outputStream.ToArray());
            }
        }

        /// <summary>
        /// DeserializeAndQueuePacketData
        /// </summary>
        /// <param name="data"></param>
        private void DeserializeAndQueuePacketData(byte[] data)
        {
            if (data == null || _remoteDriver == null)
            {
                return;
            }

            using (MemoryStream inputStream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(inputStream);
                int remoteSequence = reader.ReadInt32();

                int size = reader.ReadInt32();
                byte[] sdkData = reader.ReadBytes(size);

                IntPtr packet = Oculus.Avatar.CAPI.ovrAvatarPacket_Read((UInt32)data.Length, sdkData);

                _remoteDriver.QueuePacket(remoteSequence, new OvrAvatarPacket { ovrNativePacket = packet });
            }
        }

        /// <summary>
        /// OnPhotonSerializeView
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (!_isOVRReady)
            {
                return;
            }

            if (stream.isWriting)
            {
                if (_packetData.Count == 0)
                {
                    return;
                }

                stream.SendNext(_packetData.Count);

                foreach (byte[] b in _packetData)
                {
                    stream.SendNext(b);
                }

                _packetData.Clear();
            }

            if (stream.isReading)
            {
                int num = (int)stream.ReceiveNext();

                for (int counter = 0; counter < num; ++counter)
                {
                    byte[] data;
                    try
                    {
                        data = (byte[]) stream.ReceiveNext();
                    }
                    catch (InvalidCastException e)
                    {
                        Debug.Log("[OVRPhotonAvatarView] OnPhotonSerializeView() - Failed to cast data");

                        break;
                    }

                    DeserializeAndQueuePacketData(data);
                }
            }
        }
        #endregion
    }
}

