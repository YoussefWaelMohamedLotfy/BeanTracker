using BeanTracker.API.Data;
using BeanTracker.API.Data.Entities;
using CommunityToolkit.Datasync.Server;
using CommunityToolkit.Datasync.Server.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BeanTracker.API.Controllers;

[Route("tables/[controller]")]
public class FavouriteDrinkController(ApiDbContext context)
    : TableController<FavouriteDrinkEntity>(
        new EntityTableRepository<FavouriteDrinkEntity>(context),
        new TableControllerOptions { PageSize = 100, EnableSoftDelete = true });
