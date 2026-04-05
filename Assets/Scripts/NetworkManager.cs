using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("1. Photon Sunucularýna baðlanýlýyor...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("2. Master Sunucuya Baðlanýldý! Lobiye giriliyor...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("3. Lobiye Girildi! Boþ bir masa (Oda) aranýyor...");

        // Yenilik: Lobide durma, rastgele boþ bir odaya girmeyi dene!
        PhotonNetwork.JoinRandomRoom();
    }

    // EÐER boþ bir oda bulamazsa (yani oyunu ilk sen açtýysan) Photon bu fonksiyonu tetikler:
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("4. Boþ oda yokmuþ! Kendi masamýzý (Odamýzý) kuruyoruz...");

        RoomOptions odaAyarlari = new RoomOptions();
        odaAyarlari.MaxPlayers = 2; // Masa sadece 2 kiþilik! 3. kiþi giremez.

        // "HanýnArkaOdasý" adýnda bir oda kuruyoruz.
        PhotonNetwork.CreateRoom("HaninArkaOdasi", odaAyarlari);
    }

    // Bir odaya baþarýyla girdiðinde VEYA yeni odayý baþarýyla kurduðunda bu fonksiyon tetikler:
    public override void OnJoinedRoom()
    {
        Debug.Log("5. ODAYA GÝRÝLDÝ! Masa hazýr.");
        Debug.Log("Odada þu an " + PhotonNetwork.CurrentRoom.PlayerCount + " kiþi var.");
    }
}