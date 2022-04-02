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
        private readonly IRepository<InventoryItem> _inventoryRepo;
        private readonly IRepository<CatalogItem> _catalogRepo;
        

        public ItemsController(IRepository<InventoryItem> inventoryRepo, IRepository<CatalogItem> catalogRepo)
        {
            _inventoryRepo = inventoryRepo;
            _catalogRepo = catalogRepo;
        }

        [HttpGet(Name ="getUsersInventory")]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync (Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest(new { Message = "Cannot retrieve items for default user"});
            
            var userInventoryItemEntities = (await _inventoryRepo.GetAllAsync(items => items.UserId == userId)).ToList();
            var itemIds = userInventoryItemEntities.Select(item => item.CatalogItemId)?.ToList();
            var catalogItems = (await _catalogRepo.GetAllAsync(items=>itemIds.Contains(items.Id))).ToList();

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
                usersInventory = (await _inventoryRepo.GetAllAsync(item => item.UserId == request.UserId)).ToList();

                if (usersInventory.Select(it => it.CatalogItemId).ToList().Contains(request.CatalogItemId))
                {
                    //update item
                    int index = usersInventory.IndexOf(usersInventory.Where(it => it.UserId == request.UserId).FirstOrDefault());
                    usersInventory[index].Quantity = request.Quantity;
                    await _inventoryRepo.UpdateAsync(usersInventory[index]);
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

                    await _inventoryRepo.InsertAsync(itemToInsert);
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

                await _inventoryRepo.InsertAsync(itemToInsert);
                return Ok(new { Message = $"New user Id created -> {itemToInsert.UserId}" });
            }
     
        }
    }
}
