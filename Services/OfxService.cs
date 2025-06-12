using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Models.Transactions;

namespace SFManagement.Services;

public class OfxService : BaseService<Ofx>
{
    public OfxService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public override async Task<Ofx> Get(Guid id)
    {
        var query = await context.Ofxs.Include(x => x.OfxTransactions)
            .Include(x => x.Bank)
            .FirstOrDefaultAsync(x => x.Id == id);
        return query ?? throw new NullReferenceException("The ofx was not found");
    }

    public override async Task<List<Ofx>> List()
    {
        return await context.Ofxs
                                .Include(x => x.Bank)
                                .Where(x => !x.DeletedAt.HasValue)
                                .ToListAsync();
    }

    public async Task<Ofx> Add(IFormFile formFile, Guid bankId)
    {
        var ofx = new Ofx(ParseOfxContent(formFile, bankId), bankId, formFile.FileName);
        var toExcluded = new List<OfxTransaction>();

        foreach (var ofxTransaction in ofx.OfxTransactions)
            if (context.OfxTransactions.Any(x => x.FitId == ofxTransaction.FitId && x.BankId == bankId))
                toExcluded.Add(ofxTransaction);

        ofx.OfxTransactions = ofx.OfxTransactions.Where(x => !toExcluded.Any(te => te.FitId == x.FitId)).ToList();

        await context.Ofxs.AddAsync(ofx);
        await context.SaveChangesAsync();

        return ofx;
    }

    private List<OfxTransaction> ParseOfxContent(IFormFile formFile, Guid bankId)
    {
        using (var stream = new StreamReader(formFile.OpenReadStream()))
        {
            var lines = new List<string?>();

            while (stream.Peek() >= 0) lines.Add(stream.ReadLine());

            var tags = lines.Where(x =>
                x.Contains("<STMTTRN>") || x.Contains("<TRNTYPE>") || x.Contains("<DTPOSTED>") ||
                x.Contains("<TRNAMT>") || x.Contains("<FITID>") || x.Contains("<CHECKNUM>") || x.Contains("<MEMO>"));

            var rootElement = new XElement("root");
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


            var list = new List<OfxTransaction>();

            foreach (var element in rootElement.Descendants("STMTTRN")) list.Add(new OfxTransaction(element, bankId));

            return list;
        }
    }

    private string GetTagName(string line)
    {
        var pos_init = line.IndexOf("<") + 1;
        var pos_end = line.IndexOf(">");

        pos_end = pos_end - pos_init;

        return line.Substring(pos_init, pos_end);
    }

    private string GetTagValue(string line)
    {
        var pos_init = line.IndexOf(">") + 1;

        var retValue = line.Substring(pos_init).Trim();

        if (retValue.IndexOf("[") != -1) retValue = retValue.Substring(0, 8);

        return retValue;
    }
}