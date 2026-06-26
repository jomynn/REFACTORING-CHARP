// ── Composition root (Program.cs / DI registration) ──────────────────────────
//
// var connStr = builder.Configuration.GetConnectionString("Shop")!;
// builder.Services.AddScoped<IProductRepository>(_ => new SqlProductRepository(connStr));
// builder.Services.AddScoped<IOrderRepository>(_ => new SqlOrderRepository(connStr));
// builder.Services.AddHttpClient<IPaymentGateway, HttpPaymentGateway>(c =>
//     c.BaseAddress = new Uri(builder.Configuration["PaymentGateway:BaseUrl"]!));
// builder.Services.AddHttpClient<INotificationService, HttpNotificationService>(c =>
//     c.BaseAddress = new Uri(builder.Configuration["Notifications:BaseUrl"]!));
// builder.Services.AddScoped<OrderService>();