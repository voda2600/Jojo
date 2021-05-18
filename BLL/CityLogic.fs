namespace BLL

open DAL.Repositories
    
module CityLogic =
    let private cityRepo = new DAL.Repositories.CityRepo()
    let GetListOfCities =
           cityRepo.GetListOfCities
    let GetListOfStates =
        cityRepo.GetListOfStates
    let GetListOfCitiesById (id:int) =
         cityRepo.GetListOfCitiesById(id)
    