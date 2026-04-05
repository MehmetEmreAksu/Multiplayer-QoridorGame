using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;



public class BoardManager : MonoBehaviour
{
    [Header("Kamera Ayarlarý")]
    [SerializeField] private Transform mainCameraTransform; // Inspector'dan Main Camera'yý buraya sürükle

    // Kameranýn normal duruþu (Senin ayarladýðýn mevcut açýlar, bunlarý kendine göre deðiþtir)
    private Vector3 normalCamPos = new Vector3(0f, 12f, -8f);
    private Quaternion normalCamRot = Quaternion.Euler(60f, 0f, 0f);

    // TAB'a basýnca geçeceði Kuþ Bakýþý (Taktiksel) duruþ
    private Vector3 tacticalCamPos = new Vector3(0f, 18f, 0f); // Tam tepede
    private Quaternion tacticalCamRot = Quaternion.Euler(89.9f, 0f, 0f); // Tam aþaðý bakýyor



    // 0: Boþ, 1: Oyuncu 1, 2: Oyuncu 2
    private int[,] board = new int[9,9];
    int nextPlayer = 1; // Baþlangýçta oyuncu 1 baþlar
    [SerializeField] GameObject[] players = new GameObject[3];
    private Vector2Int[] playerPositions = new Vector2Int[3]; // Oyuncularýn konumlarýný tutar
    bool isMoving = false;


    [SerializeField] private GameObject p1Glow;
    [SerializeField] private GameObject p2Glow;

    // 0: Duvar yok, 1: Yatay Duvar, 2: Dikey Duvar
    private int[,] yatayDuvarlar = new int[8, 8];
    private int[,] dikeyDuvarlar = new int[8, 8];
    [SerializeField] private GameObject wallSlotPrefab;
    [SerializeField] private Transform wallSlotContainer;

    //oyunda taþlarý direk dizmek için kullanýlacak yer.
    [SerializeField] private GameObject WallPrefab;
    [SerializeField] private Transform WallDeck1;
    [SerializeField] private Transform WallDeck2;

    [SerializeField] private GameObject HologramWall;
    [SerializeField] private float wallPlacementOffset = 0.9f;
    bool isVertical = false; // Hologram Duvarýn dikey mi yoksa yatay mý olduðunu takip eder

    [SerializeField] private UIManager uiManager;

    [SerializeField] private Transform cameraPivot;

    private List<GameObject> p1Deck = new List<GameObject>();
    private List<GameObject> p2Deck = new List<GameObject>();

    public enum GameMode { Move, PlaceWall }
    public GameMode currentMode = GameMode.Move;


    private void Start()
    {
        uiManager.UpdateWallCount(10,10); // Baþlangýçta her iki oyuncunun da 10 duvarý var
        uiManager.UpdateTurnText(nextPlayer); // Baþlangýçta Player 1'in sýrasý

        board[4,0] = 1; // Oyuncu 1'in baþlangýç konumu
        board[4,8] = 2; // Oyuncu 2'nin baþlangýç konumu
        playerPositions[1] = new Vector2Int(4, 0); // Oyuncu 1'in baþlangýç konumu
        playerPositions[2] = new Vector2Int(4, 8); // Oyuncu 2'nin baþlangýç konumu

        KavsaklariOlustur();
        CreateDeck();
    }

    void Update()
    {
        if (Keyboard.current.tabKey.isPressed)
        {
            mainCameraTransform.localPosition = Vector3.Lerp(mainCameraTransform.localPosition, tacticalCamPos, Time.deltaTime * 10f);
            mainCameraTransform.localRotation = Quaternion.Lerp(mainCameraTransform.localRotation, tacticalCamRot, 10f * Time.deltaTime);
        }
        else
        {
            mainCameraTransform.localPosition = Vector3.Lerp(mainCameraTransform.localPosition, normalCamPos, Time.deltaTime * 10f);
            mainCameraTransform.localRotation = Quaternion.Lerp(mainCameraTransform.localRotation, normalCamRot, 10f * Time.deltaTime);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray isin = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit; //the point where the raycast hits the collider
            if (Physics.Raycast(isin, out hit, 100f))
            {
                if (hit.transform.CompareTag("Walls1") && nextPlayer == 1)
                {
                    currentMode = GameMode.PlaceWall;
                    EnableCross(true);
                    return;

                }
                else if (hit.transform.CompareTag("Walls2") && nextPlayer == 2)
                {
                    currentMode = GameMode.PlaceWall;
                    EnableCross(true);
                    return;
                }

                if (currentMode == GameMode.Move)
                {
                    HandleMoveInput(hit);
                }
                else if (currentMode == GameMode.PlaceWall)
                {
                    HandleWallPlacement(hit);
                }
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && currentMode == GameMode.PlaceWall)
        {
            currentMode = GameMode.Move;
            EnableCross(false);
            HologramWall.SetActive(false);
        }
        if (currentMode == GameMode.PlaceWall)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray isin = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit; //the point where the raycast hits the collider
            if (Physics.Raycast(isin, out hit, 100f))
            {
                WallSlotInformation slotInfo = hit.transform.GetComponent<WallSlotInformation>();
                if (slotInfo != null)
                {
                    HologramWall.SetActive(true);
                    HologramWall.transform.position = hit.transform.position + new Vector3(0, wallPlacementOffset, 0); // Duvarýn biraz yukarýda durmasý için
                    HologramWall.transform.rotation = isVertical ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 90, 0);
                }
                else
                {
                    HologramWall.SetActive(false);
                }
            }
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                isVertical = !isVertical; // Yatay/dikey durumunu tersine çevir
            }
        }
    }

    void KavsaklariOlustur()
    {
        // 9x9 tahtanýn tam aralarýnda 8x8 = 64 tane kavþak noktasý olur
        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++) // Unity'deki ileri-geri ekseni Z olduðu için deðiþkeni z yaptýk
            {
                // Senin koordinatlarýna göre mühendislik formülü:
                // Baþlangýç noktasýndan (örn X: -6) yarým adým (0.75) ileri gidip, her turda 1.5 birim ekliyoruz.
                float gercekX = -6f + 0.75f + (x * 1.5f);
                float gercekZ = 0f + 0.75f + (z * 1.5f);

                // Y eksenini 0 veya tahtanýn üstüne denk gelecek þekilde ufak bir deðer (örn 0.1f) yapabilirsin
                Vector3 kavsakPozisyonu = new Vector3(gercekX, 0.1f, gercekZ);

                // Objeyi sahneye bas ve container'ýn içine at
                GameObject yeniKavsak = Instantiate(wallSlotPrefab, kavsakPozisyonu, Quaternion.identity, wallSlotContainer);
                yeniKavsak.name = $"WallSlot_{x}_{z}"; // Hiyerarþideki ismi temiz dursun

                // Matris koordinatlarýný objenin içindeki scripte kaydet
                WallSlotInformation slotInfo = yeniKavsak.GetComponent<WallSlotInformation>();
                if (slotInfo != null)
                {
                    slotInfo.x = x;
                    slotInfo.y = z; // Matristeki Y deðerimiz, Unity'nin Z'sine denk geliyor
                }

                // SENÝN MANTIÐIN: Üretildikleri an gizli/pasif olsunlar. 
                // Sadece duvara týklandýðýnda aktif edeceðiz.
                yeniKavsak.SetActive(false);
            }
        }
    }

    void CreateDeck()
    {

         // Duvarlarýn yere yatýk durmasý için

        for (int i = 0; i < 10; i++)
        {
            // 1. OYUNCU ÝÇÝN RASTGELELÝK
            float rastgeleX1 = Random.Range(-0.05f, 0.05f); // X ekseninde milimetrik kayma
            float rastgeleZ1 = Random.Range(-0.05f, 0.05f); // Z ekseninde milimetrik kayma
            float rastgeleAci1 = Random.Range(-5f, 5f);     // 5 derece saða/sola yamukluk
            Quaternion rotation1 = Quaternion.Euler(0, rastgeleAci1, 90);

            Vector3 offset = new Vector3(rastgeleX1, i * 0.1f, rastgeleZ1); // Duvarlarýn birbirine çok yapýþmamasý için küçük bir offset

            GameObject wall1 = Instantiate(WallPrefab, WallDeck1.position + offset, rotation1, WallDeck1);
            p1Deck.Add(wall1);

            float rastgeleX2 = Random.Range(-0.05f, 0.05f);
            float rastgeleZ2 = Random.Range(-0.05f, 0.05f);
            float rastgeleAci2 = Random.Range(-5f, 5f);

            Vector3 offset2 = new Vector3(rastgeleX2, i * 0.1f, rastgeleZ2);
            Quaternion rotation2 = Quaternion.Euler(0, rastgeleAci2, 90);

            GameObject duvar2 = Instantiate(WallPrefab, WallDeck2.position + offset2, rotation2, WallDeck2);
            p2Deck.Add(duvar2);
        }
    }


    void EnableCross(bool state)
    {
        foreach(Transform cross in wallSlotContainer)
        {
            cross.gameObject.SetActive(state);
        }
    }

    void HandleMoveInput(RaycastHit hit)
    {
        AreaInformation squareLocation = hit.transform.GetComponent<AreaInformation>();
        if (squareLocation != null)
        {
            MakeMove(squareLocation.x, squareLocation.y, hit.transform.position);
        }

    }

    void HandleWallPlacement(RaycastHit hit)
    {
        WallSlotInformation slotInfo = hit.transform.GetComponent<WallSlotInformation>();
        if (slotInfo != null)
        {
            if(CanWallBePlaced(slotInfo.x, slotInfo.y))
            {
                if (isVertical)
                {
                    dikeyDuvarlar[slotInfo.x, slotInfo.y] = 1; // Dikey duvar yerleþtirildi
                }
                else
                {
                    yatayDuvarlar[slotInfo.x, slotInfo.y] = 1; // Yatay duvar yerleþtirildi
                }
                bool pathForP1 = BreadthFirstSearch(playerPositions[1], 8); // Oyuncu 1'in hedefi y=8
                bool pathForP2 = BreadthFirstSearch(playerPositions[2], 0); // Oyuncu 2'nin hedefi y=0

                if (!pathForP1 || !pathForP2)
                {
                    // Eðer duvar yerleþtirildikten sonra herhangi bir oyuncunun hedefe giden yolu kapanýyorsa, duvarý geri al
                    if (isVertical)
                    {
                        dikeyDuvarlar[slotInfo.x, slotInfo.y] = 0; // Dikey duvarý geri al
                    }
                    else
                    {
                        yatayDuvarlar[slotInfo.x, slotInfo.y] = 0; // Yatay duvarý geri al
                    }
                    Debug.Log("Bu duvar yerleþtirilemez çünkü bir oyuncunun hedefe giden yolunu kapatýyor!");
                    return;
                }

                Vector3 holoPosition = HologramWall.transform.position;
                //holoPosition.y = holoPosition.y + wallPlacementOffset;
                Instantiate(WallPrefab, holoPosition, HologramWall.transform.rotation);

                if (nextPlayer == 1)
                {
                    Destroy(p1Deck[p1Deck.Count - 1]);
                    p1Deck.RemoveAt(p1Deck.Count - 1);
                    if(p1Deck.Count == 0)
                    {
                        WallDeck1.gameObject.SetActive(false); // Duvar destesi görünmez olur
                        Debug.Log("Oyuncu 1 duvarlarý bitti, hareket yapmaya devam!");
                    }
                }
                else
                {
                    Destroy(p2Deck[p2Deck.Count - 1]);
                    p2Deck.RemoveAt(p2Deck.Count - 1);
                    if(p2Deck.Count == 0)
                    {
                        WallDeck2.gameObject.SetActive(false); // Duvar destesi görünmez olur
                        Debug.Log("Oyuncu 2 duvarlarý bitti, hareket yapmaya devam!");
                    }
                }
                uiManager.UpdateWallCount(p1Deck.Count, p2Deck.Count);
                nextPlayer = (nextPlayer) == 1 ? 2 : 1; // Sýrayý deðiþtir
                StartCoroutine(SmoothRotateCamera(nextPlayer, 0.4f));
                uiManager.UpdateTurnText(nextPlayer); // UI'daki sýra bilgisini güncelle
                p1Glow.SetActive(nextPlayer == 1);
                p2Glow.SetActive(nextPlayer == 2);
                currentMode = GameMode.Move; // Modu hareket yapmaya çevir
                EnableCross(false); // Kavþak noktalarýný gizle

            }
            Debug.Log($"Duvar yerleþtirilecek: ({slotInfo.x}, {slotInfo.y})");
        }

    }

    bool CanWallBePlaced(int x, int y)
    {
        // Artý (+) Kesiþme Kontrolü (Ayný noktada zýt yönlü duvar olamaz)
        if (isVertical && yatayDuvarlar[x, y] == 1) return false;
        if (!isVertical && dikeyDuvarlar[x, y] == 1) return false;

        if (isVertical)
        {
            if (dikeyDuvarlar[x, y] == 1) return false; // Merkez dolu

            // DÝKEY duvar Y ekseninde uzar! Bu yüzden y-1 ve y+1'e bakýlýr.
            if (y > 0 && dikeyDuvarlar[x, y - 1] == 1) return false; // Alt uç tokuþuyor mu?
            if (y < 7 && dikeyDuvarlar[x, y + 1] == 1) return false; // Üst uç tokuþuyor mu?
        }
        else // Yatay
        {
            if (yatayDuvarlar[x, y] == 1) return false; // Merkez dolu

            // YATAY duvar X ekseninde uzar! Bu yüzden x-1 ve x+1'e bakýlýr.
            if (x > 0 && yatayDuvarlar[x - 1, y] == 1) return false; // Sol uç tokuþuyor mu?
            if (x < 7 && yatayDuvarlar[x + 1, y] == 1) return false; // Sað uç tokuþuyor mu?
        }

        return true; // Hiçbir engel yok, duvar yerleþtirilebilir
    }

    // Ýki kare arasýnda (yan yana veya alt alta) duvar olup olmadýðýný söyler
    bool AradaDuvarVarMi(int x1, int y1, int x2, int y2)
    {
        // Y ekseninde (Aþaðý/Yukarý) hareket ediyoruz, aradaki yatay duvara bakacaðýz
        if (x1 == x2)
        {
            int minY = Mathf.Min(y1, y2);
            if (x1 < 8 && yatayDuvarlar[x1, minY] == 1) return true;
            if (x1 > 0 && yatayDuvarlar[x1 - 1, minY] == 1) return true;
        }
        // X ekseninde (Saða/Sola) hareket ediyoruz, aradaki dikey duvara bakacaðýz
        else if (y1 == y2)
        {
            int minX = Mathf.Min(x1, x2);
            if (y1 < 8 && dikeyDuvarlar[minX, y1] == 1) return true;
            if (y1 > 0 && dikeyDuvarlar[minX, y1 - 1] == 1) return true;
        }
        return false; // Hiçbir engele takýlmadýk, yol açýk!
    }

    bool BreadthFirstSearch(Vector2Int startPosition, int target)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[9, 9];
        queue.Enqueue(startPosition);
        visited[startPosition.x, startPosition.y] = true;
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current.y == target) return true; // Hedefe ulaþýldý
            // Dört yönü kontrol et
            if(current.x > 0 && !visited[current.x - 1, current.y] && !AradaDuvarVarMi(current.x, current.y, current.x - 1, current.y))
            {
                visited[current.x - 1, current.y] = true;
                queue.Enqueue(new Vector2Int(current.x - 1, current.y));
            }
            if (current.x < 8 && !visited[current.x + 1, current.y] && !AradaDuvarVarMi(current.x, current.y, current.x + 1, current.y))
            {
                visited[current.x + 1, current.y] = true;
                queue.Enqueue(new Vector2Int(current.x + 1, current.y));
            }
            if (current.y > 0 && !visited[current.x, current.y - 1] && !AradaDuvarVarMi(current.x, current.y, current.x, current.y - 1))
            {
                visited[current.x, current.y - 1] = true;
                queue.Enqueue(new Vector2Int(current.x, current.y - 1));
            }
            if (current.y < 8 && !visited[current.x, current.y + 1] && !AradaDuvarVarMi(current.x, current.y, current.x, current.y + 1))
            {
                visited[current.x, current.y + 1] = true;
                queue.Enqueue(new Vector2Int(current.x, current.y + 1));
            }
        }
        return false;
    }

    bool GecerliHamleMi(int eskiX, int eskiY, int yeniX, int yeniY)
    {
        int farkX = Mathf.Abs(yeniX - eskiX);
        int farkY = Mathf.Abs(yeniY - eskiY);

        // 1. NORMAL ADIM (Sað, Sol, Ýleri, Geri 1 adým)
        if ((farkX == 1 && farkY == 0) || (farkX == 0 && farkY == 1))
        {
            // Arada duvar yoksa hamle geçerlidir
            if (!AradaDuvarVarMi(eskiX, eskiY, yeniX, yeniY)) return true;
            return false;
        }

        // 2. DÜZ ÜSTÜNDEN ATLAMA (2 birim zýplama)
        if ((farkX == 2 && farkY == 0) || (farkX == 0 && farkY == 2))
        {
            int ortaX = (eskiX + yeniX) / 2;
            int ortaY = (eskiY + yeniY) / 2;

            // Ortada rakip YOLSA zýplayamayýz
            if (board[ortaX, ortaY] == 0) return false;

            // Bizimle rakip arasýnda duvar VARSA zýplayamayýz
            if (AradaDuvarVarMi(eskiX, eskiY, ortaX, ortaY)) return false;

            // Rakiple düþeceðimiz yer arasýnda duvar VARSA zýplayamayýz
            if (AradaDuvarVarMi(ortaX, ortaY, yeniX, yeniY)) return false;

            return true; // Her yer açýksa zýpla!
        }

        // 3. ÇAPRAZ ATLAMA (Ýþte O Efsane Kural!)
        if (farkX == 1 && farkY == 1)
        {
            // Rakibin L þeklinde durabileceði 2 olasý "Köþe" (Pivot) noktasý var:
            int pivot1X = eskiX; int pivot1Y = yeniY; // Rakip önümüzde/arkamýzdaysa
            int pivot2X = yeniX; int pivot2Y = eskiY; // Rakip saðýmýzda/solumuzdaysa

            // --- PÝVOT 1 KONTROLÜ ---
            if (board[pivot1X, pivot1Y] != 0) // Orada rakip var mý?
            {
                if (!AradaDuvarVarMi(eskiX, eskiY, pivot1X, pivot1Y)) // Bizimle rakip arasý açýk mý?
                {
                    // Rakibin tam arkasý tahta sýnýrý mý veya duvarla kapalý mý?
                    int rakipArkaY = pivot1Y + (yeniY - eskiY); // Geldiðimiz yönün 1 ilerisi
                    bool arkaKapaliMi = (rakipArkaY < 0 || rakipArkaY > 8) || AradaDuvarVarMi(pivot1X, pivot1Y, pivot1X, rakipArkaY);

                    if (arkaKapaliMi) // Eðer rakibin arkasý duvar/sýnýr yüzünden týkalýysa çapraza izin ver!
                    {
                        if (!AradaDuvarVarMi(pivot1X, pivot1Y, yeniX, yeniY)) // Rakiple hedef çapraz arasý açýk mý?
                        {
                            return true;
                        }
                    }
                }
            }

            // --- PÝVOT 2 KONTROLÜ ---
            if (board[pivot2X, pivot2Y] != 0)
            {
                if (!AradaDuvarVarMi(eskiX, eskiY, pivot2X, pivot2Y))
                {
                    int rakipArkaX = pivot2X + (yeniX - eskiX);
                    bool arkaKapaliMi = (rakipArkaX < 0 || rakipArkaX > 8) || AradaDuvarVarMi(pivot2X, pivot2Y, rakipArkaX, pivot2Y);

                    if (arkaKapaliMi)
                    {
                        if (!AradaDuvarVarMi(pivot2X, pivot2Y, yeniX, yeniY))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        // Yukarýdaki þartlarýn hiçbirine uymuyorsa, hamle kesinlikle yasaktýr!
        return false;
    }

    bool CheckWinCondition()
    {
        // Oyuncu 1, y=8'e ulaþýrsa kazanýr
        if (playerPositions[1].y == 8)
        {
            
            Debug.Log("Oyuncu 1 Kazandý!");
            uiManager.ShowGameOver(1); // UIManager'ý kullanarak oyun sonu panelini göster
            Time.timeScale = 0f; // Oyunu durdur
            return true;
        }
        // Oyuncu 2, y=0'a ulaþýrsa kazanýr
        if (playerPositions[2].y == 0)
        {
            Debug.Log("Oyuncu 2 Kazandý!");
            uiManager.ShowGameOver(2); // UIManager'ý kullanarak oyun sonu panelini göster
            Time.timeScale = 0f; // Oyunu durdur
            return true;
        }
        return false; // Henüz kazanan yok
    }
    void MakeMove(int x,int y, Vector3 position)
    {

        GameObject currentPlayer = players[nextPlayer];

        if (board[x, y] != 0)
        {
            Debug.Log("Illegal Move");
        }

        else
        {
            position.y += 1f; // Oyuncunun biraz yukarýda durmasý için
            Vector2Int eskiKonum = playerPositions[nextPlayer];
            if (!isMoving && GecerliHamleMi(eskiKonum.x,eskiKonum.y,x,y))
            {
                isMoving = true;
                StartCoroutine(SmoothMove(currentPlayer, position));
                //eski konumu 0 yapma
                Vector2Int oldPosition = playerPositions[nextPlayer];
                board[oldPosition.x, oldPosition.y] = 0;
                //yeni konumu güncelleme
                board[x, y] = nextPlayer;
                //oyuncunun konumunu güncelleme
                playerPositions[nextPlayer] = new Vector2Int(x, y);

                //sýrayý deðiþtirme
                nextPlayer = (nextPlayer) == 1 ? 2 : 1; // Eðer nextPlayer 1 ise 2 yap, deðilse 1 yap
                uiManager.UpdateTurnText(nextPlayer); // UI'daki sýra bilgisini güncelle
                p1Glow.SetActive(nextPlayer == 1);
                p2Glow.SetActive(nextPlayer == 2);
                StartCoroutine(SmoothRotateCamera(nextPlayer, 0.5f));

            }
        }
        return;
    }

    // void yerine IEnumerator yazýyoruz
    IEnumerator SmoothMove(GameObject piyon, Vector3 hedefPozisyon)
    {
        // Hedefle aramýzdaki mesafe çok küçük olana kadar (0.01f) döngüyü çalýþtýr
        // Tam eþitlik (==) aramak 3D dünyada float deðerler yüzünden bazen buga sokar, mesafe ölçmek daha güvenlidir.
        while (Vector3.Distance(piyon.transform.position, hedefPozisyon) > 0.01f)
        {
            // Piyonu hedefe doðru, saniyede 5 birim hýzla kaydýr.
            // Time.deltaTime: Bu iþlemi FPS'den baðýmsýz, gerçek zamana göre (saniyeye) oranlar.
            piyon.transform.position = Vector3.MoveTowards(piyon.transform.position, hedefPozisyon, 5f * Time.deltaTime);

            // SÝHÝRLÝ SATIR: "Aga bu frame'lik bu kadar, sen ekraný çiz, sonraki frame devam ederiz."
            yield return null;
        }

        // Döngü bittiðinde (hedefe vardýðýnda) tam milimetrik olarak hedefe oturt
        piyon.transform.position = hedefPozisyon;
        CheckWinCondition();//sonra bak

        isMoving =false; // Kaydýrma iþlemi bitti, yeni hareketlere izin ver

        Debug.Log("Kaydýrma iþlemi bitti, piyon hedefe ulaþtý!");
    }

    // Artýk fonksiyonumuz "kacSaniyeBeklesin" diye bir bilgi istiyor
    IEnumerator SmoothRotateCamera(int targetPlayer, float kacSaniyeBeklesin)
    {
        // SÝHÝRLÝ SATIR: Dönmeye baþlamadan önce adamýn hamlesini sindirmesini bekle!
        yield return new WaitForSeconds(kacSaniyeBeklesin);

        // Oyuncu 1 için açý 0, Oyuncu 2 için 180 derece
        float targetYAngle = (targetPlayer == 1) ? 0f : 180f;

        Quaternion startRotation = cameraPivot.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, targetYAngle, 0f);

        float duration = 1f; // Dönüþ 1 saniye sürsün
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cameraPivot.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        cameraPivot.rotation = targetRotation;
    }
}
