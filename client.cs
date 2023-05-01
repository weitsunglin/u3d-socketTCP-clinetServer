using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
public class client : MonoBehaviour{
    Socket serverSocket;
    IPAddress ip;
    IPEndPoint ipEnd;
    Thread connectThread;
    string receiveString; 
    string sendString; 
    byte[] receivePacket = new byte[1024]; //接收的資料封包，必須為位元組
    byte[] sendPacket = new byte[1024]; //傳送的資料封包，必須為位元組
    int receivePacketLength;
    public InputField inputField;
    public Text InputFieldConcept; //輸入文字UI
    public Text ChatConcept; //聊天文字UI
    public Queue<string> messageQueue = new Queue<string>(); //接受訊息柱列
    public bool isReceive;
    void InitConnect(){
        ip = IPAddress.Parse("127.0.0.1");
        ipEnd = new IPEndPoint(ip, 7000); //(127.0.0.1，7000)
        connectThread = new Thread(new ThreadStart(ReceiveFromServer));//開啟一個執行緒來處理連線，如果不用在主執行緒執行，行程會卡住，應該是卡在while
        connectThread.Start();
    }
    void CloceConnect(){
        //關閉執行緒
        if (connectThread != null){
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最後關閉伺服器
        if (serverSocket != null)
            serverSocket.Close();
        print("diconnect");
    }
    void ReceiveFromServer(){
        void Connecting(){
        if (serverSocket != null){
            serverSocket.Close();
        }
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Connect(ipEnd); //連線
            //初始化receivePacketLength、receiveString
            receivePacketLength = serverSocket.Receive(receivePacket); 
            receiveString = Encoding.ASCII.GetString(receivePacket, 0, receivePacketLength);
        }
        Connecting();
        while (true){
            receivePacket = new byte[1024];
            receivePacketLength = serverSocket.Receive(receivePacket);
            if (receivePacketLength == 0){
                Connecting();
                continue; //跳過當前循環體中的當次迴圈(while)，進入下一次迴圈
            }
            else{
                receiveString = "Server: " + Encoding.ASCII.GetString(receivePacket, 0, receivePacketLength);
                messageQueue.Enqueue(receiveString);
                isReceive = true;
            }
        }
    }
    void SendPacketToServer(string sendString){
        //清空封包陣列
        sendPacket = new byte[1024];
        sendPacket = Encoding.ASCII.GetBytes(sendString);
        serverSocket.Send(sendPacket, sendPacket.Length, SocketFlags.None);
    }

    void Start(){
        InitConnect();
    }
    void Update(){
        if (isReceive == true){
            ChatConcept.text += messageQueue.Dequeue() + "\n";
            isReceive = false;
        }
    }
    void OnDestroy(){
        CloceConnect()
    }
    //發送訊息按鈕函數
    public void SendMessage() {
        SendPacketToServer(InputFieldConcept.text);
        ChatConcept.text += "Client: " + InputFieldConcept.text + "\n"; //發送一份給自己的聊天室
        inputField.text = ""; //發送訊息，清空inputField
    }
}