using PdfFileWriter;
using ZXing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            TicketPdfMaker t = new TicketPdfMaker("Samples/Ticketmaster_l1L9x.pkpass");
            //t.ReadJson("Ticketmaster_Yw7iB/pass.json");
            t.Test(false, "Ticketmaster_l1L9x.pdf");
        }
    }
}
