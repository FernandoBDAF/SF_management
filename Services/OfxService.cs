using SFManagement.Data;
using SFManagement.Models;
using System.Xml;

namespace SFManagement.Services
{
    public class OfxService : BaseService<Ofx>
    {

        public OfxService(DataContext context) : base(context)
        {
        }

        public async Task<Ofx> Add(IFormFile formFile, Guid bankId)
        {
            await Task.Yield();

            string fileContent;

            using (var stream = new StreamReader(formFile.OpenReadStream()))
            {
                fileContent = await stream.ReadToEndAsync();
            }

            var ofx = new Ofx(ParseOfxContent(fileContent, bankId), bankId);
            var toExcluded = new List<BankTransaction>();

            foreach (var bankTransaction in ofx.BankTransactions)
                if (context.BankTransactions.Any(x => x.FitId == bankTransaction.FitId))
                    toExcluded.Add(bankTransaction);

            ofx.BankTransactions = ofx.BankTransactions.Where(x => !toExcluded.Any(te => te.FitId == x.FitId)).ToList();

            await context.Ofxs.AddAsync(ofx);
            await context.SaveChangesAsync();

            return ofx;
        }

        private List<BankTransaction> ParseOfxContent(string fileContent, Guid bankId)
        {
            var list = new List<BankTransaction>();

            using (var stringReader = new StringReader(fileContent))
            {
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.IsStartElement())
                        {
                            switch (xmlReader.Name)
                            {
                                case "STMTTRN":
                                    list.Add(new BankTransaction(xmlReader, bankId));
                                    break;
                            }
                        }
                    }
                }
            }

            return list;
        }
    }
}
