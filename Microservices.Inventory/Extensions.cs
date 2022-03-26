using Microservices.Inventory.Entities;
using static Microservices.Inventory.Dtos.Dtos;

namespace Microservices.Inventory
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto (this InventoryItem item,string Description, string Name)
        {
            return new InventoryItemDto (item.CatalogItemId, Name, Description, item.Quantity,item.AcquiredDate);
        }
    }
}
