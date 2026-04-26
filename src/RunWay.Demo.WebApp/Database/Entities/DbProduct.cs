using System.ComponentModel.DataAnnotations;

namespace Kododo.RunWay.Demo.WebApp.Database.Entities;

public class DbProduct
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }
}