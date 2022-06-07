using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yakudo.Cqrs;
using Yakudo.Next.Abstractions.Messaging;
using Yakudo.Next.Domain.Labelling.Plus;
using Yakudo.Next.Domain.Products.Products;
using Yakudo.Next.Framework.Abstractions.DataAnnotations;

// Komenda która usuwa produkt z bazy
//
namespace Yakudo.Next.Application.Products.Products.Commands
{
    [Authorize(Policy = Policies.Products.ProductTypes.Delete)]
    public class DeleteProductRequest : Command
    {
        public override string ToString()
        {
            return $"Remove product {Id}";
        }
    }

    public class DeleteProductHandler : ICommandHandler<DeleteProductRequest>
    {
        private readonly IApplicationDbContext _dbContext;

        public DeleteProductHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(DeleteProductRequest command, CancellationToken cancellationToken = default)
        {
            var aggregate = await _dbContext.Set<Product>().GetExistingAsync(m => m.Id == command.Id);
            var subcodes = await _dbContext.Set<PluVariant>().Where(x => x.PluId == aggregate.Id).ToListAsync(cancellationToken);

            _dbContext.Set<PluVariant>().RemoveRange(subcodes);
            _dbContext.Set<Product>().Remove(aggregate);

            await _dbContext.SaveChangesAsync();
        }
    }
}