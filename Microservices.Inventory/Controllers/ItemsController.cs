using Microservices.Common.Interfaces;
using Microservices.Inventory.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microservices.Inventory.Dtos.Dtos;

namespace Microservices.Inventory.Controllers
{
    //in future, move all functionality in implementation class and use controller just for routing.

    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _repository;
        private readonly IHttpCustomClient _httpCustomClient;

        public ItemsController(IRepository<InventoryItem> repository, IHttpCustomClient httpCustomClient)
        {
            _repository = repository;
            _httpCustomClient = httpCustomClient;
        }

        [HttpGet(Name ="getUsersInventory")]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync (Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest(new { Message = "Cannot retrieve items for default user"});

            
            var catalogItems = await _httpCustomClient.GetGenericAsync<CatalogItemDto>("items");
            var userInventoryItemEntities = (await _repository.GetAllAsync(items => items.UserId == userId)).ToList();

            var userInventoryItemDtos = userInventoryItemEntities.Select(inventoryItem =>
            {
                var  catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Description, catalogItem.Name);
            });

            return Ok(userInventoryItemDtos);
        }

        [HttpPost(Name ="modifyUsersInventory")]
        public async Task<ActionResult> PostAsync (GrantItemDto request)
        {
     
            
            var usersInventory = new List<InventoryItem>();

            if (request.UserId != Guid.Empty)
            {
                usersInventory = (await _repository.GetAllAsync(item => item.UserId == request.UserId)).ToList();

                if (usersInventory.Select(it => it.CatalogItemId).ToList().Contains(request.CatalogItemId))
                {
                    //update item
                    int index = usersInventory.IndexOf(usersInventory.Where(it => it.UserId == request.UserId).FirstOrDefault());
                    usersInventory[index].Quantity = request.Quantity;
                    await _repository.UpdateAsync(usersInventory[index]);
                }
                else
                {
                    var itemToInsert = new InventoryItem
                    {
                        UserId = request.UserId,
                        CatalogItemId = request.CatalogItemId,
                        Quantity = request.Quantity,
                        AcquiredDate = DateTimeOffset.UtcNow
                    };

                    await _repository.InsertAsync(itemToInsert);
                }
                return Ok();
            }
            else
            {
                //insert item
                var itemToInsert = new InventoryItem
                {
                    UserId = Guid.NewGuid(),
                    CatalogItemId = request.CatalogItemId,
                    Quantity = request.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await _repository.InsertAsync(itemToInsert);
                return Ok(new { Message = $"New user Id created -> {itemToInsert.UserId}" });
            }
     
        }
    }
}
