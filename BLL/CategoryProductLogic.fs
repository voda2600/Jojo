namespace BLL

open System
open System.Collections.Generic
open BLL
open BLL.OutputModelsForCategoryProduct
open BLL.OutputModelsForProfile
open BLL.ProductLogic
open DAL.Models
open DAL.Repositories
open ViewModels

module CategoryProductLogic =
    open Monads
    let private productRepo = new ProductRepo()
    
    let private commentRepo = new CommentRepo()
    
    
    let SetLast(model:CategoryProductsModel)(realQty:int)(qty:int) =
        if realQty <= qty then model.SetisItLast true
        model
   
        
    let SetRatingForCard (product:ProductCard) =
        let rating = product.GetProduct.Rating
        product.SetRating rating
        product
            
            
    let private SetProducts(model:CategoryProductsModel)(category:string)(offset:int)(qty:int) =
        try
            let products = productRepo.GetProductsOfCategory(category, offset, qty+1)
            let newModel = SetLast model products.Count qty
            newModel.SetProducts (products
                               |>Seq.truncate qty
                               |> Seq.toList
                               |> List.map(fun x -> new ProductCard(x))
                               |> List.map(fun x -> SetRatingForCard(x)))
            newModel|>Success
        with
        |_-> Failure "Oops, error =("
        
       
    let private SetCategory(model:CategoryProductsModel)(category:string) =
        try
            model.SetCategory (productRepo.GetCategoryByName(category).Result)
            model|>Success
        with
        |_-> Failure "Oops, error =("


    let private SetInputSearch(model:CategoryProductsModel)(inputSearch:string) =
          try
              model.SetSearch (inputSearch)
              model|>Success
          with
          |_-> Failure "Oops, error =("
    let GetProductsOfCategory(category:string)(qty:int) =
        let success = new SuccessMonad()
        let model = new CategoryProductsModel(qty)
        let result = success{
            let! modelWithProducts =  SetProducts model category 0 qty
            let! modelWithCategory =  SetCategory modelWithProducts category
            return modelWithCategory
        }
        new ModelOrError<CategoryProductsModel>(result)
        
    let GetNextProducts(category:string)(offset:int)(qty:int) =
        let success = new SuccessMonad()
        let model = new CategoryProductsModel(qty)
        let result = success{
            let! modelWithProducts =  SetProducts model category offset qty
            return modelWithProducts
        }
        new ModelOrError<CategoryProductsModel>(result)
        
    
   
       
    let SetFilteredProducts (filter:ProductFiltersViewModel)(model:CategoryProductsModel)(offset:int)(qty:int) =
        try
            let products = productRepo.GetFilteredProducts(filter, offset, qty+1)
            let newModel = SetLast model products.Count qty
            newModel.SetProducts (products
                               |> Seq.truncate qty
                               |> Seq.toList
                               |> List.map(fun x -> new ProductCard(x))
                               |> List.map(fun x -> SetRatingForCard(x)))
                               
           
            newModel|>Success
        with
        |_-> Failure "Oops, error =("
        
        
        
    let GetProductsWithFilters(filter:ProductFiltersViewModel)(offset:int)(qty:int)(inputSearch:string) =
        let success = new SuccessMonad()
        let model = new CategoryProductsModel(qty)
        let result = success{
            let! modelWithProducts =  SetFilteredProducts filter model offset qty
            let! modelWithCategory =  SetCategory modelWithProducts filter.Category
            let! modelWithSearch = SetInputSearch modelWithCategory inputSearch
            return modelWithSearch
        }
        new ModelOrError<CategoryProductsModel>(result)