using System;
using System.Collections.Generic;
using System.Linq;
using DAL.Models;
using MongoDB.Driver;

namespace DAL.Repositories
{
    public class OrderRepo:Repo
    {
        public OrderRepo():base(){}
        public OrderRepo(IMongoClient client, MarketContext marketContext):base(client, marketContext) { }

        public IQueryable<Order> QGetOrdersOfUser(string userName, IQueryable<Order> query)
        {
            return query
                .Where(p => p.Username.StartsWith(userName));
        }

        public IQueryable<Order> QAllOrders()
        {
            return _context.Orders;
        }

        public IQueryable<Order> QGetById(int id, IQueryable<Order> query)
        {
            return query.Where(p=>p.Id==id);
        } 
        
        public List<int> OrderIdsByProduct(string productId)
        {
            return _context.OrderProducts
                .Where(p => p.Product == productId)
                .Select(p => p.OrderId)
                .ToList();
        }

        

        public IQueryable<Order> QOrdersSort(IQueryable<Order> query,string sortParameter)
        {
            if (sortParameter == "New")
            {
                return query.OrderByDescending(p => p.Date);
            }
            else
            {
                 return query.OrderBy(p => p.Date);
                
            }
            
        }

        public int CountOrdersOfUser(IQueryable<Order> query)
        {
            return query.Count();
        }
        
        public IQueryable<Order> QOrdersStatus(IQueryable<Order> query, string statusValue)
        {
            return query.Where(p => p.Status == statusValue);
        }
        public List<Order> ExecuteList(IQueryable<Order> query, int offset = 0, int qty = Int32.MaxValue)
        {
            return query.Skip(offset).Take(qty).ToList();
        }

        public List<OrderProduct> ProductsOrder(int id)
        {
            return _context.OrderProducts
                .Where(p => p.OrderId == id)
                .ToList();
        }


        public int CreateOrder(string userName, string address, string deliveryType)
        {
            Order order = new Order
            {
                Username = userName,
                Date = DateTime.Now,
                Status = "Created",
                Deliverytype = deliveryType,
                Address = address,
            };
            _context.Orders.Add(order);
            _context.SaveChanges();
            var id = _context.Orders.FirstOrDefault(ord => ord.Id == order.Id).Id;
            return id; 
        }
        
        public void CreateOrderProduct(int orderId, Dictionary<string, int> prods)
        {
            foreach(var item in prods)
            {
                OrderProduct orderProduct = new OrderProduct
                {
                    OrderId = orderId,
                    Product = item.Key,
                    Qty = item.Value
                };
                _context.OrderProducts.Add(orderProduct);
            }
            _context.SaveChanges();
        }

        public Order GetOrderById(int id)
        {
            return _context.Orders.FirstOrDefault(p => p.Id == id);
        }

        public void ChangeStatus(int id, string status)
        {
            using var transaction = _context.Database.BeginTransaction();
            _context.Orders.Find(id).Status = status;
            _context.Histories.Add(new OrderHistory()
            {
                OrderId = id,
                Status = status,
                Time = DateTime.Now,
                
            });
            _context.SaveChanges();
            transaction.Commit();
        }

        public List<OrderHistory> GetTimeLine(int id)
        {
            return _context.Histories
                .Where(p => p.OrderId == id)
                .OrderBy(p=>p.Time)
                .ToList();
        }

    }
}