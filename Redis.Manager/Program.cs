using Redis.Manager.Helpers;
using Redis.Manager.RedisManager;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Redis.Manager
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-----------------------------Redis ile aktarıma hoş geldiniz---------------------------");

            CreateInstance();
            RedisSetGeoAdd();
            RedisGetGeoPosition();
            RedisGetGeoDistance();
            RedisGetGeoRadius();

            Console.ReadLine();
        }

        static void CreateInstance()
        {
            RedisHelp.Instance = new RedisCacheManager("localhost", 6379);

            Console.WriteLine("\n Redis instance oluşturuldu");
        }

        #region Redis İşlemleri
        static void RedisSetAdd()
        {
            var redis = RedisStore.RedisCache;

            redis.SetAdd("Test",    //unique KEY
                        "Value1");  // Value

            redis.SetAdd("Test", "Value2");
            redis.SetAdd("Test", "Value3");

            redis.SetAdd("Example1", "Value4");
            redis.SetAdd("Example2", "Value4");
            redis.SetAdd("Example3", "Value4");

        }
        static void RedisGet(string key)
        {
            var getval = RedisHelp.Instance.Get<string>(key);

        }
        static void RedisRemoveKey(string key)
        {
            var removekey = RedisHelp.Instance.Remove(key);
        }
        static void RedisRemoveKeyTwiceFactor(string key)
        {
            var redis = RedisStore.RedisCache;
            var removekey = redis.KeyDelete(key);
        }
        #endregion

        #region Redis GeoSpatial İşlemleri
        static void RedisSetGeoAdd()
        {
            var redis = RedisStore.RedisCache;

            redis.GeoAdd("SAMSUN",  // Key 
                41.28671123560004,  // Latitude (x)
                36.34773724569423,  // Longtitude (y)
                "LİMAN");           // Value

            redis.GeoAdd("SAMSUN", 41.31989253684319, 36.32390889787106, "AMİSOS TEPESİ");
            redis.GeoAdd("SAMSUN", 41.273772001419474, 36.36754815316191, "BANDIRMA GEMİSİ");

            Console.WriteLine("Eklenen KEY : SAMSUN  ---  Eklenen VALUE : LİMAN ");
            Console.WriteLine("Eklenen KEY : SAMSUN  ---  Eklenen VALUE : AMİSOS TEPESİ ");
            Console.WriteLine("Eklenen KEY : SAMSUN  ---  Eklenen VALUE : BANDIRMA GEMİSİ ");


        }
        static void RedisGetGeoPosition()
        {
            var redis = RedisStore.RedisCache;

            var position = redis.GeoPosition("SAMSUN", "AMİSOS TEPESİ");
            //SAMSUN keyinin içinde olan AMİSOS TEPESİ değerinin koordinat bilgilerini döner
            Console.WriteLine("SAMSUN - AMİSOS TEPESİ Koordinat bilgisi : " + position);

        }
        static void RedisGetGeoDistance()
        {
            var redis = RedisStore.RedisCache;

            double? distance = redis.GeoDistance("SAMSUN", "LİMAN", "BANDIRMA GEMİSİ", GeoUnit.Kilometers);
            //LİMAN ile BANDIRMA GEMİSİ arasındaki mesafeyi KİLOMETRE/METRE/FEET/MİLE cinsinden verir.

            Console.WriteLine("SAMSUN ilinde LİMAN ile BANDIRMA GEMİSİ arasındaki uzaklık " + distance + " km.");

        }
        static void RedisGetGeoRadius()
        {
            var redis = RedisStore.RedisCache;


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
            
            if (_results.Length > 0)
            {
                Console.WriteLine("İlgili noktaya 50000 km buffer alanındaki alanlar : ");

                foreach (var item in _results)
                {
                    Console.WriteLine("Bulunan yer : " + item.Member + " , koordinatları : " + item.Position.ToString());
                }
            }

        }
        #endregion

        #region Redis Point İşlemleri
        static void RedisSetPoint()
        {
            var redis = RedisStore.RedisCache;

            Console.WriteLine("\n point veri çekme başlandı");

            var redisbaz = RedisStore.RedisCache;

            var _baz = db.VPoint; 
            //veritabanından point verilerinizin objesel hallerini çekmelisiniz.
            //örnek point veri = POINT(32.84211996 39.86889102)

            Console.WriteLine("\n point veri çekme tamamlandı");

            RedisHelp.Instance.Remove("POINT");

            Console.WriteLine("\n point redise veri aktarımı başladı");

           
            Parallel.ForEach(_baz, item =>
            {

                if (item.OBJE_WKT != null && item.OBJE_WKT != "")
                {
                    var splt = item.OBJE_WKT.Split(' ');
                    var locX = Convert.ToDouble(splt[0].Replace(".", ","));
                    var locY = Convert.ToDouble(splt[1].Replace(".", ","));
                    //POINT nesnemizde sadece 1er tane x-y koordinat bilgileri mevcuttur.
                    //Bu sebeple x ve y koordinat bilgilierini split ederek çok rahat bir şekilde elde edebiliriz.

                    try
                    {
                        redisbaz.GeoAdd("POINT", locX, locY, item.MI_PRINX);
                        //elde ettiğimiz bu x,y koordinat bilgilerini artık POINT keyi adı altında redise aktarabiliriz.
                        //item.MI_PRINX dediğimiz ise her veri için özel bir value.
                        //eğer veritabanınızda bir uniq alan yok ise counter kullanarak buraya artan değer verebilirsiniz.

                    }
                    catch (Exception ex)
                    {

                        System.Threading.Thread.Sleep(5000);
                        redisbaz.GeoAdd("POINT", locX, locY, item.MI_PRINX);

                    }

                }
            }
      );

            Console.WriteLine("\n point redise veri aktarımı tamamlandı");

        }

        #endregion

        #region Redis Linestring-MultiLinestring İşlemleri
        static void RedisSetLineMultiline()
        {
            Console.WriteLine("\n linestring or multilinestring veri çekme başlandı");

            var redistransmisyon = RedisStore.RedisCache;

            var _transmiyon = db.VLineOrMultiline;

            Console.WriteLine("\n linestring or multilinestring veri çekme tamamlandı");

            RedisHelp.Instance.Remove("LİNEORMULTİLİNE");

            Console.WriteLine("\n linestring or multilinestring redise veri aktarımı başladı");


            Parallel.ForEach(_transmiyon, item =>
            {
                if (item.OBJE_WKT != null && item.OBJE_WKT != "")
                {
                    if (item.OBJE_WKT.Contains("MULTI"))
                    {
                        var spltdgr = item.OBJE_WKT.Remove(0, 18);
                        var replacedgr = spltdgr.Replace("), (", "*");
                        var spltson = replacedgr.Split('*');
                        var spltdgrFirst = spltson[0].Split(',');
                        var spltdgrTwice = spltson[1].Remove(spltson[1].Length - 2, 2).Split(',');
                        foreach (var itemfirst in spltdgrFirst)
                        {
                            double locX = 0.0;
                            double locY = 0.0;

                            var splittodouble = itemfirst.Split(' ');
                            if (splittodouble[0] == "")
                            {
                                locX = Convert.ToDouble(splittodouble[1].Replace(".", ","));
                                locY = Convert.ToDouble(splittodouble[2].Replace(".", ","));
                            }
                            else
                            {
                                locX = Convert.ToDouble(splittodouble[0].Replace(".", ","));
                                locY = Convert.ToDouble(splittodouble[1].Replace(".", ","));
                            }
                            try
                            {
                                redistransmisyon.GeoAdd("LİNEORMULTİLİNE", locX, locY, item.MI_PRINX);
                            }
                            catch (Exception ex)
                            {

                                System.Threading.Thread.Sleep(5000);
                                redistransmisyon.GeoAdd("LİNEORMULTİLİNE", locX, locY, item.MI_PRINX);

                            }
                        }
                        foreach (var itemfirst in spltdgrTwice)
                        {
                            double locX = 0.0;
                            double locY = 0.0;

                            var splittodouble = itemfirst.Split(' ');
                            if (splittodouble[0] == "")
                            {
                                locX = Convert.ToDouble(splittodouble[1].Replace(".", ","));
                                locY = Convert.ToDouble(splittodouble[2].Replace(".", ","));
                            }
                            else
                            {
                                locX = Convert.ToDouble(splittodouble[0].Replace(".", ","));
                                locY = Convert.ToDouble(splittodouble[1].Replace(".", ","));
                            }
                            try
                            {
                                redistransmisyon.GeoAdd("LİNEORMULTİLİNE", locX, locY, item.MI_PRINX);
                            }
                            catch (Exception ex)
                            {

                                System.Threading.Thread.Sleep(5000);
                                redistransmisyon.GeoAdd("LİNEORMULTİLİNE", locX, locY, item.MI_PRINX);

                            }
                        }
                    }
                    else if (item.OBJE_WKT.Contains("LINESTRING"))
                    {
                        var objesplit = item.OBJE_WKT.Remove(0, 12);
                        var spltdgrdvm = objesplit.Remove(objesplit.Length - 1, 1).Split(',');


                        foreach (var itemdata in spltdgrdvm)
                        {
                            double locX = 0.0;
                            double locY = 0.0;

                            var splittodouble = itemdata.Split(' ');
                            if (splittodouble[0] == "")
                            {
                                locX = Convert.ToDouble(splittodouble[1].Replace(".", ","));
                                locY = Convert.ToDouble(splittodouble[2].Replace(".", ","));
                            }
                            else
                            {
                                locX = Convert.ToDouble(splittodouble[0].Replace(".", ","));
                                locY = Convert.ToDouble(splittodouble[1].Replace(".", ","));
                            }
                            try
                            {
                                redistransmisyon.GeoAdd("LİNEORMULTİLİNE", locX, locY, item.MI_PRINX);
                            }
                            catch (Exception ex)
                            {

                                System.Threading.Thread.Sleep(5000);
                                redistransmisyon.GeoAdd("LİNEORMULTİLİNE", locX, locY, item.MI_PRINX);

                            }
                        }
                    }
                    else
                    {
                        var splt = item.OBJE_WKT.Split(' ');
                        var locX = Convert.ToDouble(splt[0].Replace(".", ","));
                        var locY = Convert.ToDouble(splt[1].Replace(".", ","));
                        try
                        {
                            redistransmisyon.GeoAdd("LİNEORMULTİLİNE", locX, locY, item.MI_PRINX);
                        }
                        catch (Exception ex)
                        {

                            System.Threading.Thread.Sleep(5000);
                            redistransmisyon.GeoAdd("LİNEORMULTİLİNE", locX, locY, item.MI_PRINX);

                        }
                    }
                }
            }
      );


            Console.WriteLine("\n linestring or multilinestring redise veri aktarımı tamamlandı");

        }
        #endregion
    }
}
