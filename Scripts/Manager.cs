using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Manager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject alertQuit;
    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()  //tạo - sản sinh nhân vật khi vào game 
    {
        Vector3 position = new Vector3(Random.Range(-12.71f, 12.63f), Random.Range(-6.48f, 6.39f), 0);
        PhotonNetwork.Instantiate(playerPrefab.name, position, playerPrefab.transform.rotation);    //Instantiate: khởi tạo
    }

    public void OnClick_LeaveBtn()  //nút Leave
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(1);
    }

    public void OnClick_ExitBtn()   //nút Quit Game
    {
        alertQuit.SetActive(true);
    }

    public void OnClick_NoBtn()     //nút No
    {
        alertQuit.SetActive(false);
    }

    public void OnClick_YesBtn()    //nút Yes
    {
        Application.Quit();         //đóng chương trình
    }

    public void OnClick_CountinueBtn()  //nút Continue
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(1);
    }

}
