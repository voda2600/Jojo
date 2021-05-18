using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Models
{
    public class Category
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string PhotoPath { get; set; } = "/img/no-image.png";
        public List<Characteristic> Characteristics { get; set; }
    }
    public class Characteristic
    {
        [Required]
        public string Name { get; set; }
        
        
        public string Unit { get; set; }
        [Required]
        public string Type { get; set; }
        
        public List<string> ValueOptions { get; set; } = new List<string>();
        
        public dynamic Value { get; set; }
        
        [BsonIgnore]
        public string ValueString { get; set; }
    }
}