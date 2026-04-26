using PyraminxCube.Rbac.AspNetCore.Extensions;
using PyraminxCube.Rbac.EntityFrameworkCore.Extensions;
using PyraminxCube.Rbac.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置 RBAC 数据库服务
builder.Services.AddRbacEntityFrameworkCore(options =>
{
    options.DbType = RbacDbType.Sqlite;
    options.ConnectionString = builder.Configuration.GetConnectionString("RbacDb")
        ?? "Data Source=rbac.db";
});

// 配置 RBAC ASP.NET Core 服务（授权处理器、当前用户等）
builder.Services.AddRbacAspNetCore();

// 配置认证（示例：使用 JWT，实际项目需要配置真实的认证方式）
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options => { /* JWT 配置 */ });
// builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 注意：如果使用认证，需要在授权之前添加
// app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// 自动迁移数据库（开发环境）
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<RbacDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
