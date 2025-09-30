using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PickleballApi.Models;

public class Game
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public GameType GameType { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public int ServingTeam { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}

