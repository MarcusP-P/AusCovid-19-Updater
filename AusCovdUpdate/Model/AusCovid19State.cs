using System.Diagnostics.Contracts;

namespace AusCovdUpdate.Model
{
    public class AusCovid19State
    {
        public int Cases { get; set; }

        public int Deaths { get; set; }

        public int Recovered { get; set; }

        public int Tested { get; set; }

        public int Active { get; set; }

        public int InHospital { get; set; }

        public int InIcu { get; set; }

        public static AusCovid19State operator - (AusCovid19State left, AusCovid19State right)
        {
            Contract.Requires (left != null);
            Contract.Requires (right != null);

            return new AusCovid19State
            {
                Cases = left.Cases - right.Cases,
                Deaths = left.Deaths - right.Deaths,
                Recovered = left.Recovered - right.Recovered,
                Tested = left.Tested - right.Tested,
                Active = left.Active - right.Active,
                InHospital = left.InHospital - right.InHospital,
                InIcu = left.InIcu - right.InIcu,
            };
        }

        public static AusCovid19State Subtract (AusCovid19State left, AusCovid19State right)
        {
            Contract.Requires (left != null);
            Contract.Requires (right != null);

            return left - right;
        }
    }
}
