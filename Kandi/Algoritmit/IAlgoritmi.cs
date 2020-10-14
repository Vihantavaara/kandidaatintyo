using System;

namespace Kandi.Algoritmit
{
    public interface IAlgoritmi
    {
        public int[] SekventiaalinenJarjestys(int[] taulukko);
        public int[] RinnakkainenJarjestys(int saikeidenMaara, int[] taulukko);
    }
}
