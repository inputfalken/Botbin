using System;
using System.Threading.Tasks;

namespace Botbin.Services.Interfaces {
    public interface IGifProvider {
        Task<Uri> Random();
        Task<Uri> Term(string term);
    }
}