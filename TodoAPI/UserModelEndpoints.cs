﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using System.Security.Cryptography;
using TodoAPI.Data;
using TodoAPI.Model;
using System.Text;

namespace TodoAPI;

public static class UserModelEndpoints
{
    public static void MapUserModelEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/UserModel").WithTags(nameof(UserModel));

        group.MapGet("/", async (TodoAPIContext db) =>
        {
            return await db.UserModel.ToListAsync();
        })
        .WithName("GetAllUserModels")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<UserModel>, NotFound>> (int id, TodoAPIContext db) =>
        {
            return await db.UserModel.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is UserModel model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetUserModelById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok<UserModel>, NotFound>> (int id, UserModel userModel, TodoAPIContext db) =>
        {
            var affected = await db.UserModel
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                  .SetProperty(m => m.Username, userModel.Username)
                  .SetProperty(m => m.Password, userModel.Password)
                  .SetProperty(m => m.Email, userModel.Email)
                );

            return affected == 1 ? TypedResults.Ok(userModel) : TypedResults.NotFound();
        })
        .WithName("UpdateUserModel")
        .WithOpenApi();

        group.MapPost("/", async (UserModel userModel, TodoAPIContext db) =>
        {

            if (userModel.Password == null)
            {
                throw new ArgumentNullException(nameof(userModel.Password), "Password cannot be null");
            }

            using var sha256 = SHA256.Create();
            var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(userModel.Password)));

            userModel.Password = hashedPassword;
            db.UserModel.Add(userModel);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/api/UserModel/{userModel.Id}", userModel);
        })
        .WithName("CreateUserModel")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, TodoAPIContext db) =>
        {
            var affected = await db.UserModel
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();

            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteUserModel")
        .WithOpenApi();

        group.MapPost("/signin", async (SignInRequest signInRequest, TodoAPIContext db) =>
        {

            if (signInRequest.Password == null)
            {
                throw new ArgumentNullException(nameof(signInRequest.Password), "Password cannot be null");
            }

            using var sha256 = SHA256.Create();
            var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(signInRequest.Password)));

            var user = await db.UserModel.FirstOrDefaultAsync(u => u.Username == signInRequest.Username && u.Password == hashedPassword);

            if (user != null)
            {
                var response = new SignInResponse { isVerified = true, Id = user.Id };
                return TypedResults.Ok(response);
            }
            else
            {
                var response = new SignInResponse { isVerified = false };
                return TypedResults.Ok(response);
            }
        })
        .WithName("SignIn")
        .WithOpenApi();

    }


}
