using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Kandi.Tietueet
{
    public class Tulokset
    {
        [Index(0)]
        public long RinnakkainenSuoritusaika { get; set; }
        [Index(1)]
        public long SekventiaalinenSuoritusaika { get; set; }
        
    }

    public class TuloksetMap : ClassMap<Tulokset>
    {
        public TuloksetMap()
        {
            Map(m => m.RinnakkainenSuoritusaika).Index(0).Name("RinnakkainenSuoritusaika");
            Map(m => m.SekventiaalinenSuoritusaika).Index(1).Name("SekventiaalinenSuoritusaika");
        }
    }
}
