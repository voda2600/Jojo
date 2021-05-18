namespace BLL

open System
open System.Collections.Generic
open BLL.Monads
open BLL.ProductLogic
open DAL.Models
open DAL.Models
open DAL.Repositories


module Card =
    let productRepo = new ProductRepo()
    let private GetProducts(ids:Dictionary<string, int>) =
                     try
                        let dict = new Dictionary<Product, int>()
                        ids
                        |>Seq.toList
                        |>List.iter(fun x -> dict.Add((productRepo.GetProductsById(x.Key).Result), x.Value))
                        dict|>Success
                     with
                     |_-> Failure "Oops, error =("
    let GetCard(ids:Dictionary<string, int>) =
        let success = new SuccessMonad()
        let result = success{
            let! hasProducts = ProductLogic.Validate ids (ids<>null && ids.Count>0) "You have no products in your card"
            let! products = GetProducts ids
            return products
        }
       
        new ModelOrError<Dictionary<Product, int>>(result)             

