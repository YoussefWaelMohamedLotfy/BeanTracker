using BeanTracker.API.Data;
using BeanTracker.API.Data.Entities;
using CommunityToolkit.Datasync.Server;
using CommunityToolkit.Datasync.Server.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BeanTracker.API.Controllers;

[Route("tables/[controller]")]
public class BreweryController(ApiDbContext context)
    : TableController<BreweryEntity>(
        new EntityTableRepository<BreweryEntity>(context),
        new TableControllerOptions { PageSize = 100, EnableSoftDelete = true });
