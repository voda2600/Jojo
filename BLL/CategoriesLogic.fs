namespace BLL

open System
open BLL
open System.Collections.Generic
open BLL.Monads
open BLL.ProductLogic
open DAL.Models
open DAL.Repositories
open MongoDB.Bson
open System.IO

module CategoriesLogic =
    let private productRepo = new ProductRepo()
    let GetListOfCategoriesNames =
        productRepo.GetCategoryNames

   
        
   
    let private Validate(model)(statement:bool)(errorMessage:string) =
        if statement
            then
                model|>Success
            else
                Failure errorMessage
    let private HasEmptyFields (fields:List<Characteristic>) =
        if fields <> null then
            fields|>Seq.toList|>List.forall(fun x -> x.Name<>null)
        else true    
    let private HasEmptyFieldsonSearch (fields:List<Category>) =
        fields|>Seq.toList|>List.forall(fun x -> x.Name<>null)
    let IsCategoryUniq (name:string) =
        let category = (productRepo.GetCategoryByName name).Result
        if category=null
            then true
            else false
       
        
    let GetSearchOfCategory (text:string)=
           let answer = productRepo.GetSearchCategory text
           answer

    let WriteToDBCategory(category:Category)(successMessage:string) =
            try
                productRepo.AddCategory(category) |> ignore
                successMessage|>Success
            with
            |_-> Failure "Can not add category =("
    let CharacteristicsUniq(successMessage:string)(category:Category) =
        if category.Characteristics <> null then
            let ch = category.Characteristics
                            |>Seq.toList
                            |>List.distinctBy(fun x -> x.Name)
            category.Characteristics <- (ch|>List)
        successMessage|>Success
        
    let AddCategory(category:Category) =
        let success = new SuccessMonad()
        let successMessage = "Category successfully created"
        let result = success{
            let! nameNotEmpty = Validate successMessage (category.Name<>null) "Name must be"
            let! uniqCategory = Validate nameNotEmpty (IsCategoryUniq category.Name) "Category already exists"
            let! fieldsAreFull = Validate uniqCategory (HasEmptyFields category.Characteristics) "Contains empty fields"
            let! uniqCharacteristics = CharacteristicsUniq fieldsAreFull category
            let! addToDB = WriteToDBCategory category uniqCharacteristics
            return addToDB
        }
        new ModelOrError<string>(result)
            
    
    let GetCharacteristics (name:string) =
        try
            let category = (productRepo.GetCategoryByName name).Result
            let characteristics = category.Characteristics |> Success
            new ModelOrError<List<Characteristic>>(characteristics)
            
        with
        |_->
            let result = Failure "Oops, error =("
            new ModelOrError<List<Characteristic>>(result)

    let DeleteCategory(categoryName:string) =
        let category = productRepo.GetCategoryByName(categoryName);
        let categoryProductsWithPhoto = productRepo.GetProductWithPhoto(category.Result.PhotoPath);
        let deleteCategory =
            try
                productRepo.DeleteCategory(categoryName) |> Success 
            with
                |_ -> Failure "Opps, error"

        match deleteCategory with
        | Success s ->
            if categoryProductsWithPhoto.Count = 0
                then 
                    let result = category.Result |> Success 
                    new ModelOrError<Category>(result)
                else 
                    let result = null |> Success
                    ModelOrError<Category>(result)
        | Failure f -> 
            let result = null |> Failure
            ModelOrError<Category>(result)
    
                       



        
             