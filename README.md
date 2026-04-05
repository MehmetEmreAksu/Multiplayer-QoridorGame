# 🛡️ Multiplayer Qoridor - Networked Board Game Architecture

Bu proje, strateji tabanlı **Qoridor** oyununun Unity motoru ve **Photon PUN 2** ağ mimarisi kullanılarak geliştirilmiş, gerçek zamanlı çok oyunculu (Multiplayer) bir versiyonudur. Proje sadece bir oyun değil; senkronizasyon, ağ otoritesi ve algoritmik yol doğrulama üzerine bir mühendislik çalışmasıdır.

---
## Kontroller
*    Direk olarak gidebileceğiniz kareye tıklayabilirsiniz.
*    **Tab** - taktiksel kamera
*    Duvarlara **Sol Tık** ile duvar yerleştirme yapabilirsiniz. **R** tuşu ile rotate edebilirsiniz. **Sağ Tık** ile duvar doymayı iptal edebilirsiniz.

## 📖 Kurulum (Installation)
1.  Bu depoyu klonlayın: 
    ```bash
    git clone [https://github.com/MehmetEmreAksu/Multiplayer-QoridorGame.git](https://github.com/MehmetEmreAksu/Multiplayer-QoridorGame.git)
    ```
2.  Projeyi Unity Hub üzerinden açın.
3.  `Photon/PhotonUnityNetworking/Resources` altındaki `PhotonServerSettings` dosyasından kendi **AppID**'nizi tanımlayın.
4.  `Menu` sahnesini başlatın ve rakibinizle eşleşin!
* Aynı AppID üzerinden GameOnline sahnesini sırasıyla çalıştırırsanız. Online olarak giriş yapacaktır. Daha sonrasında lobi mantığı ekleyerek oda numarasıyla giriş özelliği getirilecektir.
* GameOffline sahnesini çalıştırırsanız aynı bilgisayar üzerinden oynayabilirsiniz
---

## 🚀 Öne Çıkan Teknik Özellikler

### 🌐 Networking & Synchronization (Photon PUN 2)
* **Client-Server Architecture:** Oyuncu hareketleri ve duvar yerleşimleri, `Remote Procedure Calls (RPC)` kullanılarak ağ üzerinden senkronize edilmiştir.
* **Authority Management:** Hamle geçerliliği ve sıra takibi (Turn-based logic), ağ üzerindeki "Master Client" ve yerel istemci arasında veri tutarlılığını koruyacak şekilde kurgulanmıştır.
* **State Sync:** Piyon pozisyonları ve tahta durumu, ağ gecikmelerini (Latency) minimize edecek şekilde optimize edilmiştir.

### 🧠 Algoritmik Altyapı (Pathfinding & Logic)
* **BFS (Breadth-First Search) Validation:** Duvar yerleştirme mekanizması, her hamlede **BFS algoritmasını** kullanarak her iki oyuncu için de bitiş çizgisine giden en az bir yolun açık olduğunu doğrular. Geçersiz hamleleri (yol kapatma) gerçek zamanlı olarak engeller.
* **Grid System:** Oyun tahtası, dinamik bir matris yapısı üzerinden yönetilir ve koordinat tabanlı hamle kontrolü sağlar.

### 🎨 Görsel & Arayüz Mimari (Hybrid 3D/2D)
* **Diegetic UI:** Kullanıcı arayüzü (kalan duvar sayısı vb.), `World Space Canvas` kullanılarak oyun dünyasının içine (masa üzerindeki parşömen kağıtlarına) entegre edilmiştir.
* **PBR & Lighting:** Low-poly modeller, dinamik `Point Light` (Mum ışığı) ve `Normal Mapping` teknikleri kullanılarak atmosferik bir orta çağ tavernası havası yaratılmıştır.
* **2.5D Elements:** Performans ve estetik dengesi için alev efektlerinde `Billboarding` tekniği kullanılmıştır.

---

## 🛠️ Kullanılan Teknolojiler
* **Engine:** Unity 2022+ (Universal Render Pipeline - URP)
* **Networking:** Photon Engine (PUN 2)
* **Language:** C# (.NET)
* **Assets:** Custom 2D Pixel Art & Low-Poly 3D Models
* **Tools:** ProBuilder (Modeling), Laigter (Normal Map Generation)

---

## Ekran Görüntüleri

<img width="1519" height="1034" alt="Image" src="https://github.com/user-attachments/assets/358b572f-cc28-4bd7-a584-95f0e291c534" />
<img width="1919" height="1079" alt="Image" src="https://github.com/user-attachments/assets/075a0bdd-6487-4c78-bfeb-58746f667727" />
<img width="1818" height="865" alt="Image" src="https://github.com/user-attachments/assets/f821f09d-4aae-43cb-b583-13fb331ad02d" />

## 👨‍💻 Developer
**Mehmet Emre Aksu** - Computer Engineering Student at Yıldız Technical University
