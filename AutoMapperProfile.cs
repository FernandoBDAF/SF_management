using AutoMapper;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Bank, BankResponse>();
            CreateMap<BankRequest, Bank>();

            CreateMap<BankTransaction, BankResponse>();
            CreateMap<BankTransactionRequest, BankTransaction>();

            CreateMap<Client, ClientResponse>();
            CreateMap<ClientRequest, Client>();

            CreateMap<Ofx, OfxResponse>();
            CreateMap<OfxRequest, Ofx>();
        }
    }
}
