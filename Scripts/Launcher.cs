using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Text;

public class Launcher : MonoBehaviourPunCallbacks
{
    public GameObject roomListScreen;
    // Start is called before the first frame update
    void Start()
    {
        roomListScreen = GameObject.Find("Canvas").transform.Find("ConnectedSceen").transform.Find("RoomList").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject loadingScreen;
    public GameObject connectedScreen;
    public GameObject disconnectedScreen;

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default); //khi kết nối thành công tới Photon Server (Master) thì sẽ kết nối vào Lobby (sảnh chờ) 
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        loadingScreen.SetActive(false);
        disconnectedScreen.SetActive(true); //không kết nỗi bởi lí do gì thì hiện màn hình disconnect
    }

    public override void OnJoinedLobby() //hàm được gọi khi dòng PhotonNetwork.JoinLobby(TypedLobby.Default); thực hiện thành công
    {
        if(loadingScreen.activeSelf)        //activeSelf: đang kích hoạt
            loadingScreen.SetActive(false);
        if (disconnectedScreen.activeSelf)  
            disconnectedScreen.SetActive(false);
        
        connectedScreen.SetActive(true);    //màn hình chính Lobby sẽ có Create Room và Join Room
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        var list = new StringBuilder();
        base.OnRoomListUpdate(roomList);
        foreach(var room in roomList)
        {
            list.Append("Name: " + room.Name + "\t\t" + room.PlayerCount + "/" + room.MaxPlayers + " player");
        }
        roomListScreen.transform.Find("Text").GetComponent<Text>().text = "Room List" + "\n" + list.ToString() + "\n";
    }

}
