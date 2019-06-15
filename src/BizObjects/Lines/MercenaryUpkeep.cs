using LogObjects;

namespace BizObjects.Lines
{
    public class MercenaryUpkeep : Line
    {
        public MercenaryUpkeep(LogDatum logLine, decimal cost, decimal waivedCost, int bayleMarks, Zone zone = null) : base(logLine, zone)
        {
            Cost = cost;
            WaivedCost = waivedCost;
            BayleMarks = bayleMarks;
        }

        public decimal Cost { get; }
        public decimal WaivedCost { get; }
        public int BayleMarks { get; }
    }
}
