using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
// using SFManagement.Enums;
// using SFManagement.Models;
// using SFManagement.Models.Transactions;
// using SFManagement.ViewModels;
//
// namespace SFManagement.Services;
//
// public class InternalTransactionService : BaseService<InternalTransaction>
// {
//     private readonly ClaimsPrincipal _user;
//
//     public InternalTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
//         httpContextAccessor)
//     {
//         _user = httpContextAccessor.HttpContext?.User;
//     }
//
//     public async Task<List<InternalTransaction>> Transfer(Guid toId, Guid fromId,
//         InternalTransactionTransferRequest obj)
//     {
//         // var transferId = Guid.NewGuid();
//         //
//         // // Validate recipient
//         // var clientTo = await context.Clients.FirstOrDefaultAsync(x => x.Id == toId);
//         // var managerTo = await context.Managers.FirstOrDefaultAsync(x => x.Id == toId);
//         // if (clientTo == null && managerTo == null)
//         //     throw new Exception("Recipient not found - neither client nor manager exists");
//         //
//         // // Validate sender
//         // var clientFrom = await context.Clients.FirstOrDefaultAsync(x => x.Id == fromId);
//         // var managerFrom = await context.Managers.FirstOrDefaultAsync(x => x.Id == fromId);
//         // if (clientFrom == null && managerFrom == null)
//         //     throw new Exception("Sender not found - neither client nor manager exists");
//         //
//         // // Create base transaction details
//         // var baseTransaction = new
//         // {
//         //     obj.Value,
//         //     obj.Coins,
//         //     obj.ExchangeRate,
//         //     TransferId = transferId,
//         //     obj.Date,
//         //     obj.Description
//         // };
//         //
//         // // Create recipient transaction
//         // var toInternalTransaction = new InternalTransaction
//         // {
//         //     Value = baseTransaction.Value,
//         //     Coins = baseTransaction.Coins,
//         //     ExchangeRate = baseTransaction.ExchangeRate,
//         //     TransferId = baseTransaction.TransferId,
//         //     Date = baseTransaction.Date,
//         //     Description = baseTransaction.Description,
//         //     InternalTransactionType = InternalTransactionType.Income,
//         //     ClientId = clientTo?.Id,
//         //     ManagerId = managerTo?.Id
//         // };
//         //
//         // // Create sender transaction
//         // var fromInternalTransaction = new InternalTransaction
//         // {
//         //     Value = baseTransaction.Value,
//         //     Coins = baseTransaction.Coins,
//         //     ExchangeRate = baseTransaction.ExchangeRate,
//         //     TransferId = baseTransaction.TransferId,
//         //     Date = baseTransaction.Date,
//         //     Description = baseTransaction.Description,
//         //     InternalTransactionType = InternalTransactionType.Expense,
//         //     ClientId = clientFrom?.Id,
//         //     ManagerId = managerFrom?.Id
//         // };
//         //
//         // await _entity.AddAsync(toInternalTransaction);
//         // await _entity.AddAsync(fromInternalTransaction);
//         // await context.SaveChangesAsync();
//         //
//         // return new List<InternalTransaction> { toInternalTransaction, fromInternalTransaction };
//         await Task.Yield();
//         return null;
//     }
//
//     public async Task<InternalTransaction> Approve(Guid internalTransactionId, InternalTransactionApproveRequest model)
//     {
//         // var internalTransaction = _entity.FirstOrDefault(x => x.Id == internalTransactionId);
//         //
//         // if (internalTransaction == null) throw new AppException("Não foi encontrado nenhuma transação.");
//         //
//         // if (internalTransaction.ApprovedAt.HasValue) throw new AppException("Transação já aprovada.");
//         //
//         // internalTransaction.ApprovedAt = DateTime.Now;
//         // internalTransaction.TagId = model.TagId;
//         // internalTransaction.ClientId = model.ClientId;
//         // internalTransaction.ManagerId = model.ManagerId;
//         // internalTransaction.BankId = model.BankId;
//         //
//         // if (_user != null)
//         //     internalTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);
//         //
//         // context.InternalTransactions.Update(internalTransaction);
//         //
//         // await context.SaveChangesAsync();
//         //
//         // return internalTransaction;
//         await Task.Yield();
//         return null;
//     }
//
//     public async Task<InternalTransaction> Unapprove(Guid internalTransactionId)
//     {
//         var internalTransaction = _entity.FirstOrDefault(x => x.Id == internalTransactionId);
//
//         if (internalTransaction == null) throw new AppException("Não foi encontrado nenhuma transação.");
//
//         if (!internalTransaction.ApprovedAt.HasValue) throw new AppException("Transação não está aprovada.");
//
//         internalTransaction.ApprovedAt = null;
//
//         context.InternalTransactions.Update(internalTransaction);
//
//         await context.SaveChangesAsync();
//
//         return internalTransaction;
//     }
//
//     public override async Task Delete(Guid id)
//     {
//         var obj = await _entity.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
//
//         if (obj != null)
//         {
//             obj.DeletedAt = DateTime.Now;
//
//             if (obj.TransferId.HasValue)
//             {
//                 var anotherTransaction =
//                     await _entity.FirstOrDefaultAsync(x => x.TransferId == obj.TransferId && !x.DeletedAt.HasValue);
//
//                 if (anotherTransaction != null)
//                 {
//                     anotherTransaction.DeletedAt = DateTime.Now;
//                     _entity.Update(anotherTransaction);
//                 }
//             }
//
//             _entity.Update(obj);
//
//             await context.SaveChangesAsync();
//         }
//     }
// }