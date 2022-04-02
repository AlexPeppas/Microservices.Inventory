﻿using MassTransit;
using Microservices.Common.Interfaces;
using Microservices.Inventory.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microservices.Contracts.CatalogContracts;

namespace Microservices.Inventory.Consumers
{
    public class UpdatedCatalogItemConsumer : IConsumer<UpdatedItemDto>
    {
        internal IRepository<CatalogItem> repository;

        public UpdatedCatalogItemConsumer(IRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<UpdatedItemDto> context)
        {
            var rabbitMqMessage = context.Message;

            var item = await repository.GetByIdAsync(rabbitMqMessage.Id);

            if (item == null) // if it does not exist create it 
            {
                var itemToInsert = new CatalogItem
                {
                    Id = rabbitMqMessage.Id,
                    Description = rabbitMqMessage.Description,
                    Name = rabbitMqMessage.Name
                };

                await repository.InsertAsync(itemToInsert);
            }
            else
            {
                var itemToUpdate = new CatalogItem
                {
                    Id = rabbitMqMessage.Id,
                    Description = rabbitMqMessage.Description,
                    Name = rabbitMqMessage.Name
                };

                await repository.UpdateAsync(itemToUpdate);
            }
            return;
        }
    }
}
