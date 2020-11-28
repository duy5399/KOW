using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
using System.Text;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

public class MyPlayer : MonoBehaviourPun, IPunObservable
{
    public PhotonView pv;   //đối tượng trên mạng

    public AudioSource[] musicSource;

    public float moveSpeed = 1.5f;  //tốc độ di chuyển

    private Vector3 smoothMove;     //sẽ dùng gán giá trị vị trí di chuyển của người chơi khác
    private Quaternion smoothMoveRotation;  //dùng để gán vị trí xoay mặt của người chơi khác

    private GameObject sceneCamera; //Main Camera
    public GameObject playerCamera; //Player Camera

    public Text nameText;           //gán tên của nhân vật vào UI Text này

    public float offset;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;   //vị trí Bullet xuát hiện

    public int ammo = 20;           //số đạn của nhân vật
    public float timeReload = 2f;   //thời gian thay đạn

    public Text ammoText;           //gán số đạn của nhân vật vào UI Text này

    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public Image blood;             //máu của nhân vật

    public float timeRespawn = 2f;  //thời gian hổi sinh

    public GameObject alertScreen;  //bảng thông báo chết và hồi sinh

    public int playerCount;         //số người chơi có trong phòng
    public GameObject scoreBoard;   //bảng điểm

    public GameObject menu;

    public GameObject winnerScreen;
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)  //PhotonView: đối tượng trên mạng (xác định bằng viewID)
        {
            PhotonNetwork.LocalPlayer.SetScore(0);      //khởi tạo điểm nhân vật bằng 0
            nameText.text = PhotonNetwork.NickName;     //hiện tên Player của mình

            alertScreen = GameObject.Find("Canvas").transform.Find("AlertDeath").gameObject;    //tìm UI AlertDeath gán vào alertScreen

            scoreBoard = GameObject.Find("Canvas").transform.Find("ScoreBoard").gameObject;     //tìm UI ScoreBoard gán vào scoreBoard

            menu = GameObject.Find("Canvas").transform.Find("Menu").gameObject;                 //tìm UI Menu gán vào menu

            //winnerScreen = GameObject.Find("Canvas").transform.Find("AlertWinner").gameObject;

            //Camera tập trung vào Player
            playerCamera = GameObject.Find("Main Camera");   //tìm đối tượng Main Camera

            sceneCamera.SetActive(false);   //tắt Main Camera   
            playerCamera.SetActive(true);   //bật Player Camera
        }
        else
        {
            nameText.text = pv.Owner.NickName;  //pv: 1 đối tượng trên mạng qua PhontonView - bất cứ ai là chủ sở hữu pv thì sẽ lấy nametext của pv đó
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)  //PhotonView: đối tượng trên mạng (xác định bằng viewID)
        {
            ProcessInputs();    //hàm di chuyển nhân vật
            Shoot();            //hàm dùng để bắn
            Death_and_respawn();    //hàm chết và hồi sinh
            Show_score();       //hàm để xem bảng điểm
            Show_Menu();
            Victory();
            //pv.RPC("Defeat", RpcTarget.OthersBuffered);
        }
        else
        {
            smoothMovement();   //nếu không phải người chơi cục bộ - PC1 có Player Y và Duy không điều khiển Player Y mà là một người khác (Hoài)
                                //=>vị trí Player Y sẽ di chuyển từ một người khác (Hoài điều khiển từ PC2)
        }
    }    

    private void smoothMovement()   //hàm cập nhập vị trí của người chơi khác khi smoothMove và smoothMoveRotation  được update vị trí mới từ hàm OnPhotonSerializeView();
    {
        //transform: biến đổi, thay đổi
        //position: vị trí
        //rotation: xoay
        transform.position = Vector3.Lerp(transform.position, smoothMove, Time.deltaTime * 10);
        transform.rotation = Quaternion.Lerp(transform.rotation, smoothMoveRotation, Time.deltaTime * 10);  //Trong Unity, Quaternion được sử dụng để biểu diễn phép quay của mọi đối tượng
    }

    private void ProcessInputs()    //hàm điều khiển nhân vật di chuyển
    {
        var moveHorizontal = new Vector3(Input.GetAxisRaw("Horizontal"), 0);    //di chuyển theo chiều ngang //GetAxisRaw: trả về -1 0 1
        transform.position += moveHorizontal * moveSpeed * Time.deltaTime;      //Time.delta là khoảng thời gian giữa 2 frame
        var moveVertical = new Vector3(0, Input.GetAxisRaw("Vertical"));        //di chuyển theo chiều dọc
        transform.position += moveVertical * moveSpeed * Time.deltaTime;

        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;  //tính vị trí giữa con trỏ và nhân vật
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;  //Mathf.Atan2: trả về số đo góc được tính bằng đơn vị radians sao cho tan của góc đó chính bằng thương số của hai tham số truyền vào
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ + offset);  //nếu không thêm offset thì nhân vật xoay mặt theo trục x

        
    }

    public void Shoot()
    {

        if (ammo > 0)
        {
            if (Input.GetMouseButtonDown(0))    //click chuột trái thì gọi hàm khởi tạo Bullet
            {
                GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, bulletSpawn.position, bulletSpawn.rotation);   //Instantiate: khởi tạo Bullet
                musicSource[0].Play();
                ammo -= 1;
            }
            ammoText.text = "Ammo " + ammo + "/20";           
        }
        else
        {
            timeReload -= Time.deltaTime;
            ammoText.text = "Reloading " + Math.Round(timeReload, 1);   //lấy 1 số thập phân
            if (timeReload < 0)
            {
                ammo = 20;              //đặt lại đạn sau khi thay
                timeReload = 2f;        //đặt lại thời gian thay đạn
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)  //va chạm với đạn của người chơi khác
    {
        if (collision.gameObject.name == "Bullet(Clone)")           //đối tượng va chạm là đạn
        {
            Destroy(collision.gameObject);          //phá hủy đạn
            if (photonView.IsMine)
            {
                musicSource[1].Play();
                pv.RPC("TakeDamage", RpcTarget.AllBuffered);    //gọi hàm trừ máu nhân vật //gọi hàm từ xa cho các máy khác (others)
                //Gửi RPC cho những người khác và thực hiện nó ngay lập tức trên máy khách này.Người chơi mới nhận được RPC khi họ tham gia khi được đệm(cho đến khi khách hàng này rời đi).
                if(blood.fillAmount <= 0f)
                {
                    Add_Score(collision.gameObject);
                }
            }
        }
    }

    [PunRPC]    //Remote Procedure Calls - gọi các thủ tục từ xa
    public void TakeDamage()                        //hàm trừ máu 
    {
        currentHealth -= 10f;
        blood.fillAmount = currentHealth/maxHealth;    //khi nhân vận tốc với Time.delta thì object di chuyển ko phụ thuộc vào FPS của game
    }

    public void Add_Score(GameObject a)
    {
        a.gameObject.GetPhotonView().Owner.AddScore(1); //chủ sở hữu của viên đạn được cộng điểm
    }

    public void Death_and_respawn()     //hàm để giết(xóa nhân vật) và hồi sinh
    {
        if (blood.fillAmount <= 0f)      //khi máu về 0
        {
            alertScreen.SetActive(true);    //bật thông báo chết và đang hồi sinh
            alertScreen.transform.Find("Text").GetComponent<Text>().text ="YOU DIED\n" +"Respawning " + Math.Round(timeRespawn, 1) +" seconds";
            gameObject.transform.position = new Vector3(0, 500, 0);     //đưa nhân vật ra khỏi màn hình không thể nhìn
            timeRespawn -= Time.deltaTime;
            if (timeRespawn < 0)        //khi thời gian hồi sinh hết
            {
                alertScreen.SetActive(false);   //tắt thông báo chết và đang hồi sinh
                ammo = 20;                      //đặt lại đạn cho nhân vật
                Vector3 Position = new Vector3(UnityEngine.Random.Range(-12.71f, 12.63f), UnityEngine.Random.Range(-6.48f, 6.39f), 0);  //đưa nhân vật vào lại màn hình (vị trí ngẫu nhiên)
                gameObject.transform.position = Position;
                timeRespawn = 2f;               //đặt lại thời gian hồi sinh 
                pv.RPC("Rs_blood", RpcTarget.AllBuffered);               
            }
        }
    }


    [PunRPC]
    public void Rs_blood()  //hàm đặt lại máu của nhân vật
    {
        currentHealth = 100f;
        blood.fillAmount = 1f;
    }

    public void Show_score()    //hàm để xem bảng điểm
    {
        UpdateScoreBoard();     //gọi hàm cập nhật bảng điểm
        if (Input.GetKeyDown(KeyCode.Tab))  //nhấn giữ tab hiện bảng điểm
        {
            scoreBoard.SetActive(true);
        }
        else if(Input.GetKeyUp(KeyCode.Tab))     //thả tab ẩn bảng điểm
        {
            scoreBoard.SetActive(false);
        }
    }
    
    public void UpdateScoreBoard()      //hàm cập nhật bảng điểm
    {
        playerCount = PhotonNetwork.PlayerList.Length;      //đếm số lượng người chơi trong phòng
        
        var playerList = new StringBuilder();   //StringBuilder là kiểu đối tượng động, cho phép bạn mở rộng số lượng kí tự của một chuỗi. Khi bạn thay đổi nội dung chuỗi, nó không tạo một đối tượng mới trong bộ nhớ giống kiểu String, mà nó mở rộng bộ nhớ để lưu trữ giá trị chuỗi mới thay thế.
        //Đặc điểm của StringBuilder là:
        //Cho phép thao tác trực tiếp trên chuỗi ban đầu.
        //Có khả năng tự mở rộng vùng nhớ khi cần thiết.
        foreach (var player in PhotonNetwork.PlayerList)    //lấy từng người chơi trong phòng
        {
            //append: chắp thêm
            playerList.Append("Name: " + player.NickName + "\tScore: "  + player.GetScore() + "\n");    //lấy tên và điểm số của người chơi gán vào playerList (StringBuilder)
        }
        scoreBoard.transform.Find("Text").GetComponent<Text>().text = "Player Online: " + playerCount.ToString() +"\n" + playerList.ToString();   //tìm Text và in số nhân vật đếm được
    }

    public void Show_Menu()     //hàm hiện Menu (rời khỏi phòng - thoát game)
    {
        if (Input.GetKeyDown(KeyCode.Escape))   //nhấn giữ Escape hiện Menu
        {
            menu.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Escape))    //thả Escape ẩn Menu
        {
            menu.SetActive(false);
        }
    }

    public void Victory()       //hàm kiểm tra điểm số (người chiến thắng)
    {
        if(photonView.Owner.GetScore() >= 10)    //nếu điểm số đạt số điểm yêu cầu
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;       //khóa phòng chơi
            PhotonNetwork.CurrentRoom.IsVisible = false;    //ẩn phòng chơi   
            PhotonNetwork.LoadLevel(3);                     //hiện màn hình giành cho người chiến thắng
            Destroy(gameObject);
            pv.RPC("Defeat", RpcTarget.OthersBuffered);     //gọi hàm cho những người còn lại (người thất bại)
        }
    }

    [PunRPC]
    public void Defeat()        //hàm hiện màn hình cho người thất bại
    {
        PhotonNetwork.LoadLevel(4);     //hiện màn hình cho người thất bại
        Destroy(gameObject); 
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)  //hàm dùng để gửi vị trí của mình khi di chuyển đến những người chơi khác và nhận vị trí của những người chơi khác khi họ di chuyển nhân vật
    {                                                                               //PhotonStream stream: dữ liệu chúng ta gửi
        if (stream.IsWriting)       //ta gửi (send) dữ liệu - vị trí đi là đang writing
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else if (stream.IsReading)  //ta nhận (Receive) dữ liệu - vị trí của người khác là đang reading
        {
            smoothMove = (Vector3)stream.ReceiveNext(); //cập nhập mỗi lần và hàm smoothMovement(); sẽ hiện vị trí Player Y (của Hoài điều khiển) lên PC1 của Duy (Duy điều khiển Player X)
            smoothMoveRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
