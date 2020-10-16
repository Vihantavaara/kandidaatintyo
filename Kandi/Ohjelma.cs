using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using Kandi.Algoritmit;
using Kandi.Tietueet;

namespace Kandi
{
    internal class Ohjelma
    {
        private static int Saikeet { get; set; }
        private static int AlkioidenMaara { get; set; }
        private static int AjojenMaara { get; set; }
        private static string Algoritmi { get; set; }


        private static void Main()
        {
            var sijainti = Directory.GetParent(Environment.CurrentDirectory).Parent?.Parent?.Parent?.FullName + @"\tulokset\";

            while (true)
            {
                try
                {
                    Directory.CreateDirectory(sijainti);

                    Console.Write("Säikeiden lukumäärä: ");
                    Saikeet = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Alkioiden lukumäärä per taulukko: ");
                    AlkioidenMaara = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Ajojen lukumäärä: ");
                    AjojenMaara = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Käytettävä algoritmi ([m]ergesort / [q]uicksort): ");
                    Algoritmi = Console.ReadLine()?.ToLower();

                    if (!(Algoritmi == "m" || Algoritmi == "mergesort" || Algoritmi == "q" || Algoritmi == "quicksort") || Saikeet % 2 != 0)
                    {
                        throw new Exception();
                    }
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Virhe syötteissä, yritä uudelleen (Huom! Säikeet parillisena).");
                }
            }

            if (Algoritmi == "m")
            {
                Algoritmi = "mergesort";
            }

            if (Algoritmi == "q")
            {
                Algoritmi = "quicksort";
            }

            var tiedostonNimi = Algoritmi + "_Alkioita" + AlkioidenMaara + "_Saikeita" + Saikeet + "_" + DateTime.Now + ".csv";

            using var writer = new StreamWriter(sijainti + tiedostonNimi , false, Encoding.UTF8);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.Configuration.RegisterClassMap<TuloksetMap>();

            var kaikkiSuoritusajat = new List<Tulokset>();

            for (var indeksi = 0; indeksi < AjojenMaara; indeksi++)
            {
                Console.WriteLine("Suoritetaan " + (indeksi + 1) + ". ajoa");

                var sekventiaalinenKellotus = new Stopwatch();
                var rinnakkainenKellotus = new Stopwatch();
                var sekventiaalinenTaulukko = GeneroiListaSatunnaisistaAlkioista(AlkioidenMaara, 0, 2147483647);
                var rinnakkainenTaulukko = (int[])sekventiaalinenTaulukko.Clone();
                
                if (Algoritmi == "mergesort")
                {
                    var mergesort = new Mergesort();

                    sekventiaalinenKellotus.Start();
                    mergesort.SekventiaalinenJarjestys(sekventiaalinenTaulukko);
                    sekventiaalinenKellotus.Stop();

                    rinnakkainenKellotus.Start();
                    mergesort.RinnakkainenJarjestys(Saikeet, rinnakkainenTaulukko);
                    rinnakkainenKellotus.Stop();

                    if (sekventiaalinenTaulukko.SequenceEqual(rinnakkainenTaulukko) && Jarjestetty(sekventiaalinenTaulukko))
                    {
                        var suoritusajat = new Tulokset
                        {
                            RinnakkainenSuoritusaika = rinnakkainenKellotus.ElapsedMilliseconds,
                            SekventiaalinenSuoritusaika = sekventiaalinenKellotus.ElapsedMilliseconds
                        };

                        kaikkiSuoritusajat.Add(suoritusajat);
                    }
                    else
                    {
                        Console.WriteLine("Virhe mergesortin järjestämisessä");
                    }
                } 
                else if (Algoritmi == "quicksort")
                {
                    var quicksort = new Quicksort();

                    sekventiaalinenKellotus.Start();
                    quicksort.SekventiaalinenJarjestys(sekventiaalinenTaulukko);
                    sekventiaalinenKellotus.Stop();

                    rinnakkainenKellotus.Start();
                    quicksort.RinnakkainenJarjestys(Saikeet, rinnakkainenTaulukko);
                    rinnakkainenKellotus.Stop();

                    if (sekventiaalinenTaulukko.SequenceEqual(rinnakkainenTaulukko) && Jarjestetty(sekventiaalinenTaulukko))
                    {
                        var suoritusajat = new Tulokset
                        {
                            RinnakkainenSuoritusaika = rinnakkainenKellotus.ElapsedMilliseconds,
                            SekventiaalinenSuoritusaika = sekventiaalinenKellotus.ElapsedMilliseconds
                        };

                        kaikkiSuoritusajat.Add(suoritusajat);
                    }
                    else
                    {
                        Console.WriteLine("Virhe quicksortin järjestämisessä");
                    }
                }
                else
                {
                    Console.WriteLine("Väärä syöte algoritmiin");
                    break;
                }
            }

            Console.WriteLine(kaikkiSuoritusajat.Count == AjojenMaara
                ? "Kaikki suoritukset kirjattu tiedostoon."
                : "Osa suorituksista jäi kirjaamatta tiedostoon.");

            csv.WriteRecords(kaikkiSuoritusajat);
            writer.Close();
        }

        private static int[] GeneroiListaSatunnaisistaAlkioista(int alkioidenMaara, int minimi, int maksimi)
        {
            return Enumerable.Range(0, alkioidenMaara)
                .Select(arvo => RandomNumberGenerator.GetInt32(minimi, maksimi))
                .ToArray();
            
        }

        private static bool Jarjestetty(int[] taulukko)
        {
            var i = taulukko.Length - 1;

            if (i <= 0) return true;
            if ((i & 1) > 0) { if (taulukko[i] < taulukko[i - 1]) return false; i--; }
            for (var ai = taulukko[i]; i > 0; i -= 2)
                if (ai < (ai = taulukko[i - 1]) || ai < (ai = taulukko[i - 2])) return false;
            return taulukko[0] <= taulukko[1];
        }
    }
}
