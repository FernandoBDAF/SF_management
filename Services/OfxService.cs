using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using System.Xml;
using System.Xml.Linq;

namespace SFManagement.Services
{
    public class OfxService : BaseService<Ofx>
    {
        public OfxService(DataContext context) : base(context)
        {
        }

        public override async Task<Ofx> Get(Guid id)
        {
            var query = await context.Ofxs.Include(x => x.BankTransactions).Include(x => x.Bank).FirstOrDefaultAsync(x => x.Id == id);
            return query;
        }

        public override async Task<List<Ofx>> List()
        {
            return await context.Ofxs.Include(x => x.Bank).Where(x => !x.DeletedAt.HasValue).ToListAsync();
        }

        public async Task<Ofx> Add(IFormFile formFile, Guid bankId)
        {
            await Task.Yield();

            var ofx = new Ofx(ParseOfxContent(formFile, bankId), bankId, formFile.FileName);
            var toExcluded = new List<BankTransaction>();

            foreach (var bankTransaction in ofx.BankTransactions)
                if (context.BankTransactions.Any(x => x.FitId == bankTransaction.FitId && x.BankId == bankId))
                    toExcluded.Add(bankTransaction);

            ofx.BankTransactions = ofx.BankTransactions.Where(x => !toExcluded.Any(te => te.FitId == x.FitId)).ToList();

            await context.Ofxs.AddAsync(ofx);
            await context.SaveChangesAsync();

            return ofx;
        }

        private List<BankTransaction> ParseOfxContent(IFormFile formFile, Guid bankId)
        {
            using (var stream = new StreamReader(formFile.OpenReadStream()))
            {
                var lines = new List<string?>();

                while (stream.Peek() >= 0)
                {
                    lines.Add(stream.ReadLine());
                }

                var tags = lines.Where(x => x.Contains("<STMTTRN>") || x.Contains("<TRNTYPE>") || x.Contains("<DTPOSTED>") || x.Contains("<TRNAMT>") || x.Contains("<FITID>") || x.Contains("<CHECKNUM>") || x.Contains("<MEMO>"));

                XElement rootElement = new XElement("root");
                XElement son = null;

                foreach (var l in tags)
                {
                    if (l.IndexOf("<STMTTRN>") != -1)
                    {
                        son = new XElement("STMTTRN");
                        rootElement.Add(son);

                        continue;
                    }

                    var tagName = GetTagName(l);
                    var elSon = new XElement(tagName);

                    elSon.Value = GetTagValue(l);

                    son?.Add(elSon);
                }


                var list = new List<BankTransaction>();

                foreach (var element in rootElement.Descendants("STMTTRN"))
                {
                    list.Add(new BankTransaction(element, bankId));
                }

                return list;
            }
        }

        private string GetTagName(string line)
        {
            int pos_init = line.IndexOf("<") + 1;
            int pos_end = line.IndexOf(">");

            pos_end = pos_end - pos_init;

            return line.Substring(pos_init, pos_end);
        }

        private string GetTagValue(string line)
        {
            int pos_init = line.IndexOf(">") + 1;

            string retValue = line.Substring(pos_init).Trim();

            if (retValue.IndexOf("[") != -1)
            {
                retValue = retValue.Substring(0, 8);
            }

            return retValue;
        }
    }
}
