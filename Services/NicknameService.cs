using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Services
{
    public class NicknameService : BaseService<Nickname>
    {
        public NicknameService(DataContext context) : base(context)
        {
        }

        public async Task<List<Nickname>> GetByClientId(Guid clientId)
        {
            return await context.Nicknames.Where(x => x.ClientId == clientId).ToListAsync();
        }
    }
}
