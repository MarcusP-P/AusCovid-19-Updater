using System;
using System.Diagnostics.Contracts;

namespace AusCovdUpdate.Model
{
    public class Covid19Aus
    {
        public DateTime Date { get; set; }

        public AusCovid19State ACT { get; set; }

        public AusCovid19State NSW { get; set; }

        public AusCovid19State NT { get; set; }

        public AusCovid19State QLD { get; set; }

        public AusCovid19State SA { get; set; }

        public AusCovid19State TAS { get; set; }

        public AusCovid19State VIC { get; set; }

        public AusCovid19State WA { get; set; }

        public static Covid19Aus operator - (Covid19Aus left, Covid19Aus right)
        {
            Contract.Requires (left != null);
            Contract.Requires (right != null);

            return new Covid19Aus
            {
                Date = left.Date,
                NSW = left.NSW - right.NSW,
                QLD = left.QLD - right.QLD,
                VIC = left.VIC - right.VIC,
                SA = left.SA - right.SA,
                WA = left.WA - right.WA,
                TAS = left.TAS - right.TAS,
                ACT = left.ACT - right.ACT,
                NT = left.NT - right.NT,
            };
        }

        public static Covid19Aus Subtract (Covid19Aus left, Covid19Aus right)
        {
            Contract.Requires (left != null);
            Contract.Requires (right != null);

            return left - right;
        }
    }
}
