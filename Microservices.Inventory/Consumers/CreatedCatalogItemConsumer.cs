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
    public class CreatedCatalogItemConsumer : IConsumer<InsertedItemDto>
    {
        internal IRepository<CatalogItem> repository;

        public CreatedCatalogItemConsumer(IRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<InsertedItemDto> context)
        {
            var rabbitMqMessage = context.Message;

            var item = await repository.GetByIdAsync(rabbitMqMessage.Id);

            if (item != null)
            {
                return; //already consumed (duplicate record) do not insert again
            }

            var itemToInsert = new CatalogItem
            {
                Id = rabbitMqMessage.Id,
                Description = rabbitMqMessage.Description,
                Name = rabbitMqMessage.Name
            };

            await repository.InsertAsync(itemToInsert);
        }
    }
}
