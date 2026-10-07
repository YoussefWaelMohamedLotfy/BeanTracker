using BeanTracker.API.Data;
using BeanTracker.API.Data.Entities;
using CommunityToolkit.Datasync.Server;
using CommunityToolkit.Datasync.Server.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BeanTracker.API.Controllers;

[Route("tables/[controller]")]
public class CoffeeDrinkController(ApiDbContext context)
    : TableController<CoffeeDrinkEntity>(
        new EntityTableRepository<CoffeeDrinkEntity>(context),
        new TableControllerOptions { PageSize = 100, EnableSoftDelete = true });
