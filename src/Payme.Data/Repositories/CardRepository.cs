﻿using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Payme.Data.IRepositories;
using Payme.Domain.Entities.Cards;

namespace Payme.Data.Repositories;

public class CardRepository : ICardRepository
{
    private readonly string? connectionString;

    public CardRepository(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<Card> InsertAsync(Card card)
    {
        var query = @"INSERT INTO Cards (CustomerId, Type, Number, ExpiryData, Password, Balance, CreatedAt, UpdatedAt, DeletedAt, IsDeleted) 
                             VALUES (@CustomerId, @Type, @Number, @ExpiryData, @Password, @Balance, @CreatedAt, @UpdatedAt, @DeletedAt, @IsDeleted)
                             RETURNING Id";

        using (var connection = new NpgsqlConnection(connectionString))
        {
            var id = await connection.ExecuteScalarAsync<long>(query, card);
            card.Id = id;
        }
        return card;
    }

    public async Task<Card> UpdateAsync(Card card)
    {
        var query = @"UPDATE Cards SET CustomerId = @CustomerId, Type = @Type, 
                                       Number = @Number, ExpiryData = @ExpiryData, Password = @Password,
                                       Balance = @Balance,UpdatedAt = @UpdatedAt, IsDeleted = false
                                   WHERE Id = @Id";

        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.ExecuteAsync(query, card);
        }
        return card;
    }

    public async Task<bool> DeleteAsync(Card card)
    {
        var query = @"UPDATE Cards SET IsDeleted = false, DeletedAt = @DeletedAt 
                                   WHERE Id = @Id";

        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.ExecuteAsync(query, card);
        }
        return true;
    }

    public async Task<Card> SelectAsync(long id)
    {
        var query = "SELECT * FROM Cards WHERE IsDeleted = false";

        using (var connection = new NpgsqlConnection(connectionString))
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await connection.QueryFirstOrDefaultAsync<Card>(query, new { id = id });
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    public async Task<IEnumerable<Card>> SelectAllAsIEnumerableAsync()
    {
        var cardQuery = "SELECT * FROM Cards WHERE Id = @id AND IsDeleted = false";

        using (var connection = new NpgsqlConnection(connectionString))
        {
            return await connection.QueryAsync<Card>(cardQuery);
        }
    }

    public async Task<IQueryable<Card>> SelectAllIQueryableAsync()
    {
        var query = "SELECT * FROM Cards WHERE IsDeleted = false";

        using (var connection = new NpgsqlConnection(connectionString))
        {
            return (await connection.QueryAsync<Card>(query)).AsQueryable();
        }
    }
}
