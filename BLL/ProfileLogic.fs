namespace BLL
open System
open System.Collections.Generic
open System.Linq
open BLL
open BLL
open BLL.ProductLogic
open DAL.Models
open DAL.Models
open DAL.Models
open DAL.Repositories
open OutputModelsForProfile
open ViewModels
open ViewModels

module ProfileLogic =
    open Monads
    let private userRepo = new UserRepo()
    let private orderRepo = new OrderRepo()
    let private productRepo = new ProductRepo()
   
    //(Input,profile) -> GetQuery -> AddFilters -> ExecuteQuery -> SetOrders -> profile -> SetUser
    module Filters =
        let private AddStatus(status:string)(query:IQueryable<Order>) =
            try
                if status = null
                    then
                        query |> Success
                    else
                        orderRepo.QOrdersStatus(query, status) |> Success
            with
            |_-> Failure "Oops, error =("
        let private AddSorting (sort:string)(query:IQueryable<Order>) =
            try
                if sort = null
                    then
                        query |> Success
                    else
                        orderRepo.QOrdersSort(query, sort) |> Success
            with
            |_-> Failure "Oops, error =("
            
            
        let private AddId (id:int)(query:IQueryable<Order>) =
             try
                if id = 0
                    then
                        query |> Success
                    else
                        orderRepo.QGetById(id, query) |> Success
             with
             |_-> Failure "Oops, error =("
            
        let private AddName (name:string)(query:IQueryable<Order>) =
            try
                if name = null
                    then
                        query |> Success
                    else
                        orderRepo.QGetOrdersOfUser(name, query) |> Success
             with
             |_-> Failure "Oops, error =("
            
        let AddFilters (model:ProfileViewModel) (query:IQueryable<Order>) =
            let success = new SuccessMonad()
            success{
                let! idFilter = AddId model.Id query
                let! nameFilter = AddName model.UserName idFilter
                let! statusFilter = AddStatus model.Status nameFilter
                let! sortedOrders = AddSorting model.SortType statusFilter
                return sortedOrders
            }
    module OrdersWork =
            let private SetTotalRecords (profile:UserProfile) (query:IQueryable<Order>)=
                try
                    profile.SetTotal (orderRepo.CountOrdersOfUser(query))
                    query|>Success
                with
                |_->Failure "Oops, error =("
        
        
            let private GetQuery =
                try
                    orderRepo.QAllOrders() |> Success
                with
                |_-> Failure "Oops, error =("
            let private ExecuteQuery(query:IQueryable<Order>)(model:ProfileViewModel) =
                try
                    orderRepo.ExecuteList(query,((model.Page-1)*model.Qty), model.Qty)
                        |>Seq.toList
                        |>Success
                with
                |_->Failure "Oops, error =("        
            let private GetProductsOrder(order:Order) =
                orderRepo.ProductsOrder order.Id
            let private AddDictionaryRecord(order:Order)(dict:Dictionary<OrderInfo, list<ProductRecord>>) =
                let ls = GetProductsOrder order |> Seq.toList
                let productIds = ls
                                 |> List.map(fun x -> x.Product)
                                 |> List                 
                let products = productRepo.GetProductsFromList(productIds).Result
                               |> Seq.toList
                let productsRec = new List<ProductRecord>()
                
                products |> List.iter(fun x ->
                    productsRec.Add(new ProductRecord(x, ls.FirstOrDefault(fun p -> p.Product=x._id.ToString()).Qty)))
                               
                let orderInfo = new OrderInfo(order)
                
                dict.Add(orderInfo, productsRec|>Seq.toList)      
            let private DictionaryCreate(orders:list<Order>) =
                    let dict = new Dictionary<OrderInfo, list<ProductRecord>>()
                    orders |> List.iter(fun x -> AddDictionaryRecord x dict)
                    dict 
            let private SetOrders (orders:list<Order>)(profile:UserProfile) =
                try
                    let dict = DictionaryCreate orders
                    profile.SetOrderProduct dict
                    profile|>Success
                with
                |_-> Failure "Oops, error =("
            let GetOrders(profile:UserProfile,model:ProfileViewModel) =
                    let success = new SuccessMonad()
                    success{
                        let! query = GetQuery
                        let! filteredQuery = Filters.AddFilters model query
                        let! totalsQuery = SetTotalRecords profile filteredQuery
                        let! orders = ExecuteQuery totalsQuery model
                        let! profileResult = SetOrders orders profile
                        return profileResult
                    }
    let private CalculateTotal (profile:UserProfile) =
                try
                    for KeyValue(key,value) in profile.GetOrderProduct do
                        key.SetTotal (value
                                      |>List.map(fun x->Convert.ToDouble(x.GetQty)*x.GetProduct.Price)
                                      |>List.sum)
                    profile|>Success
                with
                |_->Failure "Oops, unknown error =("    
    let private SetUser (profile:UserProfile)(userName:string) =
        try
            let user = userRepo.GetUserByPk userName
            profile.SetUser user
            profile|>Success
        with
        |_->Failure "Oops, error =("
    
    let GetProfile(model:ProfileViewModel) =
        let success = new SuccessMonad()
        let profile = new UserProfile(model.Qty)
        let result = success{
            let! profileWithOrders = OrdersWork.GetOrders(profile, model)
            let! profileWithUser = SetUser profileWithOrders model.UserName
            let! profileWithTotals = CalculateTotal profileWithUser 
            return profileWithTotals 
        }
        new ModelOrError<UserProfile>(result)

    let EditMail(model:EditInfoViewModel) = 
        let result =
            try
                userRepo.EditUserMail(model.UserName, model.NewEmail) |> Success 
            with 
                |_-> Failure "Oops, error =("
        match result with
        | Success s -> "Email updated!"
        | Failure f -> f

    let private Validate(model)(statement:bool)(errorMessage:string) =
        if statement
            then
                model|>Success
            else
                Failure errorMessage

    let EditPassword(model:EditPasswordViewModel) = 
        

        let hashedPass = Crypto.strongHash model.OldPassword
        let curPass = userRepo.GetUserByPk(model.UserName).Password
        let verifyPass = Crypto.verify curPass model.OldPassword
        let passwordMatch = Validate model (verifyPass) "Wrong password"
  
        let result =
            match passwordMatch with
            | Success s -> 
                try 
                    s.NewPassword <- Crypto.strongHash s.NewPassword
                    userRepo.EditUserPassword(s.UserName, s.NewPassword) |> Success
                with
                    |_-> Failure "Database error =("
            | Failure f -> Failure f

        match result with
        | Success s -> "Password changed"
        | Failure f -> f

                

                
    
    
            

        

        
        
           