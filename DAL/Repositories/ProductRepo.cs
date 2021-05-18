
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using ViewModels;
using ViewModels.HelperModels;

namespace DAL.Repositories
{
    public class ProductRepo : Repo
    {
        public ProductRepo() : base()
        {
        }

        public ProductRepo(IMongoClient client, MarketContext marketContext) : base(client, marketContext)
        {
        }

        public List<Category> GetSearchCategory(string text)
        {
         
            if (text is null)
                text = " ";
            var database = _client.GetDatabase("Market");
            IMongoCollection<Product> collection = ProductConnect();
            var _products = collection.Find(Builders<Product>.Filter.Text(text)).ToList();
            List<string> _nameCategorys = _products.Select(x => x.CategoryName).ToList();
            var collectionCategory = database.GetCollection<Category>("Category").Find(x=> _nameCategorys.Contains(x.Name)).ToList();
            return collectionCategory;
        }

        public async Task<List<Product>> GetProductsFromList(List<string> ids)
        {
            IMongoCollection<Product> collection = ProductConnect();
            var filter = Builders<Product>.Filter.In("_id", ids.Select(p => new ObjectId(p)));
            List<Product> result = new List<Product>();
            using (var cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {

                    result.AddRange(cursor.Current.ToList());
                }
            }
            return result;
        }

        public async Task CheckAndDeleteValueOption(string id)
        {
            var database = _client.GetDatabase("Market");
            var collectionProduct = database.GetCollection<Product>("Product");    
            var collectionCateg = database.GetCollection<Category>("Category");
            var filter = new BsonDocument("_id", new ObjectId(id));
            Product result = null;
            using (var cursor = await collectionProduct.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    result = cursor.Current.FirstOrDefault();
                }
            }
            List<Characteristic> characteristics = result.Characteristics;
            foreach(var value in characteristics)
            {
              
                if (value.Type == "string")
                {
                    string Neededvalue = (string)value.Value;
                    if (IFthisValueSolo(value))
                    {
                        var NeededCategory = await GetCategoryByName(result.CategoryName);
                        var valueOp = NeededCategory.Characteristics.FirstOrDefault(k => k.Name == value.Name).ValueOptions;
                        valueOp.Remove(Neededvalue);                         
                        NeededCategory.Characteristics.FirstOrDefault(k => k.Name == value.Name).ValueOptions = valueOp;
                        collectionCateg.ReplaceOne(new BsonDocument("_id", NeededCategory.Id), NeededCategory);

                    }
                }
            }



        }
        public bool IFthisValueSolo(Characteristic value)
        {
            var database = _client.GetDatabase("Market");
            var collectionProduct = database.GetCollection<Product>("Product");
            var _productsWithSameValue = collectionProduct.Find(Builders<Product>.Filter.Text((string)value.Value)).ToList();
            if (_productsWithSameValue.Count <= 1) return true;
            else
            {
                int count = 0;
                foreach(var pr in _productsWithSameValue)
                {
                    if (pr.Characteristics.Any(x =>x.Name==value.Name && x.Type == "string" && (string)x.Value == (string)value.Value))
                    {
                        count = count + 1;
                    }
                }
                if (count <= 1) return true;
                else return false;
            }
        }


        public List<Category> GetCategoryNames()
        {
            IMongoCollection<Category> collection = ConnectCategory();
            var filter = new BsonDocument();
            return collection.Find(filter).Project(p => new Category() {Name = p.Name, PhotoPath = p.PhotoPath})
                .ToList();
        }

        public async Task<Product> GetProductsById(string product)
        {
            IMongoCollection<Product> collection = ProductConnect();
            var filter = new BsonDocument("_id", new ObjectId(product));
            Product result = null;
            using (var cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    result = cursor.Current.FirstOrDefault();
                }
            }

            return result;
        }

        private IMongoCollection<Product> ProductConnect()
        {
            var database = _client.GetDatabase("Market");
            var collection = database.GetCollection<Product>("Product");
            return collection;
        }

        public List<string> GetAllProductOfOrders(List<int> orderIds)
        {
           return _context.OrderProducts
                .Where(p => orderIds.Contains(p.OrderId))
                .Select(p => p.Product)
                .ToList();
        }

        public bool IFthisPhoroUseMoreProduct(string path)
        {
            IMongoCollection<Product> collection = ProductConnect();
            var filter = new BsonDocument("PhotoPath", path);
            var result = collection.Find(filter).ToList();
            if (result.Count > 1)
                return true;
            else
                return false;

        }

        public async Task<Category> GetCategoryByName(string name)
        {
            IMongoCollection<Category> collection = ConnectCategory();
            var filter = new BsonDocument("Name", name);
            Category result = null;
            using (var cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    result = cursor.Current.FirstOrDefault();
                }
            }

            return result;
        }

        public async Task<Product> GetProductById(string id)
        {
            IMongoCollection<Product> collection = ProductConnect();
            var filter = new BsonDocument("_id", new ObjectId(id));
            Product result = null;
            using (var cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    result = cursor.Current.FirstOrDefault();
                }
            }

            return result;
        }

        public List<Product> GetProductsOfCategory(string category, int offset = 0, int qty = Int32.MaxValue)
        {
            IMongoCollection<Product> collection = ProductConnect();
            var filter = new BsonDocument("CategoryName", category);
            var result = collection.Find(filter)
                .Skip(offset)
                .Limit(qty)
                .ToList();
            return result;
        }

        public List<Product> GetProductWithPhoto(string photoPath)
        {
            IMongoCollection<Product> collection = ProductConnect();
            var filter = new BsonDocument("PhotoPath", photoPath);
            var result = collection.Find(filter).ToList();
            return result;
        }

        public void AddCategory(Category category)
        {
            IMongoCollection<Category> collection = ConnectCategory();
            collection.InsertOne(category);
        }

        public void UpdateProductById(Product product, string _id)
        {
            product._id = new ObjectId(_id);
            IMongoCollection<Product> collection = ProductConnect();
            collection.ReplaceOne(new BsonDocument("_id", new ObjectId(_id)),product);


        }

        public void DeleteCategory(string categoryName)
        {
            IMongoCollection<Category> collection = ConnectCategory();
            var filter = new BsonDocument("Name", categoryName);
            collection.DeleteOne(filter);
        }
        public void DeleteProduct(string id)
        {
            IMongoCollection<Product> collection = ProductConnect();
            var filter = new BsonDocument("_id", new ObjectId(id));
            collection.DeleteOne(filter);
        }


        #region FilterWork

        public List<FilterDefinition<Product>> Filters(List<FilterUnit> filterUnits)
        {
            var filters = new List<FilterDefinition<Product>>();
            foreach (var filterUnit in filterUnits)
            {
                FilterDefinition<Product> filter = null;
                if (filterUnit.Type == "string" && filterUnit.Values != null)
                {

                    filter = Builders<Product>.Filter
                        .Where(p => p.Characteristics
                            .Any(x => x.Name == filterUnit.Name && filterUnit.Values.Contains(x.Value as string)));
                }
                else if (filterUnit.Type == "bool" && filterUnit.CheckBox != null)
                {
                    filter = Builders<Product>.Filter.Where(p => p.Characteristics
                        .Any(x => x.Name == filterUnit.Name && (x.Value as bool?) == filterUnit.CheckBox.Value));
                }
                else if (filterUnit.Type == "number")
                {
                    var maxFilter = FilterDefinition<Product>.Empty;
                    var minFilter = FilterDefinition<Product>.Empty;
                    if (filterUnit.MaxValue.HasValue)
                    {
                        maxFilter = Builders<Product>.Filter.Where(p => p.Characteristics
                            .Any(x => x.Name == filterUnit.Name &&
                                      (x.Value as double?) <= filterUnit.MaxValue.Value));
                    }

                    if (filterUnit.MinValue.HasValue)
                    {
                        minFilter = Builders<Product>.Filter.Where(p => p.Characteristics
                            .Any(x => x.Name == filterUnit.Name &&
                                      (x.Value as double?) >= filterUnit.MinValue.Value));
                    }

                    filter = maxFilter & minFilter;
                }

                if (filter != null)
                {
                    filters.Add(filter);
                }
            }

            return filters;
        }


        public FilterDefinition<Product> RatingFilter(ProductFiltersViewModel model)
        {
            FilterDefinition<Product> ratingFilter;
            var maxFilter = FilterDefinition<Product>.Empty;
            var minFilter = FilterDefinition<Product>.Empty;
            if (model.MaxRating.HasValue)
            {
                maxFilter = Builders<Product>.Filter.Lte("Rating", model.MaxRating.Value);
            }

            if (model.MinRating.HasValue)
            {
                minFilter = Builders<Product>.Filter.Gte("Rating", model.MinRating.Value);
            }

            ratingFilter = maxFilter & minFilter;
            return ratingFilter;
        }

        public SortDefinition<Product> RatingSorting(ProductFiltersViewModel model)
        {
            if (model.SortRating == "Low")
            {
                return Builders<Product>.Sort.Ascending(x => x.Rating);
            }

            if (model.SortRating == "High")
            {
                return Builders<Product>.Sort.Descending(x => x.Rating);
            }
            else
            {
                return null;
            }
        }

        public List<FilterDefinition<Product>> StaticFilters(ProductFiltersViewModel model)
        {
            var categoryFilter = CategoryFilter(model);
            FilterDefinition<Product> priceFilter = PriceFilter(model);
            var ratingFilter = RatingFilter(model);
            if (model.inputSearch == null) {
            return new List<FilterDefinition<Product>> { categoryFilter, priceFilter, ratingFilter };
            }
            else
            {
                var searchFilter = Builders<Product>.Filter.Text(model.inputSearch);
                return new List<FilterDefinition<Product>> { searchFilter, categoryFilter, priceFilter, ratingFilter };
            }
        }

        private SortDefinition<Product> PriceSorting(ProductFiltersViewModel model)
        {
            if (model.SortPrice == "Low")
            {
                return Builders<Product>.Sort.Ascending(x => x.Price);
            }

            if (model.SortPrice == "High")
            {
                return Builders<Product>.Sort.Descending(x => x.Price);
            }
            else
            {
                return null;
            }
        }

        private FilterDefinition<Product> PriceFilter(ProductFiltersViewModel model)
        {

            FilterDefinition<Product> priceFilter;
            var maxFilter = FilterDefinition<Product>.Empty;
            var minFilter = FilterDefinition<Product>.Empty;
            if (model.MaxPrice.HasValue)
            {
                maxFilter = Builders<Product>.Filter.Lte("Price", model.MaxPrice.Value);
            }

            if (model.MinPrice.HasValue)
            {
                minFilter = Builders<Product>.Filter.Gte("Price", model.MinPrice.Value);
            }

            priceFilter = maxFilter & minFilter;
            return priceFilter;
        }

        private FilterDefinition<Product> CategoryFilter(ProductFiltersViewModel model)
        {
            return Builders<Product>.Filter.Eq("CategoryName", model.Category);
        }

        private SortDefinition<Product> GetSortings(ProductFiltersViewModel model)
        {
            List<SortDefinition<Product>> sortings = new List<SortDefinition<Product>>();
            var priceSorting = PriceSorting(model);
            if (priceSorting != null)
            {
                sortings.Add(priceSorting);
            }

            var ratingSorting = RatingSorting(model);
            if (ratingSorting != null)
            {
                sortings.Add(ratingSorting);
            }

            if (sortings.Count > 0)
            {
                return Builders<Product>.Sort.Combine(sortings);
            }
            else
            {
                return null;

            }
        }

        public void AddProduct(Product product)
        {
            IMongoCollection<Product> collection = ProductConnect();
            collection.InsertOne(product);
        }

        public List<HProductIdWithQty> GetProductIdsOfOrder(int id)
        {
            return _context.OrderProducts
                .Where(p => p.OrderId == id)
                .Select(p => new HProductIdWithQty(p.Product, p.Qty))
                .ToList();
        }


        public void AddValueOption(string categoryName, string characteristicName, string newOption)
        {

            if (newOption != null)
            {
                IMongoCollection<Category> collection = ConnectCategory();
                collection.FindOneAndUpdate
                (
                    p => p.Name == categoryName && p.Characteristics.Any(s => s.Name == characteristicName),
                    Builders<Category>.Update.AddToSet(p => p.Characteristics[-1].ValueOptions, newOption)
                );
            }
        }

        private IMongoCollection<Category> ConnectCategory()
        {
            var database = _client.GetDatabase("Market");
            var collection = database.GetCollection<Category>("Category");
            return collection;
        }

        public Product GetProductByName(string name)
        {
            IMongoCollection<Product> collection = ProductConnect();
            var filter = new BsonDocument("Name", name);
            return collection.Find(filter).FirstOrDefault();
        }

        public List<Product> GetFilteredProducts(ProductFiltersViewModel model, int offset, int qty)
        {
            IMongoCollection<Product> collection = ProductConnect();
            var filters = new List<FilterDefinition<Product>>();
            if (model.Filters != null)
            {
                filters = Filters(model.Filters);
            }

            filters.AddRange(StaticFilters(model));
            var filter = Builders<Product>.Filter.And(filters);

            var qresult = collection.Find(filter);
            var sortings = GetSortings(model);
            if (sortings != null)
            {
                qresult = qresult.Sort(sortings);
            }

            var result = qresult
                .Skip(offset)
                .Limit(qty)
                .ToList();
            return result;
        }

        #endregion


        public void IncreaseRating(double mark, string product)
        {
            var session = _client.StartSession();
            IMongoCollection<Product> collection = ProductConnect();
            var filter = new BsonDocument("_id", new ObjectId(product));
            var doc = collection.Find(filter).FirstOrDefault();
            var updateRating = Builders<Product>.Update.Set("Rating",
                ((doc.Rating * doc.RatingQty) + mark) / (doc.RatingQty + 1));
            var updateRatingQty = Builders<Product>.Update.Set("RatingQty", doc.RatingQty + 1);
            session.StartTransaction();
            try
            {
                collection.UpdateOne(filter, updateRating);
                collection.UpdateOne(filter, updateRatingQty);
                session.CommitTransaction();
            }
            catch
            {
                session.AbortTransaction();
                throw new Exception("Opps, error =(");
            }
        }

        public void ReduceRating(double mark, string product)
        {
            var session = _client.StartSession();
            IMongoCollection<Product> collection = ProductConnect();
            var filter = new BsonDocument("_id", new ObjectId(product));
            var doc = collection.Find(filter).FirstOrDefault();

            var updateRating = Builders<Product>.Update.Set("Rating",
                ((doc.Rating * doc.RatingQty) - mark) / (doc.RatingQty - 1));

            var updateRatingQty = Builders<Product>.Update.Set("RatingQty", doc.RatingQty - 1);
            session.StartTransaction();
            try
            {
                collection.UpdateOne(filter, updateRating);
                collection.UpdateOne(filter, updateRatingQty);
                session.CommitTransaction();
            }
            catch
            {
                session.AbortTransaction();
                throw new Exception("Opps, error =(");
            }
        }
    }
}
