using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

public class NetworkManager : MonoBehaviour {
	private static NetworkManager instance;
	public static NetworkManager GetInstance() {
		GameObject main = GameObject.Find("Main");
		if (main == null) {
			main = new GameObject("Main");
			DontDestroyOnLoad(main);
		}
	
		if (instance == null) {
			instance = main.AddComponent<NetworkManager>();
		}
		return instance;
	}

        private SocketClient socket;
        static readonly object m_lockObject = new object();
        static Queue<KeyValuePair<int, ByteBuffer>> mEvents = new Queue<KeyValuePair<int, ByteBuffer>>();

        SocketClient SocketClient {
            get { 
                if (socket == null)
                    socket = new SocketClient();
                return socket;                    
            }
        }

        void Awake() {
		Init();

		// �����ֽ�˳��(������ģʽ)
		Debug.LogFormat("LittleEndian {0}", BitConverter.IsLittleEndian);
        }

        void Init() {
            SocketClient.OnRegister();
        }

        public void OnInit() {
            CallMethod("Start");
        }

        public void Unload() {
            CallMethod("Unload");
        }

        public object[] CallMethod(string func, params object[] args) {
            return Util.CallMethod("Network", func, args);
        }

        public static void AddEvent(int _event, ByteBuffer data) {
            lock (m_lockObject) {
                mEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(_event, data));
            }
        }

        /// <summary>
        /// ����Command�����ﲻ����ķ���˭��
        /// </summary>
        void Update() {
		if (mEvents.Count > 0) {
			while (mEvents.Count > 0) {
				KeyValuePair<int, ByteBuffer> _event = mEvents.Dequeue();
				if (_event.Key >= 0) {
					CallMethod("OnMessage", _event.Key, _event.Value);
				} else {
					// ������Ϣ
					if (_event.Key == Protocol.Connect) {
						CallMethod("OnConnect");
					} else if (_event.Key == Protocol.Exception) {
						CallMethod("OnException");
					}else if (_event.Key == Protocol.Disconnect) {
						CallMethod("OnDisconnect");
					}
				}
			}
		}
        }

        /// <summary>
        /// ������������
        /// </summary>
        public void SendConnect() {
            SocketClient.SendConnect();
        }

        /// <summary>
        /// ����SOCKET��Ϣ
        /// </summary>
        public void SendMessage(ByteBuffer buffer) {
            SocketClient.SendMessage(buffer);
        }

        /// <summary>
        /// ��������
        /// </summary>
	public void OnDestroy() {
            SocketClient.OnRemove();
            Debug.Log("~NetworkManager was destroy");
        }
}