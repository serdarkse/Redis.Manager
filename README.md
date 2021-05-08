# Redis.Manager

C# ve Redis

-Redis implementasyonu için kullanacağımız open source paketin ismi ServiceStack.Redis. 
-Bu paket bizim client uygulamamızı Redis sunucu ile konuşturacak ve komutları çalıştıracak bir kütüphane. 
-ServiceStack sadece Redis implementasyonu sağlamıyor. 
-Redis’in yanı sıra ORMLite ve diğer hizmetleri de sunuyorlar fakat biz Redis adaptörünü kullanacağız. 
-ServiceStack.Redis’i kullanabilmek için ServiceStack bilmemize gerek yok.


Redis Geospatial İşlemleri

-Redis GeoSpatial veri kümeleri aslında Redis'teki SortedSets'tir, bunun sırrı yoktur. 
-Temel olarak, boylam / enlem koordinatları gibi coğrafi konumsal verileri Redis'e depolamak için kolay bir yol sağlar. 
-Redis'in Geo Spatial verileri için sağladığı bazı komutlara bakalım.

Koordinat Verisi Ekleme: GEOADD  : GEOADD ile istediğim bir Dictionary Key’e ait bir kayıt oluşturabiliriz. Eğer verilen Key değeri mevcut değilse yeni bir tane oluşturulur.
Uzaklık Bulma: GEODIST :  Bir Key altında bulunan iki üyenin birbirine uzaklığını döndürür.
X Kilometreden Yakın Yerler: GEORADIUS : Belirtilen koordinata olan mesafesi parametre olarak geçilen çaptan küçük olan üyeleri döndürür.


Veri ekleme
  redis.GeoAdd("SAMSUN",  // Key 
                41.28671123560004,  // Latitude (x)
                36.34773724569423,  // Longtitude (y)
                "LİMAN");           // Value

            redis.GeoAdd("SAMSUN", 41.31989253684319, 36.32390889787106, "AMİSOS TEPESİ");
            redis.GeoAdd("SAMSUN", 41.273772001419474, 36.36754815316191, "BANDIRMA GEMİSİ");


Redise aktarılan verinin koordinat bilgisini bulma
  var position = redis.GeoPosition("SAMSUN", "AMİSOS TEPESİ");
            //SAMSUN keyinin içinde olan AMİSOS TEPESİ değerinin koordinat bilgilerini döner

İki nokta arasındaki mesafeyi bulma
double? distance = redis.GeoDistance("BASARSOFT", "BALGAT", "ISTANBUL", GeoUnit.Kilometers);

Verilen bir KEY’deki değer baz alınarak 10km çapındaki 100 veriyi getir
    double? distance = redis.GeoDistance("SAMSUN", "LİMAN", "BANDIRMA GEMİSİ", GeoUnit.Kilometers);
            //LİMAN ile BANDIRMA GEMİSİ arasındaki mesafeyi KİLOMETRE/METRE/FEET/MİLE cinsinden verir.

Verilen X,Y koordinatlarını baz alınarak 5000km çapındaki tüm verileri getir

            var Latitude = 45.424807;
            var Longtitude = -75.699234;

            var _results = redis.GeoRadius("SAMSUN",
                Longtitude,
                Latitude,
                50000,
                GeoUnit.Kilometers,
                -1,
                Order.Ascending,
                GeoRadiusOptions.WithCoordinates);
            //Longtitude , Latitude hangi koordinatlar aralığında arama işlemi yapılacağını belirtiriz
            //5000 => ne kadar bir buffer alanında arama yapılacağı belitilir.
            //GeoUnit.Kilometers => buffer değerinin hangi türden olacağını belirtir.
            //-1 => kaç tane sonuç döndüreceğini belirtir. -1 yazılırsa tüm sonuçları listeler.
            
