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

        //MAYBE retire this logic, We do not want to delete the item from Inventory when it's being deleted from the Auction House so we need to maintain the data and keep receiving Descreption etc. for user's Inventory.
        public async Task Consume(ConsumeContext<DeletedItemDto> context)
        {
            /*var rabbitMqMessage = context.Message;

            var item = await repository.GetByIdAsync(rabbitMqMessage.Id);

            if (item != null)
                await repository.DeleteAsync(item.Id);

            return;*/
        }
    }
}
