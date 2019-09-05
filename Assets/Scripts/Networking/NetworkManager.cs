using System;
using System.Text;
using App;
using App.UI;
using App.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace CINEVR.App.Networking
{
    /// <summary>
    /// NetworkManager
    /// Singleton and global class 
    /// It lives during all the program
    /// </summary>
    public sealed class NetworkManager : Singleton<NetworkManager>
    {
        #region Internal RoomIntent Class
        internal class RoomIntent
        {
            public string Host;
            public string RoomName;
            public bool IsPublic;
        }
        #endregion

        #region Constants 
        private const string RESOURCE_FOLDER_OVR_AVATARS = "OVRAvatars/";
        private const string RESOURCE_NAME_LOCAL_AVATAR = "LocalAvatar";
        private const string RESOURCE_NAME_REMOTE_AVATAR = "RemoteAvatar";

        public const string ROOM_MASTER_PHOTON_SOURCE = "MasterRoom";
        public const int MAX_PLAYER_PER_ROOM = 2;

        private const string PUBLIC_ROOM_HEADER = "Public";
        private const string PRIVATE_ROOM_HEADER = "Private";
        private const string ROOM_NAME_HEADER = "Room - ";
        private const string PHOTON_NETWORK_VERSION = "1.5";
        #endregion

        #region Private Fields

        [SerializeField] private Transform _player1SpawnTransform;
        [SerializeField] private Transform _player2SpawnTransform;

        private RoomInfo[] _roomInfos;
        private RoomIntent _createRoomIntent;

        private string _lastPrivateRoomName = "";
        private bool _wasInPrivateRoom;
        private bool _wasPaused; // used to store that we won't to do anything in OnLobby after pausing networking
        #endregion

        #region Public Properties
        public bool InLobby
        {
            get
            {
                return PhotonNetwork.insideLobby;
            }
        }

        public bool InPublicRoom
        {
            get
            {
                return PhotonNetwork.inRoom && PhotonNetwork.room.visible;
            }
        }

        public bool InPrivateRoom
        {
            get
            {
                return PhotonNetwork.inRoom && !PhotonNetwork.room.visible;
            }
        }


        public bool IsMaster
        {
            get
            {
                return PhotonNetwork.inRoom && PhotonNetwork.isMasterClient;
            }
        }

        public bool IsConnected
        {
            get { return PhotonNetwork.connected; }
        }

        public bool InRoom
        {
            get { return PhotonNetwork.inRoom; }
        }

        public int YourID { get; internal set; }
        #endregion

        #region Custom Events
        [HideInInspector]
        public UnityEvent OnNetworkConnected = new UnityEvent();
        [HideInInspector]
        public UnityEvent OnNetworkDisconnected = new UnityEvent();
        [HideInInspector]
        public UnityEvent OnNetworkJoinedRoom = new UnityEvent();
        [HideInInspector]
        public UnityEvent OnNetworkLeftRoom = new UnityEvent();
        [HideInInspector]
        public UnityEvent OnNetworkCreatedRoom = new UnityEvent();
        [HideInInspector]
        public UnityEvent OnNetworkJoinedLobby = new UnityEvent();
        [HideInInspector]
        public UnityEvent OnNetworkRandomJoinFailed = new UnityEvent();

        public class PhotonPlayerEvent : UnityEvent<PhotonPlayer> { }
        public class PhotonMessageEvent : UnityEvent<PhotonMessageInfo> { }

        public PhotonPlayerEvent OnNetworkPlayerConnected = new PhotonPlayerEvent();
        public PhotonPlayerEvent OnNetworkPlayerDisconnected = new PhotonPlayerEvent();
        public PhotonPlayerEvent OnNetworkMasterClientSwitched = new PhotonPlayerEvent();
        #endregion

        #region MonoBehaviour Functions
        private void Awake()
        {
            PhotonNetwork.autoJoinLobby = true; // Allow to joined automatically lobby on network
            PhotonNetwork.InstantiateInRoomOnly = true; // Instiantiate photon observable object only in room
        }

        private void OnEnable()
        {
            // Connect to photon 
            //
            ConnectNetworking();

            PhotonNetwork.OnEventCall += OnEvent;
        }

        private void OnDisable()
        {
            // Disconnect to photon
            //
            DisconnectNeworking();

            PhotonNetwork.OnEventCall -= OnEvent;
        }
        #endregion

        #region Public Functions
        #region Networking
        /// <summary>
        /// ConnectNetworking
        /// </summary>
        public void ConnectNetworking()
        {
            if (PhotonNetwork.connected)
            {
                return;
            }

            Debug.Log("[NetworkManager] Connect to photon().");

            PhotonNetwork.ConnectUsingSettings(PHOTON_NETWORK_VERSION);
        }

        /// <summary>
        /// DisconnectNeworking
        /// </summary>
        public void DisconnectNeworking()
        {
            Debug.Log("[NetworkManager] Disconnect from photon().");

            PhotonNetwork.Disconnect();
        }

        /// <summary>
        /// PauseNetworking
        /// When a pause is called. It leads to a player room leaving 
        /// The boolean wasPaused is used to indicate that we don't want to reconnect automatically to a public room after pausing network
        /// When the network is paused, it forces player to go in lobby
        /// </summary>
        public void PauseNetworking()
        {
            // Returns if already paused 
            //
            if (_wasPaused)
            {
                return;
            }

            Debug.Log("[NetworkManager] PauseNetworking()");

            // If we are currently in private room then it stores this private room name to reconnect after pausing
            //
            if (InRoom && !InPublicRoom)
            {
                _wasInPrivateRoom = true;
                _lastPrivateRoomName = PhotonNetwork.room.name;
            }

            LeaveRoom();

            _wasPaused = true;
        }

        /// <summary>
        /// ResumeNetworking
        /// </summary>
        public void ResumeNetworking()
        {
            // Returns if not paused
            //
            if (!_wasPaused)
            {
                return;
            }

            Debug.Log("[NetworkManager] ResumeNetworking().");

            // Ensure were are connected
            //
            ConnectNetworking();

            _wasPaused = false;

            ConnectToPublicRoom();
        }
        #endregion

        #region Rooms Behaviour Functions
        /// <summary>
        /// JoinOrCreateRoom
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="isPublic"></param>
        public void CreateRoom(string roomName, bool isPublic = true)
        {
            Debug.Log("[NetworkManager] CreateRoom(). " + roomName);

            RoomOptions roomOptions = new RoomOptions { IsVisible = isPublic, MaxPlayers = MAX_PLAYER_PER_ROOM };
            Hashtable customProperties = new Hashtable();
            roomOptions.CustomRoomProperties = customProperties;
            PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

        /// <summary>
        /// LeaveRoom
        /// </summary>
        public void LeaveRoom()
        {
            if (!InRoom)
            {
                return;
            }

            PhotonNetwork.LeaveRoom();
        }
        #endregion
        #endregion

        #region Photon Callbacks
        /// <summary>
        /// OnCreatedRoom
        /// Photon Callback
        /// </summary>
        private void OnCreatedRoom()
        {
            Debug.Log("[NetworkManager] OnCreatedRoom().");

            OnNetworkCreatedRoom.Invoke();
        }

        /// <summary>
        /// OnJoinedRoom
        /// Photon Callback
        /// </summary>
        public readonly byte InstantiateVrAvatarEventCode = 123;
        private void OnJoinedRoom()
        {
            Debug.Log(string.Format("[NetworkManager] You joined {0} \n your are the player {1}", PhotonNetwork.room.name, PhotonNetwork.player.ID));

            OnNetworkJoinedRoom.Invoke();

            int viewId = PhotonNetwork.AllocateViewID();
            PhotonNetwork.RaiseEvent(InstantiateVrAvatarEventCode, viewId, true, new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache, Receivers = ReceiverGroup.All });

            PhotonVoiceNetwork.Connect();
        }

        /// <summary>
        /// OnEvent
        /// </summary>
        /// <param name="eventcode"></param>
        /// <param name="content"></param>
        /// <param name="senderid"></param>
        private void OnEvent(byte eventcode, object content, int senderid)
        {
            Debug.Log(string.Format("[NetworkManager] OnEvent(). - code :{0}", eventcode));

            if (eventcode == InstantiateVrAvatarEventCode)
            {
                GameObject go;

                if (PhotonNetwork.player.ID == senderid)
                {
                    Debug.Log("[NetworkManager] OnEvent(). Instantiate Local avatar...");

                    go = Instantiate(Resources.Load(RESOURCE_FOLDER_OVR_AVATARS + RESOURCE_NAME_LOCAL_AVATAR)) as GameObject;

                    Vector3 pos = new Vector3(0f, Application.platform == RuntimePlatform.Android ? -1.70f : 0.0f, 0f);

                    OVRCameraRig camera = FindObjectOfType<OVRCameraRig>();

                    if (!PhotonNetwork.player.IsMasterClient)
                    {
                        camera.transform.position = _player2SpawnTransform.position;
                        camera.transform.rotation = _player2SpawnTransform.rotation;

                        go.transform.position = _player2SpawnTransform.position;
                        go.transform.rotation = _player2SpawnTransform.rotation;

                        YourID = 1;
                    }
                    else
                    {
                        camera.transform.position = _player1SpawnTransform.position;
                        camera.transform.rotation = _player1SpawnTransform.rotation;

                        go.transform.position = _player1SpawnTransform.position;
                        go.transform.rotation = _player1SpawnTransform.rotation;

                        YourID = 0;
                    }

                    InputTracking.Recenter();

                    Debug.Log(string.Format("[NetworkManager] Your are the player {0}", YourID+1));
                }
                else
                {
                    Debug.Log("[NetworkManager] OnEvent(). Instantiate remote avatar...");

                    go = Instantiate(Resources.Load( RESOURCE_FOLDER_OVR_AVATARS + RESOURCE_NAME_REMOTE_AVATAR)) as GameObject;

                    if (PhotonNetwork.player.IsMasterClient)
                    {
                        go.transform.position = _player2SpawnTransform.position;
                        go.transform.rotation = _player2SpawnTransform.rotation;
                    }
                    else
                    {
                        go.transform.position = _player1SpawnTransform.position;
                        go.transform.rotation = _player1SpawnTransform.rotation;
                    }
                }

                if (go != null)
                {
                    PhotonView pView = go.GetComponent<PhotonView>();

                    if (pView != null)
                    {
                        pView.viewID = (int)content;
                    }
                }

                if (PhotonNetwork.room.PlayerCount == 2)
                {
                    GameBehaviour.Instance.StartCountDown();
                }
            }
        }

        /// <summary>
        /// TryConnectToRoom
        /// </summary>
        private void OnJoinedLobby()
        {
            Debug.Log("[NetworkManager] OnJoinedLobby");
            ConnectToPublicRoom();
        }

        /// <summary>
        /// OnConnectedToPhoton
        /// Photon Callback
        /// </summary>
        private void OnConnectedToPhoton()
        {
            Debug.Log("[NetworkManager] OnConnectedToPhoton");

            //OnNetworkConnected.Invoke();
        }

        /// <summary>
        /// OnConnectedToPhoton
        /// Photon Callback
        /// </summary>
        private void OnFailedToConnectToPhoton()
        {
            Debug.Log("[NetworkManager] OnFailedToConnectToPhoton");

            GameObject go = Instantiate(Resources.Load(RESOURCE_FOLDER_OVR_AVATARS + RESOURCE_NAME_LOCAL_AVATAR)) as GameObject;
            Vector3 pos = new Vector3(0f, Application.platform == RuntimePlatform.Android ? -1.70f : 0.0f, 0f);
            go.transform.position = pos;
        }

        /// <summary>
        /// OnPhotonPlayerConnected
        /// </summary>
        /// <param name="player"></param>
        private void OnPhotonPlayerConnected(PhotonPlayer player)
        {
            Debug.Log("[NetworkManager] OnPhotonPlayerConnected " + player.name);

            OnNetworkPlayerConnected.Invoke(player);
        }

        ///// <summary>
        ///// OnDisconnectedFromPhoton
        ///// Photon Callback
        ///// </summary>
        //private void OnDisconnectedFromPhoton()
        //{
        //    Debug.Log("[NetworkManager] Disconnected from photon...");

        //    OnNetworkDisconnected.Invoke();
        //}

        ///// <summary>
        ///// OnPhotonRandomJoinFailed
        ///// </summary>
        private void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
           Debug.Log("[NetworkManager] OnPhotonRandomJoinFailed code :" + codeAndMsg[0] + " : " + codeAndMsg[1]);
            CreatePublicRoom();
        }

        ///// <summary>
        ///// OnPhotonCreateRoomFailed
        ///// </summary>
        ///// <param name="codeAndMsg"></param>
        //private void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        //{
        //    Debug.Log("[NetworkManager] OnPhotonCreateRoomFailed " + codeAndMsg[0] + " : " + codeAndMsg[1]);
        //    //ConnectToPublicRoom();
        //}

        ///// <summary>
        ///// OnPhotonJoinRoomFailed
        ///// </summary>
        ///// <param name="codeAndMsg"></param>
        private void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            Debug.Log("[NetworkManager] OnPhotonJoinRoomFailed " + codeAndMsg[0] + " : " + codeAndMsg[1]);
            CreatePublicRoom();

        }

        ///// <summary>
        ///// OnPhotonPlayerConnected
        ///// </summary>
        ///// <param name="player"></param>
        //private void OnPhotonPlayerConnected(PhotonPlayer player)
        //{
        //    Debug.Log("[NetworkManager] OnPhotonPlayerConnected " + player.name);

        //    OnNetworkPlayerConnected.Invoke(player);
        //}

        ///// <summary>
        ///// OnPhotonPlayerDisconnected
        ///// </summary>
        ///// <param name="player"></param>
        //private void OnPhotonPlayerDisconnected(PhotonPlayer player)
        //{
        //    Debug.Log("[NetworkManager] OnPhotonPlayerDisconnected " + player.name);

        //    OnNetworkPlayerDisconnected.Invoke(player);
        //}

        /// <summary>
        /// Photon automatically switches the master if the old one decides to leave his room. 
        /// This method is called for every photonPlayer in this room.
        /// </summary>
        /// <remarks>The former MasterClient is still in the player list when this method get called.</remarks>
        /// <param name="newMasterClient"></param>
        private void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            //Debug.Log("[NetworkManager] OnMasterClientSwitched new master is " + newMasterClient.name);

            //if (PhotonNetwork.room.visible)
            //{
            //    OnNetworkMasterClientSwitched.Invoke(newMasterClient);
            //}
            //else
            //{
            //    Debug.Log("[NetworkManager] OnMasterClientSwitched leave room " + newMasterClient.name);

            //    LeaveRoom();
            //}
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Connect to NetworkController public room
        /// </summary>
        private void ConnectToPublicRoom()
        {
            // Try to join room 
            //
            PhotonNetwork.JoinRandomRoom();
        }

        private void CreatePublicRoom()
        {
            RoomOptions otps = new RoomOptions()
            {
                IsOpen = true,
                IsVisible = true,
                MaxPlayers = MAX_PLAYER_PER_ROOM
            };

            Guid guid = new Guid();
            PhotonNetwork.CreateRoom(guid.ToString(), otps, TypedLobby.Default);
        }
        #endregion
    }
}