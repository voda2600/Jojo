namespace BLL

open System
open System.Collections.Generic
open DAL.Models
open DAL.Models
open DAL.Models
open DAL.Models
open DAL.Repositories



module OutputModelsForProfile =

    type OrderInfo(order:Order) =
        let mutable Order:Order = order
        let mutable totalPrice:double = 0.0
        member this.SetOrder x =
            Order<-x
        member this.SetTotal x =
            totalPrice <- x
            
        member this.GetOrder =
            Order
        member this.GetTotal =
            totalPrice
    type ProductRecord(product, _qty) =
        let mutable Product:Product = product
        let mutable Qty:int = _qty
            
        let total =
            product.Price*(double)Qty
            
        member this.GetTotal =
            total
        member this.GetQty =
            Qty

        member this.GetProduct =
            Product
        member this.SetQty x =
            Qty<-x
            
        member this.SetProduct x=
            Product <- x
            
            
    [<AllowNullLiteral>]       
    type UserProfile(_qty) =
        let mutable User:User = null
        let mutable Order_Product:Dictionary<OrderInfo, list<ProductRecord>> = null               
        
        let mutable totalRecords:int = 0
        
        let qty:int = _qty
        
        member this.GetQty=
            qty
        
        member this.SetUser x =
            User<-x
        member this.GetUser =
            User
            
        member this.SetTotal x =
            totalRecords<-x
        member this.GetTotal =
            totalRecords
        member this.SetOrderProduct x =
            Order_Product<-x
        member this.GetOrderProduct =
            Order_Product
            
    [<AllowNullLiteral>]         
    type OrderInfoForAdmin() =
        let mutable Order:Order = null
        
        let mutable Products:List<ProductRecord> = null
        
        member this.Total =
            if Products<>null then
                Products
                |>Seq.toList
                |>List.sumBy(fun x -> ((double)x.GetQty*x.GetProduct.Price))
            else
                0.0
        
        member this.GetOrder =
            Order
        member this.SetOrder x =
            Order <- x
        
        member this.GetProducts =
            Products
        member this.SetProducts x =
            Products<-x
        
    [<AllowNullLiteral>]    
    type TimeLine() =
        let mutable list:List<OrderHistory> = null
        let mutable Order:Order = null
        
        member this.SetOrder x =
            Order<-x
        member this.SetLine x=
            list<-x
        
        member this.GetLine =
            list
        member this.GetOrder =
            Order
        
module OutputModelsForProduct =
    
    
    [<AllowNullLiteral>]    
    type ProductPage() =
        let mutable product:Product = null
        let mutable reviews:List<Comment> = null
        let mutable relatedProducts:List<Product> = null
        
        let mutable rating:double = 0.0
        
        
        member this.GetProduct =
            product
        member this.ReviewsCount =
            reviews.Count
        member this.GetRating =
            if this.ReviewsCount <> 0 then
                let sumMarks = reviews
                                |>Seq.toList
                                |>List.map(fun x -> x.Mark)
                                |>List.sum
                sumMarks/(Convert.ToDouble(this.ReviewsCount))
            else
                0.0
        member this.GetComments =
            reviews
        member this.GetRelatedProducts =
            relatedProducts
        
        member this.SetProduct x =
            product<-x
        member this.SetComments x =
            reviews <- x
        member this.SetRelatedProducts x =
            relatedProducts <- x

module OutputModelsForCity =
    
    
    [<AllowNullLiteral>]    
    type ProductPage() =
        let mutable city:City = null
        let mutable citys:List<City> = null
        
        let mutable rating:double = 0.0
        
        
        member this.GetCityG =
            city.StateId
        member this.CitysCountY =
            citys.Count
        member this.GetCity =
            if this.CitysCountY = 0 then
                let sum = citys
                                |>Seq.toList
                                |>List.map(fun x -> x.StateId)
                                |>List.sum
                sum+this.CitysCountY
            else
                0
        member this.GetComments =
            citys
        member this.GetCountofCity =
            citys.Count
        
        member this.SetCity x =
            city<-x
        member this.SetCitys x =
            citys <- x
        member this.SetProg x =
             rating <- x
            
            

module OutputModelsForCategoryProduct =
    type ProductCard(_product) =
        let product:Product = _product
        let mutable rating:double = 0.0
        
        member this.GetProduct =
            product
        member this.SetRating x =
            rating<-x
            
        member this.GetRating =
            rating    
    [<AllowNullLiteral>] 
    type CategoryProductsModel(_qty) =
        let mutable products:list<ProductCard> = list.Empty
        let mutable category:Category = null
        let mutable inputSearch:string = null
        let mutable isItLast: bool = false
        
        
        let qty:int = _qty
        
        member this.GetisItLast =
            isItLast
        member this.SetisItLast x=
            isItLast<-x
         
        member this.GetQty =
            qty
        
        member this.SetProducts x =
            products<-x
        member this.GetProducts =
            products
        member this.SetCategory x =
            category<-x
        member this.GetCategory =
            category
        member this.SetSearch x =
            inputSearch<-x
        member this.GetSearch =
            inputSearch
    
    
    