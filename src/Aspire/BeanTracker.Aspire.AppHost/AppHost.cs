var builder = DistributedApplication.CreateBuilder(args);

var adminPassword = builder.AddParameter("Password", secret: true);

var postgres = builder
    .AddPostgres("postgres", password: adminPassword, port: 5432)
    .WithImageTag("alpine")
    //.WithDataVolume()
    .WithVolume("beantracker-pg-data", "/var/lib/postgresql")
    .WithPgAdmin(x => x.WithImageTag("latest").WithHostPort(5050).WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);

var KeycloakDb = postgres.AddDatabase("Keycloak-Db");

var keycloak = builder
    .AddKeycloak("keycloak", 8081, adminPassword: adminPassword)
    .WithImageTag("latest")
    .WithPostgres(KeycloakDb)
    .WaitFor(KeycloakDb)
    .WithRealmImport("./Realms")
    .WithLifetime(ContainerLifetime.Persistent);

var publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
    .WithAnonymousAccess();

var mauiApp = builder.AddMauiProject("mauiapp", "../../Apps/BeanTracker.MAUI/BeanTracker.MAUI.csproj");

if (OperatingSystem.IsWindows())
{
    mauiApp.AddWindowsDevice();
}

if (OperatingSystem.IsMacOS())
{
    mauiApp.AddMacCatalystDevice();

    mauiApp.AddiOSSimulator()
        .WithOtlpDevTunnel();
}

mauiApp.AddAndroidDevice()
    .WithOtlpDevTunnel();

await builder.Build().RunAsync();
