using Microsoft.EntityFrameworkCore;
using PaymentProcessing.Application.Services;
using PaymentProcessing.Domain.Entities;
using PaymentProcessing.Domain.Interfaces;
using PaymentProcessing.Domain.Interfaces.Repositories;
using PaymentProcessing.Domain.Model;
using PaymentProcessing.Infrastructure;
using PaymentProcessing.Infrastructure.Data;
using PaymentProcessing.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//For test purposes decided to use in memory db
builder.Services.AddDbContext<EFDBContext>(options =>
    options.UseInMemoryDatabase("InMemoryDb"));

builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IPaymentProviderRepository, PaymentProviderRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IngestTransactionService>();
builder.Services.AddScoped<TransactionQueryService>();

var app = builder.Build();

//Seeding Data for Test purposes
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EFDBContext>();

    if (!context.PaymentProviders.Any())
    {
        context.PaymentProviders.AddRange
         (
         
            new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "PayPal" },
            new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "Trustly" },
            new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "Stripe" },
            new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "Square" },
            new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "Zelle" },
            new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "Venmo" },
            new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "ApplePay" },
            new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "GooglePay" }
        );
    }

    if (!context.PaymentMethods.Any())
    {
        context.PaymentMethods.AddRange
        (
            new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "CreditCard" },
            new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "DebitCard" },
            new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "ACH" },
            new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "Wallet" },
            new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "Crypto" },
            new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "BankTransfer" },
            new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "CashApp" }
        );
    }

    await context.SaveChangesAsync();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
