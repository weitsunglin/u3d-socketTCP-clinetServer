using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
public class server : MonoBehaviour{
    Socket serverSocket; 
    Socket clientSocket; 
    IPEndPoint ipEnd;
    Thread connectThread;
    string receiveString; 
    string sendString; 
    byte[] receivePacket = new byte[ 1024 ]; //接收的資料封包，必須為位元組
    byte[] sendPacket = new byte[ 1024 ]; //傳送的資料封包，必須為位元組  
    int receivePacketLength ;
    public Queue< string > messageQueue = new Queue<string>(); //接受訊息柱列
    public bool isReceive;
    public InputField inputField;
    public Text InputConcept; //輸入文字UI
    public Text ChatConcept; //聊天文字UI
    void InitConnect() {        
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//定義套接字型別,在主執行緒中定義
        ipEnd = new IPEndPoint(IPAddress.Any, 7000);
        serverSocket.Bind(ipEnd);//連線
        serverSocket.Listen(10);//開始偵聽,最大10個連線
        connectThread = new Thread(new ThreadStart(ReceiveFromClient));//開啟一個執行緒來處理連線，如果不用在主執行緒執行，行程會卡住，應該是卡在while
        connectThread.Start();
    }
    void CloceConnect(){
        //先關閉客戶端
        if (clientSocket != null){
            clientSocket.Close();
        }
        //再關閉執行緒
        if (connectThread != null){
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最後關閉伺服器
        serverSocket.Close();
    }
    void ReceiveFromClient() {
        void Connecting() {
            if (clientSocket != null)
                clientSocket.Close();
            clientSocket = serverSocket.Accept();//一旦接受連線，建立一個客戶端
            IPEndPoint ipEndClient = (IPEndPoint)clientSocket.RemoteEndPoint;
            print("Connect with " + ipEndClient.Address.ToString() + ":" + ipEndClient.Port.ToString());//輸出客戶端的IP和埠，看是哪個客戶端傳來訊息
            sendString = "Welcome to my server";
            SendPacketToClient(sendString);//連線成功則傳送資料
        }
        Connecting();//連線
        while (true){
            receivePacket = new byte[ 1024 ] ;
            receivePacketLength  = clientSocket.Receive(receivePacket);//接收資料
            //如果收到的資料長度為0，則重連並進入下一個迴圈
            if (receivePacketLength  == 0){
                Connecting();
                continue;
            }
            else{        
                receiveString = "Client: " + Encoding.ASCII.GetString(receivePacket, 0, receivePacketLength );
                messageQueue.Enqueue(receiveString);  /*queue解決unity函數庫跟thread有衝突的問題*/
                isReceive = true;
            }
        }
    }
    void SendPacketToClient(string sendString){
        sendPacket = new byte[1024]; //初始化sendPacket
        sendPacket = Encoding.ASCII.GetBytes(sendString);//資料型別轉換
        clientSocket.Send(sendPacket, sendPacket.Length, SocketFlags.None);//傳送
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
        SendPacketToClient(InputConcept.text);
        ChatConcept.text += "Server: " + InputConcept.text + "\n"; //發送訊息，送一份給自己的聊天的文字
        inputField.text = ""; //發送訊息，清空inputField
    }
}