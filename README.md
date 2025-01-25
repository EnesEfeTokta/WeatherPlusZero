![WeatherPlusZero.png](WeatherPlusZero/Images/AppLogo.png)

# Weather Plus Zero
> Windows platformları için uygun, C# dilinde basit ve kullanışlı hava durumu uygulaması.

## Genel 🙌

Weather Plus Zero, modern ve kullanıcı dostu bir hava durumu uygulamasıdır. Weather Plus Zero, Windows işletim sistemleri için tasarlanmış, C# programlama dili kullanılarak geliştirilmiş kapsamlı bir hava durumu uygulamasıdır. Kullanıcıların günlük hayatlarını planlamalarına yardımcı olmak için tasarlanmıştır.

## Özellikler ⭐

**Ana Özellikler:**

- Anlık hava durumu bilgileri.
- Saatlik ve günlük tahminler.
- Sıcaklık, nem, rüzgar hızı ve yön bilgileri.
- Görsel hava durumu göstergeleri.
- Çoklu şehir desteği.
- Bildirim sistemi.

**Teknik Özellikler:**

- Windows uyumlu modern arayüz.
- Hızlı ve optimize edilmiş performans.
- Gerçek zamanlı veri güncellemeleri.
- Düşük sistem kaynağı kullanımı.

**Hedef Kitle:**

- Günlük hava durumu takibi yapan kullanıcılar.
- Profesyonel planlamacılar.
- Outdoor aktivite tutkunları.
- Hava durumuna bağlı çalışan profesyoneller.

**Kullanıcı Deneyimi:**

Uygulama, kullanıcı dostu arayüzü ve sezgisel tasarımı ile her yaştan kullanıcının kolayca kullanabileceği şekilde tasarlanmıştır. Minimal ve modern tasarım anlayışı, kullanıcılara kesintisiz bir deneyim sunar.

## GitHub ⛓️‍💥

[Weather Plus Zero GitHub Repo](https://github.com/EnesEfeTokta/WeatherPlusZero)

## Drawio 🪢
[Weather Plus Zero Drawio](https://github.com/EnesEfeTokta/WeatherPlusZero/blob/main/WeatherPlusZero/Planning/WeatherPlusZero.drawio)

## İletişim ☎️

- [enesefetokta009@gmail.com](mailto:enesefetokta009@gmail.com)
- (+90) 541 586 9564
- https://www.linkedin.com/in/enes-efe-tokta-6567151b5/
- https://github.com/EnesEfeTokta

## Logo ve Görseller 🖼️

![WeatherPlusZero.png](WeatherPlusZero/Images/AppLogo.png)

## Kaynaklar ➕

### Görseller

- https://www.freepik.com/free-ai-image/portrait-was-captured-streets-depicting-everyday-life-urban-culture_65362546.htm#fromView=search&page=3&position=7&uuid=cb6478b5-be31-4935-bcd2-443f0f8a6cc5 → Yağmurlu hava arkaplan görseli.
- https://www.freepik.com/free-photo/yellow-flowers_1484001.htm#fromView=search&page=1&position=10&uuid=c08af2dc-e1e9-4b82-bb66-54c40f2f152c → Güneşli hava arkaplanı görseli.
- https://www.freepik.com/free-photo/storm-clouds_1172981.htm#fromView=search&page=1&position=40&uuid=48f3c3fb-ead3-41c4-b7ac-80ba4190c82d → Bulutlu hava arkaplanı görseli.
- https://www.freepik.com/free-photo/closeup-shot-tree-branch-snowy-weather_12040237.htm#fromView=search&page=1&position=29&uuid=96710d47-8fa4-46c1-9061-bb6ca10aabcb → Karlı hava arkaplanı görseli.

## Görevler 📝

[Görevler](https://www.notion.so/1531fb14c4a880c29cd1e8274f998d62?pvs=21)

## Proje Planlama ve Tasarım 🎨

Gösterilecek olan verileri:

- Sıcaklık (Fahrenheit ve Celsius cinsinden…),
- Nem,
- Rüzgar,
- Yağış

Eş zamanlı olarak takip edilecek şehir sayısı en fazla üç adet olunabilecek.

Bildirim sistemi stabil olarak her 5 saatte bir hava durumu hakkında bilgi seçilen birincil şehrin hava durumu hakkında bildirim ile haberdar edecek. Bildirimin içinde sıcaklık ve hava durumu hakkında temel bilgi içeriyor. Ekstradan kullanıcı bildirim sıklığını kendine göre düzenleyebilecektir.

Kullanıcı arayüzü sade bir tasarım prensibine dayalıdır.  Kullanıcının lokasyon araması yaparak şehirlerin hava durumu bilgilerini öğrenmesine olanak tanıyan arama çubuğunun olmasıyla birlikte tarih ve saat bilgisini de gösteren bir UI eleman bulunuyor. Sıcaklık, basınç, nem ve rüzgar verileri gösteriliyor. Kullanıcının gün batımı ve gün doğumunu da takip edecek bir zaman çizelgesine sahiptir. Gelecek günlerde ki hava durumlarını listeleyen bir yapıda kullanılmıştır. Ekstradan kullanıcı ek ayarlar için ve hesap bilgileri için bir düğme konumlandırıldı. Bu düğme ile kullanıcı ek özelliklere erişebilecek.

![Bir başlık ekleyin.png](https://prod-files-secure.s3.us-west-2.amazonaws.com/7aeaee67-246b-42dd-969b-c7f35ff9e952/d49a1714-f1ea-42b6-a294-1b9691f4356c/Bir_balk_ekleyin.png)

## Teknolojik Altyapı Seçimi ☑️

Geliştirme için .Net 6 veya 7 üstü LTS sürümler tercih edildi. Çünkü geleceğe dönük geliştirme ve sorunlarla karşılaşma ihtimalinin düşük olması göz önüne alındı.

Kullanılacak UI Framework ise WPF oldu. Hem modern hem de güncel olması geliştirme sürecinde kolaylıklar tanıyacaktır.

Veri kaynağı olarak ise OpenWeatherMap tercih edildi. Böylece verilere daha ekonomik ve kolay ulaşmış oluruz. Eğer hangi bir aksi durum yaşanır ise Visual Crossing Weather aracını kullanabiliriz.

## Teknik Dokümantasyon 🧑‍💻

### API Referansları

### Veritabanı Şeması

### Sistem Mimarisi

## **Güvenlik ve Performans 🛡️**

### Güvenlik Protokolleri

### Performans Optimizasyonu

### Yük Testleri

## **Kullanıcı Dokümantasyonu 📖**

### Kullanım Kılavuzu

### SSS (Sıkça Sorulan Sorular)

### Sorun Giderme Rehberi

## **Kalite Güvence ✨**

### Test Senaryoları

### Hata Raporlama Prosedürleri

### Kalite Metrikleri

## **Sürdürülebilirlik ve Bakım 🛠️**

### Bakım Planı

### Güncelleme Politikası

### Destek Prosedürleri

## Kullanıcı Arayüzü 🧮

## Yasal ve Hukuki Uyarılar ⚠️

## Pazarlama & Tanıtım Stratejileri 🖊️

## Geliştirici Notları 🗒️

<details>
    <summary>
        <b> 22/12/2024 XAML ile View Katmanı Yapıldı </b>
    </summary>
    <b>Aşama 1: Temel Pencere Yapısının Oluşturulması</b>
    <ul>
        <li><b>Hedef:</b> Uygulama penceresinin ana hatlarını ve temel özelliklerini tanımlamak.</li>
        <li><b>Uygulama:</b>
            <ul>
                <li><code>Window</code> elementi kullanılarak ana uygulama penceresi oluşturuldu.</li>
                <li>Pencere başlığı "Weather Plus Zero" olarak ayarlandı (<code>Title="Weather Plus Zero"</code>).</li>
                <li>Pencere boyutları 460 piksel yükseklik ve 800 piksel genişlik olarak belirlendi (<code>Height="460" Width="800"</code>).</li>
                <li>Pencerenin yeniden boyutlandırılması engellendi (<code>ResizeMode="NoResize"</code>).</li>
                <li>Pencerenin açılışta ekranın ortasında konumlanması sağlandı (<code>WindowStartupLocation="CenterScreen"</code>).</li>
                <li>Uygulama simgesi "Images\AppLogo.png" olarak tanımlandı (<code>Icon="Images\AppLogo.png"</code>).</li>
                <li>Ana içerik alanı için bir <code>Grid</code> elementi oluşturuldu ve "GeneralGrid" olarak adlandırıldı (<code>&lt;Grid x:Name="GeneralGrid"&gt;</code>).</li>
            </ul>
        </li>
    </ul>
    <b>Aşama 2: Ana Layout'un Tanımlanması (Grid Yapısı)</b>
    <ul>
        <li><b>Hedef:</b> Pencere içeriğini düzenlemek için satır ve sütun yapısını oluşturmak.</li>
        <li><b>Uygulama:</b>
            <ul>
                <li><code>GeneralGrid</code> içerisinde beş satır (<code>RowDefinition</code>) ve beş sütun (<code>ColumnDefinition</code>) tanımlandı. Bu yapı, farklı UI öğelerinin yerleşimini kontrol etmek için temel bir çerçeve sağlar.</li>
                <li>Satır yükseklikleri ve sütun genişlikleri piksel cinsinden belirtilerek, öğelerin boyutlandırılması üzerinde hassas kontrol sağlandı.</li>
            </ul>
        </li>
    </ul>
    <b>Aşama 3: Arka Plan Görselinin Entegrasyonu</b>
      <ul>
        <li><b>Hedef:</b> Uygulamaya görsel bir zenginlik katmak için bir arka plan resmi eklemek.</li>
        <li><b>Uygulama:</b>
            <ul>
                <li><code>GeneralGrid.Background</code> özelliği kullanılarak bir <code>ImageBrush</code> tanımlandı.</li>
                <li>Arka plan resmi olarak "Images\RainyWeatherInTheEvening.jpg" dosyası seçildi.</li>
                <li><code>Stretch="UniformToFill"</code> özelliği ile resmin en-boy oranını koruyarak tüm alanı kaplaması sağlandı.</li>
            </ul>
        </li>
    </ul>
    <b>Aşama 4: Şehir Arama Bölümünün Oluşturulması</b>
    <ul>
        <li><b>Hedef:</b> Kullanıcının hava durumu bilgilerini görüntülemek istediği şehri girmesini sağlamak.</li>
        <li><b>Uygulama:</b>
        <ul>
            <li><code>CityNameSearchBorder</code> adında yuvarlatılmış köşelere sahip bir <code>Border</code> elementi oluşturuldu (<code>CornerRadius="10"</code>).</li>
            <li>Arka plan rengi yarı saydam beyaz olarak ayarlandı (<code>Background="#33FFFFFF"</code>).</li>
            <li>Kenarlık rengi ve kalınlığı tanımlandı (<code>BorderBrush="#66FFFFFF" BorderThickness="0"</code>).</li>
            <li><code>CityNameSearchGrid</code> adında bir <code>Grid</code> içerisinde metin giriş alanı (<code>TextBox</code>) ve arama ikonu (<code>Button</code>) yerleştirildi.</li>
            <li><code>CityNameSearchTextBox</code> adında bir <code>TextBox</code> oluşturuldu.
            <ul>
                <li>Arka planı şeffaf yapıldı (<code>Background="Transparent"</code>).</li>
                <li>Kenarlığı kaldırıldı (<code>BorderThickness="0"</code>).</li>
                <li>Yazı rengi gri olarak ayarlandı (<code>Foreground="Gray"</code>).</li>
                <li>Dikey olarak ortalandı (<code>VerticalAlignment="Center"</code>).</li>
                <li>Sol taraftan 5 piksel iç boşluk verildi (<code>Padding="5,0,0,0"</code>).</li>
                <li>Sağ taraftan 40 piksel boşluk bırakıldı (<code>Margin="0,0,40,0"</code>).</li>
                <li>Yüksekliği 17 piksel ve yazı boyutu 12 piksel olarak belirlendi (<code>Height="17" FontSize="12"</code>).</li>
                <li>Odaklanma ve odak kaybı olayları için (<code>GotFocus</code>, <code>LostFocus</code>) ve tuşa basma olayları için (<code>KeyDown</code>) ilgili event handler'lar tanımlandı.</li>
                <li>Varsayılan metin olarak "Search for city..." yazıldı.</li>
                <li>Odaklanma durumunda metni temizleyen bir <code>Style.Trigger</code> eklendi.</li>
            </ul>
            </li>
            <li><code>CityNameSearchButton</code> adında bir <code>Button</code> oluşturuldu.
            <ul>
                <li>İçeriği büyüteç ikonu olarak ayarlandı (<code>Content="🔍"</code>).</li>
                <li>Genişlik ve yüksekliği 30 piksel olarak belirlendi (<code>Width="30" Height="30"</code>).</li>
                <li>Arka planı şeffaf yapıldı (<code>Background="Transparent"</code>).</li>
                <li>Yazı rengi beyaz olarak ayarlandı (<code>Foreground="White"</code>).</li>
                <li>Kenarlığı kaldırıldı (<code>BorderThickness="0"</code>).</li>
                <li>Sağa ve dikey olarak ortalandı (<code>HorizontalAlignment="Right" VerticalAlignment="Center"</code>).</li>
                <li>Tıklama olayı için (<code>Click</code>) event handler tanımlandı.</li>
            </ul>
            </li>
        </ul>
        </li>
    </ul>
    
</details>

## Sürüm Notları 🆕