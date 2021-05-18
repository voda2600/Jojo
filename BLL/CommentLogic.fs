namespace BLL

open System.Linq.Expressions
open BLL.Monads
open BLL.OutputModelsForCategoryProduct
open DAL.Models
open DAL.Repositories

module DeleteComment =
    // id -> GetComment -> GetProduct -> DeleteComment -> UpdateRating
    let private commentRepo = new CommentRepo()
    let private productRepo = new ProductRepo()
    
        
    let private ReduceRating (mark:double)(productId:string) =
        try
            productRepo.ReduceRating(mark, productId)|>Success
        with
        |_-> Failure "Oops, error =("
    let private GetComment (id:int) =
        try
            commentRepo.GetCommentByPk id |> Success
        with
        |_-> Failure "Oops, error =("
        
    let private DeleteComment(comment:Comment) =
        try
            commentRepo.DeleteComment(comment)|>Success
        with
        |_->
            productRepo.IncreaseRating(comment.Mark, comment.Product)
            Failure "Oops, error =("
        
    let Do (id:int) =
        let success = new SuccessMonad()
        let result = success{
            let! comment = GetComment id
            let! newRating = ReduceRating comment.Mark comment.Product
            let! deletedComment = DeleteComment comment
            return deletedComment
        }
        match result with
        |Success s -> "Comment successfully deleted"
        |Failure f -> f

