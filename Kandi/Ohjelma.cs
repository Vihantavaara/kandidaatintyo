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
            while (true)
            {
                try
                {
                    Console.Write("Säikeiden lukumäärä: ");
                    Saikeet = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Alkioiden lukumäärä per taulukko: ");
                    AlkioidenMaara = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Ajojen lukumäärä: ");
                    AjojenMaara = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Käytettävä algoritmi ([m]ergesort / [q]uicksort): ");
                    Algoritmi = Console.ReadLine().ToLower();

                    if (!(Algoritmi == "m" || Algoritmi == "mergesort" || Algoritmi == "q" || Algoritmi == "quicksort"))
                    {
                        throw new Exception();
                    }
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Virhe syötteissä, yritä uudelleen.");
                }
            }
            
            var tiedostonNimi = (Algoritmi == "m" || Algoritmi == "mergesort") ?
                "Mergesort_Maara" + AlkioidenMaara + "_Saikeet" + Saikeet + "_" + DateTime.Now + ".csv"
                :
                "Quicksort_Maara" + AlkioidenMaara + "_Saikeet" + Saikeet + "_" + DateTime.Now + ".csv";
            using var writer = new StreamWriter(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + @"\" + tiedostonNimi , false,
                Encoding.UTF8);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.Configuration.RegisterClassMap<TuloksetMap>();

            var kaikkiSuoritusajat = new List<Tulokset>();

            for (var indeksi = 0; indeksi < AjojenMaara; indeksi++)
            {
                var sekventiaalinenKellotus = new Stopwatch();
                var rinnakkainenKellotus = new Stopwatch();
                var sekventiaalinenTaulukko = GeneroiListaSatunnaisistaAlkioista(AlkioidenMaara, 0, 2147483647);
                var rinnakkainenTaulukko = (int[])sekventiaalinenTaulukko.Clone();
                
                if (Algoritmi == "m" || Algoritmi == "mergesort")
                {
                    var mergesort = new Mergesort();

                    sekventiaalinenKellotus.Start();
                    var jarjestettySekventiaalinenTaulukko= mergesort.SekventiaalinenJarjestys(sekventiaalinenTaulukko);
                    sekventiaalinenKellotus.Stop();

                    rinnakkainenKellotus.Start();
                    var jarjestettyRinnakkainenTaulukko = mergesort.RinnakkainenJarjestys(Saikeet, rinnakkainenTaulukko);
                    rinnakkainenKellotus.Stop();

                    if (jarjestettySekventiaalinenTaulukko.SequenceEqual(jarjestettyRinnakkainenTaulukko))
                    {
                        var suoritusajat = new Tulokset()
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
                else if (Algoritmi == "q" || Algoritmi == "quicksort")
                {
                    //TODO
                }
                else
                {
                    Console.WriteLine("Väärä syöte algoritmiin");
                    break;
                }
            }
            csv.WriteRecords(kaikkiSuoritusajat);
            writer.Close();
        }

        private static int[] GeneroiListaSatunnaisistaAlkioista(int alkioidenMaara, int minimi, int maksimi)
        {
            return Enumerable.Range(0, alkioidenMaara)
                .Select(arvo => RandomNumberGenerator.GetInt32(minimi, maksimi))
                .ToArray();
            
        }
    }
}
