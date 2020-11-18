using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kandi.Algoritmit
{
    public class Quicksort : IAlgoritmi
    {
        /// <summary>
        /// Sekventiaalisen ajon aloituspiste.
        /// </summary>
        /// <param name="taulukko">Järjestettävä taulukko</param>
        public void SekventiaalinenJarjestys(int[] taulukko)
        {

            QuickSort(taulukko, 0, taulukko.Length - 1);

        }

        /// <summary>
        /// Rinnakkaisen ajon aloituspiste.
        /// </summary>
        /// <param name="tyolaistenMaara">Käytettävien säikeiden määrä, mukaanlukien pääsäie</param>
        /// <param name="taulukko">Järjestettävä taulukko</param>
        public void RinnakkainenJarjestys(int tyolaistenMaara, int[] taulukko)
        {
            var aputaulukko = (int[])taulukko.Clone();

            // Käytettävät säikeet miinus pääsäie.
            var tyolaiset = new Task[tyolaistenMaara - 1];

            // Iteraatioiden määrä.
            var iteraatiot = (int)Math.Log(tyolaistenMaara, 2);

            // alkioiden määrä per säie ja jakojäännös joka jätetään pääsäikeelle.
            var osuudenKoko = taulukko.Length / tyolaistenMaara;
            var jakojaannos = taulukko.Length % tyolaistenMaara;

            var barrier = new Barrier(tyolaistenMaara, b =>
            {
                // Kaksinkertaistetaan osuudet joka iteraation jälkeen.
                osuudenKoko <<= 1;

                // Vaihdetaan joka iteraatiolla taulukon ja aputaulukon paikkaa.
                var temp = aputaulukko;
                aputaulukko = taulukko;
                taulukko = temp;
            });

            // Delegaatti, jonka jokainen säie suorittaa. Sisältää indeksin.
            void Tehtava(object objekti)
            {
                // Jokaisen säikeen ja sitä vastaavan osuuden indeksi.
                var indeksi = (int)objekti;

                // Lasketaan osuuden rajaavat indeksit
                var vasen = indeksi * osuudenKoko;
                var oikea = (indeksi + 1) * osuudenKoko - 1 + jakojaannos;

                // Jätetään jakojäännös pääsäikeelle.
                if (indeksi > 0)
                {
                    vasen += jakojaannos;
                }

                // Suoritetaan iteraation järjestäminen
                QuickSort(taulukko, vasen, oikea);

                // Odotetaan, että kaikki säikeet ovat valmiita joka iteraatiolla.
                barrier.SignalAndWait();

                // Poistetaan joka toinen säie joka iteraatiossa.
                for (var j = 0; j < iteraatiot; j++)
                {
                    if (indeksi % 2 == 1)
                    {
                        barrier.RemoveParticipant();
                        break;
                    }

                    var uusiOikea = oikea + osuudenKoko / 2;

                    // Päivitetään uusien osuuksien indeksit.
                    indeksi >>= 1;

                    // Yhdistetään iteraation osuudet.
                    Merge(taulukko, aputaulukko, vasen, oikea, oikea + 1, uusiOikea, vasen);

                    // Siirretään osuuden oikeaa rajaa merkitsevää indeksiä yhdistämisen jälkeen.
                    oikea = uusiOikea;

                    // Odotetaan, että kaikki säikeet ovat yhdistäneet toisen osuuden.
                    barrier.SignalAndWait();
                }
            }

            // Muiden säikeiden suoritus.
            for (var i = 0; i < tyolaiset.Length; i++)
            {
                tyolaiset[i] = Task.Factory.StartNew(Tehtava, i + 1);
            }

            // Pääsäikeen suoritus.
            Tehtava(0);

            // Mikäli iteraatiot ovat parittomia, pitää aputaulukko kopioida taulukkoon,
            // sillä viimeisellä iteraatiolla aputaulukko on järjestetty.
            if (iteraatiot % 2 != 0)
                Array.Copy(aputaulukko, taulukko, taulukko.Length);
        }

        /// <summary>
        /// Quicksort. Sekä sekventiaalinen että rinnakkainen versio käyttävät tätä. Toimii rekursiivisesti.
        /// </summary>
        /// <param name="taulukko"></param>
        /// <param name="vasen"></param>
        /// <param name="oikea"></param>
        private static void QuickSort(int[] taulukko, int vasen, int oikea)
        {

            // Varmistetaan, että rajojen vasen indeksi ei ole suurempi tai sama kuin oikea indeksi.
            if (vasen >= oikea)
                return;

            var pivotti = Partition(taulukko, vasen, oikea);
            QuickSort(taulukko, vasen, pivotti - 1);
            QuickSort(taulukko, pivotti + 1, oikea);

        }

        /// <summary>
        /// Osituksen järjestäminen pivottia hyödyntäen.
        /// </summary>
        /// <param name="taulukko"></param>
        /// <param name="vasen"></param>
        /// <param name="oikea"></param>
        /// <returns></returns>
        private static int Partition(int[] taulukko, int vasen, int oikea)
        {
            var pivotti = taulukko[oikea];

            var iteroija1 = vasen;
            for (var iteroija2 = vasen; iteroija2 < oikea; iteroija2++)
            {
                if (taulukko[iteroija2] > pivotti) continue;

                var apumuuttuja = taulukko[iteroija2];
                taulukko[iteroija2] = taulukko[iteroija1];
                taulukko[iteroija1] = apumuuttuja;
                iteroija1++;
            }

            taulukko[oikea] = taulukko[iteroija1];
            taulukko[iteroija1] = pivotti;

            return iteroija1;
        }

        /// <summary>
        /// Osuuksien yhdistäminen. Siirretään taulukossa alkiot suuruusjärjestykseen.
        /// </summary>
        /// <param name="taulukko">Järjestettävä taulukko</param>
        /// <param name="aputaulukko">Kopio taulukosta</param>
        /// <param name="ensimmaisenPuolenVasen">Ensimmäisen yhdistettävän osuuden vasen indeksi</param>
        /// <param name="ensimmaisenPuolenOikea">Ensimmäisen yhdistettävän osuuden oikea indeksi</param>
        /// <param name="toisenPuolenVasen">Toisen yhdistettävän osuuden vasen indeksi</param>
        /// <param name="toisenPuolenOikea">Toisen yhdistettävän osuuden oikea indeksi</param>
        /// <param name="alitaulukonVasenAlkio">vasemmanpuoleisin indeksi molempia osuuksia katsoen</param>
        private static void Merge(int[] taulukko, int[] aputaulukko, int ensimmaisenPuolenVasen, int ensimmaisenPuolenOikea, int toisenPuolenVasen, int toisenPuolenOikea, int alitaulukonVasenAlkio)
        {
            var alitaulukonOikeaAlkio = alitaulukonVasenAlkio + ensimmaisenPuolenOikea - ensimmaisenPuolenVasen + toisenPuolenOikea - toisenPuolenVasen + 1;
            for (; alitaulukonVasenAlkio <= alitaulukonOikeaAlkio; alitaulukonVasenAlkio++)
            {
                if (ensimmaisenPuolenVasen > ensimmaisenPuolenOikea)
                    taulukko[alitaulukonVasenAlkio] = aputaulukko[toisenPuolenVasen++];
                else if (toisenPuolenVasen > toisenPuolenOikea)
                    taulukko[alitaulukonVasenAlkio] = aputaulukko[ensimmaisenPuolenVasen++];
                else
                    taulukko[alitaulukonVasenAlkio] = (aputaulukko[ensimmaisenPuolenVasen] <= aputaulukko[toisenPuolenVasen])
                        ? aputaulukko[ensimmaisenPuolenVasen++]
                        : aputaulukko[toisenPuolenVasen++];
            }
        }
    }
}
