using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PickleballApi.Models;

public class MatchStatistics
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int HomeWins { get; set; }
    public int AwayWins { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
