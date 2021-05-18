using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MongoDB.Bson;
#nullable enable
namespace DAL.Models
{
    
    public class Product
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        
        public double Price { get; set; }
        public string Brand { get; set; }
        public string CategoryName { get; set; }
        
        public double Rating { get; set; }
        
        public double RatingQty { get; set; }
        public string PhotoPath { get; set; } = "/img/no-image.png";

        
        public bool IsAvailable()
        {
            ProductRepo repo = new ProductRepo();
            if (repo.GetCategoryByName(CategoryName).Result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public List<Characteristic>? Characteristics { get; set; }
        
    }
}