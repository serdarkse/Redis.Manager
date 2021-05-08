using Redis.Manager.Helpers;
using Redis.Manager.RedisManager;
using StackExchange.Redis;
using System;

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


    }
}
