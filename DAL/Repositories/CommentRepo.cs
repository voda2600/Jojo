using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Repositories
{
    public class CommentRepo:Repo
    {
        public CommentRepo():base(){}
        public CommentRepo(IMongoClient client, MarketContext marketContext):base(client, marketContext) { }

        public List<Comment> GetCommentsOfProduct(string product)
        {
            return _context.Comments
                .Where(p => p.Product == product)
                .ToList();
        }

        public Comment GetUserCommentOfProduct(string userName, string product)
        {
           return _context.Comments.FirstOrDefault(p => p.Username == userName && p.Product == product);
        }
        public async Task AddComment(double mark, string product, string content, string userName)
        {
            var database = _client.GetDatabase("Market");
            var collection = database.GetCollection<Product>("Product");
            var filter = Builders<Product>.Filter.Eq("_id", new ObjectId(product));
            var doc = collection.Find(filter).FirstOrDefault();
            var updateQty = Builders<Product>.Update.Inc("RatingQty", 1);
            UpdateDefinition<Product> updateRating = null;
            if (doc.RatingQty != 0)
            {
                updateRating = Builders<Product>
                    .Update.Set("Rating", ((doc.Rating * doc.RatingQty) + mark) / doc.RatingQty);
            }
            else
            {
                updateRating = Builders<Product>
                    .Update.Set("Rating", mark);
            }

            var updatingRating = collection.UpdateOneAsync(filter, updateRating);
            var updatingQty = collection.UpdateOneAsync(filter, updateQty);
            _context.Comments.Add(
                new Comment() {Content = content, Mark = mark, Username = userName, Product = product});
            var addingComment = _context.SaveChangesAsync();
            await updatingRating;
            await updatingQty;
            await addingComment;
        }

        public Comment GetCommentByPk(int id)
        {
            return _context.Comments.FirstOrDefault(p => p.Id == id);
        }

        public void DeleteComment(Comment comment)
        {
            _context.Comments.Remove(comment);
            _context.SaveChanges();
        }
    }
}