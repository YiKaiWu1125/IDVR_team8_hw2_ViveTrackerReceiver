using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Text.RegularExpressions;


public class TransformReceiver : MonoBehaviour
{
    [Header("Step4: Check and match your Server SendPort.")]
    public int listenPort = 12345; 

    private UdpClient udpClient;
    private IPEndPoint senderEndPoint;
    [Header("Step3-2: (Must) Put Your all Trackers in the List.")]
    public GameObject[] viveTrackerTarget; // add your tracker here
    [Header("Step3-3: (Must) Set Your all Trackers Serials number.")]
    public int[] trackerSerial;

    void Start()
    {
        for(int i = 0; i < trackerSerial.Length; i++){
            if(trackerSerial[i] >= viveTrackerTarget.Length){
                Debug.LogError("Wrong trackerSerial has been set, please check again");
            }
        }
        udpClient = new UdpClient(listenPort);
        senderEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

        // udp receive data start
        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);

    }

    void Update()
    {

    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        // Parse received data, Data format{1stposition.x, 1stposition.y, 1stposition.z, 1strotation.x, 1strotation.y, 1strotation.z, 1strotation.w : 2stposition.x .....}
        byte[] receivedBytes = udpClient.EndReceive(ar, ref senderEndPoint);
        string receivedData = Encoding.ASCII.GetString(receivedBytes);
        
        string[] trackerPositionAndRotation = receivedData.Split(':');
        int numOfTracker = trackerPositionAndRotation.Length;  // How many tracker
        int numTransform = 7; // 3 Position + 4 Quaternion
        string [,] eachTrackerPositionAndRotation = new string[numOfTracker, numTransform]; // Tracker num * 7 (3 Position + 4 Quaternion)

        for(int trackerIdx = 0; trackerIdx < numOfTracker; trackerIdx++){
            string[] singleTrackerPositionAndRotation = trackerPositionAndRotation[trackerIdx].Split(',');
            for(int transformIdx = 0; transformIdx < numTransform; transformIdx++){
                eachTrackerPositionAndRotation[trackerIdx, transformIdx] = singleTrackerPositionAndRotation[transformIdx];
            }
        }
        // Set Target transform value
        for(int i = 0; i < viveTrackerTarget.Length; i++){
            Vector3 targetPosition = new Vector3(float.Parse(eachTrackerPositionAndRotation[trackerSerial[i], 0]), float.Parse(eachTrackerPositionAndRotation[trackerSerial[i], 1]), float.Parse(eachTrackerPositionAndRotation[trackerSerial[i], 2]));
            Quaternion targetRotation = new Quaternion(float.Parse(eachTrackerPositionAndRotation[trackerSerial[i], 3]), float.Parse(eachTrackerPositionAndRotation[trackerSerial[i], 4]), float.Parse(eachTrackerPositionAndRotation[trackerSerial[i], 5]), float.Parse(eachTrackerPositionAndRotation[trackerSerial[i], 6]));
            UpdateObjectTransform(viveTrackerTarget[i], targetPosition, targetRotation);
        }

        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    private void OnDisable()
    {
        udpClient.Close();
    }

    private void UpdateObjectTransform(GameObject target, Vector3 updatePosition, Quaternion updateRotation){
        UnityMainThreadDispatcher.Instance().Enqueue(() =>{
            target.transform.localPosition = updatePosition;
            target.transform.localRotation = updateRotation;
        });
    }
}