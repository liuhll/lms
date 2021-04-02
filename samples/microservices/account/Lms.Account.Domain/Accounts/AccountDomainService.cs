using System.Threading.Tasks;
using Arch.EntityFrameworkCore.UnitOfWork;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.AutoMapper;
using Lms.Core;
using Lms.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Account.Domain.Accounts
{
    public class AccountDomainService : IAccountDomainService
    {
        public async Task<Account> Create(Account account)
        {
            using var unitOfWork = GetUnitOfWork();
            var accountRepository = unitOfWork.GetRepository<Account>();
            var exsitAccountCount = accountRepository.Count(p => p.Name == account.Name);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{account.Name}名称的账号");
            }

            exsitAccountCount = accountRepository.Count(p => p.Email == account.Email);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{account.Email}Email的账号");
            }

            var insertResult = await accountRepository.InsertAsync(account);
            await unitOfWork.SaveChangesAsync();
            return insertResult.Entity;
        }

        public async Task<Account> GetAccountByName(string name)
        {
            using var unitOfWork = GetUnitOfWork();
            var accountRepository = unitOfWork.GetRepository<Account>();
            var accountEntry = await accountRepository.GetFirstOrDefaultAsync(predicate: p => p.Name == name);
            if (accountEntry == null)
            {
                throw new BusinessException($"不存在名称为{name}的账号");
            }

            return accountEntry;
        }

        public async Task<Account> GetAccountById(long id)
        {
            using var unitOfWork = GetUnitOfWork();
            var accountRepository = unitOfWork.GetRepository<Account>();
            var accountEntry = await accountRepository.GetFirstOrDefaultAsync(predicate: p => p.Id == id);
            if (accountEntry == null)
            {
                throw new BusinessException($"不存在Id为{id}的账号");
            }

            return accountEntry;
        }

        public async Task<Account> Update(UpdateAccountInput input)
        {
            var account = await GetAccountById(input.Id);
            using var unitOfWork = GetUnitOfWork();
            var accountRepository = unitOfWork.GetRepository<Account>();
            if (!account.Email.Equals(input.Email))
            {
                var exsitAccountCount = accountRepository.Count(p => p.Email == input.Email);
                if (exsitAccountCount > 0)
                {
                    throw new BusinessException($"系统中已经存在Email为{input.Email}的账号");
                }
            }

            if (!account.Name.Equals(input.Name))
            {
                var exsitAccountCount = accountRepository.Count(p => p.Name == input.Name);
                if (exsitAccountCount > 0)
                {
                    throw new BusinessException($"系统中已经存在Name为{input.Name}的账号");
                }
            }

            account = input.MapTo(account);

            accountRepository.Update(account);
            await unitOfWork.SaveChangesAsync();
            return account;
        }

        private IUnitOfWork GetUnitOfWork()
        {
            return EngineContext.Current.ServiceProvider.CreateScope().ServiceProvider.GetService<IUnitOfWork>();
        }
    }
}