namespace BLL

open System.Collections.Generic
open System.Net
open System.Net.Mail
open System.Security.Claims
open BLL.Monads
open BLL.OutputModelsForProfile
open BLL.ProductLogic
open DAL.Models
open DAL.Repositories
open BLL.ProfileLogic
open System

module OrderLogic =
    
    let private orderRepo = new OrderRepo()
    let private productRepo = new ProductRepo()
    let private userRepo = new UserRepo()
    
    let private SetOrder(model:OrderInfoForAdmin)(id:int) =
        try
            let order = orderRepo.GetOrderById id
            model.SetOrder order
            model|>Success
        with
        |_-> Failure "Can not find order"
    let private SetProducts(model:OrderInfoForAdmin)(id:int) =
        try
            let productIdsWithQty = productRepo.GetProductIdsOfOrder(id)
            let productIds = productIdsWithQty|>Seq.toList|>List.map(fun x -> x.ProductId)|>List
            let products = productRepo.GetProductsFromList(productIds).Result
            let records = products
                        |>Seq.toList
                        |>List.map(fun x -> new ProductRecord(x, (productIdsWithQty
                                                                  |>Seq.toList
                                                                  |>List.find(fun p -> p.ProductId = x._id.ToString())).Qty))
                        |>List
            model.SetProducts records
            model |>Success
        with
        |_-> Failure "Can not find products"
    
    let GetOrderRecordBy(id:int) =
        let model = new OrderInfoForAdmin()
        let success = new SuccessMonad()
        let result = success{
                let! modelWithOrder = SetOrder model id
                let! modelWithProducts = SetProducts modelWithOrder id
                return modelWithProducts
            }
        new ModelOrError<OrderInfoForAdmin>(result)
        
    let UpdateStatus(message:string)(orderId:int)(status:string) =
        try
            orderRepo.ChangeStatus(orderId,status)
            message|>Success
        with
        |_-> Failure "Can not update status"
        
    let SendEmail(m:string)(status:string)(orderId:int)(url:string) =
        try
            let user = userRepo.GetUserBy(orderId)
            let from = new MailAddress("ilshatm24@gmail.com", "b3iElectronics")
            let destination = new MailAddress(user.Email)
            let message = new MailMessage(from, destination)
            message.Subject <- "Order status update"
            message.Body <- "Order "+orderId.ToString()+" changed status to "+status+"\n"+url
            let smtp = new SmtpClient("smtp.gmail.com", 587)
            smtp.Credentials<-new NetworkCredential("ilshatm24@gmail.com", "zmslgdhiirwwwhci")
            smtp.EnableSsl<-true
            smtp.Send(message)
            m|>Success
        with
        |_-> Failure "Status updated, but email notification wasn't send"    
    let ChangeStatus(orderId:int)(status:string)(url:string) =
        let success = new SuccessMonad()
        let message = "Status successfully updated"
        let result = success{
                        let! statusReallyNew = Validate message (orderRepo.GetOrderById(orderId).Status<>status) ("Order is already in status "+status)                              
                        let! newStatus = UpdateStatus statusReallyNew orderId status
                        let! emailSent = SendEmail newStatus status orderId url
                        return emailSent
                    }
        match result with
        |Success s -> s
        |Failure f -> f
            
    
    let SetTimeLine(model:TimeLine)(orderId:int) =
        try
            model.SetLine (orderRepo.GetTimeLine(orderId))
            model|>Success
        with
        |_-> Failure "Can not load report, please try again later"
        
    let SetFirstDate(model:TimeLine)(orderId:int) =
        try
            model.SetOrder (orderRepo.GetOrderById(orderId))
            model|>Success
        with
        |_-> Failure "Can not load report, please try again later"
        
        
    
        
    let GetTimeLine(id:int)(user:ClaimsPrincipal) =
        let model = new TimeLine()
        let success =  new SuccessMonad()
        let result =
            success{
                let! userAuth = Validate model (user.Identity.IsAuthenticated) "Log in first!"
                let! userHaveAccess = Validate userAuth (userRepo.GetUserBy(id).Username=user.Identity.Name || user.IsInRole("Admin"))
                                          "You have no access!"
                let! modelWithLine = SetTimeLine userHaveAccess id
                let! modelWithFirstDate = SetFirstDate modelWithLine id
                return modelWithFirstDate
            }
            
        new ModelOrError<TimeLine>(result)

    let isCardAvaliable(cardMonth: int)(cardYear: int) =
        let result =
            if cardYear = DateTime.Now.Year && cardMonth <= DateTime.Now.Month then
                false
            else
                true
        result

    let isListOfNamesCategoriesContainProdsCategory(prodsId: string) =
        let allCategories = productRepo.GetCategoryNames()
        let allCategoriesNames = new List<string>()
        for category in allCategories do
            allCategoriesNames.Add(category.Name)
        try
            let prodCategory = productRepo.GetProductById(prodsId).Result.CategoryName
            allCategoriesNames.Contains(prodCategory)
        with
        |_-> false

    let GetUnavaliableProducts(prods: Dictionary<string, int>) =
        let unavalibaleProducts = new List<string>()
        for prod in prods do
            let boolean = isListOfNamesCategoriesContainProdsCategory prod.Key
            if not boolean then
                unavalibaleProducts.Add prod.Key
        unavalibaleProducts
            
    let CreateOrder(userName: string)(address: string)(deliveryType: string)(prods: Dictionary<string, int>)(cardMonth: int)(cardYear: int) =
        let valid = isCardAvaliable cardMonth cardYear
        if valid then
            let result = 
                try
                    let createOrder = orderRepo.CreateOrder(userName, address, deliveryType)
                    orderRepo.CreateOrderProduct(createOrder, prods) |> Success
                with
                    |_ -> Failure "Oops, error"
            match result with
                |Success s -> "Ok"
                |Failure f -> f
            else "Card is not avaliable"
            
            
        
        
        
    
    
        