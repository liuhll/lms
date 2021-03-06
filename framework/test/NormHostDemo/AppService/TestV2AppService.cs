using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Silky.Rpc.Runtime.Server;

namespace NormHostDemo.AppService
{
    [ServiceKey("v2", 1)]
    public class TestV2AppService : ITestAppService
    {
        public async Task<TestOut> Create(TestInput input)
        {
            return new()
            {
                Address = input.Address,
                Name = input.Name + "v2",
            };
        }

        public Task<string> Update(TestInput input)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> Delete(string name)
        {
            return name + "v2";
        }

        public Task<string> Search(TestInput query)
        {
            throw new System.NotImplementedException();
        }

        public string Form(TestInput query)
        {
            throw new System.NotImplementedException();
        }

        public async Task<TestOut> Get(string name)
        {
            return new()
            {
                Name = name + "v2"
            };
        }

        public Task<TestOut> GetById(long id)
        {
            throw new System.NotImplementedException();
        }


        public Task<string> UpdatePart(TestInput input)
        {
            throw new System.NotImplementedException();
        }
    }
}