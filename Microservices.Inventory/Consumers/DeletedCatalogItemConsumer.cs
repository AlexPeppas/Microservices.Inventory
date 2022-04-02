using MassTransit;
using Microservices.Common.Interfaces;
using Microservices.Inventory.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microservices.Contracts.CatalogContracts;

namespace Microservices.Inventory.Consumers
{
    public class DeletedCatalogItemConsumer : IConsumer<DeletedItemDto>
    {
        internal IRepository<CatalogItem> repository;

        public DeletedCatalogItemConsumer(IRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<DeletedItemDto> context)
        {
            var rabbitMqMessage = context.Message;

            var item = await repository.GetByIdAsync(rabbitMqMessage.Id);

            if (item != null)
                await repository.DeleteAsync(item.Id);

            return;
        }
    }
}
