using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kandi.Algoritmit
{
    public class Mergesort : IAlgoritmi
    {
        /// <summary>
        /// Sekventiaalisen ajon aloituspiste. Kloonataan taulukko, jotta
        /// vältetään ylimääräistä allokointia ja kopiointia vaihtelemalla
        /// joka rekursiossa taulukoiden paikkoja.
        /// </summary>
        /// <param name="taulukko">Järjestettävä taulukko</param>
        public void SekventiaalinenJarjestys(int[] taulukko)
        {
            var apuTaulukko = (int[])taulukko.Clone();

            MergeSort(taulukko, apuTaulukko, 0, taulukko.Length - 1);

        }

        /// <summary>
        /// Rinnakkaisen ajon aloituspiste. Kloonataan taulukko, jotta
        /// vältetään ylimääräistä allokointia ja kopiointia vaihtelemalla
        /// joka rekursiossa taulukoiden paikkoja.
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
            void WorkAction(object objekti)
            {
                // Jokaisen säikeen ja sitä vastaavan osuuden indeksi.
                var indeksi = (int) objekti;
                
                // Lasketaan osuuden rajaavat indeksit
                var vasen = indeksi * osuudenKoko;
                var oikea = (indeksi + 1) * osuudenKoko - 1 + jakojaannos;

                // Jätetään jakojäännös pääsäikeelle.
                if (indeksi > 0)
                {
                    vasen += jakojaannos;
                }

                // Suoritetaan iteraation järjestäminen
                MergeSort(taulukko, aputaulukko, vasen, oikea);

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
                tyolaiset[i] = Task.Factory.StartNew(WorkAction, i + 1);
            }

            // Pääsäikeen suoritus.
            WorkAction(0);

            // Mikäli iteraatiot ovat parittomia, pitää aputaulukko kopioida taulukkoon,
            // sillä viimeisellä iteraatiolla aputaulukko on järjestetty.
            if (iteraatiot % 2 != 0)
                Array.Copy(aputaulukko, taulukko, taulukko.Length);
        }

        /// <summary>
        /// Mergesort, jota sekä sekventiaalinen että rinnakkainen versio käyttävät.
        /// Toimii rekursiivisesti.
        /// </summary>
        /// <param name="taulukko">Järjestettävä taulukko</param>
        /// <param name="aputaulukko">Kopio taulukosta</param>
        /// <param name="vasen">Osuuden vasen indeksi</param>
        /// <param name="oikea">Osuuden oikea indeksi</param>
        private static void MergeSort(int[] taulukko, int[] aputaulukko, int vasen, int oikea)
        {

            // Varmistetaan, että rajojen vasen indeksi ei ole suurempi tai sama kuin oikea indeksi.
            if (vasen >= oikea)
                return;
            var keskiosa = (vasen + oikea) / 2;
            
            // Rekursio, vaihdetaan aputaulukon ja taulukon paikkaa ylimääräisen kopioinnin
            // ja allokoinnin välttämiseksi.
            MergeSort(aputaulukko, taulukko, vasen, keskiosa);
            MergeSort(aputaulukko, taulukko, keskiosa + 1, oikea);
            
            // Yhdistetään ylläolevien järjestämisien mukaiset osuudet.
            Merge(taulukko, aputaulukko, vasen, keskiosa, keskiosa + 1, oikea, vasen);
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
