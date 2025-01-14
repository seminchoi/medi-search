using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogServer.Models;

public class InstitutionHour
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int InstitutionId { get; set; }
    
    [StringLength(4)]
    public string? MonStart { get; set; }
    
    [StringLength(4)]
    public string? MonEnd { get; set; }
    
    [StringLength(4)]
    public string? TuesStart { get; set; }
    
    [StringLength(4)]
    public string? TuesEnd { get; set; }
    
    [StringLength(4)]
    public string? WedStart { get; set; }
    
    [StringLength(4)]
    public string? WedEnd { get; set; }
    
    [StringLength(4)]
    public string? ThursStart { get; set; }
    
    [StringLength(4)]
    public string? ThursEnd { get; set; }
    
    [StringLength(4)]
    public string? FriStart { get; set; }
    
    [StringLength(4)]
    public string? FriEnd { get; set; }
    
    [StringLength(4)]
    public string? SatStart { get; set; }
    
    [StringLength(4)]
    public string? SatEnd { get; set; }
    
    [StringLength(4)]
    public string? SunStart { get; set; }
    
    [StringLength(4)]
    public string? SunEnd { get; set; }

    // Navigation property
    [ForeignKey("InstitutionId")]
    public virtual Institution? Institution { get; set; }
}
