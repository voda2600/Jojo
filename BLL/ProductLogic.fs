namespace BLL
open System
open System.Collections.Generic
open System.Reflection.PortableExecutable
open BLL.Monads
open BLL.OutputModelsForProduct
open DAL.Models
open DAL.Repositories
open ViewModels




module ProductLogic =
    
    open Monads
                
    type ModelOrError<'a when 'a:null>(monad:Result<'a, string>) =
        let Model =
            match monad with
            | Success s -> s
            | Failure f -> null
        let ErrorMessage =
            match monad with
            |Success s -> null
            |Failure f -> f
        member this.GetModel =
            Model
        member this.GetErrorMessage =
            ErrorMessage
                
                
                
    let private productRepo = new ProductRepo()
    let private commentRepo = new CommentRepo()
    let private orderRepo = new OrderRepo()

    let private SetProduct(productView:ProductPage)(id:string) =
        try
            let product = productRepo.GetProductsById(id).Result
            productView.SetProduct product
            productView|>Success
        with
        |_-> Failure "Can not find product =("
    let private SetComments(productView:ProductPage)(id:string) =
        try
            let comments = commentRepo.GetCommentsOfProduct(id)
            productView.SetComments comments
            productView|>Success
        with
        |_-> Failure "Oops, error =("
        
    
    let private SetRelatedProducts(productView:ProductPage)(id:string) =
        try
           let ordersIds = orderRepo.OrderIdsByProduct(id)
           let productIds = productRepo.GetAllProductOfOrders(ordersIds)
           let resultIds = productIds
                               |> Seq.toList
                               |> List.sortByDescending(fun x -> productIds
                                                                        |>Seq.toList
                                                                        |>List.countBy(fun x -> x=id))
                               |> List.where(fun x -> x<>id)
                               |> List.distinct
                               |> List.truncate 4
           let relatedProducts = resultIds
                                 |>List.map(fun x -> productRepo.GetProductsById(x).Result)
                                 |>List.filter(fun x -> (x<>null && productRepo.GetCategoryByName(x.CategoryName).Result<>null))
           productView.SetRelatedProducts (relatedProducts|>List)
           productView|>Success
        with
        |_-> Failure "Oops, error =("
    
    let GetProductView (id:string) =
         let productView = new ProductPage()
         let success = new SuccessMonad()
         let result = success{
             let! productViewWithProduct = SetProduct productView id
             let! productViewWithComments = SetComments productViewWithProduct id
             let! productViewWithRelatedProducts = SetRelatedProducts productViewWithComments id
             return productViewWithRelatedProducts
         }
         new ModelOrError<ProductPage>(result)
    
    let DeleteProduct(id:string) =
        productRepo.CheckAndDeleteValueOption(id).Wait();
        let product = productRepo.GetProductById(id);
        let deleteProduct=
            try
                productRepo.DeleteProduct(id) |> Success 
            with
                |_ -> Failure "Opps, error"
        let productProductsWithPhoto = productRepo.GetProductWithPhoto(product.Result.PhotoPath);

        match deleteProduct with
        | Success s ->
            if productProductsWithPhoto.Count = 0
                then 
                    let result = product.Result |> Success 
                    new ModelOrError<Product>(result)
                else 
                    let result = null |> Success
                    ModelOrError<Product>(result)
        | Failure f -> 
            let result = null |> Failure
            ModelOrError<Product>(result)


    let public Validate(model)(statement:bool)(errorMessage:string) =
        if statement
            then
                model|>Success
            else
                Failure errorMessage
    let WriteToDbComment(reviewModel:ReviewViewModel) =
        try
            commentRepo.AddComment(Convert.ToDouble(reviewModel.Qty), reviewModel.Product, reviewModel.Review, reviewModel.UserName).Wait()|>Success
        with
        |_->Failure "Can not add comment"
        
    
    let GetProductById(id:string) =
        let product = productRepo.GetProductsById(id).Result 
        product 

        
    let AddComment(reviewModel:ReviewViewModel) =
        let model = new Comment()
        model.Mark<-Convert.ToDouble(reviewModel.Qty)
        model.Product <- reviewModel.Product
        model.Username <- reviewModel.UserName
        model.Content <- reviewModel.Review
        
        let ReviewLength =
            if reviewModel.Review <> null then
                reviewModel.Review.Length<150
            else
                true
                
        let success= new SuccessMonad()
        let result =
            success{
            let! uniqComment = Validate 0 ((commentRepo.GetUserCommentOfProduct(reviewModel.UserName,reviewModel.Product))=null) "You can left only one comment"
            let! commentLength = Validate 0 (ReviewLength) "Your review is too long"
            let! addComment = WriteToDbComment reviewModel
            return addComment
        }
        
        match result with
        |Success s -> ModelOrError<Comment>(model|>Success)
        |Failure f -> ModelOrError<Comment>(f|>Failure)
                
    let private HandleDoubleAsString(number:string) =
        if number<>null then
            number.Replace(".", ",")
        else
            number
    let private SingleValueTransform(characteristic:Characteristic) =
        if characteristic.Type = "bool" then
            if characteristic.ValueString = "true" then characteristic.Value<-true
            else characteristic.Value<-false
        else if characteristic.Type = "number" then
            characteristic.Value <- Convert.ToDouble(HandleDoubleAsString(characteristic.ValueString))
            if(Convert.ToDouble(characteristic.Value)>1000000000.0
               || Convert.ToDouble(characteristic.Value) < -1000000000.0) then
                raise (System.Exception(""))
        else
            if characteristic.ValueString<>null then
                characteristic.Value <- (characteristic.ValueString.Trim())
            else characteristic.Value <- "-"    
            
    
    let private ValuesTransform (product:Product) =
        try
            if product.Characteristics<>null then
                product.Characteristics
                |> Seq.toList
                |> List.iter(fun x -> SingleValueTransform(x))
            product |> Success
        with
        |_-> Failure "Value is not correct"
    let private AddProduct(product:Product) =
        try
            productRepo.AddProduct(product)
            product|>Success
        with
        |_-> Failure "Oops, error =("

    let private UpdateProductByID(product:Product)(_id:string) =
        try
            productRepo.UpdateProductById(product,_id)
            product|>Success
        with
        |_-> Failure "Oops, Update Failed"
        
    let private GetValueOfCharacteristicByName(product:Product)(name:string) =
        
        let result = product.Characteristics
                        |>Seq.toList
                        |>List.filter(fun x -> x.Name=name)
                        |>List.map(fun x -> x.Value)
        result.Head                
    
    
    let private AddValueOption(product:Product) =
         try
             if product.Characteristics <> null then
                 product.Characteristics
                            |> Seq.toList
                            |> List.filter(fun x -> x.Type="string")
                            |> List.iter(fun x -> productRepo
                                                      .AddValueOption
                                                      (product.CategoryName,
                                                       x.Name,
                                                       Convert.ToString(GetValueOfCharacteristicByName product (x.Name.Trim())))
                                                      )
             |>Success
             
         with
         |_-> Failure "Oops, error =("
         
        
        
    let InsertProduct(product:Product) =
        let success=new SuccessMonad()
        let result = success{
            let! hasCategory = Validate product (product.CategoryName<>"-") "Choose category"
            let! hasName = Validate hasCategory (product.Name<>null) "Name must be"
            let! hasPrice = Validate hasName (product.Price>0.0) "Price is invalid"
            let! hasBrand = Validate hasPrice (product.Brand<>null) "Brand must be"
            let! uniqueName = Validate hasBrand ((productRepo.GetProductByName(product.Name))=null)
                                  "This product already exists"
            let! transformedValue = ValuesTransform uniqueName
            let! addedProduct = AddProduct transformedValue
            let! valueOptionSet = AddValueOption addedProduct
            return valueOptionSet
        }
        match result with
        |Success s ->
            "Product successfully added"
        |Failure f -> f


    let IFthisPhoroUseMoreProduct(path:string) =
         let resultMy = productRepo.IFthisPhoroUseMoreProduct(path)
         resultMy

    let InsertProductByID(product:Product)(_id:string) =
        productRepo.CheckAndDeleteValueOption(_id).Wait();
        let success=new SuccessMonad()
        let result = success{
            let! hasCategory = Validate product (product.CategoryName<>"-") "Choose category"
            let! hasName = Validate hasCategory (product.Name<>null) "Name must be"
            let! hasPrice = Validate hasName (product.Price>0.0) "Price is invalid"
            let! hasBrand = Validate hasPrice (product.Brand<>null) "Brand must be"
            let! transformedValue = ValuesTransform hasBrand
            let! addedProduct = UpdateProductByID transformedValue _id
            let! res = AddValueOption addedProduct
            return res
        }

        match result with
        |Success s ->
            "Product successfully updated"
        |Failure f -> f
    
    
    let ValidateAddToCard(productId:string)(card:Dictionary<string, int>) =
        let success = new SuccessMonad()
        let product = productRepo.GetProductsById(productId).Result
        let message = "Product added to card!"
        let keys = card.Keys|>List
        let result = success {
            let! hasCategory = Validate message (productRepo.GetCategoryByName(product.CategoryName)<>null)
                                   "Product is no longer available!"
            let! notInCard = Validate hasCategory (not(keys.Contains(product._id.ToString()))) "Product already in your card!"
            return notInCard                       
        }
        match result with
        |Success s ->
            card.Add(productId, 1)
            s
        |Failure f -> f


        